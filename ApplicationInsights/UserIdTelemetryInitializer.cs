using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Grammophone.Domos.WebCore.ApplicationInsights
{
	/// <summary>
	/// Telemetry initializer to set the 'userid' property of the Applicatoin Insight telemetry to the authenticated user.
	/// </summary>
	public class UserIdTelemetryInitializer : ITelemetryInitializer
	{
		private readonly IHttpContextAccessor httpContextAccessor;

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="httpContextAccessor">The accessor to the HTTP context.</param>
		public UserIdTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
		{
			this.httpContextAccessor = httpContextAccessor;
		}

		/// <inheritdoc/>
		public void Initialize(ITelemetry telemetry)
		{
			var httpContext = httpContextAccessor.HttpContext;
			
			if (httpContext == null) return;
			
			if (!httpContext.User.Identity?.IsAuthenticated ?? true) return;

			var userName = httpContext.User?.Identity?.Name;

			if (!string.IsNullOrWhiteSpace(userName))
			{
				// This is the readable ID that appears in Analytics as user_Id
				telemetry.Context.User.Id = userName;

				// Optional: also set AuthenticatedUserId if you want both
				telemetry.Context.User.AuthenticatedUserId = userName;
			}
		}
	}
}
