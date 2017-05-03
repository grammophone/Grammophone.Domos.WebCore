using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Extensions for <see cref="ModelStateDictionary"/>
	/// </summary>
	public static class ModelStateExtensions
	{
		/// <summary>
		/// Adds the specified error message to the errors collection for the model-state
		/// dictionary that is associated with the specified property.
		/// </summary>
		/// <typeparam name="M">The type of the model being bound.</typeparam>
		/// <param name="modelState">The model state.</param>
		/// <param name="propertySelector">Expression specifying the property having the error.</param>
		/// <param name="message">The error message.</param>
		public static void AddModelError<M>(
			this ModelStateDictionary modelState, 
			Expression<Func<M, object>> propertySelector, 
			string message)
		{
			if (modelState == null) throw new ArgumentNullException(nameof(modelState));
			if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
			if (message == null) throw new ArgumentNullException(nameof(message));

			string propertyKey = GenericExpressionHelper.GetExpressionText(propertySelector);

			modelState.AddModelError(propertyKey, message);
		}

		/// <summary>
		/// Adds the specified exception to the errors collection for the model-state
		/// dictionary that is associated with the specified property.
		/// </summary>
		/// <typeparam name="M">The type of the model being bound.</typeparam>
		/// <param name="modelState">The model state.</param>
		/// <param name="propertySelector">Expression specifying the property having the error.</param>
		/// <param name="exception">The exception.</param>
		public static void AddModelError<M>(
			this ModelStateDictionary modelState,
			Expression<Func<M, object>> propertySelector,
			Exception exception)
		{
			if (modelState == null) throw new ArgumentNullException(nameof(modelState));
			if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			string propertyKey = GenericExpressionHelper.GetExpressionText(propertySelector);

			modelState.AddModelError(propertyKey, exception);
		}
	}
}
