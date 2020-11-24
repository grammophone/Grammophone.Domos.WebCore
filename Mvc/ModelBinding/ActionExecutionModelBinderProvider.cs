using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Provider for <see cref="ActionExecutionModelBinder"/>.
	/// </summary>
	public class ActionExecutionModelBinderProvider : IModelBinderProvider
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionModelBinderProvider()
		{
		}

		#endregion

		#region IModelBinderProvider implementation

		/// <summary>
		/// Returns an <see cref="ActionExecutionModelBinder"/>
		/// if the bound model derives from <see cref="ActionExecutionModel"/>, else returns null.
		/// </summary>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			if (!typeof(ActionExecutionModel).IsAssignableFrom(context.Metadata.ModelType))
			{
				return null;
			}

			return new ActionExecutionModelBinder(context);
		}

		#endregion
	}
}
