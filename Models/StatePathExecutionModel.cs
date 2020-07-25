using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;
using Grammophone.Domos.Logic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Grammophone.Domos.WebCore.Models
{
	/// <summary>
	/// Model for the execution of a state path
	/// on a stateful object.
	/// </summary>
	[Serializable]
	public class StatePathExecutionModel : ActionExecutionModel
	{
		#region Private fields

		private readonly Func<string, IReadOnlyDictionary<string, ParameterSpecification>> parameterSpecificationFunction;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="parameterSpecificationFunction">A function to supply the parameters neeed for execution of a state path by code name.</param>
		/// <param name="statePathCodeName">The code name of the state path being executed.</param>
		public StatePathExecutionModel(Func<string, IReadOnlyDictionary<string, ParameterSpecification>> parameterSpecificationFunction, string statePathCodeName)
			: base(statePathCodeName)
		{
			if (parameterSpecificationFunction == null) throw new ArgumentNullException(nameof(parameterSpecificationFunction));

			this.parameterSpecificationFunction = parameterSpecificationFunction;
		}

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="parameterSpecificationFunction">A function to supply the parameters neeed for execution of a state path by code name.</param>
		/// <param name="valueProvider">The value provider which will supply the ActionCodeName property.</param>
		/// <param name="prefix">Optional prefix for the key in <paramref name="valueProvider"/>.</param>
		public StatePathExecutionModel(
			Func<string, IReadOnlyDictionary<string, ParameterSpecification>> parameterSpecificationFunction,
			IValueProvider valueProvider,
			string prefix = null)
			: base(valueProvider, prefix)
		{
			if (parameterSpecificationFunction == null) throw new ArgumentNullException(nameof(parameterSpecificationFunction));

			this.parameterSpecificationFunction = parameterSpecificationFunction;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The ID of the stateful object upon which the state path will be executed.
		/// </summary>
		public long StatefulID { get; set; }

		#endregion

		#region Protected methods

		/// <summary>
		/// Uses the function supplied in the constructor to get the parameter specifications for the execution of a state path.
		/// </summary>
		/// <param name="statePathCodeName">The code name of the state path to be executed.</param>
		/// <returns>Returns a dictionary of parameter specifications by parameter name.</returns>
		protected internal override IReadOnlyDictionary<string, ParameterSpecification> GetParameterSpecifications(string statePathCodeName)
			=> parameterSpecificationFunction(statePathCodeName);

		#endregion
	}

	/// <summary>
	/// Model for the execution of a state path
	/// on a stateful object associated with a workflow manager.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="ST">The type of state transitions, derived from <see cref="StateTransition{U}"/>.</typeparam>
	/// <typeparam name="SO">The type of stateful object, implementing <see cref="IStateful{U, ST}"/>.</typeparam>
	public class StatePathExecutionModel<U, ST, SO> : StatePathExecutionModel
		where U : User
		where ST : StateTransition<U>
		where SO : IStateful<U, ST>
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="workflowManager">The associated workflow manager.</param>
		/// <param name="statePathCodeName">The code name of the state path being executed.</param>
		public StatePathExecutionModel(IWorkflowManager<U, ST, SO> workflowManager, string statePathCodeName)
			: base(workflowManager.GetPathParameterSpecifications, statePathCodeName)
		{
		}

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="workflowManager">The associated workflow manager.</param>
		/// <param name="valueProvider">The value provider which will supply the ActionCodeName property.</param>
		/// <param name="prefix">Optional prefix for the key in <paramref name="valueProvider"/>.</param>
		public StatePathExecutionModel(
			IWorkflowManager<U, ST, SO> workflowManager,
			IValueProvider valueProvider,
			string prefix = null)
			: base(workflowManager.GetPathParameterSpecifications, valueProvider, prefix)
		{
		}
	}
}
