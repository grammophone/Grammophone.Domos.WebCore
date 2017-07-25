using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.DataAccess;
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
		/// If the request is an AJAX one and the exception is of type <see cref="UserException"/>
		/// or a descendant of <see cref="AccessDeniedException"/> or of <see cref="IntegrityViolationException"/>,
		/// transform it into a <see cref="UserErrorModel"/> and return it as a JSON
		/// with an appropriate error HTTP status code.
		/// </summary>
		/// <param name="filterContext">The context containing the exception.</param>
		/// <remarks>
		/// <see cref="AccessDeniedException"/> and descendants yeild HTTP status code 403 "Forbidden".
		/// <see cref="UniqueConstraintViolationException"/> and <see cref="ReferentialConstraintViolationException"/>
		/// yield HTTP Status 409 "Conflict".
		/// All other rexceptions result to 500 "Internal Server Error".
		/// </remarks>
		public void OnException(ExceptionContext filterContext)
		{
			if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				var exception = filterContext.Exception;

				UserErrorModel userErrorModel = null;

				var statusCode = HttpStatusCode.InternalServerError;

				if (exception is AccessDeniedException
					|| exception is IntegrityViolationException)
				{
					string userMessage;

					switch (exception)
					{
						case AccessDeniedException accessDeniedException:
							statusCode = HttpStatusCode.Forbidden;
							userMessage = ErrorMessages.ACCESS_DENIED;

							var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();

							telemetry.TrackException(exception);
							break;

						case UniqueConstraintViolationException uniqueConstraintViolationException:
							statusCode = HttpStatusCode.Conflict;
							userMessage = ErrorMessages.UNIQUENESS_CONSTRAINT_VIOLATION;
							break;

						case ReferentialConstraintViolationException referentialConstraintViolationException:
							statusCode = HttpStatusCode.Conflict;
							userMessage = ErrorMessages.RELATIONAL_CONSTRAINT_VIOLATION;
							break;

						default:
							statusCode = HttpStatusCode.InternalServerError;
							userMessage = ErrorMessages.GENERIC_ERROR;
							break;
					}

					userErrorModel = new UserErrorModel(userMessage);
				}
				else if (exception is UserException userException)
				{
					userErrorModel = new UserErrorModel(userException);
				}

				if (userErrorModel != null)
				{
					filterContext.Result = new JsonResult
					{
						Data = userErrorModel,
						ContentEncoding = Encoding.UTF8,
						JsonRequestBehavior = JsonRequestBehavior.AllowGet
					};

					filterContext.HttpContext.Response.StatusCode = (int)statusCode;

					filterContext.ExceptionHandled = true;
				}
			}
		}
	}
}
