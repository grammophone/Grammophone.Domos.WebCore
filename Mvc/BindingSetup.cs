using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Grammophone.Domos.Domain;

namespace Grammophone.Domos.Web.Mvc
{
	/// <summary>
	/// Encapsulates propert registering of <see cref="Mvc.ActionExecutionModelBinderProvider"/>,
	/// <see cref="Mvc.EntityModelBinderProvider"/>, <see cref="Mvc.DomosValidatorProvider"/>,
	/// <see cref="Mvc.CustomValidatorProvider"/>, <see cref="Mvc.DomosMetadataProvider"/>.
	/// </summary>
	public static class BindingSetup
	{
		#region Private fields

		private static int isRegistered;

		#endregion

		#region Public properties

		/// <summary>
		/// Binds an instance derived from <see cref="Models.ActionExecutionModel"/>
		/// using the <see cref="ActionExecutionModelBinder"/>.
		/// </summary>
		public static ActionExecutionModelBinderProvider ActionExecutionModelBinderProvider { get; }

		/// <summary>
		/// Selects the <see cref="KeyedEntityModelBinder{K}"/> for binding to
		/// entities implementing the <see cref="IEntityWithID{K}"/> interface.
		/// Hides <see cref="IUserTrackingEntity"/>
		/// properties from binding.
		/// </summary>
		public static EntityModelBinderProvider EntityModelBinderProvider { get; }

		/// <summary>
		/// If a model derives from <see cref="Models.ActionExecutionModel"/>, augments its validated properties
		/// using its <see cref="Models.ActionExecutionModel.GetParameterSpecifications()"/> method,
		/// otherwise it behaves like a <see cref="DataAnnotationsModelValidatorProvider"/>.
		/// </summary>
		public static DomosValidatorProvider DomosValidatorProvider { get; }

		/// <summary>
		/// A provider which allows registration of model types
		/// to be handled by <see cref="CustomValidator"/>.
		/// </summary>
		public static CustomValidatorProvider CustomValidatorProvider { get; }

		/// <summary>
		/// Metadata provider supporting standard entities with data annotations and
		/// extended parameters for entities derived from <see cref="Models.ActionExecutionModel"/>.
		/// </summary>
		public static DomosMetadataProvider DomosMetadataProvider { get; }

		#endregion

		#region Public methods

		/// <summary>
		/// Static initialization.
		/// </summary>
		static BindingSetup()
		{
			ActionExecutionModelBinderProvider = new ActionExecutionModelBinderProvider();
			EntityModelBinderProvider = new EntityModelBinderProvider();
			DomosValidatorProvider = new DomosValidatorProvider();
			CustomValidatorProvider = new CustomValidatorProvider();
			DomosMetadataProvider = new DomosMetadataProvider();
		}

		/// <summary>
		/// Register the the providers under the MVC system.
		/// </summary>
		public static void Register()
		{
			int wasRegistered = Interlocked.CompareExchange(ref isRegistered, 1, 0);

			if (wasRegistered == 0)
			{
				ModelBinderProviders.BinderProviders.Add(ActionExecutionModelBinderProvider);
				ModelBinderProviders.BinderProviders.Add(EntityModelBinderProvider);

				ModelMetadataProviders.Current = DomosMetadataProvider;

				var dataAnnotationsModelValidatorProvider =
					ModelValidatorProviders.Providers.OfType<DataAnnotationsModelValidatorProvider>().FirstOrDefault();

				if (dataAnnotationsModelValidatorProvider != null)
					ModelValidatorProviders.Providers.Remove(dataAnnotationsModelValidatorProvider);

				ModelValidatorProviders.Providers.Add(DomosValidatorProvider);
				ModelValidatorProviders.Providers.Add(CustomValidatorProvider);
			}
		}

		#endregion
	}
}
