using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace SubterfugeServerConsole
{
    public class LoggerInterceptor : Interceptor
    {
        private string[] blacklist =
        {
            "Login",
            "LoginWithToken",
            "RegisterAccount",
        };
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Name;
            logMessage(calledMethod, request);

            TResponse response = null;
            try
            {
                response = base.BlockingUnaryCall(request, context, continuation);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{calledMethod}: {e.Message}");
                Console.WriteLine($"{calledMethod}: {e.StackTrace}");
            }
            logMessage(calledMethod, response);
            return response;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Split('/').Last();
            logMessage(calledMethod, request);
            TResponse response = null;
            try
            {
                response = await base.UnaryServerHandler(request, context, continuation);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{calledMethod}: {e.Message}");
                Console.WriteLine($"{calledMethod}: {e.StackTrace}");
            }
            logMessage(calledMethod, response);
            return response;
        }

        public void logMessage<TRequest>(String calledMethod, TRequest request)
        {
            if(blacklist.Contains(calledMethod)) {
                Console.WriteLine($"{calledMethod}: {request}");
            }
        }
    }
}