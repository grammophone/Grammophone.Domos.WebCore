using System;
using System.Net;

namespace Grammophone.Domos.WebCore.Models
{
	/// <summary>
	/// Response for an exception that is translatable to a <see cref="Models.UserErrorModel"/>.
	/// </summary>
	[Serializable]
	internal class UserErrorModelResponse
	{
		/// <summary>
		/// Name name of the key to store the <see cref="UserErrorModelResponse"/> in context properties dictionary.
		/// </summary>
		public const string Key = nameof(UserErrorModelResponse);

		/// <summary>
		/// The user error model resolved for the exception thrown.
		/// </summary>
		public UserErrorModel UserErrorModel { get; set; }

		/// <summary>
		/// The HTTP status code appropriate for the exception thrown.
		/// </summary>
		public HttpStatusCode HttpStatusCode { get; set; }

		/// <summary>
		/// If true, this is considered a system error to be investigated, else, this is an error that is addressed to the user but not raising system concerns.
		/// </summary>
		public bool IsSystemError { get; set; }
	}
}
