using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace SubterfugeServerConsole
{
    public class LoggerInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Name;
            Console.WriteLine($"{calledMethod}: {request}");
            TResponse response = null;
            try
            {
                response = base.BlockingUnaryCall(request, context, continuation);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{calledMethod}: {e.Message}");
            }
            Console.WriteLine($"{calledMethod}: {response}");
            return response;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string calledMethod = context.Method.Split('/').Last();
            Console.WriteLine($"{calledMethod}: {request}");
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
            Console.WriteLine($"{calledMethod}: {response}");
            return response;
        }
    }
}