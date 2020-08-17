using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Grammophone.Domos.WebCore.Authorization
{
	/// <summary>
	/// Authorization handler for <see cref="PermissionAuthorizationRequirement"/>.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="D">The type of the domain container, deried from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of the logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	public class PermissionAuthorizationHandler<U, D, S> : AuthorizationHandler<PermissionAuthorizationRequirement>
		where U : User
		where D : IUsersDomainContainer<U>
		where S : LogicSession<U, D>
	{
		#region Private fields

		private readonly IServiceProvider serviceProvider;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

			this.serviceProvider = serviceProvider;
		}

		#endregion

		#region Protected methods

		/// <inheritdoc/>
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (requirement == null) throw new ArgumentNullException(nameof(requirement));

			var logicSession = serviceProvider.GetService<S>();

			// If a logic session is available...
			if (logicSession != null)
			{
				// If the user has the permission irrespective of segregation...
				if (logicSession.HasPermission(requirement.PermissionName))
				{
					// ...succeed authorization.
					context.Succeed(requirement);

					return Task.CompletedTask;
				}

				// Else, attempt to get the segregation ID from the route data.
				var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

				// If the HttpContext can be accessed...
				if (httpContextAccessor != null)
				{
					// ...and if a parser for a segregation ID from route data is defined...
					var segregationRouteParser = serviceProvider.GetService<ISegregationRouteParser>();

					if (segregationRouteParser != null)
					{
						var httpContext = httpContextAccessor.HttpContext;

						var routeData = httpContext.GetRouteData();

						// ...attempt to get the segregation ID.
						long? maybeSegregationID = segregationRouteParser.TryGetSegregationIdFromRouteData(routeData);

						// If a segregation ID was found in the route data...
						if (maybeSegregationID.HasValue)
						{
							// Test the permission against the segregation ID.
							if (logicSession.HasPermission(requirement.PermissionName, maybeSegregationID.Value))
							{
								context.Succeed(requirement);

								return Task.CompletedTask;
							}
						}
					}
				}

			}

			// If we reached here, components were absent and/or all permission checks were negative.
			context.Fail();

			return Task.CompletedTask;
		}

		#endregion
	}
}
