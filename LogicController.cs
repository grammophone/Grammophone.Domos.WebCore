using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Base for controllers associated with a Domos logic session.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="D">The type of the domainContainer, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	/// <remarks>
	/// Uses the authentication environment to determine the logged-in user.
	/// </remarks>
	public abstract class LogicController<U, D, S> : Controller
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
			=> logicSession ?? (logicSession = new S());

		#endregion

		#region Public methods

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

		#endregion

		#region Protected methods

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
