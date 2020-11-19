using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Caching;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata
{
	/// <summary>
	/// Factory for obtaining <see cref="ActionExecutionModelMetadata"/>
	/// for an instance derived from <see cref="ActionExecutionModel"/>.
	/// </summary>
	public static class ActionExecutionModelMetadataFactory
	{
		#region Private fields

		private static readonly MRUCache<ActionExecutionModelMetadataKey, ActionExecutionModelMetadata> metadataCache;

		#endregion

		#region Construction

		static ActionExecutionModelMetadataFactory()
		{
			metadataCache = new MRUCache<ActionExecutionModelMetadataKey, ActionExecutionModelMetadata>(CreateMetadata, 4096);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Obtain <see cref="ActionExecutionModelMetadata"/>
		/// for an instance derived from <see cref="ActionExecutionModel"/>.
		/// </summary>
		/// <param name="metadataProvider">The metadata provider to </param>
		/// <param name="metadataDetailsProvider"></param>
		/// <param name="actionExecutionModel"></param>
		/// <returns>Returns the model metadata.</returns>
		public static ActionExecutionModelMetadata GetMetadata(
			IModelMetadataProvider metadataProvider,
			ICompositeMetadataDetailsProvider metadataDetailsProvider,
			ActionExecutionModel actionExecutionModel)
		{
			if (metadataProvider == null) throw new ArgumentNullException(nameof(metadataProvider));
			if (metadataDetailsProvider == null) throw new ArgumentNullException(nameof(metadataDetailsProvider));
			if (actionExecutionModel == null) throw new ArgumentNullException(nameof(actionExecutionModel));

			var key = new ActionExecutionModelMetadataKey(actionExecutionModel, metadataProvider, metadataDetailsProvider);

			return metadataCache.Get(key);
		}

		#endregion

		#region Private methods

		private static ActionExecutionModelMetadata CreateMetadata(ActionExecutionModelMetadataKey metadataKey)
			=> new ActionExecutionModelMetadata(metadataKey.MetadataProvider, metadataKey.MetadataDetailsProvider, metadataKey.Model);

		#endregion
	}
}
