using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
		/// Dictionary key for the <see cref="TypeRegistration"/> class.
		/// </summary>
		internal class TypeRegistrationKey : IEquatable<TypeRegistrationKey>
		{
			#region Public properties

			public Type ControllerType { get; internal set; }

			public Type ModelType { get; internal set; }

			#endregion

			#region IEquatable<TypeRegistrationKey> Members

			public bool Equals(TypeRegistrationKey other)
			{
				if (other == null) return false;

				return this.ControllerType == other.ControllerType &&
					this.ModelType == other.ModelType;
			}

			#endregion

			#region Public methods

			public override bool Equals(object obj)
			{
				return this.Equals(obj as TypeRegistrationKey);
			}

			public override int GetHashCode()
			{
				int code = this.ModelType.GetHashCode();

				if (this.ControllerType != null) code = 23 * code + this.ControllerType.GetHashCode();

				return code;
			}

			#endregion
		}

		/// <summary>
		/// A registration of a model type for custom validation.
		/// </summary>
		public class TypeRegistration
		{
			#region Private fields

			private Dictionary<string, IReadOnlyList<ValidationAttribute>> attributesByPropertyName;

			private TypeRegistrationKey key;

			#endregion

			#region Construction

			/// <summary>
			/// Create.
			/// </summary>
			/// <param name="modelType">The type of model to assign custom validation.</param>
			public TypeRegistration(Type modelType)
			{
				if (modelType == null) throw new ArgumentNullException(nameof(modelType));

				attributesByPropertyName = new Dictionary<string, IReadOnlyList<ValidationAttribute>>();

				key = new TypeRegistrationKey();
				key.ModelType = modelType;

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
			public Type ModelType
			{
				get
				{
					return key.ModelType;
				}
			}

			/// <summary>
			/// If not null, filters the controller name.
			/// </summary>
			public Type ControllerType
			{
				get
				{
					return key.ControllerType;
				}
			}

			#endregion

			#region Internal properties

			/// <summary>
			/// Used as a dictionary key for the class.
			/// </summary>
			internal TypeRegistrationKey Key
			{
				get
				{
					return key;
				}
			}

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
			/// Filters the controller name.
			/// </summary>
			public TypeRegistration FilterControllerType(Type controllerType)
			{
				if (controllerType == null) throw new ArgumentNullException(nameof(controllerType));

				key.ControllerType = controllerType;

				return this;
			}

			/// <summary>
			/// Assign validation to a property.
			/// </summary>
			/// <param name="propertyName">The name of the property.</param>
			/// <param name="attributes">the validation attributes to assign to the property.</param>
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

			#endregion

		}

		#endregion

		#region Private fields

		private readonly AttributesValidatorProvider defaultProvider;

		private Dictionary<TypeRegistrationKey, TypeRegistration> registrationsByTypeAndController;

		private Dictionary<TypeRegistrationKey, TypeRegistration> registrationsByType;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public CustomValidatorProvider()
		{
			defaultProvider = new AttributesValidatorProvider();

			registrationsByTypeAndController = new Dictionary<TypeRegistrationKey, TypeRegistration>();
			registrationsByType = new Dictionary<TypeRegistrationKey, TypeRegistration>();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Register a custom validation for a model.
		/// </summary>
		public CustomValidatorProvider Register(TypeRegistration typeRegistration)
		{
			if (typeRegistration == null) throw new ArgumentNullException(nameof(typeRegistration));

			if (typeRegistration.ControllerType != null)
			{
				registrationsByTypeAndController[typeRegistration.Key] = typeRegistration;
			}
			else
			{
				registrationsByType[typeRegistration.Key] = typeRegistration;
			}

			return this;
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

			TypeRegistration registration;

			var keyWithTypeAndController = new TypeRegistrationKey
			{
				ModelType = modelType,
				ControllerType = controllerType
			};

			if (registrationsByTypeAndController.TryGetValue(keyWithTypeAndController, out registration))
			{
				return GetValidators(metadata, context, registration);
			}

			var keyWithType = new TypeRegistrationKey
			{
				ModelType = modelType
			};

			if (registrationsByType.TryGetValue(keyWithType, out registration))
			{
				return GetValidators(metadata, context, registration);
			}

			return defaultProvider.GetValidators(metadata, context);
		}

		#endregion

		#region Private methods

		private IEnumerable<ModelValidator> GetValidators(
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
			else
			{
				if (registration.AllowsDefaultValidation)
				{
					return defaultProvider.GetValidators(metadata, context);
				}
			}

			return Enumerable.Empty<ModelValidator>();
		}

		#endregion
	}
}
