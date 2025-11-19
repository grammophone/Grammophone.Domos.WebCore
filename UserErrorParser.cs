using System;
using System.Net;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;

namespace Grammophone.Domos.WebCore
{
	/// <summary>
	/// Parser that produces <see cref="UserErrorModelResponse"/>.
	/// </summary>
	internal static class UserErrorParser
	{
		/// <summary>
		/// Attempt to return a <see cref="UserErrorModelResponse"/> for an <paramref name="exception"/>, is it pertains to a user-displayable error, else return null.
		/// </summary>
		public static UserErrorModelResponse TryParseException(Exception exception)
		{
			if (exception is AccessDeniedException
				|| exception is IntegrityViolationException)
			{
				var statusCode = HttpStatusCode.InternalServerError;

				string userMessage = ErrorMessages.GENERIC_ERROR;

				bool isSystemError = true;

				if (exception is AccessDeniedException)
				{
					statusCode = HttpStatusCode.Forbidden;
					userMessage = ErrorMessages.ACCESS_DENIED;
				}
				else if (exception is UniqueConstraintViolationException)
				{
					statusCode = HttpStatusCode.Conflict;
					userMessage = ErrorMessages.UNIQUENESS_CONSTRAINT_VIOLATION;
					isSystemError = false;
				}
				else if (exception is ReferentialConstraintViolationException)
				{
					statusCode = HttpStatusCode.Conflict;
					userMessage = ErrorMessages.RELATIONAL_CONSTRAINT_VIOLATION;
					isSystemError = false;
				}

				return new UserErrorModelResponse
				{
					HttpStatusCode = statusCode,
					UserErrorModel = new UserErrorModel(userMessage, exception.GetType().FullName),
					IsSystemError = isSystemError
				};
			}

			if (exception is UserException userException)
			{
				return new UserErrorModelResponse
				{
					HttpStatusCode = HttpStatusCode.InternalServerError,
					IsSystemError = false,
					UserErrorModel = new UserErrorModel(userException)
				};
			}

			return null;
		}
	}
}
