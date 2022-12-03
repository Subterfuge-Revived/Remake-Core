using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole
{
    public class JwtInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Split('/').Last();
            context.UserState["user"] = null;
            
            // Do not authorize these methods:
            string[] whitelist =
            {
                "Login",
                "LoginWithToken",
                "RegisterAccount",
                "HealthCheck",
            };
            
            if(!whitelist.Contains(calledMethod)) {
                // The endpoint requires authorization before performing.
                // Get JWT header
                Metadata.Entry entry = context.RequestHeaders.Get("authorization");
                if (entry?.Value != null)
                {
                    if (JwtManager.ValidateToken(entry.Value, out var uuid))
                    {
                        // Validate user exists.
                        DbUserModel dbUserModel = await DbUserModel.GetUserFromGuid(uuid);
                        if (dbUserModel != null)
                        {
                            context.UserState["user"] = dbUserModel;
                        }
                    }
                }
            }
            
            // Do regular request and return
            var response = await base.UnaryServerHandler(request, context, continuation);
            return response;
        }
    }
}