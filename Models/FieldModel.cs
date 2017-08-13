using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.Web.Models
{
	/// <summary>
	/// Specifies a field with a name.
	/// </summary>
	/// <typeparam name="T">The type of the item associated with the field.</typeparam>
	public class FieldModel<T>
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="item">The item associated with the field.</param>
		public FieldModel(string fieldName, T item)
		{
			if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));

			this.FieldName = fieldName;
			this.Item = item;
		}

		/// <summary>
		/// The name of the field.
		/// </summary>
		public string FieldName { get; }

		/// <summary>
		/// The item associated with the field.
		/// </summary>
		public T Item { get; }
	}
}
