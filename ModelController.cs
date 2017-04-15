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
	/// Controller facilitating model binding.
	/// </summary>
	public abstract class ModelController : Controller
	{
		#region Protected methods

		/// <summary>
		/// Updates the specified model instance using values from the controller's current value provider
		/// and included properties.
		/// </summary>
		/// <typeparam name="M">The type of the model.</typeparam>
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns></returns>
		protected bool TryUpdateModel<M>(
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
		/// <param name="model">The model to update.</param>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns></returns>
		protected bool TryUpdateModel<M>(
			M model,
			string prefix,
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
		/// <param name="model">The model to update.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns></returns>
		protected void UpdateModel<M>(
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
		/// <param name="model">The model to update.</param>
		/// <param name="prefix">The prefix to use when looking up values in the value provider.</param>
		/// <param name="includedPropertiesSelectors">Array of expressions of included properties.</param>
		/// <returns></returns>
		protected void UpdateModel<M>(
			M model,
			string prefix,
			params Expression<Func<M, object>>[] includedPropertiesSelectors)
			where M : class
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (includedPropertiesSelectors == null) throw new ArgumentNullException(nameof(includedPropertiesSelectors));

			string[] includedProperties = GetPropertyNames(includedPropertiesSelectors);

			UpdateModel(model, prefix, includedProperties);
		}

		#endregion

		#region Private methods

		private static string[] GetPropertyNames<M>(Expression<Func<M, object>>[] propertySelectors)
			where M : class
		{
			string[] propertyNames = new string[propertySelectors.Length];

			for (int i = 0; i < propertySelectors.Length; i++)
			{
				propertyNames[i] = ExpressionHelper.GetExpressionText(propertySelectors[i]);
			}

			return propertyNames;
		}

		#endregion
	}
}
