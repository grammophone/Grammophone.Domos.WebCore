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
		/// Get the fields of the action parameters, taking into account
		/// the model prefix.
		/// </summary>
		/// <param name="viewContext">The context of the view being rendered.</param>
		/// <returns>
		/// Returns the collection of fields whose <see cref="FieldModel{T}.FieldName"/>
		/// is the name of each parameter, taking into account any prefix.
		/// </returns>
		public IEnumerable<FieldModel<Logic.ParameterSpecification>> GetParameterFields(ViewContext viewContext)
		{
			if (viewContext == null) throw new ArgumentNullException(nameof(viewContext));

			if (this.ActionCodeName == null)
				throw new ApplicationException($"The {nameof(ActionCodeName)} property is not set.");

			var parameterSpecificationsByKey = GetParameterSpecifications(this.ActionCodeName);

			return from ps in parameterSpecificationsByKey.Values
						 orderby ps.Caption
						 select new FieldModel<Logic.ParameterSpecification>(GetFieldName(viewContext, ps), ps);
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

		#region Private methods

		private static string GetFieldName(ViewContext viewContext, Logic.ParameterSpecification parameterSpecification)
		{
			if (viewContext == null) throw new ArgumentNullException(nameof(viewContext));
			if (parameterSpecification == null) throw new ArgumentNullException(nameof(parameterSpecification));

			string prefix = viewContext.ViewData.TemplateInfo.HtmlFieldPrefix;

			if (String.IsNullOrEmpty(prefix))
				return $"Execution.{parameterSpecification.Key}";
			else
				return $"{prefix}.Execution.{parameterSpecification.Key}";
		}

		#endregion
	}
}
