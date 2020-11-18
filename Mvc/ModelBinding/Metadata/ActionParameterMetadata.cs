using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata
{
	/// <summary>
	/// Metadata for a dynamic property based on a <see cref="ParameterSpecification"/>.
	/// </summary>
	public class ActionParameterMetadata : DefaultModelMetadata
	{
		#region Private fields

		private static readonly PropertyInfo nullPropertyInfo = typeof(ActionParameterMetadata).GetProperty(nameof(DisplayName));

		private readonly ParameterSpecification parameterSpecification;

		#endregion

		#region Construction

		internal ActionParameterMetadata(
			IModelMetadataProvider provider,
			ICompositeMetadataDetailsProvider detailsProvider,
			ParameterSpecification parameterSpecification,
			ActionExecutionModel containerModel)
			: base(provider, detailsProvider, CreateMetadataDetails(parameterSpecification, containerModel))
		{
			this.parameterSpecification = parameterSpecification;
		}

		private static DefaultMetadataDetails CreateMetadataDetails(ParameterSpecification parameterSpecification, ActionExecutionModel containerModel)
		{
			if (parameterSpecification == null) throw new ArgumentNullException(nameof(parameterSpecification));
			if (containerModel == null) throw new ArgumentNullException(nameof(containerModel));

#pragma warning disable CS0618 // Type or member is obsolete
			var parameterMetadataIdentity = ModelMetadataIdentity.ForProperty(parameterSpecification.Type, parameterSpecification.Key, containerModel.GetType());
#pragma warning restore CS0618 // Type or member is obsolete

			var parameterAttributes = ModelAttributes.GetAttributesForProperty(containerModel.GetType(), nullPropertyInfo, parameterSpecification.Type);

			var propertyMetadataDetails = new DefaultMetadataDetails(
				parameterMetadataIdentity,
				parameterAttributes)
			{
				PropertyGetter = thisModel => ((ActionExecutionModel)thisModel).Parameters[parameterSpecification.Key],
				PropertySetter = (thisModel, value) => ((ActionExecutionModel)thisModel).Parameters[parameterSpecification.Key] = value,
				ValidationMetadata = new ValidationMetadata
				{
					IsRequired = parameterSpecification.IsRequired
				},
				DisplayMetadata = new DisplayMetadata()
			};

			var dataTypeAttribute = parameterSpecification.ValidationAttributes.OfType<DataTypeAttribute>().FirstOrDefault();

			if (dataTypeAttribute != null)
			{
				propertyMetadataDetails.DisplayMetadata.DataTypeName = dataTypeAttribute.GetDataTypeName();
			}

			foreach (var attribute in parameterSpecification.ValidationAttributes)
			{
				propertyMetadataDetails.ValidationMetadata.ValidatorMetadata.Add(attribute);
			}

			return propertyMetadataDetails;
		}

		#endregion

		#region Public 

		/// <summary>
		/// Reflects the <see cref="ParameterSpecification.IsRequired"/> property of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override bool IsRequired => parameterSpecification.IsRequired;

		/// <summary>
		/// Always false.
		/// </summary>
		public override bool IsReadOnly => false;

		/// <summary>
		/// Reflects the <see cref="ParameterSpecification.Description"/> property of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override string Description => parameterSpecification.Description;

		/// <summary>
		/// Reflects the <see cref="ParameterSpecification.Caption"/> property of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override string DisplayName => parameterSpecification.Caption;

		#endregion
	}
}
