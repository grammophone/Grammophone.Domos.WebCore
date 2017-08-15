using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grammophone.Domos.Web.Models
{
	/// <summary>
	/// Model for the execution of an action.
	/// </summary>
	public abstract class ActionExecutionModel
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		public ActionExecutionModel()
		{
			this.Parameters = new Dictionary<string, object>();
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The code name of the action being executed.
		/// </summary>
		public string ActionCodeName { get; set; }

		/// <summary>
		/// The collection of parameters specified for the action, indexed by their key.
		/// </summary>
		public IDictionary<string, object> Parameters { get; }

		#endregion

		#region Public methods

		/// <summary>
		/// Get the parameters corresponding to <see cref="ActionCodeName"/>, indexed by their key.
		/// </summary>
		/// <exception cref="ApplicationException">
		/// Thrown when the <see cref="ActionCodeName"/> property is not set.
		/// </exception>
		public IReadOnlyDictionary<string, Logic.ParameterSpecification> GetParameterSpecifications()
		{
			if (this.ActionCodeName == null)
				throw new ApplicationException($"The {nameof(ActionCodeName)} property is not set.");

			return GetParameterSpecifications(this.ActionCodeName);
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Get the parameter specifications for the action implied by <see cref="ActionCodeName"/>.
		/// </summary>
		/// <param name="statePathCodeName">The code name of the state path.</param>
		/// <remarks>
		/// For example, in order to implement this method for workflow state paths,
		/// see <see cref="Logic.WorkflowManager{U, BST, D, S, ST, SO, C}.GetPathParameterSpecifications(string, string)"/>.
		/// </remarks>
		protected internal abstract IReadOnlyDictionary<string, Logic.ParameterSpecification> GetParameterSpecifications(string statePathCodeName);

		#endregion
	}
}
