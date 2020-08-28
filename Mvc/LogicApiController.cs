using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Base class for API controllers served by a Domos logic session.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="D">The type of the domainContainer, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	/// <remarks>
	/// Lazy loading for the logic session is set to false by default for API scenarios, otherwise there is a
	/// danger to overserialize the whole database during a response.
	/// Eager-fetch what you need to include in a response.
	/// </remarks>
	[ApiController]
	public abstract class LogicApiController<U, D, S> : LogicController<U, D, S>
		where U : User
		where D : IUsersDomainContainer<U>
		where S : LogicSession<U, D>
	{
		/// <inheritdoc />
		protected LogicApiController(S logicSession) : base(logicSession)
		{
			// Lazy loading is set to false by default for API scenarios, otherwise there is a
			// danger to overserialize the whole database during a response.
			// Eager-fetch what you need to include in a response.
			logicSession.IsLazyLoadingEnabled = false;
		}
	}
}
