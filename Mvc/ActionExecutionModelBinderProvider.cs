using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Binds an instance derived from <see cref="Models.ActionExecutionModel"/>
	/// using the <see cref="ActionExecutionModelBinder"/>.
	/// </summary>
	public class ActionExecutionModelBinderProvider : IModelBinderProvider
	{
		/// <summary>
		/// If the <paramref name="modelType"/> is derived from <see cref="Models.ActionExecutionModel"/>,
		/// return a <see cref="ActionExecutionModelBinder"/>, else return null.
		/// </summary>
		public IModelBinder GetBinder(Type modelType)
		{
			if (typeof(Models.ActionExecutionModel).IsAssignableFrom(modelType))
				return new ActionExecutionModelBinder();
			else
				return null;
		}
	}
}
