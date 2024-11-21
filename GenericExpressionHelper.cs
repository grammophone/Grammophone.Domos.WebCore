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
		/// Get a model explorer for an expression traversing properties of the model recursively.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TValue">The type of the value the expression selects from the model.</typeparam>
		/// <param name="viewData">The view data.</param>
		/// <param name="expression">The expression accessing the property of the model.</param>
		/// <returns>Returns a model explorer for the expression.</returns>
		public static ModelExplorer GetModelExplorer<TModel, TValue>(ViewDataDictionary<TModel> viewData, Expression<Func<TModel, TValue>> expression)
		{
			if (viewData == null) throw new ArgumentNullException(nameof(viewData));
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			string expressionText = modelExpressionProvider.GetExpressionText(expression);

			return GetModelExplorer(viewData, expressionText);
		}

		/// <summary>
		/// Get a model explorer for an expression traversing properties of the model recursively.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <param name="viewData">The view data.</param>
		/// <param name="expressionText">The expression text for the property of the model.</param>
		/// <returns>Returns a model explorer for the expression.</returns>
		public static ModelExplorer GetModelExplorer<TModel>(ViewDataDictionary<TModel> viewData, string expressionText)
		{
			if (viewData == null) throw new ArgumentNullException(nameof(viewData));
			if (expressionText == null) throw new ArgumentNullException(nameof(expressionText));

			ModelExplorer modelExplorer = viewData.ModelExplorer;

			foreach (string expressionComponent in expressionText.Split('.'))
			{
				if (String.IsNullOrEmpty(expressionComponent)) continue;

				modelExplorer = modelExplorer.GetExplorerForProperty(expressionComponent);
			}

			return modelExplorer;
		}
	}
}
