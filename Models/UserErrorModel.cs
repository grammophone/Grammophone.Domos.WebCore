using Grammophone.Domos.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.Web.Models
{
	/// <summary>
	/// An error which is intended to be displayed to the user.
	/// </summary>
	public class UserErrorModel
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="displayMessage">The display message to be set.</param>
		public UserErrorModel(string displayMessage)
		{
			if (displayMessage == null) throw new ArgumentNullException(nameof(displayMessage));

			this.DisplayMessage = displayMessage;
		}

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="userException">The user exception containing the dispayed message.</param>
		public UserErrorModel(UserException userException)
		{
			if (userException == null) throw new ArgumentNullException(nameof(userException));

			this.DisplayMessage = userException.Message;
			this.ExceptionName = userException.GetType().FullName;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The message intended to be displayed to the user.
		/// </summary>
		public string DisplayMessage { get; private set; }

		/// <summary>
		/// The name of the exception.
		/// </summary>
		public string ExceptionName { get; private set; }

		#endregion
	}
}
