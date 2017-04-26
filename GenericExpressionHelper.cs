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
	/// Helper class to get the model names from geneereic lambda expressions.
	/// </summary>
	/// <remarks>
	/// This class extends the ASP.NET MVC <see cref="ExpressionHelper"/>
	/// to handle generic lambda expressions of type Func{T, Object} correctly,
	/// along with the specific ones of type Func{T, P}.
	/// </remarks>
	public static class GenericExpressionHelper
	{
		/// <summary>
		/// Get the model name from a generic lambda expression.
		/// </summary>
		/// <param name="expression">The generic lambda expression.</param>
		/// <returns>Returns the model name.</returns>
		public static string GetExpressionText(LambdaExpression expression)
		{
			if (expression == null) throw new ArgumentNullException(nameof(expression));

			// Is the lambda expression of the type "m => Convert(m.Value))"?
			// If so, unpack the inner property expression.
			var unaryExpression = expression.Body as UnaryExpression;

			if (unaryExpression != null)
			{
				if (unaryExpression.NodeType == ExpressionType.Convert)
				{
					expression = Expression.Lambda(unaryExpression.Operand, expression.Parameters);
				}
			}

			return ExpressionHelper.GetExpressionText(expression);
		}
	}
}
