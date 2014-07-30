﻿using System;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using Applified.Common.OwinDependencyInjection;
using Applified.Core.Extensibility.Contracts;
using Applified.Core.ServiceContracts;
using Microsoft.Practices.Unity;

namespace Applified.Core.Handlers
{
    class FeatureSynchronizationHandler : IApplicationEventHandler
    {
        public Task OnStartup(IUnityContainer container, IDependencyScope scope)
        {
            var featureService = scope.Resolve<IFeatureService>();
            return featureService.SynchronizeIntegratedFeaturesWithDatabaseAsync(
                AppDomain.CurrentDomain.BaseDirectory);
        }

        public void OnShutdown() { }
    }
}
