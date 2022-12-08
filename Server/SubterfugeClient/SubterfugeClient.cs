using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SubterfugeClient.Authorization;
using SubterfugeRemakeService;
using SubterfugeServerConsole;

namespace SubterfugeClient
{
    public class SubterfugeClient : subterfugeService.subterfugeServiceClient
    {
        public static async Task Main(String[] args)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:44397/RegisterAccount");
            Debug.Print(response.ToString());
        }
        
        public SubterfugeClient(string host, string port) : base(new Channel($"{host}:{port}", ChannelCredentials.Insecure).Intercept(new LoggerInterceptor()).Intercept((new JwtClientInterceptor())))
        {
            Auth auth = new Auth();
        }

        public override AuthorizationResponse LoginWithToken(AuthorizedTokenRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AuthorizationResponse response =  base.LoginWithToken(request, headers, deadline, cancellationToken);
            Auth.Login(response.Token);
            return response;
        }

        public override AuthorizationResponse LoginWithToken(AuthorizedTokenRequest request, CallOptions options)
        {
            AuthorizationResponse response = base.LoginWithToken(request, options);
            Auth.Login(response.Token);
            return response;
        }

        public override AsyncUnaryCall<AuthorizationResponse> LoginWithTokenAsync(AuthorizedTokenRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AsyncUnaryCall<AuthorizationResponse> response = base.LoginWithTokenAsync(request, headers, deadline, cancellationToken);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }

        public override AsyncUnaryCall<AuthorizationResponse> LoginWithTokenAsync(AuthorizedTokenRequest request, CallOptions options)
        {
            
            AsyncUnaryCall<AuthorizationResponse> response = base.LoginWithTokenAsync(request, options);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }

        public override AuthorizationResponse Login(AuthorizationRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AuthorizationResponse response =  base.Login(request, headers, deadline, cancellationToken);
            Auth.Login(response.Token);
            return response;
        }

        public override AuthorizationResponse Login(AuthorizationRequest request, CallOptions options)
        {
            AuthorizationResponse response =  base.Login(request, options);
            Auth.Login(response.Token);
            return response;
        }

        public override AsyncUnaryCall<AuthorizationResponse> LoginAsync(AuthorizationRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AsyncUnaryCall<AuthorizationResponse> response = base.LoginAsync(request, headers, deadline, cancellationToken);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }

        public override AsyncUnaryCall<AuthorizationResponse> LoginAsync(AuthorizationRequest request, CallOptions options)
        {
            AsyncUnaryCall<AuthorizationResponse> response = base.LoginAsync(request, options);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }

        public override AccountRegistrationResponse RegisterAccount(AccountRegistrationRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            AccountRegistrationResponse response =  base.RegisterAccount(request, headers, deadline, cancellationToken);
            Auth.Login(response.Token);
            return response;
        }

        public override AccountRegistrationResponse RegisterAccount(AccountRegistrationRequest request, CallOptions options)
        {
            AccountRegistrationResponse response =  base.RegisterAccount(request, options);
            Auth.Login(response.Token);
            return response;
        }

        public override AsyncUnaryCall<AccountRegistrationResponse> RegisterAccountAsync(AccountRegistrationRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            AsyncUnaryCall<AccountRegistrationResponse> response =  base.RegisterAccountAsync(request, headers, deadline, cancellationToken);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }

        public override AsyncUnaryCall<AccountRegistrationResponse> RegisterAccountAsync(AccountRegistrationRequest request, CallOptions options)
        {
            AsyncUnaryCall<AccountRegistrationResponse> response =  base.RegisterAccountAsync(request, options);
            response.ResponseAsync.ContinueWith((authResponse) => { Auth.Login(authResponse.Result.Token); });
            return response;
        }
    }
}