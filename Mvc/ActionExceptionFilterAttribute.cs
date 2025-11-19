using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Catches <see cref="ActionException"/> and executes
	/// the <see cref="ActionException.TargetActionResult"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ActionExceptionFilterAttribute : Attribute, IExceptionFilter
	{
		/// <summary>
		/// Test whether the exception thrown is <see cref="ActionException"/>
		/// an handle it.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.Exception is ActionException actionException)
			{
				filterContext.Result = actionException.TargetActionResult;

				filterContext.ExceptionHandled = true;
			}
		}
	}
}
