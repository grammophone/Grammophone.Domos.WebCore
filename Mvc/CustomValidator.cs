using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// A model validator which overrides the attributes specified on the model
	/// with a custom list of attributes mapped by property.
	/// </summary>
	public class CustomValidator : ModelValidator
	{
		#region Auxilliary classes

		private class AttributesValidatorProvider : DataAnnotationsModelValidatorProvider
		{
			public IEnumerable<ModelValidator> GetValidatorsForAttributes(
				ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
			{
				return this.GetValidators(metadata, context, attributes);
			}
		}

		#endregion

		#region Private fields

		private static readonly AttributesValidatorProvider attributesValidatorProvider =
			new AttributesValidatorProvider();

		private readonly IReadOnlyDictionary<string, IReadOnlyList<ValidationAttribute>> validatorAttributesPerProperty;

		private readonly IReadOnlyDictionary<string, ModelMetadata> propertiesMetadataByName;

		private readonly Func<ModelMetadata, ControllerContext, IEnumerable<Attribute>, IEnumerable<ModelValidator>> validatorsResolver;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="metadata">The metadata of the model.</param>
		/// <param name="controllerContext">The context of the controller.</param>
		/// <param name="validatorAttributesPerProperty">
		/// A multi-dictionary containing the <see cref="ValidationAttribute"/>s
		/// assigned per property.
		/// </param>
		public CustomValidator(
			ModelMetadata metadata,
			ControllerContext controllerContext,
			IReadOnlyDictionary<string, IReadOnlyList<ValidationAttribute>> validatorAttributesPerProperty)
			: base(metadata, controllerContext)
		{
			if (validatorAttributesPerProperty == null) throw new ArgumentNullException(nameof(validatorAttributesPerProperty));

			this.validatorAttributesPerProperty = validatorAttributesPerProperty;

			propertiesMetadataByName = metadata.Properties.ToDictionary(pm => pm.PropertyName);

			validatorsResolver = attributesValidatorProvider.GetValidatorsForAttributes;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Performs server-side validation.
		/// </summary>
		/// <param name="container">The object containing the model or null if root.</param>
		/// <returns>Returns the validation results.</returns>
		public override IEnumerable<ModelValidationResult> Validate(object container)
		{
			bool allPropertiesValid = true;

			foreach (var entry in validatorAttributesPerProperty)
			{
				string propertyName = entry.Key;
				var attributes = entry.Value;

				ModelMetadata propertyMetadata;

				if (!propertiesMetadataByName.TryGetValue(propertyName, out propertyMetadata)) continue;

				var propertyValidators = validatorsResolver(propertyMetadata, this.ControllerContext, attributes);

				foreach (var propertyValidator in propertyValidators)
				{
					var propertyResults = propertyValidator.Validate(this.Metadata.Model);

					foreach (var propertyResult in propertyResults)
					{
						allPropertiesValid = false;

						string memberName = String.IsNullOrEmpty(propertyResult.MemberName) ?
							propertyMetadata.PropertyName :
							String.Format("{0}.{1}", propertyMetadata.PropertyName, propertyResult.MemberName);

						yield return new ModelValidationResult
						{
							MemberName = memberName,
							Message = propertyResult.Message
						};
					}
				}
			}

			if (allPropertiesValid && validatorAttributesPerProperty.ContainsKey(String.Empty))
			{
				var rootValidators = validatorsResolver(this.Metadata, this.ControllerContext, validatorAttributesPerProperty[String.Empty]);

				foreach (var rootValidator in rootValidators)
				{
					var rootResults = rootValidator.Validate(null);

					foreach (var rootResult in rootResults)
					{
						yield return new ModelValidationResult
						{
							MemberName = rootResult.MemberName,
							Message = rootResult.Message
						};
					}
				}
			}
		}

		/// <summary>
		/// Gethers client validation rules from all supplied attributes 
		/// which support the <see cref="IClientValidatable"/> interface.
		/// </summary>
		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			foreach (var entry in validatorAttributesPerProperty)
			{
				var propertyName = entry.Key;
				var attributes = entry.Value;

				ModelMetadata propertyMetadata;

				if (!propertiesMetadataByName.TryGetValue(propertyName, out propertyMetadata)) continue;

				var propertyValidators = validatorsResolver(propertyMetadata, this.ControllerContext, attributes);

				foreach (var propertyValidator in propertyValidators)
				{
					var clientValildator = propertyValidator as IClientValidatable;

					if (clientValildator != null)
					{
						var clientRules = clientValildator.GetClientValidationRules(propertyMetadata, this.ControllerContext);

						foreach (var clientRule in clientRules)
						{
							yield return clientRule;
						}
					}
				}
			}
		}

		#endregion
	}
}
