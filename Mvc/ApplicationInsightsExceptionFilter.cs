using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.DataAccess;
using Grammophone.Domos.Logic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// MVC Filter attribute to record system exceptions to Applications Insights,
	/// excluding exceptions targeted to the user.
	/// </summary>
	public class ApplicationInsightsExceptionFilter : Attribute, IExceptionFilter
	{
		/// <summary>
		/// Records the system exceptions and does nothing for the rest.
		/// </summary>
		public void OnException(ExceptionContext actionExecutedContext)
		{
			var exception = actionExecutedContext.Exception;

			switch (exception)
			{
				case ActionException actionException:
				//case AccessDeniedException accessDeniedException:
				case IntegrityViolationException integrityConstraintExceptinon:
				case UserException userException:
				//case InvalidOperationException invalidOperationException:
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
