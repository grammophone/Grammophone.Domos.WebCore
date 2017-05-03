using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Common functionality for <see cref="ViewPage{U, D, S}"/> and <see cref="ModelViewPage{M, U, D, S}"/>
	/// descendants.
	/// </summary>
	/// <typeparam name="U">The type of the user.</typeparam>
	/// <typeparam name="D">The type of the domain container, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of Domos logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	internal class ViewPageTrait<U, D, S>
		where U : User
		where D : IUsersDomainContainer<U>
		where S : LogicSession<U, D>, new()
	{
		#region Private fields

		private WebViewPage viewPage;

		private Lazy<S> lazyLogicSession;

		#endregion

		#region Construction

		public ViewPageTrait(WebViewPage viewPage)
		{
			if (viewPage == null) throw new ArgumentNullException(nameof(viewPage));

			this.viewPage = viewPage;

			lazyLogicSession = new Lazy<S>(() =>
			{
				if (this.viewPage.ViewContext?.Controller is LogicController<U, D, S> logicController)
				{
					return logicController.LogicSession;
				}

				return null;
			});
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The logic session associated with the view, if driven by a <see cref="LogicController{U, D, S}"/>,
		/// else null.
		/// </summary>
		public S LogicSession => lazyLogicSession.Value;

		#endregion
	}
}
