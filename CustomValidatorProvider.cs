using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// A provider which allows registration of model types
	/// to be handled by <see cref="CustomValidator"/> and defering other types to 
	/// the default ASP.NET <see cref="DataAnnotationsModelValidatorProvider"/>.
	/// </summary>
	public class CustomValidatorProvider : ModelValidatorProvider
	{
		#region Auxillary classes

		/// <summary>
		/// Provider for default behavior.
		/// </summary>
		private class AttributesValidatorProvider : DataAnnotationsModelValidatorProvider
		{
			public IEnumerable<ModelValidator> GetValidatorsForAttributes(
				ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
			{
				return this.GetValidators(metadata, context, attributes);
			}
		}

		/// <summary>
		/// A registration of a model type for custom validation.
		/// </summary>
		public class TypeRegistration
		{
			#region Private fields

			private readonly Dictionary<string, IReadOnlyList<ValidationAttribute>> attributesByPropertyName;

			private HashSet<Type> allowedControllerTypes;

			#endregion

			#region Construction

			/// <summary>
			/// Create.
			/// </summary>
			/// <param name="modelType">The type of model to assign custom validation.</param>
			public TypeRegistration(Type modelType)
			{
				if (modelType == null) throw new ArgumentNullException(nameof(modelType));

				this.ModelType = modelType;

				attributesByPropertyName = new Dictionary<string, IReadOnlyList<ValidationAttribute>>();

				this.AllowsDefaultValidation = true;
			}

			#endregion

			#region Public properties

			/// <summary>
			/// If true, the default validation will be appended along with the custom one.
			/// The default value is true.
			/// </summary>
			public bool AllowsDefaultValidation { get; internal set; }

			/// <summary>
			/// Map of validation attributes by model property name.
			/// </summary>
			public IReadOnlyDictionary<string, IReadOnlyList<ValidationAttribute>> AttributesByPropertyName
			{
				get
				{
					return attributesByPropertyName;
				}
			}

			/// <summary>
			/// The type of the model being assigned the custom validator.
			/// </summary>
			public Type ModelType { get; private set; }

			#endregion

			#region Public methods

			/// <summary>
			/// Omit default validation defined in attributes and other standard APIs on the models
			/// and only allow the custom handling defined in this provider.
			/// </summary>
			public TypeRegistration OmitDefaultValidation()
			{
				this.AllowsDefaultValidation = false;

				return this;
			}

			/// <summary>
			/// Add an allowed controller type.
			/// </summary>
			/// <remarks>
			/// Inheritance is not checked. Add separate invokations when
			/// allowing for controller subtypes.
			/// </remarks>
			public TypeRegistration FilterByControllerType(Type controllerType)
			{
				if (controllerType == null) throw new ArgumentNullException(nameof(controllerType));

				if (allowedControllerTypes == null) allowedControllerTypes = new HashSet<Type>();

				allowedControllerTypes.Add(controllerType);

				return this;
			}

			/// <summary>
			/// Assign validation to a property.
			/// </summary>
			/// <param name="propertyName">The name of the property.</param>
			/// <param name="attributes">the validation attributes to assign to the property.</param>
			/// <returns>Returns the configured type registration.</returns>
			public TypeRegistration ValidateProperty(string propertyName, params ValidationAttribute[] attributes)
			{
				if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
				if (attributes == null) throw new ArgumentNullException(nameof(attributes));

				IReadOnlyList<ValidationAttribute> publicList;

				List<ValidationAttribute> list;

				if (!attributesByPropertyName.TryGetValue(propertyName, out publicList))
				{
					list = new List<ValidationAttribute>();
					attributesByPropertyName[propertyName] = list;
				}
				else
				{
					list = (List<ValidationAttribute>)publicList;
				}

				list.AddRange(attributes);

				return this;
			}

			/// <summary>
			/// Returns true when the validator provider applies to a controller.
			/// </summary>
			/// <param name="controllerType">The type of a controller.</param>
			public bool AllowsControllerType(Type controllerType)
			{
				if (controllerType == null) throw new ArgumentNullException(nameof(controllerType));

				if (allowedControllerTypes == null) return true; // Null marks all allowed.

				return allowedControllerTypes.Contains(controllerType);
			}

			#endregion
		}

		/// <summary>
		/// A registration of a model type for custom validation.
		/// </summary>
		/// <typeparam name="T">The type of the model.</typeparam>
		public class TypeRegistration<T> : TypeRegistration
		{
			#region Construction

			internal TypeRegistration() : base(typeof(T))
			{
			}

			#endregion

			#region Public properties

			/// <summary>
			/// Assign validation to a property.
			/// </summary>
			/// <typeparam name="P">The type of the property.</typeparam>
			/// <param name="propertySelector">The selector function for the property.</param>
			/// <param name="attributes">the validation attributes to assign to the property.</param>
			/// <returns>Returns the configured type registration.</returns>
			public TypeRegistration<T> ValidateProperty<P>(
				Expression<Func<T, P>> propertySelector,
				params ValidationAttribute[] attributes)
			{
				if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
				if (attributes == null) throw new ArgumentNullException(nameof(attributes));

				string propertyPath = ExpressionHelper.GetExpressionText(propertySelector);

				ValidateProperty(propertyPath, attributes);

				return this;
			}

			#endregion
		}

		#endregion

		#region Private fields

		private readonly AttributesValidatorProvider defaultProvider;

		private IDictionary<Type, IList<TypeRegistration>> registrationsByType;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public CustomValidatorProvider()
		{
			defaultProvider = new AttributesValidatorProvider();

			registrationsByType = new Dictionary<Type, IList<TypeRegistration>>();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Register a custom validation for a model.
		/// </summary>
		/// <returns>
		/// Returns the type registration of the model to be further configured.
		/// </returns>
		public TypeRegistration Register(Type type)
		{
			var typeRegistration = new TypeRegistration(type);

			Initialize(typeRegistration);

			return typeRegistration;
		}

		/// <summary>
		/// Register a custom validation for a model.
		/// </summary>
		/// <typeparam name="T">The type of the model.</typeparam>
		/// <returns>
		/// Returns the type registration of the model to be further configured.
		/// </returns>
		public TypeRegistration<T> Register<T>()
		{
			var typeRegistration = new TypeRegistration<T>();

			Initialize(typeRegistration);

			return typeRegistration;
		}

		/// <summary>
		/// If there is a custom validator for a model, return it, else defer to the default validator.
		/// </summary>
		public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
		{
			if (metadata == null) throw new ArgumentNullException(nameof(metadata));
			if (context == null) throw new ArgumentNullException(nameof(context));

			Type modelType = metadata.ContainerType;

			// We have registrations according to the container.
			// However, if we are validating the root object, search registrations for
			// it with mapped properties set to the empty string.
			if (modelType == null) modelType = metadata.ModelType;

			Type controllerType = context.Controller.GetType();

			IList<TypeRegistration> typeRegistrations;

			if (registrationsByType.TryGetValue(modelType, out typeRegistrations))
			{
				foreach (var typeRegistration in typeRegistrations)
				{
					var validators = TryGetValidators(metadata, context, typeRegistration);

					if (validators != null) return validators;
				}
			}

			return defaultProvider.GetValidators(metadata, context);
		}

		#endregion

		#region Private methods

		private IEnumerable<ModelValidator> TryGetValidators(
			ModelMetadata metadata,
			ControllerContext context,
			TypeRegistration registration)
		{
			IReadOnlyList<ValidationAttribute> attributes;

			if (metadata.PropertyName != null &&
				registration.AttributesByPropertyName.TryGetValue(metadata.PropertyName, out attributes))
			{
				var validators = from v in defaultProvider.GetValidatorsForAttributes(metadata, context, attributes)
												 where !(v is RequiredAttributeAdapter) // RequiredAttributeAdapter is added irrespectively of given attributes.
												 select v;

				if (registration.AllowsDefaultValidation)
				{
					var defaultValidators = defaultProvider.GetValidators(metadata, context);

					validators = validators.Concat(defaultValidators);
				}

				return validators;
			}

			return null;
		}

		private void Initialize(TypeRegistration typeRegistration)
		{
			IList<TypeRegistration> typeRegistrations;

			if (!registrationsByType.TryGetValue(typeRegistration.ModelType, out typeRegistrations))
			{
				typeRegistrations = new List<TypeRegistration>();

				registrationsByType[typeRegistration.ModelType] = typeRegistrations;
			}

			typeRegistrations.Add(typeRegistration);
		}

		#endregion
	}
}
