using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Grammophone.Domos.WebCore.Mvc.ViewFeatures
{
	/// <summary>
	/// An HTML helper able to render the dynamic <see cref="ActionExecutionModel.Parameters"/> in <see cref="ActionExecutionModel"/>.
	/// </summary>
	/// <typeparam name="TModel">The strong-type model of the view.</typeparam>
	public class ActionExecutionHtmlHelper<TModel> : HtmlHelper<TModel>
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionHtmlHelper(
			IHtmlGenerator htmlGenerator,
			ICompositeViewEngine viewEngine,
			IModelMetadataProvider metadataProvider,
			IViewBufferScope bufferScope,
			HtmlEncoder htmlEncoder,
			UrlEncoder urlEncoder,
			ModelExpressionProvider modelExpressionProvider)
			: base(htmlGenerator, viewEngine, metadataProvider, bufferScope, htmlEncoder, urlEncoder, modelExpressionProvider)
		{
		}

		#endregion

		#region Public methods

		/// <summary>
		/// If the model derives from <see cref="ActionExecutionModel"/>, enrich the metadata before rendering the editor, else
		/// use the default metadata.
		/// </summary>
		protected override IHtmlContent GenerateEditor(ModelExplorer modelExplorer, string htmlFieldName, string templateName, object additionalViewData)
		{
			if (modelExplorer == null) throw new ArgumentNullException(nameof(modelExplorer));

			if (htmlFieldName != null && modelExplorer.Container != null && typeof(ActionExecutionModel).IsAssignableFrom(modelExplorer.Container.ModelType))
			{
				var actionExecutionModel = (ActionExecutionModel)modelExplorer.Container.Model;

				var compositeMetadataDetailsProvider = this.ViewContext.HttpContext.RequestServices.GetService<ICompositeMetadataDetailsProvider>();

				var actionExecutionModelMetadata = //new DefaultModelMetadata(this.MetadataProvider, compositeMetadataDetailsProvider, details);
					new ActionExecutionModelMetadata(this.MetadataProvider, compositeMetadataDetailsProvider, actionExecutionModel, modelExplorer.Metadata.ContainerMetadata);

				var actionExecutionModelExplorer = new ModelExplorer(this.MetadataProvider, modelExplorer.Container, actionExecutionModelMetadata, actionExecutionModel);

				var parameterExplorer = actionExecutionModelExplorer.GetExplorerForProperty(htmlFieldName);

				return base.GenerateEditor(parameterExplorer, htmlFieldName, templateName, additionalViewData);
			}

			return base.GenerateEditor(modelExplorer, htmlFieldName, templateName, additionalViewData);
		}

		#endregion

		#region Private methods

		private object GetParameterValue(Models.ActionExecutionModel model, ParameterSpecification parameterSpecification)
		{
			if (model.Parameters.TryGetValue(parameterSpecification.Key, out object value))
			{
				return value;
			}
			else
			{
				return parameterSpecification.GetDefaultValue();
			}
		}

		#endregion
	}
}
