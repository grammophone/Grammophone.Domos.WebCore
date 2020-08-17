using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Grammophone.Domos.WebCore.Authorization
{
	/// <summary>
	/// Implementation of <see cref="ISegregationRouteParser"/> via a function.
	/// </summary>
	public class SegregationRouteParser : ISegregationRouteParser
	{
		private readonly Func<RouteData, long?> segregationIdExtractor;

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="segregationIdExtractor">The function to extract the segregation ID from route data.</param>
		public SegregationRouteParser(Func<RouteData, long?> segregationIdExtractor)
		{
			if (segregationIdExtractor == null) throw new ArgumentNullException(nameof(segregationIdExtractor));

			this.segregationIdExtractor = segregationIdExtractor;
		}

		/// <inheritdoc />
		public long? TryGetSegregationIdFromRouteData(RouteData routeData) => segregationIdExtractor(routeData);
	}
}
