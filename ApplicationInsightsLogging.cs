using System;
using System.Collections.Generic;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Mvc;

namespace Grammophone.Domos.WebCore
{
	/// <summary>
	/// Common Application Insights logging logic.
	/// </summary>
	internal static class ApplicationInsightsLogging
	{
		/// <summary>
		/// Log an exctption depending on its severity and visibility to the user of the application.
		/// </summary>
		public static void LogException(Exception exception)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			switch (exception)
			{
				case ActionException actionException:
					var actionTelemetry = new Microsoft.ApplicationInsights.TelemetryClient();

					actionTelemetry.TrackTrace(actionException.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning,
						new Dictionary<string, string> { ["ExceptionTypeName"] = actionException.GetType().FullName, ["ExceptionStackTrace"] = actionException.StackTrace });

					break;

				//case AccessDeniedException accessDeniedException:
				case IntegrityViolationException integrityConstraintExceptinon:
					var integrityConstraintMessageTelemetry = new Microsoft.ApplicationInsights.TelemetryClient();

					integrityConstraintMessageTelemetry.TrackTrace(integrityConstraintExceptinon.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information,
						new Dictionary<string, string> { ["ExceptionTypeName"] = integrityConstraintExceptinon.GetType().FullName, ["ExceptionStackTrace"] = integrityConstraintExceptinon.StackTrace });

					break;

				case UserException userException:
					var userMessageTelemetry = new Microsoft.ApplicationInsights.TelemetryClient();

					userMessageTelemetry.TrackTrace(userException.Message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information,
						new Dictionary<string, string> { ["ExceptionTypeName"] = userException.GetType().FullName, ["ExceptionStackTrace"] = userException.StackTrace });

					//case InvalidOperationException invalidOperationException:
					// These exceptions are intended to communicate errors to the user,
					// thus do not record them as system errors.
					break;

				default:
					var exceptionTelemetry = new Microsoft.ApplicationInsights.TelemetryClient();

					exceptionTelemetry.TrackException(exception);

					break;
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
