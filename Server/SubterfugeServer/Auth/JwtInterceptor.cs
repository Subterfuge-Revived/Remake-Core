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

namespace SubterfugeServerConsole
{
    public class JwtInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Split('/').Last();
            bool isValidRequest = false;
            
            // Do not authorize these methods:
            string[] whitelist =
            {
                "Login",
                "RegisterAccount",
                "HealthCheck",
            };
            
            if(whitelist.Contains(calledMethod))
            {
                isValidRequest = true;
            }
            else
            {
                // The endpoint requires authorization before performing.
                // Get JWT header
                Metadata.Entry entry = context.RequestHeaders.Get("authorization");
                string token;
                if (entry.Value != null)
                {
                    token = entry.Value;
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Unauthorized."));
                }


                string uuid;
                if (JwtManager.ValidateToken(token, out uuid))
                {
                    // Validate user exists.
                    RedisUserModel user = await RedisUserModel.getUser(Guid.Parse(uuid));
                    if (user != null)
                    {
                        isValidRequest = true;
                        context.UserState["user"] = user;
                    }
                }
            }

            if (isValidRequest)
            {
                // Do regular request and return
                var response = await base.UnaryServerHandler(request, context, continuation);
                return response;
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }
    }
}