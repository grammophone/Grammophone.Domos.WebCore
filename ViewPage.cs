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
	/// Base for Razor views cooperating with the Domos system.
	/// In order to create a replacement for the default base,
	/// subclass this to a specialized abstract class with no type arguments.
	/// </summary>
	/// <typeparam name="U">The type of the user.</typeparam>
	/// <typeparam name="D">The type of the domain container, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of Domos logic session, derived from <see cref="Session{U, D}"/>.</typeparam>
	/// <remarks>
	/// In order to use this class as a base view, change the default base as described 
	/// in http://haacked.com/archive/2011/02/21/changing-base-type-of-a-razor-view.aspx/
	/// or use the "inherits" directive inside the Razor view.
	/// </remarks>
	public abstract class ViewPage<U, D, S> : WebViewPage
		where U : User
		where D : IUsersDomainContainer<U>
		where S : Session<U, D>, new()
	{
		#region Private fields

		private ViewPageTrait<U, D, S> trait;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ViewPage()
		{
			trait = new ViewPageTrait<U, D, S>(this);
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The currently authenticated Domos user, if there is one and
		/// if the view is driven by a <see cref="LogicController{U, D, S}"/>,
		/// else null.
		/// </summary>
		public U CurrentUser => this.LogicSession?.User;

		#endregion

		#region Protected properties

		/// <summary>
		/// The logic session associated with the view,
		/// if the view is driven by a <see cref="LogicController{U, D, S}"/>,
		/// else null.
		/// </summary>
		protected S LogicSession => trait.LogicSession;

		#endregion
	}

	/// <summary>
	/// Base for Razor views cooperating with the Domos system.
	/// In order to create a replacement for the default base,
	/// subclass this to a specialized abstract class with no type arguments.
	/// </summary>
	/// <typeparam name="U">The type of the user.</typeparam>
	/// <typeparam name="D">The type of the domain container, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">
	/// The type of Domos logic session, derived from <see cref="Session{U, D}"/> and offering public domain access.
	/// </typeparam>
	/// <typeparam name="PD">The type of public domain repository.</typeparam>
	/// <remarks>
	/// In order to use this class as a base view, change the default base as described 
	/// in http://haacked.com/archive/2011/02/21/changing-base-type-of-a-razor-view.aspx/
	/// or use the "inherits" directive inside the Razor view.
	/// </remarks>
	public abstract class ViewPage<U, D, S, PD> : ViewPage<U, D, S>
		where U : User
		where D : IUsersDomainContainer<U>
		where S : Session<U, D>, IPublicDomainProvider<D, PD>, new()
		where PD : PublicDomain<D>
	{
		#region Public properties

		/// <summary>
		/// Public queriable entities,
		/// if the view is driven by a <see cref="LogicController{U, D, S}"/>,
		/// else null.
		/// </summary>
		public PD PublicDomain => this.LogicSession?.PublicDomain;

		#endregion
	}
}
