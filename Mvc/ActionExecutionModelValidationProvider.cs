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
	/// Augments the strong-typed properties with the contents of
	/// parameters defined by method <see cref="Models.ActionExecutionModel.GetParameterSpecifications()"/>
	/// of an instance derived from <see cref="Models.ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelValidationProvider : DataAnnotationsModelValidatorProvider
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="container">
		/// The container of the <see cref="Models.ActionExecutionModel.Parameters"/>
		/// used to augment the type descriptor.
		/// </param>
		public ActionExecutionModelValidationProvider(Models.ActionExecutionModel container)
		{
			this.Container = container;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The container of the <see cref="Models.ActionExecutionModel.Parameters"/>
		/// used to augment the type descriptor.
		/// </summary>
		public Models.ActionExecutionModel Container { get; }

		#endregion

		#region Protected methods

		/// <summary>
		/// Augments the strong-type properties with
		/// the ones defined in the <see cref="Models.ActionExecutionModel.Parameters"/>
		/// of the <see cref="Container"/>.
		/// </summary>
		/// <param name="type">The type holding the strong-type properties.</param>
		/// <returns>Returns the augmented type descriptor.</returns>
		protected override ICustomTypeDescriptor GetTypeDescriptor(Type type)
		{
			var baseΤypeDescriptor = base.GetTypeDescriptor(type);

			return new ActionExecutionTypeDescriptor(baseΤypeDescriptor, this.Container);
		}

		#endregion
	}
}
