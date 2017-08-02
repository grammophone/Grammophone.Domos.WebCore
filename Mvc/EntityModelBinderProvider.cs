using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Selects the <see cref="KeyedEntityModelBinder{K}"/> for binding to
	/// entities implementing the <see cref="IEntityWithID{K}"/> interface.
	/// Hides <see cref="IUserTrackingEntity"/>
	/// properties from binding.
	/// </summary>
	public class EntityModelBinderProvider : IModelBinderProvider
	{
		#region IModelBinderProvider Members

		/// <summary>
		/// If a given <paramref name="modelType"/> is an entity type
		/// implementing <see cref="IEntityWithID{K}"/>, bind it 
		/// using <see cref="KeyedEntityModelBinder{K}"/>.
		/// </summary>
		public IModelBinder GetBinder(Type modelType)
		{
			if (modelType == null) throw new ArgumentNullException(nameof(modelType));

			if (typeof(IEntityWithID<long>).IsAssignableFrom(modelType))
				return new ValueKeyedEntityModelBinder<long>();
			else if (typeof(IEntityWithID<int>).IsAssignableFrom(modelType))
				return new ValueKeyedEntityModelBinder<int>();
			else if (typeof(IEntityWithID<Guid>).IsAssignableFrom(modelType))
				return new ValueKeyedEntityModelBinder<Guid>();
			else if (typeof(IEntityWithID<object>).IsAssignableFrom(modelType)) 
				return new KeyedEntityModelBinder<object>(); // Cacthes all other IEntityWithID<K> due to covariance.
			else
				return null;
		}

		#endregion
	}
}
