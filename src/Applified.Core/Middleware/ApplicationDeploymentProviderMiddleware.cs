#region Copyright (C) 2014 Applified.NET 
// Copyright (C) 2014 Applified.NET
// http://www.applified.net

// This file is part of Applified.NET.

// Applified.NET is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.

// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using Applified.Common.Exceptions;
using Applified.Common.OwinDependencyInjection;
using Applified.Core.ServiceContracts;
using Microsoft.Owin;
using Microsoft.Owin.Security.Provider;

namespace Applified.Core.Middleware
{
    class ApplicationDeploymentProviderMiddleware : OwinMiddleware
    {
        public ApplicationDeploymentProviderMiddleware(OwinMiddleware next) 
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            var scope = context.GetRequestContainer();
            var server = scope.Resolve<IServerEnvironment>();
            var application = scope.Resolve<ICurrentContext>();

            var activeDeploymentId = application.DeploymentId;

            if (!activeDeploymentId.HasValue)
            {
                throw new NoActiveDeploymentException(null);
            }

            var desiredPath = Path.Combine(server.DeploymentDirectory, activeDeploymentId.ToString());

            if (!Directory.Exists(desiredPath))
            {
                return DownloadDeployment(
                    scope,
                    server.DeploymentDirectory,
                    desiredPath,
                    activeDeploymentId.Value
                    ).ContinueWith(inner => Next.Invoke(context));
            }

            return Next.Invoke(context);
        }

        private async Task DownloadDeployment(IDependencyScope scope, string workingDirectory, string targetDirectory, Guid deploymentId)
        {
            var deploymentService = scope.Resolve<IDeploymentService>();

            var obj = await deploymentService
                .GetDeploymentPayloadInternalAsync(deploymentId)
                .ConfigureAwait(false);

            var workingName = Guid.NewGuid().ToString();
            var workingPath = Path.Combine(workingDirectory, workingName);

            File.WriteAllBytes(workingPath, obj.Data);
            ZipFile.ExtractToDirectory(workingPath, targetDirectory);
        }
    }
}
