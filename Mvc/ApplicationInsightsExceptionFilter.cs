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

			ApplicationInsightsLogging.LogException(exception);
		}
	}
}
