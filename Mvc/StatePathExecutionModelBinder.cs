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
	/// Binds a <see cref="StatePathExecutionModel"/>.
	/// </summary>
	public class StatePathExecutionModelBinder : DefaultModelBinder
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public StatePathExecutionModelBinder()
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

			if (bindingContext.Model is StatePathExecutionModel statePathExecutionModel)
			{
				var defaultParametersDescriptor = propertyDescriptors.Find(nameof(StatePathExecutionModel.Parameters), false);

				if (defaultParametersDescriptor != null) propertyDescriptors.Remove(defaultParametersDescriptor);

				string statePathCodeName = statePathExecutionModel.StatePathCodeName;

				// If the StatePathCodeName proeprty is not yet bound, search in the value provider.
				if (statePathCodeName == null)
				{
					string prefix = controllerContext.Controller.ViewData.TemplateInfo.HtmlFieldPrefix;

					string statePathFieldName =
						String.IsNullOrEmpty(prefix) ?
						nameof(StatePathExecutionModel.StatePathCodeName) :
						$"{prefix}.{nameof(StatePathExecutionModel.StatePathCodeName)}";

					var statePathCodeNameResult = bindingContext.ValueProvider.GetValue(statePathFieldName);

					statePathCodeName = statePathCodeNameResult?.AttemptedValue;
					
					if (statePathCodeName == null)
					{
						throw new ApplicationException("The state path code name is not specified in the model.");
					}
				}

				var parameterSpecificationsByKey = statePathExecutionModel.GetParameterSpecifications(statePathCodeName);

				foreach (var parameterSpecification in parameterSpecificationsByKey.Values)
				{
					propertyDescriptors.Add(new StatePathParameterDescriptor(parameterSpecification));
				}
			}

			return propertyDescriptors;
		}

		#endregion
	}
}
