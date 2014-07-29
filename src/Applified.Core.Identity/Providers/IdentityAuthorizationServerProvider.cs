﻿using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Applified.Common.OwinDependencyInjection;
using Applified.Core.DataAccess.Contracts;
using Applified.Core.Entities.Identity;
using Applified.Core.Identity.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.OAuth;

namespace Applified.Core.Identity.Providers
{

    public class IdentityAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(
            OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;


            if (context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                var scope = context.OwinContext.Environment.GetRequestContainer() as IServiceProvider;
                var userManager = scope.Resolve<UserManager>();
                var clients = scope.Resolve<IRepository<OAuthClient>>();

                try
                {
                    var client = await clients.Query()
                        .FirstOrDefaultAsync(clientEntity => clientEntity.Id == clientId);

                    if (client != null && client.Active)
                    {
                        if (string.IsNullOrEmpty(client.Secret) ||
                            userManager.PasswordHasher.VerifyHashedPassword(
                                client.Secret, clientSecret) == PasswordVerificationResult.Success)
                        {
                            context.OwinContext.Set("oauth:client", client);
                            context.Validated(clientId);

                            return;
                        }
                    }

                    context.SetError("invalid_client", "Client credentials are invalid.");
                    context.Rejected();

                    return;
                }
                catch
                {
                    context.SetError("server_error");
                    context.Rejected();

                    return;
                }
            }

            context.SetError(
                "invalid_client",
                "Client credentials could not be retrieved through the Authorization header.");

            context.Rejected();
        }


        public override async Task GrantResourceOwnerCredentials(
            OAuthGrantResourceOwnerCredentialsContext context)
        {
            var client = context.OwinContext.Get<OAuthClient>("oauth:client");
            if (client.AllowedGrant == OAuthGrant.ResourceOwner)
            {
                var scope = context.OwinContext.Environment.GetRequestContainer() as IServiceProvider;
                var userManager = scope.Resolve<UserManager>();

                UserAccount user;
                try
                {
                    user = await userManager.FindAsync(context.UserName, context.Password);
                }
                catch
                {
                    context.SetError("server_error");
                    context.Rejected();
                    return;
                }


                if (user != null)
                {
                    try
                    {
                        var identity = await userManager.CreateIdentityAsync(
                            user,
                            DefaultAuthenticationTypes.ExternalBearer);

                        context.Validated(identity);
                    }
                    catch
                    {
                        // The ClaimsIdentity could not be created by the UserManager.
                        context.SetError("server_error");
                        context.Rejected();
                    }
                }
                else
                {
                    // The resource owner credentials are invalid or resource owner does not exist.
                    context.SetError(
                        "access_denied",
                        "The resource owner credentials are invalid or resource owner does not exist.");


                    context.Rejected();
                }
            }
            else
            {
                // Client is not allowed for the 'Resource Owner Password Credentials Grant'.
                context.SetError(
                    "invalid_grant",
                    "Client is not allowed for the 'Resource Owner Password Credentials Grant'");


                context.Rejected();
            }
        }
    }
}