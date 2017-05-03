using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// An exception to force a redirection or other action,
	/// usually because of insufficient conditions or other errors
	/// during the execution of a controller.
	/// </summary>
	[Serializable]
	public class ActionException : SystemException
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="targetActionResult">The action to be taken.</param>
		/// <param name="innerException">Optional exception that caused the detour.</param>
		public ActionException(ActionResult targetActionResult, Exception innerException = null)
			: base("Exceptional controller action taken.", innerException)
		{
			if (targetActionResult == null) throw new ArgumentNullException(nameof(targetActionResult));

			this.TargetActionResult = targetActionResult;
		}

		/// <summary>
		/// Used for serialization.
		/// </summary>
		protected ActionException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		/// <summary>
		/// The target action to be taken.
		/// </summary>
		public ActionResult TargetActionResult { get; }
	}
}
