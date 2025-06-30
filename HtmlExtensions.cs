using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Grammophone.Domos.WebCore
{
	/// <summary>
	/// Extensions for <see cref="HtmlHelper"/>.
	/// </summary>
	public static class HtmlExtensions
	{
		#region Public methods

		/// <summary>
		/// Output an unescaped fragment when a boolean value is true.
		/// </summary>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="value">The value to test.</param>
		/// <param name="fragment">The HTML fragment.</param>
		public static IHtmlContent When(this IHtmlHelper htmlHelper, bool value, string fragment)
		{
			if (htmlHelper == null) throw new ArgumentNullException("htmlHelper");
			if (fragment == null) throw new ArgumentNullException("fragment");

			if (value)
			{
				return new HtmlString(fragment);
			}
			else
			{
				return HtmlString.Empty;
			}
		}

		/// <summary>
		/// Determine whether a model field with any subproperties is valid.
		/// Model validation must have taken place before this call, 
		/// else the field is presumed always valid.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the model's field.</typeparam>
		/// <param name="htmlHelper">The html helper.</param>
		/// <param name="fieldSelector">An expression specifying the field.</param>
		/// <returns>Returns whether the field is valid. If the field was not bound, it returns false.</returns>
		public static bool IsValid<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> fieldSelector)
		{
			if (htmlHelper == null) throw new ArgumentNullException("htmlHelper");
			if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");

			string fieldPath = htmlHelper.NameFor(fieldSelector).ToString();

			var modelState = htmlHelper.ViewData.ModelState;

			if (modelState.TryGetValue(fieldPath, out ModelStateEntry modelStateEntry))
			{
				return modelStateEntry.ValidationState == ModelValidationState.Valid;
			}

			return true;
		}

		/// <summary>
		/// Get the collection of validation mesasges for a field. If the field is valid, the collection is empty.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the model's field.</typeparam>
		/// <param name="htmlHelper">The html helper.</param>
		/// <param name="fieldSelector">An expression specifying the field.</param>
		/// <returns>Returns the collection of error messages, or an empty collection if the field is valid.</returns>
		public static IEnumerable<string> ValidationMessagesFor<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> fieldSelector)
		{
			if (htmlHelper == null) throw new ArgumentNullException("htmlHelper");
			if (fieldSelector == null) throw new ArgumentNullException("fieldSelector");

			string fieldPath = htmlHelper.NameFor(fieldSelector).ToString();

			var modelState = htmlHelper.ViewData.ModelState;

			if (modelState.TryGetValue(fieldPath, out ModelStateEntry modelStateEntry))
			{
				return modelStateEntry.Errors.Select(e => e.ErrorMessage);
			}

			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Output an html fragment when a model field with any subproperties is valid.
		/// Model validation must have taken place before this call, 
		/// else the field is presumed always valid.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the model's field.</typeparam>
		/// <param name="htmlHelper">The html helper.</param>
		/// <param name="fieldSelector">An expression specifying the field.</param>
		/// <param name="fragment">The HTML fragment to output when the field is valid.</param>
		public static IHtmlContent WhenValid<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> fieldSelector,
			string fragment)
		{
			bool isValid = IsValid(htmlHelper, fieldSelector);

			return When(htmlHelper, isValid, fragment);
		}

		/// <summary>
		/// Output an html fragment when a model field with any subproperties is not valid.
		/// Model validation must have taken place before this call, 
		/// else the field is presumed always valid.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the model's field.</typeparam>
		/// <param name="htmlHelper">The html helper.</param>
		/// <param name="fieldSelector">An expression specifying the field.</param>
		/// <param name="fragment">The HTML fragment to output when the field is not valid.</param>
		public static IHtmlContent WhenNotValid<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> fieldSelector,
			string fragment)
		{
			bool isValid = IsValid(htmlHelper, fieldSelector);

			return When(htmlHelper, !isValid, fragment);
		}

		/// <summary>
		/// When a field is not valid, outputs the bootstrap "has error" class name,
		/// else returns the empty string.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the model's field.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">Expression defining the model's field.</param>
		public static IHtmlContent ValidationClassFor<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression)
		{
			return htmlHelper.WhenNotValid(modelPropertyExpression, "has-error");
		}

		/// <summary>
		/// Show a partial view for a part of the view's model.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="partialViewName">The name of the partial view template.</param>
		/// <param name="modelPropertyExpression">An expression that defines the part inside the main model.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		public static Task<IHtmlContent> PartialForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			string partialViewName,
			Expression<Func<TModel, TField>> modelPropertyExpression,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (partialViewName == null) throw new ArgumentNullException(nameof(partialViewName));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			string propertyName = GenericExpressionHelper.GetExpressionText(modelPropertyExpression);

			var propertyModelExplorer = GenericExpressionHelper.GetModelExplorer(htmlHelper.ViewData, modelPropertyExpression);

			return PartialForAsync<TModel, TField>(htmlHelper, partialViewName, propertyName, propertyModelExplorer, additionalViewData);
		}

		/// <summary>
		/// Show a partial view in folder /EditorFieldTemplates for a part of the view's model, similar to the EditorFor method.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">An expression that defines the part inside the main model.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		public static Task<IHtmlContent> EditorFieldForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			string propertyName = GenericExpressionHelper.GetExpressionText(modelPropertyExpression);

			var propertyModelExplorer = GenericExpressionHelper.GetModelExplorer(htmlHelper.ViewData, modelPropertyExpression);

			var metadata = propertyModelExplorer.Metadata;

			string templateName = GetTemplateName(metadata);

			return PartialForAsync<TModel, TField>(htmlHelper, $"EditorFieldTemplates/{templateName}", propertyName, propertyModelExplorer, additionalViewData);
		}

		/// <summary>
		/// Show a partial view in folder /EditorFieldTemplates for a part of the view's model, similar to the EditorFor method.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">An expression that defines the part inside the main model.</param>
		/// <param name="templateName">The name of the template inside the /EditorFieldTemplates folder to use.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		public static Task<IHtmlContent> EditorFieldForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression,
			string templateName,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));
			if (templateName == null) throw new ArgumentNullException(nameof(templateName));

			return PartialForAsync<TModel, TField>(htmlHelper, $"EditorFieldTemplates/{templateName}", modelPropertyExpression, additionalViewData);
		}

		/// <summary>
		/// Show a partial view in folder /DisplayFieldTemplates for a part of the view's model, similar to the DisplayFor method.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">An expression that defines the part inside the main model.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		public static Task<IHtmlContent> DisplayFieldForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			string propertyName = GenericExpressionHelper.GetExpressionText(modelPropertyExpression);

			var propertyModelExplorer = GenericExpressionHelper.GetModelExplorer(htmlHelper.ViewData, modelPropertyExpression);

			var metadata = propertyModelExplorer.Metadata;

			string templateName = GetTemplateName(metadata);

			return PartialForAsync<TModel, TField>(htmlHelper, $"DisplayFieldTemplates/{templateName}", propertyName, propertyModelExplorer, additionalViewData);
		}

		/// <summary>
		/// Show a partial view in folder /DisplayFieldTemplates for a part of the view's model, similar to the DisplayFor method.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">An expression that defines the part inside the main model.</param>
		/// <param name="templateName">The template name inside the /DisplayFieldTemplates folder to use.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		public static Task<IHtmlContent> DisplayFieldForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression,
			string templateName,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));
			if (templateName == null) throw new ArgumentNullException(nameof(templateName));

			return PartialForAsync<TModel, TField>(htmlHelper, $"DisplayFieldTemplates/{templateName}", modelPropertyExpression, additionalViewData);
		}

		/// <summary>
		/// Get the description for a property of the model of a view, if any.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the property within the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">The expression specifying the property inside the model.</param>
		/// <returns>Returns the MVC string containing the plain text of the description, if found, else returns empty content.</returns>
		public static IHtmlContent DescriptionFor<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			ModelMetadata metadata = GetPropertyMetadata(htmlHelper, modelPropertyExpression);

			return GetDescriptionContent(metadata);
		}

		/// <summary>
		/// Get the description for a property of the model of a view, if any.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpressionText">The expression text specifying the property inside the model.</param>
		/// <returns>Returns the MVC string containing the plain text of the description, if found, else returns empty content.</returns>
		public static IHtmlContent Description<TModel>(
			this IHtmlHelper<TModel> htmlHelper,
			string modelPropertyExpressionText)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpressionText == null) throw new ArgumentNullException(nameof(modelPropertyExpressionText));

			ModelMetadata metadata = GetPropertyMetadata(htmlHelper, modelPropertyExpressionText);

			return GetDescriptionContent(metadata);
		}

		/// <summary>
		/// Get the description for the model of the view, if any.
		/// </summary>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <returns>Returns the MVC string containing the plain text of the prompt, if found, else returns empty content.</returns>
		public static IHtmlContent DescriptionForModel(this IHtmlHelper htmlHelper)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));

			var metadata = htmlHelper.ViewData.ModelMetadata;

			return GetDescriptionContent(metadata);
		}

		/// <summary>
		/// Get the prompt for a property of the model of a view, if any.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <typeparam name="TField">The type of the property within the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpression">The expression specifying the property inside the model.</param>
		/// <returns>Returns the MVC string containing the plain text of the prompt, if found, else returns empty content.</returns>
		public static IHtmlContent PromptFor<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TField>> modelPropertyExpression)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpression == null) throw new ArgumentNullException(nameof(modelPropertyExpression));

			ModelMetadata metadata = GetPropertyMetadata(htmlHelper, modelPropertyExpression);

			return GetPromptContent(metadata);
		}

		/// <summary>
		/// Get the prompt for a property of the model of a view, if any.
		/// </summary>
		/// <typeparam name="TModel">The type of the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="modelPropertyExpressionText">The expression text specifying the property inside the model.</param>
		/// <returns>Returns the MVC string containing the plain text of the prompt, if found, else returns empty content.</returns>
		public static IHtmlContent Prompt<TModel>(
			this IHtmlHelper<TModel> htmlHelper,
			string modelPropertyExpressionText)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (modelPropertyExpressionText == null) throw new ArgumentNullException(nameof(modelPropertyExpressionText));

			ModelMetadata metadata = GetPropertyMetadata(htmlHelper, modelPropertyExpressionText);

			return GetPromptContent(metadata);
		}

		/// <summary>
		/// Get the prompt for the model of the view, if any.
		/// </summary>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <returns>Returns the MVC string containing the plain text of the prompt, if found, else returns empty content.</returns>
		public static IHtmlContent PromptForModel(this IHtmlHelper htmlHelper)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));

			var metadata = htmlHelper.ViewData.ModelMetadata;

			return GetPromptContent(metadata);
		}

		/// <summary>
		/// Output an HTNL attribute only if a condition is met, else out[ut nothing.
		/// </summary>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="condition">The condition for writing the attribute/</param>
		/// <param name="attributeName">The attribute name.</param>
		/// <param name="attributeValue">The attribute value.</param>
		public static IHtmlContent ConditionalAttribute(this IHtmlHelper htmlHelper, bool condition, string attributeName, object attributeValue)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));
			if (attributeValue == null) throw new ArgumentNullException(nameof(attributeValue));

			if (condition)
			{
				return new HtmlString($"{attributeName}=\"{attributeValue}\"");
			}
			else
			{
				return HtmlString.Empty;
			}
		}

		#endregion

		#region Private methods

		private static IHtmlContent GetDescriptionContent(ModelMetadata metadata)
		{
			string description = metadata?.Description;

			if (description != null)
				return new HtmlString(description);
			else
				return HtmlString.Empty;
		}

		private static IHtmlContent GetPromptContent(ModelMetadata metadata)
		{
			string prompt = metadata?.Placeholder;

			if (prompt != null)
				return new HtmlString(prompt);
			else
				return HtmlString.Empty;
		}

		private static ModelMetadata GetPropertyMetadata<TModel, TField>(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TField>> modelPropertyExpression)
		{
			var modelExplorer = GenericExpressionHelper.GetModelExplorer(htmlHelper.ViewData, modelPropertyExpression);
			
			return modelExplorer.Metadata;
		}

		private static ModelMetadata GetPropertyMetadata<TModel>(IHtmlHelper<TModel> htmlHelper, string modelPropertyExpressionText)
		{
			var modelExplorer = GenericExpressionHelper.GetModelExplorer(htmlHelper.ViewData, modelPropertyExpressionText);

			return modelExplorer.Metadata;
		}

		/// <summary>
		/// Show a partial view for a part of the view's model.
		/// </summary>
		/// <typeparam name="TModel">The type of the main model.</typeparam>
		/// <typeparam name="TField">The type of the part in the model.</typeparam>
		/// <param name="htmlHelper">The HTML helper.</param>
		/// <param name="partialViewName">The name of the partial view template.</param>
		/// <param name="propertyName">The relative path of the property relative to the current model.</param>
		/// <param name="propertyModelExplorer">The model explorer for the property.</param>
		/// <param name="additionalViewData">
		/// Optional anonymous <see cref="object"/> or <see cref="IDictionary{String, Object}"/>
		/// that can contain additional view data that will be merged into the
		/// <see cref="ViewDataDictionary{TModel}"/> instance created for the template.
		/// </param>
		/// <returns>Returns the rendered partial view.</returns>
		/// <remarks>
		/// This method takes care of the model prefix which the partial view must include
		/// in its input field expressions.
		/// </remarks>
		private static Task<IHtmlContent> PartialForAsync<TModel, TField>(
			this IHtmlHelper<TModel> htmlHelper,
			string partialViewName,
			string propertyName,
			ModelExplorer propertyModelExplorer,
			object additionalViewData = null)
		{
			if (htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));
			if (partialViewName == null) throw new ArgumentNullException(nameof(partialViewName));
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			if (propertyModelExplorer == null) throw new ArgumentNullException(nameof(propertyModelExplorer));

			var viewData = htmlHelper.ViewData;

			string fullPropertyName = viewData.TemplateInfo.GetFullHtmlFieldName(propertyName);

			var partialViewData = new ViewDataDictionary<TField>(viewData, propertyModelExplorer.Model);

			return PartialForAsync(htmlHelper, partialViewName, propertyModelExplorer, additionalViewData, fullPropertyName, partialViewData);
		}

		private static Task<IHtmlContent> PartialForAsync<TModel>(
			IHtmlHelper<TModel> htmlHelper,
			string partialViewName,
			ModelExplorer propertyModelExplorer,
			object additionalViewData,
			string fullPropertyName, 
			ViewDataDictionary partialViewData)
		{
			partialViewData.TemplateInfo.HtmlFieldPrefix = fullPropertyName;

			if (additionalViewData != null)
			{
				if (additionalViewData is IDictionary<string, object> additionalViewDictionary)
				{
					foreach (var additionalViewEntry in additionalViewDictionary)
					{
						partialViewData.Add(additionalViewEntry.Key, additionalViewEntry.Value);
					}
				}
				else
				{
					foreach (var propertyInfo in additionalViewData.GetType().GetProperties())
					{
						partialViewData.Add(propertyInfo.Name, propertyInfo.GetValue(additionalViewData));
					}
				}
			}

			partialViewData.ModelExplorer = propertyModelExplorer;

			return htmlHelper.PartialAsync(partialViewName, propertyModelExplorer.Model, partialViewData);
		}

		/// <summary>
		/// Get the template name that matches the model described in <paramref name="metadata"/>.
		/// </summary>
		private static string GetTemplateName(ModelMetadata metadata)
			=> metadata.TemplateHint ?? metadata.DataTypeName ?? (metadata.IsEnum ? "Enum" : metadata.UnderlyingOrModelType.Name);

		#endregion
	}
}
