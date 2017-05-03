using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;

namespace Grammophone.Domos.Web.Http
{
	/// <summary>
	/// Base for API controllers associated with a Domos logic session.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="D">The type of the domainContainer, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	/// <remarks>
	/// Uses the authentication environment to determine the logged-in user.
	/// Lazy loading is set to false by default for API scenarios, otherwise there is a
	/// danger to overserialize the whole database during a response.
	/// Eager-fetch what you need to include in a response.
	/// </remarks>
	public class LogicApiController<U, D, S> : ApiController
		where U : User
		where D : IUsersDomainContainer<U>
		where S : LogicSession<U, D>, new()
	{
		#region Private fields

		private S logicSession;

		#endregion

		#region Protected properties

		/// <summary>
		/// The LifeAccount session associated with the controller.
		/// </summary>
		protected internal S LogicSession
			=> logicSession ?? (logicSession = CreateLogicSession());

		#endregion

		#region Protected methods

		/// <summary>
		/// Closes the session.
		/// </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources;
		/// false to release only unmanaged.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			FlushSession();

			base.Dispose(disposing);
		}

		/// <summary>
		/// Creates a logic session for the controller.
		/// The default implementation has lazy loading turned off.
		/// </summary>
		protected virtual S CreateLogicSession()
		{
			S session = new S();

			// Lazy loading is set to false by default for API scenarios, otherwise there is a
			// danger to overserialize the whole database during a response.
			// Eager-fetch what you need to include in a response.
			session.IsLazyLoadingEnabled = false;

			return session;
		}

		/// <summary>
		/// Closes the session, of opened, forcing a new one to be opened 
		/// if the <see cref="LogicSession"/> property is refernced again.
		/// </summary>
		protected virtual void FlushSession()
		{
			if (logicSession != null)
			{
				logicSession.Dispose();
				logicSession = null;
			}
		}

		#endregion
	}
}
