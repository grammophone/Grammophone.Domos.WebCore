using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grammophone.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Handles errors of AJAX calls and transforms the response into
	/// a <see cref="UserErrorModel"/> JSON respone if the exception was a <see cref="UserException"/>.
	/// or a descendant of <see cref="AccessDeniedException"/> or of <see cref="IntegrityViolationException"/>.
	/// </summary>
	/// <remarks>
	/// The filter determines whether this is an AJAX request by examining if
	/// there is a "X-Requested-With" header having value "XMLHttpRequest".
	/// </remarks>
	public class HandleAjaxErrorAttribute : Attribute, IExceptionFilter
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
			if (!IsInteractiveRequest(filterContext))
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
						case AccessDeniedException _:
						case AccessDeniedDomainException _:
							statusCode = HttpStatusCode.Forbidden;
							userMessage = ErrorMessages.ACCESS_DENIED;

							var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();

							telemetry.TrackException(exception);
							break;

						case UniqueConstraintViolationException _:
							statusCode = HttpStatusCode.Conflict;
							userMessage = ErrorMessages.UNIQUENESS_CONSTRAINT_VIOLATION;
							break;

						case ReferentialConstraintViolationException _:
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
					filterContext.Result = new JsonResult(userErrorModel);

					filterContext.HttpContext.Response.StatusCode = (int)statusCode;

					filterContext.ExceptionHandled = true;
				}
			}
		}

		private bool IsInteractiveRequest(ExceptionContext filterContext)
		{
			// If this is an AJAX call, this is not a browser navigation request.
			if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest") return false;

			// If the request was originated from a controller...
			if (filterContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
			{
				// If the controller was marked as [ApiController], this is not an interactive UI request.
				if (controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(ApiControllerAttribute), true).Length > 0) return false;
			}

			return true;
		}
	}
}
