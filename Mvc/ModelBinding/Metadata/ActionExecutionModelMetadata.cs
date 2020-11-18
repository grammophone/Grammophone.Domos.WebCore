using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata
{
	/// <summary>
	/// Metadata for classes deriving from <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelMetadata : DefaultModelMetadata
	{
		#region Private fields

		private readonly ActionExecutionModel model;

		private readonly ModelPropertyCollection properties;

		private readonly IModelMetadataProvider modelMetadataProvider;

		private readonly ICompositeMetadataDetailsProvider detailsProvider;

		#endregion

		#region Construction

		internal ActionExecutionModelMetadata(
			IModelMetadataProvider modelMetadataProvider,
			ICompositeMetadataDetailsProvider detailsProvider,
			ActionExecutionModel model)
			: base(modelMetadataProvider, detailsProvider, CreateMetadataDetails(model))
		{
			this.model = model;
			this.modelMetadataProvider = modelMetadataProvider;
			this.detailsProvider = detailsProvider;

			this.properties = new ModelPropertyCollection(CreatePropertiesMetadata());
		}

		private IEnumerable<ModelMetadata> CreatePropertiesMetadata()
		{
			var parameterSpecifications = model.GetParameterSpecifications();

			var properties = new List<ModelMetadata>(parameterSpecifications.Count + base.Properties.Count());

			// Add all the original properties except "Parameters".
			properties.AddRange(base.Properties.Where(p => p.Name != nameof(ActionExecutionModel.Parameters)));

			foreach (var parameterSpecification in parameterSpecifications.Values)
			{
				var parameterMetadata = new ActionParameterMetadata(modelMetadataProvider, detailsProvider, parameterSpecification, model);

				properties.Add(parameterMetadata);
			}

			return properties;
		}

		private static DefaultMetadataDetails CreateMetadataDetails(ActionExecutionModel model)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));

			var modelMetadataIdentity = ModelMetadataIdentity.ForType(model.GetType());

			var modelAttributes = ModelAttributes.GetAttributesForType(model.GetType());

			var modelMetadataDetails = new DefaultMetadataDetails(modelMetadataIdentity, modelAttributes);

			return modelMetadataDetails;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Returns metadata for the strong-type properties in the model
		/// with added metadata for entries in <see cref="ActionExecutionModel.Parameters"/>.
		/// </summary>
		public override ModelPropertyCollection Properties => properties;

		#endregion
	}
}
