using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Binder for entities derived from <see cref="IEntityWithID{K}"/>.
	/// Hides the <see cref="IUserTrackingEntity{U}"/> and <see cref="IUserGroupTrackingEntity{U}"/>
	/// properties from binding.
	/// </summary>
	/// <typeparam name="K">The type of the key of the entity.</typeparam>
	public class KeyedEntityModelBinder<K> : DefaultModelBinder
	{
		#region Private fields

		private static readonly MethodInfo UpdateEntitiesCollectionMethod;

		private static readonly ConcurrentDictionary<Type, Type> EntityCollectionInterfacesByType;

		#endregion

		#region Construction

		/// <summary>
		/// Static initialization.
		/// </summary>
		static KeyedEntityModelBinder()
		{
			UpdateEntitiesCollectionMethod =
				typeof(KeyedEntityModelBinder<K>)
				.GetMethod("UpdateEntitiesCollectionImpl", BindingFlags.Static | BindingFlags.NonPublic);

			EntityCollectionInterfacesByType = new ConcurrentDictionary<Type, Type>();
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Touch graph edges' 'LastModificationDate' property to ensure full security.
		/// </summary>
		protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			base.OnModelUpdated(controllerContext, bindingContext);

			var trackingEntity = bindingContext.Model as ITrackingEntity;

			if (trackingEntity != null)
			{
				trackingEntity.LastModificationDate = DateTime.UtcNow;
			}
		}

		/// <summary>
		/// If the property implements <see cref="IEntityWithID{K}"/>, and if the original ID
		/// is not equal to the ID, create and assign a new entity instead of populating the existing property,
		/// else fall back to the base implementation.
		/// If the property is a collection of elements implementing <see cref="IEntityWithID{K}"/>,
		/// update the collection in place based on the ID keys of the elements instead of
		/// replacing all elements.
		/// </summary>
		protected override object GetPropertyValue(
			ControllerContext controllerContext,
			ModelBindingContext bindingContext,
			PropertyDescriptor propertyDescriptor,
			IModelBinder propertyBinder)
		{
			var originalValue = bindingContext.ModelMetadata.Model;

			if (originalValue != null)
			{
				// Determine if the value is an entity.
				var originalEntityValue = originalValue as IEntityWithID<K>;

				if (originalEntityValue != null)
				{
					K originalID = originalEntityValue.ID;

					string idPropertyName = CreateSubPropertyName(bindingContext.ModelName, "ID");

					var newIDResult = bindingContext.ValueProvider.GetValue(idPropertyName);

					if (newIDResult != null)
					{
						object newIDValue = newIDResult.ConvertTo(typeof(K));

						if (newIDValue is K)
						{
							K newID = (K)newIDValue;

							if (!AreKeysEqual(originalID, newID))
							{
								// Force creating a new instance instead of populating the existing.
								bindingContext.ModelMetadata.Model = null;
							}

							return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
						}
					}
				}

				var modelType = bindingContext.ModelMetadata.ModelType;

				var collectionType = GetEntityCollectionInterfaceType(modelType);

				// Try to determine whether the model is an ICollection<Entity>.
				if (collectionType != null)
				{
					var elementType = collectionType.GetGenericArguments()[0];

					var collectionBindingContext = new ModelBindingContext()
					{
						ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => originalValue, modelType),
						ModelName = bindingContext.ModelName,
						ModelState = bindingContext.ModelState,
						PropertyFilter = bindingContext.PropertyFilter,
						ValueProvider = bindingContext.ValueProvider
					};

					return UpdateEntitiesCollection(controllerContext, collectionBindingContext, elementType);
				}
			}

			return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
		}

		/// <summary>
		/// Overrides base in order to
		/// prevent security-sensitive properties from binding.
		/// </summary>
		protected override PropertyDescriptorCollection GetModelProperties(
			ControllerContext controllerContext,
			ModelBindingContext bindingContext)
		{
			var properties = base.GetModelProperties(controllerContext, bindingContext);

			var filteredProperties = new List<PropertyDescriptor>(properties.Count);

			// Remove security sensitive properties to inhibit them from binding.
			for (int i = 0; i < properties.Count; i++)
			{
				var property = properties[i];

				switch (property.Name)
				{
					case "CreationDate":
					case "CreatorUser":
					case "CreatorUserID":
					case "LastModifierUserID":
					case "LastModifierUser":
					case "OwningUsers":
					case "OwningUserID":
					case "OwningUser":
						continue;

					default:
						filteredProperties.Add(property);
						break;
				}
			}

			return new PropertyDescriptorCollection(filteredProperties.ToArray(), true);
		}

		/// <summary>
		/// Test whether two key values are equal.
		/// </summary>
		/// <param name="key1">The first key value.</param>
		/// <param name="key2">The second key value.</param>
		/// <remarks>
		/// The default implementation uses <see cref="Object.Equals(object, object)"/>
		/// </remarks>
		protected virtual bool AreKeysEqual(K key1, K key2)
		{
			return Object.Equals(key1, key2);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Gets any ICollection{E} implemented interfaces of a type,
		/// where E is a descendant of <see cref="IEntityWithID{K}"/>, or returns null
		/// if there is no such interface in the hierarchy.
		/// </summary>
		/// <remarks>
		/// Uses <see cref="EntityCollectionInterfacesByType"/> as a cache,
		/// invoking <see cref="DiscoverCollectionInterfaceType"/> upon cache miss.
		/// </remarks>
		private static Type GetEntityCollectionInterfaceType(Type modelType)
		{
			if (modelType == null) throw new ArgumentNullException(nameof(modelType));

			return EntityCollectionInterfacesByType.GetOrAdd(modelType, DiscoverCollectionInterfaceType);
		}

		private static Type DiscoverCollectionInterfaceType(Type type)
		{
			if (IsEntityCollectionInterface(type)) return type;

			var collectionTypes = type.FindInterfaces(
				(t, _) => IsEntityCollectionInterface(t),
				null);

			return collectionTypes.FirstOrDefault();
		}

		private static bool IsEntityCollectionInterface(Type interfaceType)
		{
			if (interfaceType.IsGenericType)
			{
				if (typeof(ICollection<>) == interfaceType.GetGenericTypeDefinition())
				{
					if (typeof(IEntityWithID<K>).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Adds the entities in <paramref name="changedCollection"/>
		/// whose IDs are not present in the <paramref name="originalCollection"/>.
		/// </summary>
		private static void UpdateEntitiesCollectionImpl<E>(
			ICollection<E> originalCollection,
			System.Collections.IEnumerable changedCollection)
			where E : IEntityWithID<K>
		{
			if (originalCollection == null) throw new ArgumentNullException(nameof(originalCollection));

			if (changedCollection == null) return;

			var originalIDs = new HashSet<K>();

			foreach (E element in originalCollection)
			{
				originalIDs.Add(element.ID);
			}

			foreach (E element in changedCollection)
			{
				if (!originalIDs.Contains(element.ID)) originalCollection.Add(element);
			}
		}

		/// <summary>
		/// Adds the entities in <paramref name="changedCollection"/>
		/// whose IDs are not present in the <paramref name="originalCollection"/>.
		/// </summary>
		private static void UpdateEntitiesCollection(Type entityType, object originalCollection, object changedCollection)
		{
			if (entityType == null) throw new ArgumentNullException(nameof(entityType));

			var updateMethod = UpdateEntitiesCollectionMethod.MakeGenericMethod(entityType);

			updateMethod.Invoke(null, new object[] { originalCollection, changedCollection });
		}

		#region Code originating from base source code

		internal object UpdateEntitiesCollection(ControllerContext controllerContext, ModelBindingContext bindingContext, Type elementType)
		{
			var originalCollection = (IEnumerable<IEntityWithID<K>>)bindingContext.Model;

			var originalEntitiesByID = originalCollection.ToDictionary(e => e.ID);

			bool stopOnIndexNotFound;
			IEnumerable<string> indexes;

			GetIndexes(bindingContext, out stopOnIndexNotFound, out indexes);

			IModelBinder elementBinder = Binders.GetBinder(elementType);

			// build up a list of items from the request
			List<object> modelList = new List<object>();
			foreach (string currentIndex in indexes)
			{
				string subIndexKey = CreateSubIndexName(bindingContext.ModelName, currentIndex);
				if (!bindingContext.ValueProvider.ContainsPrefix(subIndexKey))
				{
					if (stopOnIndexNotFound)
					{
						// we ran out of elements to pull
						break;
					}
					else
					{
						continue;
					}
				}

				K id = default(K);

				bool isIdSet = false;

				var idValue = bindingContext.ValueProvider.GetValue(subIndexKey + ".ID");

				if (idValue != null)
				{
					object idObject = idValue.ConvertTo(typeof(K));

					if (idObject is K)
					{
						id = (K)idObject;
						isIdSet = true;
					}
				}

				ModelBindingContext innerContext = new ModelBindingContext()
				{
					ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() =>
					{
						IEntityWithID<K> existingEntity;

						if (isIdSet && originalEntitiesByID.TryGetValue(id, out existingEntity))
							return existingEntity;
						else
							return null;
					},
					elementType),
					ModelName = subIndexKey,
					ModelState = bindingContext.ModelState,
					PropertyFilter = bindingContext.PropertyFilter,
					ValueProvider = bindingContext.ValueProvider
				};
				object thisElement = elementBinder.BindModel(controllerContext, innerContext);

				// we need to merge model errors up
				AddValueRequiredMessageToModelState(controllerContext, bindingContext.ModelState, subIndexKey, elementType, thisElement);
				modelList.Add(thisElement);
			}

			// if there weren't any elements at all in the request, just return
			if (modelList.Count == 0)
			{
				return originalCollection;
			}

			// Update the original collection
			UpdateEntitiesCollection(elementType, originalCollection, modelList);

			return originalCollection;
		}

		private static void GetIndexes(ModelBindingContext bindingContext, out bool stopOnIndexNotFound, out IEnumerable<string> indexes)
		{
			string indexKey = CreateSubPropertyName(bindingContext.ModelName, "index");
			ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(indexKey);

			if (valueProviderResult != null)
			{
				string[] indexesArray = valueProviderResult.ConvertTo(typeof(string[])) as string[];
				if (indexesArray != null)
				{
					stopOnIndexNotFound = false;
					indexes = indexesArray;
					return;
				}
			}

			// just use a simple zero-based system
			stopOnIndexNotFound = true;
			indexes = GetZeroBasedIndexes();
		}

		private static IEnumerable<string> GetZeroBasedIndexes()
		{
			int i = 0;
			while (true)
			{
				yield return i.ToString(CultureInfo.InvariantCulture);
				i++;
			}
		}

		// If the user specified a ResourceClassKey try to load the resource they specified.
		// If the class key is invalid, an exception will be thrown.
		// If the class key is valid but the resource is not found, it returns null, in which
		// case it will fall back to the MVC default error message.
		private static string GetUserResourceString(ControllerContext controllerContext, string resourceName)
		{
			string result = null;

			if (!String.IsNullOrEmpty(ResourceClassKey) && (controllerContext != null) && (controllerContext.HttpContext != null))
			{
				result = controllerContext.HttpContext.GetGlobalResourceObject(ResourceClassKey, resourceName, CultureInfo.CurrentUICulture) as string;
			}

			return result;
		}

		private static string GetValueInvalidResource(ControllerContext controllerContext)
		{
			return GetUserResourceString(controllerContext, "PropertyValueInvalid") ?? "Invalid value";
		}

		private static string GetValueRequiredResource(ControllerContext controllerContext)
		{
			return GetUserResourceString(controllerContext, "PropertyValueRequired") ?? "Value is required";
		}

		private static void AddValueRequiredMessageToModelState(ControllerContext controllerContext, ModelStateDictionary modelState, string modelStateKey, Type elementType, object value)
		{
			if (value == null && !TypeAllowsNullValue(elementType) && modelState.IsValidField(modelStateKey))
			{
				modelState.AddModelError(modelStateKey, GetValueRequiredResource(controllerContext));
			}
		}

		private static bool TypeAllowsNullValue(Type type)
		{
			if (!type.IsValueType) return true;

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return true;
			}

			return false;
		}

		#endregion

		#endregion
	}
}
