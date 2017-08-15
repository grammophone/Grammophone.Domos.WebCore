using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Web.Models;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Binds a <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelBinder : DefaultModelBinder
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionModelBinder()
		{
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// If the model is a state path execution, augment the default property descriptors
		/// with the parameters configured for the state path.
		/// </summary>
		protected override PropertyDescriptorCollection GetModelProperties(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var propertyDescriptors = base.GetModelProperties(controllerContext, bindingContext);

			if (bindingContext.Model is ActionExecutionModel statePathExecutionModel)
			{
				string actionCodeName = statePathExecutionModel.ActionCodeName;

				// If the ActionCodeName property is not yet bound, search in the value provider.
				if (actionCodeName == null)
				{
					string prefix = controllerContext.Controller.ViewData?.TemplateInfo?.HtmlFieldPrefix;

					string actionCodeFieldName =
						String.IsNullOrEmpty(prefix) ?
						nameof(ActionExecutionModel.ActionCodeName) :
						$"{prefix}.{nameof(ActionExecutionModel.ActionCodeName)}";

					var actionCodeNameResult = bindingContext.ValueProvider.GetValue(actionCodeFieldName);

					actionCodeName = actionCodeNameResult?.AttemptedValue;

					if (actionCodeName == null)
					{
						throw new ApplicationException("The action code name is not specified in the model.");
					}
				}

				var parameterSpecificationsByKey = statePathExecutionModel.GetParameterSpecifications(actionCodeName);

				return ActionExecutionTypeDescriptor.AugmentPropertiesWithParameters(propertyDescriptors, parameterSpecificationsByKey);
			}
			else
			{
				return propertyDescriptors;
			}
		}

		#endregion
	}
}
