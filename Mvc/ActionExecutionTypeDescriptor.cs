using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Type descriptor for instances derived from <see cref="Models.ActionExecutionModel"/>.
	/// It augments the strong-typed properties with the
	/// parameters defined in <see cref="Models.ActionExecutionModel.GetParameterSpecifications()"/>.
	/// </summary>
	internal class ActionExecutionTypeDescriptor : CustomTypeDescriptor
	{
		#region Private fields

		private PropertyDescriptorCollection properties;

		#endregion

		#region Cosntruction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="parent">The strong-type type descriptor.</param>
		/// <param name="container">The instance which defines the parameters.</param>
		internal ActionExecutionTypeDescriptor(
			ICustomTypeDescriptor parent, 
			Models.ActionExecutionModel container)
			: base(parent)
		{
			properties = AugmentPropertiesWithParameters(parent.GetProperties(), container.GetParameterSpecifications());
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Augment a set of strong-type properties with
		/// a collection of <see cref="Logic.ParameterSpecification"/>s.
		/// </summary>
		/// <param name="properties">The strong-type properties.</param>
		/// <param name="parameterSpecificationsByKey">The parameter specifications indexed by their key.</param>
		/// <returns>Returns a read-only collection of augmented properties.</returns>
		public static PropertyDescriptorCollection AugmentPropertiesWithParameters(
			PropertyDescriptorCollection properties, 
			IReadOnlyDictionary<string, Logic.ParameterSpecification> parameterSpecificationsByKey)
		{
			if (properties == null) throw new ArgumentNullException(nameof(properties));
			if (parameterSpecificationsByKey == null) throw new ArgumentNullException(nameof(parameterSpecificationsByKey));

			var augmentedProperties = new List<PropertyDescriptor>(properties.Count + parameterSpecificationsByKey.Count);

			for (int i = 0; i < properties.Count; i++)
			{
				var property = properties[i];

				if (property.Name != nameof(Models.ActionExecutionModel.Parameters))
				{
					augmentedProperties.Add(property);
				}
			}

			foreach (var parameterSpecification in parameterSpecificationsByKey.Values)
			{
				augmentedProperties.Add(new ActionParameterDescriptor(parameterSpecification));
			}

			return new PropertyDescriptorCollection(augmentedProperties.ToArray(), true);
		}

		/// <summary>
		/// Returns the augmented property desriptors.
		/// </summary>
		/// <returns>
		/// Returns a <see cref="PropertyDescriptorCollection"/> containing the property descriptions
		/// for the object represented by this type descriptor.
		/// </returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			return properties;
		}

		/// <summary>
		/// Returns a filtered collection of augmented property descriptors for the object represented by this type descriptor.
		/// </summary>
		/// <param name="attributes">An array of attributes to use as a filter. This can be null.</param>
		/// <returns>
		/// Returns a <see cref="PropertyDescriptorCollection"/> containing the property descriptions
		/// for the object represented by this type descriptor.
		/// </returns>
		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			if (attributes == null || attributes.Any(a => a.IsDefaultAttribute())) return properties;

			var filteredProperties = new List<PropertyDescriptor>(properties.Count);

			for (int i = 0; i < properties.Count; i++)
			{
				var property = properties[i];

				for (int j = 0; j < attributes.Length; j++)
				{
					var attribute = attributes[j];

					if (property.Attributes.Matches(attribute))
					{
						filteredProperties.Add(property);
					}
				}
			}

			return new PropertyDescriptorCollection(filteredProperties.ToArray(), true);
		}

		#endregion
	}
}
