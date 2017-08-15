using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Logic;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Metadata for <see cref="Models.ActionExecutionModel"/>.
	/// </summary>
	internal class ActionExecutionMetadata : CachedDataAnnotationsModelMetadata
	{
		#region Private fields

		private List<ModelMetadata> properties;

		#endregion

		#region Construction

		public ActionExecutionMetadata(DomosMetadataProvider provider,
			CachedDataAnnotationsModelMetadata prototype,
			Func<object> modelAccessor)
			: base(prototype, modelAccessor)
		{
			var model = (Models.ActionExecutionModel)modelAccessor();

			var parameterSpecifications = model.GetParameterSpecifications();

			properties = new List<ModelMetadata>(parameterSpecifications.Count + prototype.Properties.Count());

			// Add all the original properties except "Parameters".
			properties.AddRange(prototype.Properties.Where(p => p.PropertyName != nameof(Models.ActionExecutionModel.Parameters)));

			foreach (var parameterSpecification in parameterSpecifications.Values)
			{
				object parameterValue = GetParameterValue(model, parameterSpecification);

				var property = new CachedDataAnnotationsModelMetadata(
					provider, 
					typeof(Models.ActionExecutionModel),
					parameterSpecification.Type,
					parameterSpecification.Key,
					parameterSpecification.ValidationAttributes)
				{
					IsReadOnly = false,
					IsRequired = parameterSpecification.IsRequired,
					Container = model,
					Model = parameterValue,
					DisplayName = parameterSpecification.Caption,
					Description = parameterSpecification.Description,
				};

				properties.Add(property);

				this.AdditionalValues[parameterSpecification.Key] = parameterValue;
			}
		}

		public ActionExecutionMetadata(
			CachedDataAnnotationsModelMetadataProvider provider, 
			Type containerType, 
			Type modelType, 
			string propertyName, 
			IEnumerable<Attribute> attributes)
			: base(provider, containerType, modelType, propertyName, attributes)
		{
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The combined strong-type properties and action parameters.
		/// </summary>
		public override IEnumerable<ModelMetadata> Properties => properties;

		#endregion

		#region Private methods

		private object GetParameterValue(Models.ActionExecutionModel model, ParameterSpecification parameterSpecification)
		{
			if (model.Parameters.TryGetValue(parameterSpecification.Key, out object value))
			{
				return value;
			}
			else
			{
				return parameterSpecification.GetDefaultValue();
			}
		}

		#endregion
	}
}
