using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding
{
	/// <summary>
	/// Provider for <see cref="EntityCollectionModelBinder{E}"/>,
	/// for collection models implementing <see cref="ICollection{T}"/> having elements implementing <see cref="IEntityWithID{K}"/>.
	/// </summary>
	public class EntityCollectionModelBinderProvider : IModelBinderProvider
	{
		/// <summary>
		/// If the model is a collection implementing <see cref="ICollection{T}"/> having elements implementing <see cref="IEntityWithID{K}"/>,
		/// returns an <see cref="EntityCollectionModelBinder{E}"/>, else null.
		/// </summary>
		/// <param name="context">The binder context.</param>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			Type modelType = context.Metadata.ModelType;

			// Search for ICollection<E> in the implemented interfaces.
			foreach (var collectionInterfaceType in modelType.GetInterfaces())
			{
				if (!collectionInterfaceType.IsGenericType) continue;

				if (collectionInterfaceType.GetGenericTypeDefinition() != typeof(ICollection<>)) continue;

				// Get the type of element of the collection.
				Type elementType = collectionInterfaceType.GenericTypeArguments[0];

				// Search for IEntityWithID<K> in the implmented interfaces.
				foreach (var elementInterfaceType in elementType.GetInterfaces())
				{
					if (!elementInterfaceType.IsGenericType) continue;

					if (elementInterfaceType.GetGenericTypeDefinition() != typeof(IEntityWithID<>)) continue;

					// If found, return an EntityCollectionModelBinder<E>.
					return CreateBinder(context, elementType);
				}
			}

			// If we reach here, this model type is not handled by this binder provider.
			return null;
		}

		private IModelBinder CreateBinder(ModelBinderProviderContext context, Type elementType)
		{
			var binderType = typeof(EntityCollectionModelBinder<>).MakeGenericType(elementType);
			var elementBinder = context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType));

			var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
			var mvcOptions = context.Services.GetRequiredService<IOptions<MvcOptions>>().Value;

			return (IModelBinder)Activator.CreateInstance(
				binderType,
				elementBinder,
				loggerFactory,
				true /* allowValidatingTopLevelNodes */,
				mvcOptions
			);
		}
	}
}
