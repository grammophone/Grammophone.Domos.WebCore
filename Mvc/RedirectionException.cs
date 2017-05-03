using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// An exception to force a redirection, usually because of insufficient conditions to
	/// complete an action.
	/// </summary>
	[Serializable]
	public class RedirectionException : ApplicationException
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="targetURL">The target URL of the redirection.</param>
		public RedirectionException(string targetURL)
		{
			if (targetURL == null) throw new ArgumentNullException(nameof(targetURL));

			this.TargetURL = targetURL;
		}

		/// <summary>
		/// Used for serialization.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected RedirectionException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		/// <summary>
		/// The target URL of the redirection.
		/// </summary>
		public string TargetURL { get; }
	}
}
