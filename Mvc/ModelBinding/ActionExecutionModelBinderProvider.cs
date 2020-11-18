using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Model binder provider for <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelBinderProvider : IModelBinderProvider
	{
		#region Private field

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionModelBinderProvider()
		{
		}

		#endregion

		#region Public methods

		/// <summary>
		/// If the model in metadata is <see cref="ActionExecutionModel"/>,
		/// create and return a <see cref="ActionExecutionModelBinder"/>.
		/// </summary>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			if (typeof(ActionExecutionModel).IsAssignableFrom(context.Metadata.ModelType))
			{
				return new ActionExecutionModelBinder();
			}

			return null;
		}

		#endregion
	}
}
