using System;
using System.Net;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Grammophone.Domos.WebCore.ApplicationInsights
{
	/// <summary>
	/// Application Insights telemetry initializer that takes into account
	/// responses caused from <see cref="UserException"/>,
	/// <see cref="AccessDeniedException"/> and <see cref="IntegrityViolationException"/>
	/// descendants to serve meaningful responses.
	/// </summary>
	public class DomosRequestTelemetryInitializer : ITelemetryInitializer
	{
		private readonly IHttpContextAccessor httpContextAccessor;

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="httpContextAccessor">The accessor to the HTTP context.</param>
		public DomosRequestTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
		{
			if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));

			this.httpContextAccessor = httpContextAccessor;
		}

		/// <inheritdoc/>
		public void Initialize(ITelemetry telemetry)
		{
			if (telemetry is RequestTelemetry requestTelemetry)
			{
				var userErrorModelResponse = httpContextAccessor.HttpContext?.Items?[UserErrorModelResponse.Key] as UserErrorModelResponse;

				if (userErrorModelResponse != null)
				{
					string userErrorType = userErrorModelResponse.HttpStatusCode switch
					{
						HttpStatusCode.BadRequest => "Validation Error",
						HttpStatusCode.Forbidden => "Access Denied",
						HttpStatusCode.Conflict => "Integrity Violation",
						_ => "Logic Error"
					};

					requestTelemetry.Success = !userErrorModelResponse.IsSystemError;
					requestTelemetry.Properties["userErrorMessage"] = userErrorModelResponse.UserErrorModel.DisplayMessage;
					requestTelemetry.Properties["userErrorType"] = userErrorType;
					requestTelemetry.Properties["userErrorExceptionType"] = userErrorModelResponse.UserErrorModel.ExceptionName;
					requestTelemetry.Properties["isUserError"] = "true";
				}
				else if (requestTelemetry.ResponseCode == "400")
				{
					requestTelemetry.Success = true;
					requestTelemetry.Properties["userErrorMessage"] = "Validation Error";
					requestTelemetry.Properties["userErrorType"] = "Validation Error";
					requestTelemetry.Properties["isUserError"] = "true";
				}
				else
				{
					requestTelemetry.Properties["isUserError"] = "false";
				}
			}
		}
	}
}
