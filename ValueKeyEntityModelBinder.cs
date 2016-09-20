using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.Mvc
{
	/// <summary>
	/// Binder for entities derived from <see cref="IEntityWithID{K}"/>,
	/// where <typeparamref name="K"/> is a value type implementing <see cref="IEquatable{K}"/>.
	/// Hides the <see cref="IUserTrackingEntity{U}"/> and <see cref="IUserGroupTrackingEntity{U}"/>
	/// properties from binding.
	/// </summary>
	/// <typeparam name="K">The type of the key of the entity.</typeparam>
	public class ValueKeyEntityModelBinder<K> : EntityModelBinder<K>
		where K : struct, IEquatable<K>
	{
		/// <summary>
		/// Test whether two key values are equal.
		/// </summary>
		/// <param name="key1">The first key value.</param>
		/// <param name="key2">The second key value.</param>
		/// <remarks>
		/// Uses the <see cref="IEquatable{K}.Equals(K)"/> implementation.
		/// </remarks>
		protected override bool AreKeysEqual(K key1, K key2)
		{
			return key1.Equals(key2);
		}
	}
}
