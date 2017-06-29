using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Controller facilitating model binding with better strong-typing.
	/// </summary>
	public abstract class ModelController : Controller
	{
		#region Protected methods

		/// <summary>
		/// Render the output of a view to a string.
		/// </summary>
		/// <param name="viewPath">The path to the view.</param>
		/// <param name="model">The model for the view.</param>
		/// <param name="partial">If true, the view is partial.</param>
		/// <returns>Returns the string from the view rendering.</returns>
		protected string RenderViewToString(
			string viewPath,
			object model = null,
			bool partial = true)
		{
			if (viewPath == null) throw new ArgumentNullException(nameof(viewPath));

			var controllerContext = this.ControllerContext;

			// First find the ViewEngine for this view.
			ViewEngineResult viewEngineResult = null;

			if (partial)
				viewEngineResult = ViewEngines.Engines.FindPartialView(controllerContext, viewPath);
			else
				viewEngineResult = ViewEngines.Engines.FindView(controllerContext, viewPath, null);

			if (viewEngineResult == null)
				throw new ArgumentException("View cannot be found.", nameof(viewPath));

			// Get the view and attach the model to view data.
			var view = viewEngineResult.View;
			controllerContext.Controller.ViewData.Model = model;

			string result = null;

			using (var writer = new System.IO.StringWriter())
			{
				var viewContext = new ViewContext(
					controllerContext, 
					view,
					controllerContext.Controller.ViewData,
					controllerContext.Controller.TempData,
					writer);

				view.Render(viewContext, writer);

				result = writer.ToString();
			}

			return result;
		}

		/// <summary>
		/// Attempt to update the specified model instance using values from the controller's current value provider.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModel<M, VM>(M model, Expression<Func<VM, M>> modelSelector)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string modelPrefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			return TryUpdateModel(model, modelPrefix);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModel<M, VM>(M model, Expression<Func<VM, M>> modelSelector)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string modelPrefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			UpdateModel(model, modelPrefix);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelIncluding<M>(
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (includedPropertiesSelectors == null) throw new ArgumentNullException(nameof(includedPropertiesSelectors));

			string[] includedProperties = GetPropertyNames(includedPropertiesSelectors);

			return TryUpdateModel(model, includedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelIncluding<M>(
			string prefix,
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (includedPropertiesSelectors == null) throw new ArgumentNullException(nameof(includedPropertiesSelectors));

			string[] includedProperties = GetPropertyNames(includedPropertiesSelectors);

			return TryUpdateModel(model, prefix, includedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelIncluding<M, VM>(
			Expression<Func<VM, M>> modelSelector,
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string prefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			return TryUpdateModelIncluding(prefix, model, includedPropertiesSelectors);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelIncluding<M>(
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (includedPropertiesSelectors == null) throw new ArgumentNullException(nameof(includedPropertiesSelectors));

			string[] includedProperties = GetPropertyNames(includedPropertiesSelectors);

			UpdateModel(model, includedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelIncluding<M>(
			string prefix,
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (includedPropertiesSelectors == null) throw new ArgumentNullException(nameof(includedPropertiesSelectors));

			string[] includedProperties = GetPropertyNames(includedPropertiesSelectors);

			UpdateModel(model, prefix, includedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelIncluding<M, VM>(
			Expression<Func<VM, M>> modelSelector,
			M model,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string prefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			UpdateModelIncluding(prefix, model, includedPropertiesSelectors);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider,
		/// excluding the specified properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelExcluding<M>(
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (excludedPropertiesSelectors == null) throw new ArgumentNullException(nameof(excludedPropertiesSelectors));

			string[] excludedProperties = GetPropertyNames(excludedPropertiesSelectors);

			return TryUpdateModel(model, String.Empty, null, excludedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider,
		/// excluding the specified properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelExcluding<M>(
			string prefix,
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (excludedPropertiesSelectors == null) throw new ArgumentNullException(nameof(excludedPropertiesSelectors));

			string[] excludedProperties = GetPropertyNames(excludedPropertiesSelectors);

			return TryUpdateModel(model, prefix, null, excludedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider,
		/// excluding the specified properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <returns>Returns true when the included model properties were valid.</returns>
		protected bool TryUpdateModelExcluding<M, VM>(
			Expression<Func<VM, M>> modelSelector,
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string prefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			return TryUpdateModelExcluding(model, excludedPropertiesSelectors);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider,
		/// excluding the specified properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelExcluding<M>(
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (excludedPropertiesSelectors == null) throw new ArgumentNullException(nameof(excludedPropertiesSelectors));

			string[] excludedProperties = GetPropertyNames(excludedPropertiesSelectors);

			UpdateModel(model, String.Empty, null, excludedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelExcluding<M>(
			string prefix,
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (excludedPropertiesSelectors == null) throw new ArgumentNullException(nameof(excludedPropertiesSelectors));

			string[] excludedProperties = GetPropertyNames(excludedPropertiesSelectors);

			UpdateModel(model, prefix, null, excludedProperties);
		}

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <typeparam name="VM">The type of object which contains the model.</typeparam>
		/// <param name="modelSelector">The expression which extracts the model from which container.</param>
		/// <param name="model">The model to update.</param>
		/// <param name="excludedPropertiesSelectors">Array of expressions of excluded properties.</param>
		/// <exception cref="InvalidOperationException">Thrown when the included properties are not valid.</exception>
		protected void UpdateModelExcluding<M, VM>(
			Expression<Func<VM, M>> modelSelector,
			M model,
			params Expression<Func<M, object>>[] excludedPropertiesSelectors)
			where M : class
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string prefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			UpdateModelExcluding(prefix, model, excludedPropertiesSelectors);
		}

		#endregion

		#region Private methods

		private static string[] GetPropertyNames<M>(Expression<Func<M, object>>[] propertySelectors)
			where M : class
		{
			string[] propertyNames = new string[propertySelectors.Length];

			for (int i = 0; i < propertySelectors.Length; i++)
			{
				propertyNames[i] = GenericExpressionHelper.GetExpressionText(propertySelectors[i]);
			}

			return propertyNames;
		}

		#endregion
	}
}
