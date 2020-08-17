using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Grammophone.Domos.WebCore.Authorization
{
	/// <summary>
	/// Helpers for adding ASP.NET Core authorization based on the Domos security model.
	/// </summary>
	public static class AuthorizationExtensions
	{
		/// <summary>
		/// Add an authorization policy requiring that the current user has a given Domos permission.
		/// </summary>
		/// <param name="authorizationOptions">The authorization options to configure.</param>
		/// <param name="permissionName">The name of the Domos permission.</param>
		public static void AddPermissionPolicy(this AuthorizationOptions authorizationOptions, string permissionName)
		{
			if (authorizationOptions == null) throw new ArgumentNullException(nameof(authorizationOptions));
			if (permissionName == null) throw new ArgumentNullException(nameof(permissionName));

			authorizationOptions.AddPolicy(permissionName, builder =>
			{
				builder.Requirements.Add(new PermissionAuthorizationRequirement(permissionName));
			});
		}

		/// <summary>
		/// Register an authorization handler for Domos permissions.
		/// </summary>
		/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
		/// <typeparam name="D">The type of the domain container, deried from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
		/// <typeparam name="S">The type of the logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
		/// <param name="services">The services container.</param>
		/// <returns>Returns the services container.</returns>
		public static IServiceCollection AddPermissionAuthorizationHandler<U, D, S>(this IServiceCollection services)
			where U : User
			where D : IUsersDomainContainer<U>
			where S : LogicSession<U, D>
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			return services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler<U, D, S>>();
		}

		/// <summary>
		/// Add a parser for segregation ID from <see cref="RouteData"/>.
		/// </summary>
		/// <param name="services">The services container.</param>
		/// <param name="segregationIdExtractor">The function to extract the segregation ID from route data.</param>
		/// <returns>Returns the services container.</returns>
		public static IServiceCollection AddSegregationRouteParser(this IServiceCollection services, Func<RouteData, long?> segregationIdExtractor)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (segregationIdExtractor == null) throw new ArgumentNullException(nameof(segregationIdExtractor));

			var parser = new SegregationRouteParser(segregationIdExtractor);

			return services.AddSingleton<ISegregationRouteParser>(parser);
		}
	}
}
