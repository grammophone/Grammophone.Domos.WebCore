using Microsoft.AspNetCore.Mvc.Filters;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Action filter to append the 'Accept-CH' header.
	/// </summary>
	public class AcceptChFilterAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// THe contents of the 'Accept'CH' header.
		/// </summary>
		public virtual string AcceptChContent => "Sec-CH-UA-Platform-Version, Sec-CH-UA-Arch, Sec-CH-UA-Bitness, Sec-CH-UA-Full-Version, Sec-CH-UA-Model";

		/// <summary>
		/// Append the 'Accept-CH' header.
		/// </summary>
		/// <param name="filterContext"></param>
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var headers = filterContext.HttpContext.Response.Headers;

			if (!headers.ContainsKey("Accept-CH"))
				headers.Add("Accept-CH", this.AcceptChContent);

			base.OnActionExecuting(filterContext);
		}
	}
}
