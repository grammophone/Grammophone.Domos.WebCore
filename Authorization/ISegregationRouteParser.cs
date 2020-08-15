using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Grammophone.Domos.WebCore.Authorization
{
	/// <summary>
	/// Interface for parsing route data to discover a segregation ID, if one exists.
	/// </summary>
	interface ISegregationRouteParser
	{
		/// <summary>
		/// Attempt to get the ID of the segregation in the route data, if a segregation ID is specified in it, else return null.
		/// </summary>
		/// <param name="routeData">The route data</param>
		long? TryGetSegregationIdFromRouteData(RouteData routeData);
	}
}
