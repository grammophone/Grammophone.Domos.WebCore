using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Metadata provider supporting standard entities with data annotations and
	/// extended parameters for entities derived from <see cref="Models.ActionExecutionModel"/>.
	/// </summary>
	public class DomosMetadataProvider : CachedDataAnnotationsModelMetadataProvider
	{
		/// <summary>
		/// If the model is a <see cref="Models.ActionExecutionModel"/>,
		/// return combined properties and parameters metadata, else fallback to default.
		/// </summary>
		protected override CachedDataAnnotationsModelMetadata CreateMetadataFromPrototype(
			CachedDataAnnotationsModelMetadata prototype, 
			Func<object> modelAccessor)
		{
			if (typeof(Models.ActionExecutionModel).IsAssignableFrom(prototype.ModelType))
			{
				return new ActionExecutionMetadata(this, prototype, modelAccessor);
			}
			else
			{
				return base.CreateMetadataFromPrototype(prototype, modelAccessor);
			}
		}
	}
}
