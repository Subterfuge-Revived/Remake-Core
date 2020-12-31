using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SubterfugeClient.Authorization;
using SubterfugeRemakeService;

namespace SubterfugeClient
{
    class SubterfugeClient
    {
        public async static Task Main(string[] args)
        {
            Auth auth = new Auth();
            
            var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            var invoker = channel.Intercept(new JwtClientInterceptor());
            var client =  new subterfugeService.subterfugeServiceClient(invoker);

            client.HealthCheck(new HealthCheckRequest());
            
            try
            {
                AccountRegistrationResponse registerResponse = client.RegisterAccount(new AccountRegistrationRequest()
                    {Email = "test@test.com", Password = "Test", Username = "Test"});
                
                Auth.Login(registerResponse.Token);
                Console.WriteLine($"Created user: {registerResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"User already created. Login instead.");   
            }

            try
            {
                AuthorizationResponse loginResponse = await client.LoginAsync(new AuthorizationRequest() { Password = "Test", Username = "Test" });
                Auth.Login(loginResponse.Token);
                Console.WriteLine($"Logged in user: {loginResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Unable to login...");   
            }

            client.AuthorizedHealthCheck(new AuthorizedHealthCheckRequest());
        }
    }
}