﻿using System;
using System.Threading.Tasks;
using Applified.Core.ServiceContracts;
using Microsoft.Practices.Unity;

namespace Applified.Utilities.ApplifiedAdmin.Commands
{
    class DisableFeatureCommand : AppContextCommandBase
    {
        public DisableFeatureCommand(Options options, IUnityContainer container) 
            : base(options, container)
        {
        }

        public override async Task AppContextInvoke(IUnityContainer scope)
        {
            var featureService = scope.Resolve<IFeatureService>();

            var feature = await featureService.FindFeature(Options.TargetFeature);
            if (feature == null)
            {
                Console.WriteLine("Feature not found");
                return;
            }

            await featureService.DeleteApplicationFeatureAsync(feature.Id);

            Console.WriteLine("Feature disabled");
        }
    }
}
