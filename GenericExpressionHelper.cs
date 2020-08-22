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
			=> modelExpressionProvider.GetExpressionText(expression);

		/// <summary>
		/// Returns a <see cref="ModelExpression"/> describing an <paramref name="expression"/>.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TValue">The type of the <paramref name="expression"/> result.</typeparam>
		/// <param name="viewData">
		/// The <see cref="ViewDataDictionary{TModel}"/> containing the <see cref="ViewDataDictionary{TModel}.Model"/>
		/// against which the expression is evaluated.
		/// </param>
		/// <param name="expression">The expression on the model.</param>
		/// <returns>Returns the <see cref="ModelExpression"/> corresponding to the <paramref name="expression"/>.</returns>
		public static ModelExpression CreateModelExpression<TModel, TValue>(ViewDataDictionary<TModel> viewData, Expression<Func<TModel, TValue>> expression)
			=> modelExpressionProvider.CreateModelExpression(viewData, expression);
	}
}
