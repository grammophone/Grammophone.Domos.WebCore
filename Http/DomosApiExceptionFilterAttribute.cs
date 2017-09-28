using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;
using Grammophone.Domos.Web.Models;

namespace Grammophone.Domos.Web.Http
{
	/// <summary>
	/// Exception filter for Web API which exploits <see cref="UserException"/>,
	/// <see cref="AccessDeniedException"/> and <see cref="IntegrityViolationException"/>
	/// descendants to serve meaningful responses.
	/// </summary>
	public class DomosApiExceptionFilterAttribute : ExceptionFilterAttribute
	{
		/// <summary>
		/// Filters the exceptions and transforms response.
		/// </summary>
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			var exception = actionExecutedContext.Exception;

			if (exception is AccessDeniedException
				|| exception is IntegrityViolationException)
			{
				var statusCode = HttpStatusCode.InternalServerError;

				string userMessage = ErrorMessages.GENERIC_ERROR;

				if (exception is AccessDeniedException)
				{
					statusCode = HttpStatusCode.Forbidden;
					userMessage = ErrorMessages.ACCESS_DENIED;
				}
				else if (exception is UniqueConstraintViolationException)
				{
					statusCode = HttpStatusCode.Conflict;
					userMessage = ErrorMessages.UNIQUENESS_CONSTRAINT_VIOLATION;
				}
				else if (exception is ReferentialConstraintViolationException)
				{
					statusCode = HttpStatusCode.Conflict;
					userMessage = ErrorMessages.RELATIONAL_CONSTRAINT_VIOLATION;
				}

				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateResponse(
					statusCode,
					new UserErrorModel(userMessage));

				return;
			}

			if (exception is UserException userException)
			{
				actionExecutedContext.Response =
					actionExecutedContext.Request.CreateResponse(
					HttpStatusCode.InternalServerError,
					new UserErrorModel(userException));

				return;
			}
			else
			{
				base.OnException(actionExecutedContext);
			}
		}

	}
}
