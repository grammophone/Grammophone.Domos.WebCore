using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;
using Grammophone.Domos.Logic;

namespace Grammophone.Domos.Web.Models
{
	/// <summary>
	/// Model for executing a state path against a stateful object.
	/// </summary>
	/// <typeparam name="SO">The type of the stateful object.</typeparam>
	public class StatefulExecutionModel<SO>
		where SO : IStateful
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="stateful">The stateful object to execute the path on.</param>
		/// <param name="statePath">The state path to execute.</param>
		/// <param name="execution">The path execution model.</param>
		public StatefulExecutionModel(SO stateful, StatePath statePath, StatePathExecutionModel execution)
		{
			if (stateful == null) throw new ArgumentNullException(nameof(stateful));
			if (statePath == null) throw new ArgumentNullException(nameof(statePath));
			if (execution == null) throw new ArgumentNullException(nameof(execution));

			this.Stateful = stateful;
			this.StatePath = statePath;
			this.Execution = execution;
		}

		/// <summary>
		/// The stateful object.
		/// </summary>
		public SO Stateful { get; }

		/// <summary>
		/// The state path to be executed on the <see cref="Stateful"/>.
		/// </summary>
		public StatePath StatePath { get; }

		/// <summary>
		/// Contains the parameters of the execution of the <see cref="StatePath"/>
		/// against the <see cref="Stateful"/>.
		/// </summary>
		public StatePathExecutionModel Execution { get; }
	}

	/// <summary>
	/// Model for executing a state path against a stateful object.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="ST">The type of state transitions, derived from <see cref="StateTransition{U}"/>.</typeparam>
	/// <typeparam name="SO">The type of stateful object, implementing <see cref="IStateful{U, ST}"/>.</typeparam>
	public class StatefulExecutionModel<U, ST, SO> : StatefulExecutionModel<SO>
		where U : User
		where ST : StateTransition<U>
		where SO : IStateful<U, ST>
	{
		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="stateful">The stateful object to execute the path on.</param>
		/// <param name="statePath">The state path to execute.</param>
		/// <param name="workflowManager">The workflow manager for the <paramref name="stateful"/> object.</param>
		public StatefulExecutionModel(SO stateful, StatePath statePath, IWorkflowManager<U, ST, SO> workflowManager)
			: base(stateful, statePath, CreateStatePathExecution(stateful, statePath, workflowManager))
		{
		}

		private static StatePathExecutionModel CreateStatePathExecution(SO stateful, StatePath statePath, IWorkflowManager<U, ST, SO> workflowManager)
		{
			if (stateful == null) throw new ArgumentNullException(nameof(stateful));
			if (statePath == null) throw new ArgumentNullException(nameof(statePath));
			if (workflowManager == null) throw new ArgumentNullException(nameof(workflowManager));

			return new StatePathExecutionModel<U, ST, SO>(workflowManager, statePath.CodeName)
			{
				StatefulID = stateful.ID
			};
		}
	}
}
