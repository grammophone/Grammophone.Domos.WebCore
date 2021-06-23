using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Model binder for a collection of items implementing <see cref="IEntityWithID{K}"/>.
	/// </summary>
	/// <typeparam name="K">The type of the key of the elements.</typeparam>
	/// <typeparam name="E">The type of the collcetion's elements.</typeparam>
	public class EntityCollectionModelBinder<K, E> : CollectionModelBinder<E>
		where E : IEntityWithID<K>, new()
	{
		#region Auxilliary types

		private record CollectionResult
		{
			public IEnumerable<E> Model { get; init; }

			public IValidationStrategy ValidationStrategy { get; init; }
		}

		/// <summary>
		/// Taken from https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/ElementalValueProvider.cs
		/// </summary>
		internal class ElementalValueProvider : IValueProvider
		{
			public ElementalValueProvider(string key, string value, CultureInfo culture)
			{
				Key = key;
				Value = value;
				Culture = culture;
			}

			public CultureInfo Culture { get; }

			public string Key { get; }

			public string Value { get; }

			public bool ContainsPrefix(string prefix)
			{
				return ModelStateDictionary.StartsWithPrefix(prefix, Key);
			}

			public ValueProviderResult GetValue(string key)
			{
				if (string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
				{
					return new ValueProviderResult(Value, Culture);
				}
				else
				{
					return ValueProviderResult.None;
				}
			}
		}

		/// <summary>
		/// Taken from https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Validation/DefaultCollectionValidationStrategy.cs
		/// The default implementation of <see cref="IValidationStrategy"/> for a collection.
		/// </summary>
		/// <remarks>
		/// This implementation handles cases like:
		/// <example>
		///     Model: IList&lt;Student&gt;
		///     Query String: ?students[0].Age=8&amp;students[1].Age=9
		///
		///     In this case the elements of the collection are identified in the input data set by an incrementing
		///     integer index.
		/// </example>
		///
		/// or:
		///
		/// <example>
		///     Model: IDictionary&lt;string, int&gt;
		///     Query String: ?students[0].Key=Joey&amp;students[0].Value=8
		///
		///     In this case the dictionary is treated as a collection of key-value pairs, and the elements of the
		///     collection are identified in the input data set by an incrementing integer index.
		/// </example>
		///
		/// Using this key format, the enumerator enumerates model objects of type matching
		/// <see cref="ModelMetadata.ElementMetadata"/>. The indices of the elements in the collection are used to
		/// compute the model prefix keys.
		/// </remarks>
		private class DefaultCollectionValidationStrategy : IValidationStrategy
		{
			private static readonly MethodInfo _getEnumerator = typeof(DefaultCollectionValidationStrategy)
					.GetMethod(nameof(GetEnumerator), BindingFlags.Static | BindingFlags.NonPublic);

			/// <summary>
			/// Gets an instance of <see cref="DefaultCollectionValidationStrategy"/>.
			/// </summary>
			public static readonly DefaultCollectionValidationStrategy Instance = new DefaultCollectionValidationStrategy();
			private readonly ConcurrentDictionary<Type, Func<object, IEnumerator>> _genericGetEnumeratorCache = new ConcurrentDictionary<Type, Func<object, IEnumerator>>();

			private DefaultCollectionValidationStrategy()
			{
			}

			/// <inheritdoc />
			public IEnumerator<ValidationEntry> GetChildren(
					ModelMetadata metadata,
					string key,
					object model)
			{
				var enumerator = GetEnumeratorForElementType(metadata, model);
				return new Enumerator(metadata.ElementMetadata, key, enumerator);
			}

			public IEnumerator GetEnumeratorForElementType(ModelMetadata metadata, object model)
			{
				Func<object, IEnumerator> getEnumerator = _genericGetEnumeratorCache.GetOrAdd(
						key: metadata.ElementType,
						valueFactory: (type) =>
						{
							var getEnumeratorMethod = _getEnumerator.MakeGenericMethod(type);
							var parameter = Expression.Parameter(typeof(object), "model");
							var expression =
											Expression.Lambda<Func<object, IEnumerator>>(
													Expression.Call(null, getEnumeratorMethod, parameter),
													parameter);
							return expression.Compile();
						});

				return getEnumerator(model);
			}

			// Called via reflection.
			private static IEnumerator GetEnumerator<T>(object model)
			{
				return (model as IEnumerable<T>)?.GetEnumerator() ?? ((IEnumerable)model).GetEnumerator();
			}

			private class Enumerator : IEnumerator<ValidationEntry>
			{
				private readonly string _key;
				private readonly ModelMetadata _metadata;
				private readonly IEnumerator _enumerator;

				private ValidationEntry _entry;
				private int _index;

				public Enumerator(
						ModelMetadata metadata,
						string key,
						IEnumerator enumerator)
				{
					_metadata = metadata;
					_key = key;
					_enumerator = enumerator;

					_index = -1;
				}

				public ValidationEntry Current => _entry;

				object IEnumerator.Current => Current;

				public bool MoveNext()
				{
					_index++;
					if (!_enumerator.MoveNext())
					{
						return false;
					}

					var key = ModelNames.CreateIndexModelName(_key, _index);
					var model = _enumerator.Current;

					_entry = new ValidationEntry(_metadata, key, model);

					return true;
				}

				public void Dispose()
				{
				}

				public void Reset()
				{
					_enumerator.Reset();
				}
			}
		}

		/// <summary>
		/// Taken from https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Validation/ExplicitIndexCollectionValidationStrategy.cs
		/// An implementation of <see cref="IValidationStrategy"/> for a collection bound using 'explicit indexing'
		/// style keys.
		/// </summary>
		/// <remarks>
		/// This implementation handles cases like:
		/// <example>
		///     Model: IList&lt;Student&gt;
		///     Query String: ?students.index=Joey,Katherine&amp;students[Joey].Age=8&amp;students[Katherine].Age=9
		///
		///     In this case, 'Joey' and 'Katherine' need to be used in the model prefix keys, but cannot be inferred
		///     form inspecting the collection. These prefixes are captured during model binding, and mapped to
		///     the corresponding ordinal index of a model object in the collection. The enumerator returned from this
		///     class will yield two 'Student' objects with corresponding keys 'students[Joey]' and 'students[Katherine]'.
		/// </example>
		///
		/// Using this key format, the enumerator enumerates model objects of type matching
		/// <see cref="ModelMetadata.ElementMetadata"/>. The keys captured during model binding are mapped to the elements
		/// in the collection to compute the model prefix keys.
		/// </remarks>
		private class ExplicitIndexCollectionValidationStrategy : IValidationStrategy
		{
			/// <summary>
			/// Creates a new <see cref="ExplicitIndexCollectionValidationStrategy"/>.
			/// </summary>
			/// <param name="elementKeys">The keys of collection elements that were used during model binding.</param>
			public ExplicitIndexCollectionValidationStrategy(IEnumerable<string> elementKeys)
			{
				if (elementKeys == null)
				{
					throw new ArgumentNullException(nameof(elementKeys));
				}

				ElementKeys = elementKeys;
			}

			/// <summary>
			/// Gets the keys of collection elements that were used during model binding.
			/// </summary>
			public IEnumerable<string> ElementKeys { get; }

			/// <inheritdoc />
			public IEnumerator<ValidationEntry> GetChildren(
					ModelMetadata metadata,
					string key,
					object model)
			{
				var enumerator = DefaultCollectionValidationStrategy.Instance.GetEnumeratorForElementType(metadata, model);
				return new Enumerator(metadata.ElementMetadata, key, ElementKeys, enumerator);
			}

			private class Enumerator : IEnumerator<ValidationEntry>
			{
				private readonly string _key;
				private readonly ModelMetadata _metadata;
				private readonly IEnumerator _enumerator;
				private readonly IEnumerator<string> _keyEnumerator;

				private ValidationEntry _entry;

				public Enumerator(
						ModelMetadata metadata,
						string key,
						IEnumerable<string> elementKeys,
						IEnumerator enumerator)
				{
					_metadata = metadata;
					_key = key;

					_keyEnumerator = elementKeys.GetEnumerator();
					_enumerator = enumerator;
				}

				public ValidationEntry Current => _entry;

				object IEnumerator.Current => Current;

				public bool MoveNext()
				{
					if (!_keyEnumerator.MoveNext())
					{
						return false;
					}

					if (!_enumerator.MoveNext())
					{
						return false;
					}

					var model = _enumerator.Current;
					var key = ModelNames.CreateIndexModelName(_key, _keyEnumerator.Current);

					_entry = new ValidationEntry(_metadata, key, model);

					return true;
				}

				public void Dispose()
				{
				}

				public void Reset()
				{
					throw new NotImplementedException();
				}
			}
		}

		#endregion

		#region Private fields

		private readonly int maxModelBindingCollectionSize = FormReader.DefaultValueCountLimit;

		private static readonly IValueProvider EmptyValueProvider = new CompositeValueProvider();

		#endregion

		#region Construction

		/// <inheritdoc/>
		public EntityCollectionModelBinder(
			IModelBinder keyBinder,
			ModelMetadata keyMetadata,
			IModelBinder elementBinder,
			ILoggerFactory loggerFactory)
		: base(elementBinder, loggerFactory)
		{
			if (keyBinder == null) throw new ArgumentNullException(nameof(keyBinder));
			if (keyMetadata == null) throw new ArgumentNullException(nameof(keyMetadata));

			this.KeyMetadata = keyMetadata;
			this.KeyBinder = keyBinder;
		}

		/// <inheritdoc/>
		public EntityCollectionModelBinder(
			IModelBinder keyBinder,
			ModelMetadata keyMetadata,
			IModelBinder elementBinder,
			ILoggerFactory loggerFactory,
			bool allowValidatingTopLevelNodes)
			: base(elementBinder, loggerFactory, allowValidatingTopLevelNodes)
		{
			if (keyBinder == null) throw new ArgumentNullException(nameof(keyBinder));
			if (keyMetadata == null) throw new ArgumentNullException(nameof(keyMetadata));

			this.KeyMetadata = keyMetadata;
			this.KeyBinder = keyBinder;

			this.AllowValidatingTopLevelNodes = allowValidatingTopLevelNodes;
		}

		/// <inheritdoc/>
		public EntityCollectionModelBinder(
			IModelBinder keyBinder,
			ModelMetadata keyMetadata,
			IModelBinder elementBinder,
			ILoggerFactory loggerFactory,
			bool allowValidatingTopLevelNodes,
			MvcOptions mvcOptions)
			: base(elementBinder, loggerFactory, allowValidatingTopLevelNodes, mvcOptions)
		{
			if (keyBinder == null) throw new ArgumentNullException(nameof(keyBinder));
			if (keyMetadata == null) throw new ArgumentNullException(nameof(keyMetadata));

			this.KeyMetadata = keyMetadata;
			this.KeyBinder = keyBinder;

			maxModelBindingCollectionSize = mvcOptions.MaxModelBindingCollectionSize;

			this.AllowValidatingTopLevelNodes = allowValidatingTopLevelNodes;
		}

		#endregion

		#region Protected properties

		/// <summary>
		/// The metadata for keys of type <typeparamref name="K"/>.
		/// </summary>
		protected ModelMetadata KeyMetadata { get; }
		
		/// <summary>
		/// <see cref="IModelBinder"/> for keys of type <typeparamref name="K"/>.
		/// </summary>
		protected IModelBinder KeyBinder { get; }

		/// <summary>
		/// Indication that validation of top-level models is enabled. If <see langword="true"/> and
		/// <see cref="ModelMetadata.IsBindingRequired"/> is <see langword="true"/> for a top-level model, the binder
		/// adds a <see cref="ModelStateDictionary"/> error when the model is not bound.
		/// </summary>
		protected bool AllowValidatingTopLevelNodes { get; } = true;

		#endregion

		#region Public methods

		/// <inheritdoc/>
		public override async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

			var existingCollection = bindingContext.Model as ICollection<E>;

			if (existingCollection == null)
			{
				await base.BindModelAsync(bindingContext);

				return;
			}

			if (bindingContext == null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			var model = bindingContext.Model;

			if (!bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
			{
				// If we failed to find data for a top-level model, then generate a
				// default 'empty' model (or use existing Model) and return it.
				if (bindingContext.IsTopLevelObject)
				{
					if (model == null)
					{
						model = CreateEmptyCollection(bindingContext.ModelType);
					}

					if (AllowValidatingTopLevelNodes)
					{
						AddErrorIfBindingRequired(bindingContext);
					}

					bindingContext.Result = ModelBindingResult.Success(model);
				}

				return;
			}

			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			CollectionResult result;
			if (valueProviderResult == ValueProviderResult.None)
			{
				result = await BindComplexCollection(bindingContext, existingCollection);
			}
			else
			{
				result = await BindSimpleCollection(bindingContext, valueProviderResult, existingCollection);
			}

			var boundCollection = result.Model;
			if (model == null)
			{
				model = ConvertToCollectionType(bindingContext.ModelType, boundCollection);
			}
			else
			{
				// Special case for TryUpdateModelAsync(collection, ...) scenarios. Model is null in all other cases.
				CopyToModel(model, boundCollection);
			}

			Debug.Assert(model != null);
			if (result.ValidationStrategy != null)
			{
				bindingContext.ValidationState.Add(model, new ValidationStateEntry()
				{
					Strategy = result.ValidationStrategy,
				});
			}

			if (valueProviderResult != ValueProviderResult.None)
			{
				// If we did simple binding, then modelstate should be updated to reflect what we bound for ModelName.
				// If we did complex binding, there will already be an entry for each index.
				bindingContext.ModelState.SetModelValue(
						bindingContext.ModelName,
						valueProviderResult);
			}

			bindingContext.Result = ModelBindingResult.Success(model);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Taken from https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Binders/CollectionModelBinder.cs
		/// </summary>
		private static IEnumerable<string> GetIndexNamesFromValueProviderResult(ValueProviderResult valueProviderResult)
		{
			var indexes = (string[])valueProviderResult;
			return (indexes == null || indexes.Length == 0) ? null : indexes;
		}

		/// <summary>
		/// Based on https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Binders/CollectionModelBinder.cs
		/// Used when the ValueProvider contains the collection to be bound as multiple elements, e.g. foo[0], foo[1].
		/// </summary>
		private Task<CollectionResult> BindComplexCollection(ModelBindingContext bindingContext, ICollection<E> existingCollection = null)
		{
			var indexPropertyName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, "index");

			// Remove any value provider that may not use indexPropertyName as-is. Don't match e.g. Model[index].
			var valueProvider = bindingContext.ValueProvider;
			if (valueProvider is IKeyRewriterValueProvider keyRewriterValueProvider)
			{
				valueProvider = keyRewriterValueProvider.Filter() ?? EmptyValueProvider;
			}

			var valueProviderResultIndex = valueProvider.GetValue(indexPropertyName);
			var indexNames = GetIndexNamesFromValueProviderResult(valueProviderResultIndex);

			return BindComplexCollectionFromIndexes(bindingContext, indexNames, existingCollection);
		}

		/// <summary>
		/// Based on https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Binders/CollectionModelBinder.cs
		/// Used when the ValueProvider contains the collection to be bound as a single element, e.g. the raw value
		/// is [ "1", "2" ] and needs to be converted to an int[].
		/// </summary>
		private async Task<CollectionResult> BindSimpleCollection(
			ModelBindingContext bindingContext,
			ValueProviderResult values,
			ICollection<E> existingCollection = null)
		{
			var boundCollection = new List<E>();

			var elementMetadata = bindingContext.ModelMetadata.ElementMetadata;

			var existingElementsByID = existingCollection?.ToDictionary(e => e.ID);

			foreach (var value in values)
			{
				bindingContext.ValueProvider = new CompositeValueProvider
				{
          // our temporary provider goes at the front of the list
          new ElementalValueProvider(bindingContext.ModelName, value, values.Culture),
					bindingContext.ValueProvider
				};

				E existingElement = default;
				bool foundExistingElement = false;

				if (existingCollection != null)
				{
					string idPropertyName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, nameof(IEntityWithID<object>.ID));

					using (bindingContext.EnterNestedScope(
						this.KeyMetadata,
						fieldName: nameof(IEntityWithID<object>.ID),
						modelName: idPropertyName,
						model: null))
					{
						await this.KeyBinder.BindModelAsync(bindingContext);

						K id = (K)bindingContext.Result.Model;

						foundExistingElement = existingElementsByID.TryGetValue(id, out existingElement);
					}
				}

				// Enter new scope to change ModelMetadata and isolate element binding operations.
				using (bindingContext.EnterNestedScope(
					elementMetadata,
					fieldName: bindingContext.FieldName,
					modelName: bindingContext.ModelName,
					model: foundExistingElement ? existingElement : null))
				{
					await ElementBinder.BindModelAsync(bindingContext);

					if (bindingContext.Result.IsModelSet)
					{
						var boundValue = bindingContext.Result.Model;
						boundCollection.Add(boundValue is E element ? element : new E());
					}
				}
			}

			return new CollectionResult
			{
				Model = boundCollection
			};
		}

		/// <summary>
		/// Based on https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Mvc/Mvc.Core/src/ModelBinding/Binders/CollectionModelBinder.cs
		/// </summary>
		private async Task<CollectionResult> BindComplexCollectionFromIndexes(
			ModelBindingContext bindingContext,
			IEnumerable<string> indexNames,
			ICollection<E> existingCollection = null)
		{
			bool indexNamesIsFinite;
			if (indexNames != null)
			{
				indexNamesIsFinite = true;
			}
			else
			{
				indexNamesIsFinite = false;
				var limit = maxModelBindingCollectionSize == int.MaxValue ?
						int.MaxValue :
						maxModelBindingCollectionSize + 1;
				indexNames = Enumerable
						.Range(0, limit)
						.Select(i => i.ToString(CultureInfo.InvariantCulture));
			}

			var elementMetadata = bindingContext.ModelMetadata.ElementMetadata;

			var boundCollection = new List<E>();

			var existingElementsByID = existingCollection?.ToDictionary(e => e.ID);

			foreach (var indexName in indexNames)
			{
				var fullChildName = ModelNames.CreateIndexModelName(bindingContext.ModelName, indexName);

				E existingElement = default;
				bool foundExistingElement = false;

				if (existingCollection != null)
				{
					string idPropertyName = ModelNames.CreatePropertyModelName(fullChildName, nameof(IEntityWithID<object>.ID));

					using (bindingContext.EnterNestedScope(
						this.KeyMetadata,
						fieldName: nameof(IEntityWithID<object>.ID),
						modelName: idPropertyName,
						model: null))
					{
						await this.KeyBinder.BindModelAsync(bindingContext);

						K id = (K)bindingContext.Result.Model;

						foundExistingElement = existingElementsByID.TryGetValue(id, out existingElement);
					}
				}

				ModelBindingResult? result;
				using (bindingContext.EnterNestedScope(
						elementMetadata,
						fieldName: indexName,
						modelName: fullChildName,
						model: foundExistingElement ? existingElement : null))
				{
					await this.ElementBinder.BindModelAsync(bindingContext);
					result = bindingContext.Result;
				}

				var didBind = false;
				object boundValue = null;
				if (result != null && result.Value.IsModelSet)
				{
					didBind = true;
					boundValue = result.Value.Model;
				}

				// infinite size collection stops on first bind failure
				if (!didBind && !indexNamesIsFinite)
				{
					break;
				}

				boundCollection.Add(boundValue is E element ? element : new E());
			}

			// Did the collection grow larger than the limit?
			if (boundCollection.Count > FormReader.DefaultValueCountLimit)
			{
				// Look for a non-empty name. Both ModelName and OriginalModelName may be empty at the top level.
				var name = string.IsNullOrEmpty(bindingContext.ModelName) ?
						(string.IsNullOrEmpty(bindingContext.OriginalModelName) &&
								bindingContext.ModelMetadata.MetadataKind != ModelMetadataKind.Type ?
								bindingContext.ModelMetadata.Name :
								bindingContext.OriginalModelName) : // This name may unfortunately be empty.
						bindingContext.ModelName;

				throw new InvalidOperationException(String.Format("Exceeded elements items count {3} set in {1}.{2} while binding collection {0} of elements of type {4}.",
						name,
						nameof(MvcOptions),
						nameof(MvcOptions.MaxModelBindingCollectionSize),
						maxModelBindingCollectionSize,
						bindingContext.ModelMetadata.ElementType));
			}

			return new CollectionResult
			{
				Model = boundCollection,

				// If we're working with a fixed set of indexes then this is the format like:
				//
				//  ?parameter.index=zero&parameter.index=one&parameter.index=two&parameter[zero]=0&parameter[one]=1&parameter[two]=2...
				//
				// We need to provide this data to the validation system so it can 'replay' the keys.
				// But we can't just set ValidationState here, because it needs the 'real' model.
				ValidationStrategy = indexNamesIsFinite ?
							new ExplicitIndexCollectionValidationStrategy(indexNames) :
							null,
			};
		}

		/// <summary>
		/// Attempts to amend the existing collection differentially instead of deleting all items and reinsert them.
		/// </summary>
		/// <param name="target">The object holding the target collection.</param>
		/// <param name="sourceCollection">The source collection of the bound items to copy from or null if nothing is bound.</param>
		protected override void CopyToModel(object target, IEnumerable<E> sourceCollection)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));

			var targetCollection = target as ICollection<E>;

			if (targetCollection == null || targetCollection.IsReadOnly)
			{
				base.CopyToModel(targetCollection, sourceCollection);

				return;
			}

			var sourceSet = new HashSet<E>(sourceCollection);
			var targetSet = new HashSet<E>(targetCollection);

			foreach (var item in targetSet)
			{
				if (!sourceSet.Contains(item)) targetCollection.Remove(item);
			}

			foreach (var item in sourceCollection)
			{
				if (!targetSet.Contains(item)) targetCollection.Add(item);
			}
		}

		#endregion
	}
}
