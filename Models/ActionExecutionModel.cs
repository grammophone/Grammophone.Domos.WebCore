using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Grammophone.Domos.WebCore.Models
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
		/// <param name="actionCodeName">The code name of the action being executed.</param>
		public ActionExecutionModel(string actionCodeName)
		{
			if (actionCodeName == null) throw new ArgumentNullException(nameof(actionCodeName));

			this.ActionCodeName = actionCodeName;
			this.Parameters = new Dictionary<string, object>();
		}

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="valueProvider">The value provider which will supply the <see cref="ActionCodeName"/> property.</param>
		/// <param name="prefix">Optional prefix for the key in <paramref name="valueProvider"/>.</param>
		public ActionExecutionModel(IValueProvider valueProvider, string prefix = null)
		{
			if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

			string actionCodeNameKey = prefix != null ? $"{prefix}.{nameof(ActionCodeName)}" : nameof(ActionCodeName);

			var actionCodeNameResult = valueProvider.GetValue(actionCodeNameKey);

			if (actionCodeNameResult == null || String.IsNullOrEmpty(actionCodeNameResult.FirstValue))
			{
				throw new ApplicationException($"The value provider does not contain an item under key '{actionCodeNameKey}'.");
			}

			this.ActionCodeName = actionCodeNameResult.FirstValue;
			this.Parameters = new Dictionary<string, object>();
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The code name of the action being executed.
		/// </summary>
		public string ActionCodeName { get; }

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
