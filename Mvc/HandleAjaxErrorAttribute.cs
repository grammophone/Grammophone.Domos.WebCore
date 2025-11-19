using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grammophone.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Handles errors of AJAX calls and transforms the response into
	/// a <see cref="UserErrorModel"/> JSON respone if the exception was a <see cref="UserException"/>.
	/// or a descendant of <see cref="AccessDeniedException"/> or of <see cref="IntegrityViolationException"/>.
	/// </summary>
	/// <remarks>
	/// The filter determines whether this is an AJAX request by examining if
	/// there is a "X-Requested-With" header having value "XMLHttpRequest".
	/// </remarks>
	[Obsolete("Use DomosExceptionFilterAttribute instead.")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HandleAjaxErrorAttribute : DomosExceptionFilterAttribute
	{
	}
}
