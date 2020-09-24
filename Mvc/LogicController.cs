using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.DataAccess;
using Grammophone.Domos.Domain;
using Grammophone.Domos.Domain.Workflow;
using Grammophone.Domos.Logic;
using Grammophone.Domos.WebCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Grammophone.Domos.WebCore.Mvc
{
	/// <summary>
	/// Base for controllers associated with a Domos logic session.
	/// </summary>
	/// <typeparam name="U">The type of the user, derived from <see cref="User"/>.</typeparam>
	/// <typeparam name="D">The type of the domainContainer, derived from <see cref="IUsersDomainContainer{U}"/>.</typeparam>
	/// <typeparam name="S">The type of logic session, derived from <see cref="LogicSession{U, D}"/>.</typeparam>
	public abstract class LogicController<U, D, S> : Controller
		where U : User
		where D : IUsersDomainContainer<U>
		where S : LogicSession<U, D>
	{
		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="logicSession">The Domos logic session.</param>
		public LogicController(S logicSession)
		{
			if (logicSession == null) throw new ArgumentNullException(nameof(logicSession));

			this.LogicSession = logicSession;
		}

		#endregion

		#region Protected properties

		/// <summary>
		/// The LifeAccount session associated with the controller.
		/// </summary>
		protected internal S LogicSession { get; }

		#endregion

		#region Public methods

		/// <inheritdoc/>
		[NonAction]
		public override bool TryValidateModel(object model)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = base.TryValidateModel(model);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override bool TryValidateModel(object model, string prefix)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = base.TryValidateModel(model, prefix);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync(object model, Type modelType, string prefix)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, modelType, prefix);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider)
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix, valueProvider);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider, Func<ModelMetadata, bool> propertyFilter)
			where TModel : class
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix, valueProvider, propertyFilter);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions)
			where TModel : class
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix, includeExpressions);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, Func<ModelMetadata, bool> propertyFilter)
			where TModel : class
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix, propertyFilter);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider, params Expression<Func<TModel, object>>[] includeExpressions)
			where TModel : class
		{
			bool previousLazyLoadingSetting = this.LogicSession.IsLazyLoadingEnabled;

			this.LogicSession.IsLazyLoadingEnabled = false;

			try
			{
				bool validationResult = await base.TryUpdateModelAsync(model, prefix, valueProvider, includeExpressions);

				return validationResult;
			}
			finally
			{
				this.LogicSession.IsLazyLoadingEnabled = previousLazyLoadingSetting;
			}
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Using the controller's value provider, Bind a <see cref="IStatefulObjectExecutionModel"/> for executing a path on a stateful object.
		/// Any validation errors are capured in ModelState.
		/// </summary>
		/// <typeparam name="ST">The type of state transitions of the stateful object.</typeparam>
		/// <typeparam name="SO">The type of the stateful object.</typeparam>
		/// <typeparam name="WM">The type of the workflow manager for the stateful object.</typeparam>
		/// <param name="workflowManager">he workflow manager for the stateful object.</param>
		/// <param name="prefix">Optional prefix for the <see cref="IStatefulObjectExecutionModel"/> being bound.</param>
		/// <returns>Returns the model requested. Check ModelState for any validation errors.</returns>
		protected async Task<StatefulObjectExecutionModel<U, ST, SO>> BindStatefulObjectExecutionModelAsync<ST, SO, WM>(WM workflowManager, string prefix = null)
			where ST : StateTransition<U>
			where SO : IStateful<U, ST>
			where WM : IWorkflowManager<U, ST, SO>
		{
			if (workflowManager == null) throw new ArgumentNullException(nameof(workflowManager));

			var valueProvider = await CompositeValueProvider.CreateAsync(this.ControllerContext);

			var executionModel = new StatePathExecutionModel<U, ST, SO>(workflowManager, valueProvider, nameof(StatefulObjectExecutionModel<SO>.ExecutionModel));

			string executionModelPrefix;

			if (!String.IsNullOrEmpty(prefix))
				executionModelPrefix = $"{prefix}.{nameof(StatefulObjectExecutionModel<SO>.ExecutionModel)}";
			else
				executionModelPrefix = nameof(StatefulObjectExecutionModel<SO>.ExecutionModel);

			await TryUpdateModelAsync(executionModel, executionModelPrefix);

			var stateful = await workflowManager.GetStatefulObjectAsync(executionModel.StatefulID);

			var statePath = await workflowManager.GetStatePathAsync(executionModel.ActionCodeName);

			var stateTransitions = workflowManager.GetStateTransitions(stateful);

			return new StatefulObjectExecutionModel<U, ST, SO>(stateful, statePath, executionModel, stateTransitions);
		}

		/// <summary>
		/// Using the controller's value provider, Bind a <see cref="IStatefulObjectExecutionModel"/> for executing a path on a stateful object.
		/// Any validation errors are capured in ModelState.
		/// </summary>
		/// <typeparam name="ST">The type of state transitions of the stateful object.</typeparam>
		/// <typeparam name="SO">The type of the stateful object.</typeparam>
		/// <typeparam name="WM">The type of the workflow manager for the stateful object.</typeparam>
		/// <typeparam name="VM">The type of the view model containing the <see cref="IStatefulObjectExecutionModel"/>.</typeparam>
		/// <param name="workflowManager">he workflow manager for the stateful object.</param>
		/// <param name="modelSelector">The expression to extract <see cref="IStatefulObjectExecutionModel"/> from the view model of type <typeparamref name="VM"/>.</param>
		/// <returns>Returns the model requested. Check ModelState for any validation errors.</returns>
		protected async Task<StatefulObjectExecutionModel<U, ST, SO>> BindStatefulObjectExecutionModelAsync<ST, SO, WM, VM>(
			WM workflowManager, 
			Expression<Func<VM, IStatefulObjectExecutionModel>> modelSelector)
			where ST : StateTransition<U>
			where SO : IStateful<U, ST>
			where WM : IWorkflowManager<U, ST, SO>
		{
			if (modelSelector == null) throw new ArgumentNullException(nameof(modelSelector));

			string prefix = GenericExpressionHelper.GetExpressionText(modelSelector);

			return await BindStatefulObjectExecutionModelAsync<ST, SO, WM>(workflowManager, prefix);
		}

		/// <summary>
		/// Create a <see cref="IStatefulObjectExecutionModel"/> for executing a state path on a stateful object.
		/// </summary>
		/// <typeparam name="ST">The type of state transitions of the stateful object.</typeparam>
		/// <typeparam name="SO">The type of the stateful object.</typeparam>
		/// <typeparam name="WM">The type of the workflow manager for the stateful object.</typeparam>
		/// <param name="workflowManager">he workflow manager for the stateful object.</param>
		/// <param name="statefulID">The ID of the stateful object.</param>
		/// <param name="statePathCodeName">The code name of the state path to be executed on the stateful object.</param>
		/// <returns>Returns the model requested.</returns>
		protected async Task<StatefulObjectExecutionModel<U, ST, SO>> CreateStatefulObjectExecutionModelAsync<ST, SO, WM>(
			WM workflowManager,
			long statefulID,
			string statePathCodeName)
			where ST : StateTransition<U>
			where SO : IStateful<U, ST>
			where WM : IWorkflowManager<U, ST, SO>
		{
			if (workflowManager == null) throw new ArgumentNullException(nameof(workflowManager));
			if (statePathCodeName == null) throw new ArgumentNullException(nameof(statePathCodeName));

			var stateful = await workflowManager.GetStatefulObjectAsync(statefulID);

			var statePath = await workflowManager.GetStatePathAsync(statePathCodeName);

			return new StatefulObjectExecutionModel<U, ST, SO>(stateful, statePath, workflowManager);
		}

		/// <summary>
		/// Create a model for executing a state path on a stateful object.
		/// </summary>
		/// <typeparam name="ST">The type of state transitions of the stateful object.</typeparam>
		/// <typeparam name="SO">The type of the stateful object.</typeparam>
		/// <typeparam name="WM">The type of the workflow manager for the stateful object.</typeparam>
		/// <param name="workflowManager">he workflow manager for the stateful object.</param>
		/// <param name="statefulID">The ID of the stateful object.</param>
		/// <param name="statePathID">The ID of the state path to be executed on the stateful object.</param>
		/// <returns>Returns the model requested.</returns>
		protected async Task<StatefulObjectExecutionModel<U, ST, SO>> CreateStatefulObjectExecutionModelAsync<ST, SO, WM>(WM workflowManager, long statefulID, long statePathID)
			where ST : StateTransition<U>
			where SO : IStateful<U, ST>
			where WM : IWorkflowManager<U, ST, SO>
		{
			if (workflowManager == null) throw new ArgumentNullException(nameof(workflowManager));

			var stateful = await workflowManager.GetStatefulObjectAsync(statefulID);

			var statePath = await workflowManager.GetStatePathAsync(statePathID);

			return new StatefulObjectExecutionModel<U, ST, SO>(stateful, statePath, workflowManager);
		}

		/// <summary>
		/// Get the ID of the currently logged-in user or null if the user is anonymous.
		/// </summary>
		protected long? GetCurrentUserID()
		{
			if (this.User == null) return null;

			var userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier);

			if (userIdClaim == null) return null;

			if (long.TryParse(userIdClaim.Value, out long userID))
			{
				return userID;
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}
