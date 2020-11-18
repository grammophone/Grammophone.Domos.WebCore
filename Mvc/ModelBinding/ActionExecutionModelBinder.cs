using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Binder for <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelBinder : IModelBinder
	{
		#region Private fields

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionModelBinder()
		{
		}

		#endregion

		#region IModelBinder implementation

		/// <inheritdoc/>
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

			if (!typeof(ActionExecutionModel).IsAssignableFrom(bindingContext.ModelType)) return Task.CompletedTask;

			return Task.CompletedTask;
		}

		#endregion
	}
}
