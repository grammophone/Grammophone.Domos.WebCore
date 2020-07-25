using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Extension methods for value providers.
	/// </summary>
	public static class ValueProviderExtensions
	{
		/// <summary>
		/// Attempt to get the value of a model property from a value provider,
		/// else return null if the value was not specified.
		/// </summary>
		/// <typeparam name="M">The type of the model being bound to the value provider.</typeparam>
		/// <param name="valueProvider">The Value provoder.</param>
		/// <param name="modelPropertyExpression">Expression for selecting the property from the bound model.</param>
		/// <returns>Returns the result found if the value exists, else null.</returns>
		public static ValueProviderResult GetValue<M>(
			this IValueProvider valueProvider, 
			Expression<Func<M, object>> modelPropertyExpression)
		{
			if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			string propertyKey = GenericExpressionHelper.GetExpressionText(modelPropertyExpression);

			return valueProvider.GetValue(propertyKey);
		}
	}
}
