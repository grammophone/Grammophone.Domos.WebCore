using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Grammophone.Domos.WebCore.Authorization
{
	/// <summary>
	/// Requirement for the current Domos user to have a permission.
	/// </summary>
	public class PermissionAuthorizationRequirement : IAuthorizationRequirement
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="permissionName">The name of the Domos permission.</param>
		public PermissionAuthorizationRequirement(string permissionName)
		{
			if (permissionName == null) throw new ArgumentNullException(nameof(permissionName));

			this.PermissionName = permissionName;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The name of the Domos permission.
		/// </summary>
		public string PermissionName { get; }

		#endregion
	}
}
