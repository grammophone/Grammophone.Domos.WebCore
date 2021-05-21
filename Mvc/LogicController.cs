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
using Grammophone.Domos.WebCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
		#region Auxilliary classes

		/// <summary>
		/// An <see cref="IDisposable"/> value-type for turning off <see cref="LogicSession"/>'s <see cref="LogicSession{U, D}.IsLazyLoadingEnabled"/>
		/// until <see cref="Dispose"/> method restores it.
		/// </summary>
		protected struct DisabledLazyLoadScope : IDisposable
		{
			private readonly LogicController<U, D, S> controller;

			private readonly bool originalLazyLoadValue;

			/// <summary>
			/// Create.
			/// </summary>
			/// <param name="controller">The logic controller.</param>
			public DisabledLazyLoadScope(LogicController<U, D, S> controller)
			{
				if (controller == null) throw new ArgumentNullException(nameof(controller));

				this.controller = controller;

				originalLazyLoadValue = controller.LogicSession.IsLazyLoadingEnabled;

				controller.LogicSession.IsLazyLoadingEnabled = false;
			}

			/// <summary>
			/// Restores the previous setting of <see cref="LogicSession{U, D}.IsLazyLoadingEnabled"/> of <see cref="LogicSession"/>.
			/// </summary>
			public void Dispose() => controller.LogicSession.IsLazyLoadingEnabled = originalLazyLoadValue;
		}

		private class MetadataObjectModelValidator : ObjectModelValidator
		{
			private readonly MvcOptions _mvcOptions;

			public MetadataObjectModelValidator(IModelMetadataProvider modelMetadataProvider, IList<IModelValidatorProvider> validatorProviders, MvcOptions mvcOptions)
				: base(modelMetadataProvider, validatorProviders)
			{
				_mvcOptions = mvcOptions;
			}

			public override ValidationVisitor GetValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider, ValidatorCache validatorCache, IModelMetadataProvider metadataProvider, ValidationStateDictionary validationState)
			{
				var visitor = new ValidationVisitor(
								actionContext,
								validatorProvider,
								validatorCache,
								metadataProvider,
								validationState)
				{
					MaxValidationDepth = _mvcOptions.MaxValidationDepth,
					ValidateComplexTypesIfChildValidationFails = _mvcOptions.ValidateComplexTypesIfChildValidationFails,
				};

				return visitor;
			}
		}

		/// <summary>
		/// Installs a <see cref="IModelMetadataProvider"/> able to handle a model derived from <see cref="ActionExecutionModel"/>
		/// in addition to the the behavior of the pre-existing <see cref="IModelMetadataProvider"/>
		/// until <see cref="Dispose"/> is invoked, when the pre-existing provider is restored.
		/// </summary>
		protected class ActionExecutionModelMetadataProviderScope : IModelMetadataProvider, IDisposable
		{
			#region Private fields

			private readonly ControllerBase controller;

			private readonly ActionExecutionModel actionExecutionModel;

			private readonly IModelMetadataProvider originalModelMetadataProvider;

			private readonly IObjectModelValidator originalObjectModelValidator;

			#endregion

			#region Construction

			internal ActionExecutionModelMetadataProviderScope(
				LogicController<U, D, S> controller,
				ActionExecutionModel actionExecutionModel)
			{
				this.controller = controller;
				this.actionExecutionModel = actionExecutionModel;

				originalModelMetadataProvider = controller.MetadataProvider;
				originalObjectModelValidator = controller.ObjectValidator;

				controller.MetadataProvider = this;

				var mvcOptions = controller.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>().Value;

				controller.ObjectValidator = new MetadataObjectModelValidator(
					this,
					mvcOptions.ModelValidatorProviders,
					mvcOptions);
			}

			#endregion

			#region Public methods

			/// <summary>
			/// Restores the original metadata provider of the controller.
			/// </summary>
			public void Dispose()
			{
				controller.MetadataProvider = originalModelMetadataProvider;
				controller.ObjectValidator = originalObjectModelValidator;
			}

			/// <inheritdoc/>
			public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
			{
				if (actionExecutionModel != null && typeof(ActionExecutionModel).IsAssignableFrom(modelType))
				{
					var compositeMetadataDetailsProvider = controller.HttpContext.RequestServices.GetService<ICompositeMetadataDetailsProvider>();

					var metadata = ActionExecutionModelMetadataFactory.GetMetadata(controller.MetadataProvider, compositeMetadataDetailsProvider, actionExecutionModel);

					return metadata.GetMetadataForProperties(modelType);
				}
				else
				{
					return originalModelMetadataProvider.GetMetadataForProperties(modelType);
				}
			}

			/// <inheritdoc/>
			public ModelMetadata GetMetadataForType(Type modelType)
			{
				if (modelType == null) throw new ArgumentNullException(nameof(modelType));

				if (typeof(ActionExecutionModel).IsAssignableFrom(modelType))
				{
					var compositeMetadataDetailsProvider = controller.HttpContext.RequestServices.GetService<ICompositeMetadataDetailsProvider>();

					return ActionExecutionModelMetadataFactory.GetMetadata(controller.MetadataProvider, compositeMetadataDetailsProvider, actionExecutionModel);
				}
				else
				{
					return originalModelMetadataProvider.GetMetadataForType(modelType);
				}
			}

			#endregion
		}

		#endregion

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
		/// The Domos session associated with the controller.
		/// </summary>
		protected internal S LogicSession { get; }

		#endregion

		#region Public methods

		/// <inheritdoc/>
		[NonAction]
		public override bool TryValidateModel(object model)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return base.TryValidateModel(model);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override bool TryValidateModel(object model, string prefix)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return base.TryValidateModel(model, prefix);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync(object model, Type modelType, string prefix)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, modelType, prefix);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider)
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix, valueProvider);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider, Func<ModelMetadata, bool> propertyFilter)
			where TModel : class
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix, valueProvider, propertyFilter);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions)
			where TModel : class
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix, includeExpressions);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, Func<ModelMetadata, bool> propertyFilter)
			where TModel : class
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix, propertyFilter);
			}
		}

		/// <inheritdoc/>
		[NonAction]
		public new async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, IValueProvider valueProvider, params Expression<Func<TModel, object>>[] includeExpressions)
			where TModel : class
		{
			using (GetDisabledLazyLoadScope())
			using (GetActionExecutionModelMetadataProviderScope(model as ActionExecutionModel))
			{
				return await base.TryUpdateModelAsync(model, prefix, valueProvider, includeExpressions);
			}
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Returns an <see cref="IDisposable"/> handle to turn off the <see cref="LogicSession"/>'s <see cref="LogicSession{U, D}.IsLazyLoadingEnabled"/>
		/// until <see cref="IDisposable.Dispose"/> method is called and the setting is restored. Suited for 'using' statement.
		/// </summary>
		protected DisabledLazyLoadScope GetDisabledLazyLoadScope() => new(this);

		/// <summary>
		/// Returns an <see cref="IDisposable"/> <see cref="IModelMetadataProvider"/> able to handle a model derived from <see cref="ActionExecutionModel"/>
		/// in addition to the the behavior of the pre-existing <see cref="IModelMetadataProvider"/>
		/// until <see cref="IDisposable.Dispose"/> is invoked, when the pre-existing provider is restored.
		/// </summary>
		/// <param name="model">The instance derived from <see cref="ActionExecutionModel"/> to be able to get metadata for.</param>
		protected ActionExecutionModelMetadataProviderScope GetActionExecutionModelMetadataProviderScope(ActionExecutionModel model) => new(this, model);

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

			string executionModelPrefix;

			if (!String.IsNullOrEmpty(prefix))
				executionModelPrefix = $"{prefix}.{nameof(StatefulObjectExecutionModel<SO>.ExecutionModel)}";
			else
				executionModelPrefix = nameof(StatefulObjectExecutionModel<SO>.ExecutionModel);

			var valueProvider = await CompositeValueProvider.CreateAsync(this.ControllerContext);

			var executionModel = new StatePathExecutionModel<U, ST, SO>(workflowManager, valueProvider, executionModelPrefix);

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
