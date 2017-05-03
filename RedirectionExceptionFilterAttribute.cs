using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Catches <see cref="RedirectionException"/> and redirects to
	/// the <see cref="RedirectionException.TargetURL"/>.
	/// </summary>
	public class RedirectionExceptionFilterAttribute : FilterAttribute, IExceptionFilter
	{
		/// <summary>
		/// Test whether the exception thrown is <see cref="RedirectionException"/>
		/// an handle it.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public void OnException(ExceptionContext filterContext)
		{
			var redirectionException = filterContext.Exception as RedirectionException;

			if (redirectionException != null)
			{
				filterContext.Result = new RedirectResult(redirectionException.TargetURL);

				filterContext.ExceptionHandled = true;
			}
		}
	}
}
