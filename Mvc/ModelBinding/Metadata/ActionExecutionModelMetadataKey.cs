using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata
{
	/// <summary>
	/// Key for metadata of a class derived from <see cref="ActionExecutionModel"/>.
	/// </summary>
	internal class ActionExecutionModelMetadataKey : IEquatable<ActionExecutionModelMetadataKey>
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		internal ActionExecutionModelMetadataKey(
			ActionExecutionModel model,
			IModelMetadataProvider metadataProvider,
			ICompositeMetadataDetailsProvider metadataDetailsProvider)
		{
			if (model == null) throw new ArgumentNullException(nameof(model));
			if (metadataProvider == null) throw new ArgumentNullException(nameof(metadataProvider));
			if (metadataDetailsProvider == null) throw new ArgumentNullException(nameof(metadataDetailsProvider));

			this.Model = model;
			this.MetadataProvider = metadataProvider;
			this.MetadataDetailsProvider = metadataDetailsProvider;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The type of the <see cref="ActionExecutionModel"/> model.
		/// </summary>
		public Type ModelType => this.Model.GetType();

		/// <summary>
		/// The code name of the action being executed.
		/// </summary>
		public string ActionCodeName => this.Model.ActionCodeName;

		/// <summary>
		/// The metadata provider to be used upon cache miss.
		/// Not taken into account in equality tests or <see cref="GetHashCode"/>.
		/// </summary>
		public IModelMetadataProvider MetadataProvider { get; }

		/// <summary>
		/// The metadata details provider to be used upon cache miss.
		/// Not taken into account in equality tests or <see cref="GetHashCode"/>.
		/// </summary>
		public ICompositeMetadataDetailsProvider MetadataDetailsProvider { get; }

		#endregion

		#region Internal properties

		/// <summary>
		/// The instance derived from <see cref="ActionExecutionModel"/>.
		/// </summary>
		internal ActionExecutionModel Model { get; }

		#endregion

		#region IEquatable/Object implementation

		/// <inheritdoc/>
		public override bool Equals(object obj)
			=> obj is ActionExecutionModelMetadataKey key && Equals(key);

		/// <inheritdoc/>
		public bool Equals(ActionExecutionModelMetadataKey other)
			=> this.ActionCodeName == other.ActionCodeName && EqualityComparer<Type>.Default.Equals(this.ModelType, other.ModelType);

		/// <inheritdoc/>
		public override int GetHashCode()
			=> HashCode.Combine(this.ActionCodeName, this.ModelType);

		/// <inheritdoc/>
		public static bool operator ==(ActionExecutionModelMetadataKey left, ActionExecutionModelMetadataKey right) => left.Equals(right);

		/// <inheritdoc/>
		public static bool operator !=(ActionExecutionModelMetadataKey left, ActionExecutionModelMetadataKey right) => !(left == right);

		#endregion
	}
}
