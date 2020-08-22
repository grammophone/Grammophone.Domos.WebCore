using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Grammophone.Domos.WebCore
{
	/// <summary>
	/// Helper class to get the model names from geneereic lambda expressions.
	/// </summary>
	/// <remarks>
	/// This class extends the ASP.NET MVC <see cref="ExpressionHelper"/>
	/// to handle generic lambda expressions of type Func{T, Object} correctly,
	/// along with the specific ones of type Func{T, P}.
	/// </remarks>
	public static class GenericExpressionHelper
	{
		private static readonly ModelExpressionProvider modelExpressionProvider = 
			new ModelExpressionProvider(new EmptyModelMetadataProvider());

		/// <summary>
		/// Get the model name from a generic lambda expression.
		/// </summary>
		/// <param name="expression">The generic lambda expression.</param>
		/// <returns>Returns the model name.</returns>
		public static string GetExpressionText<TModel, TField>(Expression<Func<TModel, TField>> expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			return modelExpressionProvider.GetExpressionText(expression);
		}
	}
}
