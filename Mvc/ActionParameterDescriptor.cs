using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Dynamic property descriptor for <see cref="Logic.ParameterSpecification"/>.
	/// </summary>
	internal class ActionParameterDescriptor : PropertyDescriptor
	{
		#region Constants

		private const string NOT_A_STATE_PATH_EXECUTION = "The component is not a state path execution model.";

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="parameterSpecification">The state path parameter specification.</param>
		public ActionParameterDescriptor(Logic.ParameterSpecification parameterSpecification)
			: base(parameterSpecification.Key, parameterSpecification.ValidationAttributes.ToArray())
		{
			this.ParameterSpecification = parameterSpecification;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The state path parameter specification.
		/// </summary>
		public Logic.ParameterSpecification ParameterSpecification { get; }

		/// <summary>
		/// returns the type of <see cref="Models.ActionExecutionModel"/>.
		/// </summary>
		public override Type ComponentType => typeof(Models.ActionExecutionModel);

		/// <summary>
		/// Returns false.
		/// </summary>
		public override bool IsReadOnly => false;

		/// <summary>
		/// Returns the <see cref="Logic.ParameterSpecification.Type"/>
		/// property of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override Type PropertyType => this.ParameterSpecification.Type;

		/// <summary>
		/// Returns the <see cref="Logic.ParameterSpecification.Description"/> property
		/// of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override string Description => this.ParameterSpecification.Description;

		/// <summary>
		/// Returns the <see cref="Logic.ParameterSpecification.Caption"/> property
		/// of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override string DisplayName => this.ParameterSpecification.Caption;

		#endregion

		#region Public methods

		/// <summary>
		/// Returns true if the <paramref name="component"/> is <see cref="Models.ActionExecutionModel"/>.
		/// </summary>
		public override bool CanResetValue(object component)
		{
			return component is Models.ActionExecutionModel;
		}

		/// <summary>
		/// If the <paramref name="component"/> is <see cref="Models.ActionExecutionModel"/>,
		/// returns the value in its <see cref="Models.ActionExecutionModel.Parameters"/> dictionary
		/// under the <see cref="Logic.ParameterSpecification.Key"/> of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override object GetValue(object component)
		{
			if (component is Models.ActionExecutionModel statePathExecutionModel)
			{
				return statePathExecutionModel.Parameters[this.ParameterSpecification.Key];
			}
			else
			{
				throw new ApplicationException(NOT_A_STATE_PATH_EXECUTION);
			}
		}

		/// <summary>
		/// If the <paramref name="component"/> is <see cref="Models.ActionExecutionModel"/>,
		/// resets the value in its <see cref="Models.ActionExecutionModel.Parameters"/> dictionary
		/// under the <see cref="Logic.ParameterSpecification.Key"/> of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override void ResetValue(object component)
		{
			if (component is Models.ActionExecutionModel statePathExecutionModel)
			{
				object value = null;

				if (this.ParameterSpecification.Type.IsValueType)
				{
					value = Activator.CreateInstance(this.ParameterSpecification.Type);
				}

				statePathExecutionModel.Parameters[this.ParameterSpecification.Key] = value;
			}
			else
			{
				throw new ApplicationException(NOT_A_STATE_PATH_EXECUTION);
			}
		}

		/// <summary>
		/// If the <paramref name="component"/> is <see cref="Models.ActionExecutionModel"/>,
		/// checks type compatibility and
		/// sets the value in its <see cref="Models.ActionExecutionModel.Parameters"/> dictionary
		/// under the <see cref="Logic.ParameterSpecification.Key"/> of <see cref="ParameterSpecification"/>.
		/// </summary>
		public override void SetValue(object component, object value)
		{
			if (component is Models.ActionExecutionModel statePathExecutionModel)
			{
				if (value == null && this.ParameterSpecification.Type.IsValueType
					|| value != null && !this.ParameterSpecification.Type.IsAssignableFrom(value.GetType()))
				{
					throw new ArgumentException("The value is not compatible to the parameter.", nameof(value));
				}

				statePathExecutionModel.Parameters[this.ParameterSpecification.Key] = value;
			}
			else
			{
				throw new ApplicationException(NOT_A_STATE_PATH_EXECUTION);
			}
		}

		/// <summary>
		/// Returns true if the <paramref name="component"/> is
		/// a <see cref="Models.ActionExecutionModel"/>.
		/// </summary>
		public override bool ShouldSerializeValue(object component)
			=> component is Models.ActionExecutionModel;

		#endregion
	}
}
