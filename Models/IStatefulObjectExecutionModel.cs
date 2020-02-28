using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;

namespace Grammophone.Domos.Web.Models
{
	/// <summary>
	/// Non-specialized interface for the execution of a state path over a stateful object.
	/// </summary>
	public interface IStatefulObjectExecutionModel
	{
		/// <summary>
		/// The stateful object.
		/// </summary>
		IStateful Stateful { get; }

		/// <summary>
		/// The state path to be executed on the <see cref="Stateful"/>.
		/// </summary>
		StatePath StatePath { get; }

		/// <summary>
		/// Contains the parameters of the execution of the <see cref="StatePath"/>
		/// against the <see cref="Stateful"/>.
		/// </summary>
		StatePathExecutionModel ExecutionModel { get; }
	}

	/// <summary>
	/// Specialized interface for the execution of a state path over a stateful object.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="ST">The type of state transitions, derived from <see cref="StateTransition{U}"/>.</typeparam>
	/// <typeparam name="SO">The type of stateful object, implementing <see cref="IStateful{U, ST}"/>.</typeparam>
	public interface IStatefulObjectExecutionModel<U, out ST, out SO>
		where U : User
		where ST : StateTransition<U>
		where SO : IStateful
	{
		/// <summary>
		/// The stateful object.
		/// </summary>
		SO Stateful { get; }

		/// <summary>
		/// The state path to be executed on the <see cref="Stateful"/>.
		/// </summary>
		StatePath StatePath { get; }

		/// <summary>
		/// Contains the parameters of the execution of the <see cref="StatePath"/>
		/// against the <see cref="Stateful"/>.
		/// </summary>
		StatePathExecutionModel ExecutionModel { get; }

		/// <summary>
		/// The set of state transitions of the <see cref="Stateful"/> object.
		/// </summary>
		IQueryable<ST> StateTransitions { get; }
	}
}
