using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Logic;
using Grammophone.Domos.Web.Models;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Handles errors of AJAX calls and transforms the response into
	/// a <see cref="UserErrorModel"/> JSON respone if the exception wasa <see cref="UserException"/>.
	/// </summary>
	/// <remarks>
	/// The filter determines whether this is an AJAX request by examining if
	/// there is a "X-Requested-With" header having value "XMLHttpRequest".
	/// </remarks>
	public class HandleAjaxErrorAttribute : FilterAttribute, IExceptionFilter
	{
		/// <summary>
		/// If the request is an AJAX one and the exception is of type <see cref="UserException"/>,
		/// transform it into a <see cref="UserErrorModel"/> and return it as a JSON.
		/// </summary>
		/// <param name="filterContext"></param>
		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				if (filterContext.Exception is UserException userException)
				{
					filterContext.Result = new JsonResult
					{
						Data = new UserErrorModel(userException),
						ContentEncoding = Encoding.UTF8,
						JsonRequestBehavior = JsonRequestBehavior.AllowGet
					};

					filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

					filterContext.ExceptionHandled = true;
				}
			}
		}
	}
}
