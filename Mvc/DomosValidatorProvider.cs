using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// If a model derives from <see cref="Models.ActionExecutionModel"/>, augments its validated properties
	/// using its <see cref="Models.ActionExecutionModel.GetParameterSpecifications()"/> method,
	/// otherwise it behaves like a <see cref="DataAnnotationsModelValidatorProvider"/>.
	/// </summary>
	public class DomosValidatorProvider : ModelValidatorProvider
	{
		#region Private fields

		private DataAnnotationsModelValidatorProvider dataAnnotationsModelValidatorProvider;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public DomosValidatorProvider()
		{
			dataAnnotationsModelValidatorProvider = new DataAnnotationsModelValidatorProvider();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// If a model derives from <see cref="Models.ActionExecutionModel"/>, augments its validated properties
		/// using its <see cref="Models.ActionExecutionModel.GetParameterSpecifications()"/> method,
		/// otherwise it behaves like a <see cref="DataAnnotationsModelValidatorProvider"/>.
		/// </summary>
		public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
		{
			if (metadata == null) throw new ArgumentNullException(nameof(metadata));

			if (metadata.Container is Models.ActionExecutionModel actionExecutionModel)
			{
				var actionExecutionModelValidatorProvider = new ActionExecutionModelValidationProvider(actionExecutionModel);

				return actionExecutionModelValidatorProvider.GetValidators(metadata, context);
			}
			else
			{
				return dataAnnotationsModelValidatorProvider.GetValidators(metadata, context);
			}
		}

		#endregion
	}
}
