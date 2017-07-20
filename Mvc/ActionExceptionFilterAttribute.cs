using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Catches <see cref="ActionException"/> and executes
	/// the <see cref="ActionException.TargetActionResult"/>.
	/// </summary>
	public class ActionExceptionFilterAttribute : FilterAttribute, IExceptionFilter
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
