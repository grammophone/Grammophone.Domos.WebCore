using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Binder for <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionModelBinder : IModelBinder
	{
		#region Private fields

		private readonly ModelBinderProviderContext providerContext;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		internal ActionExecutionModelBinder(
			ModelBinderProviderContext providerContext
			)
		{
			this.providerContext = providerContext;
		}

		#endregion

		#region IModelBinder implementation

		/// <inheritdoc/>
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

			var model = (ActionExecutionModel)bindingContext.Model;

			var metadataProvider = bindingContext.HttpContext.RequestServices.GetService<IModelMetadataProvider>();
			var metadataDetailsProvider = bindingContext.HttpContext.RequestServices.GetService<ICompositeMetadataDetailsProvider>();
			var loggerFactory = bindingContext.HttpContext.RequestServices.GetService<ILoggerFactory>();

			bindingContext.ModelMetadata = ActionExecutionModelMetadataFactory.GetMetadata(
				metadataProvider,
				metadataDetailsProvider,
				model);

			var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>(bindingContext.ModelMetadata.Properties.Count);

			foreach (var propertyMetadata in bindingContext.ModelMetadata.Properties)
			{
				propertyBinders[propertyMetadata] = providerContext.CreateBinder(propertyMetadata);
			}

#pragma warning disable CS0618 // Type or member is obsolete
			var modelBinder = new ComplexTypeModelBinder(propertyBinders, loggerFactory);
#pragma warning restore CS0618 // Type or member is obsolete

			await modelBinder.BindModelAsync(bindingContext);
		}

		#endregion
	}
}
