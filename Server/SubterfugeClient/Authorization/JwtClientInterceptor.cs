using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SubterfugeClient.Authorization;

namespace SubterfugeClient
{
    public class JwtClientInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            // Authorize any method if the user is logged in.
            addJwtHeaders(ref context);
            return base.BlockingUnaryCall(request, context, continuation);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            // Authorize any method if the user is logged in.
            addJwtHeaders(ref context);
            return base.AsyncUnaryCall(request, context, continuation);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            // Authorize any method if the user is logged in.
            addJwtHeaders(ref context);
            return base.AsyncServerStreamingCall(request, context, continuation);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            // Authorize any method if the user is logged in.
            addJwtHeaders(ref context);
            return base.AsyncClientStreamingCall(context, continuation);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            // Authorize any method if the user is logged in.
            addJwtHeaders(ref context);
            return base.AsyncDuplexStreamingCall(context, continuation);
        }
        
        private void addJwtHeaders<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            if (Auth.isLoggedIn)
            {
                var headers = context.Options.Headers;

                // Call doesn't have a headers collection to add to.
                // Need to create a new context with headers for the call.
                if (headers == null)
                {
                    headers = new Metadata();
                    var options = context.Options.WithHeaders(headers);
                    context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
                }

                // Add caller metadata to call headers
                headers.Add("authorization", Auth.token);
            }
        }
    }
}