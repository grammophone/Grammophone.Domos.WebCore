using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata
{
	/// <summary>
	/// Composite metadata provider for <see cref="ActionExecutionModel"/>.
	/// </summary>
	public class ActionExecutionICompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
	{
		/// <inheritdoc/>
		public void CreateBindingMetadata(BindingMetadataProviderContext context)
		{
		}

		/// <inheritdoc/>
		public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
		{
		}

		/// <inheritdoc/>
		public void CreateValidationMetadata(ValidationMetadataProviderContext context)
		{
		}
	}
}
