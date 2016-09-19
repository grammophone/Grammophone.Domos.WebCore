using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Selects the <see cref="EntityModelBinder{K}"/> for binding to
	/// entities implementing the <see cref="IEntityWithID{K}"/> interface.
	/// Hides <see cref="IUserTrackingEntity"/> and <see cref="IUserGroupTrackingEntity{U}"/>
	/// properties from binding.
	/// </summary>
	public class EntityModelBinderProvider : IModelBinderProvider
	{
		#region IModelBinderProvider Members

		/// <summary>
		/// If a given <paramref name="modelType"/> is an entity type
		/// implementing <see cref="IEntityWithID{K}"/>, bind it 
		/// using <see cref="EntityModelBinder{K}"/>.
		/// </summary>
		public IModelBinder GetBinder(Type modelType)
		{
			if (modelType == null) throw new ArgumentNullException(nameof(modelType));

			if (typeof(IEntityWithID<long>).IsAssignableFrom(modelType))
				return new EntityModelBinder<long>();
			else if (typeof(IEntityWithID<int>).IsAssignableFrom(modelType))
				return new EntityModelBinder<int>();
			else
				return null;
		}

		#endregion
	}
}
