using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;

namespace Grammophone.Domos.Web.Http
{
	/// <summary>
	/// Web API Filter attribute to record system exceptions to Applications Insights,
	/// excluding exceptions targeted to the user.
	/// </summary>
	public class ApplicationInsightsApiExceptionFilter : ExceptionFilterAttribute
	{
		/// <summary>
		/// Records the system exceptions and does nothing for the rest.
		/// </summary>
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			var exception = actionExecutedContext.Exception;

			switch (exception)
			{
				case AccessDeniedException accessDeniedException:
				case IntegrityViolationException integrityConstraintExceptinon:
				case UserException userException:
				case InvalidOperationException invalidOperationException:
					// These exceptions are intended to communicate errors to the user,
					// thus do not record them as system errors.
					break;

				default:
					var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();

					telemetry.TrackException(exception);

					break;
			}
		}
	}
}
