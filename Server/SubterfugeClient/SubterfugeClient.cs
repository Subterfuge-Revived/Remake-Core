using System;
using System.Threading.Tasks;
using Grpc.Core;
using SubterfugeRemakeService;

namespace SubterfugeClient
{
    class SubterfugeClient
    {
        public async static Task Main(string[] args)
        {
            var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            var client =  new subterfugeService.subterfugeServiceClient(channel);
            try
            {
                AccountRegistrationResponse registerResponse = client.RegisterAccount(new AccountRegistrationRequest()
                    {Email = "test@test.com", Password = "Test", Username = "Test"});
                Console.WriteLine($"Created user: {registerResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"User already created. Login instead.");   
            }

            try
            {
                AuthorizationResponse loginResponse = await client.LoginAsync(new AuthorizationRequest() { Password = "Test", Username = "Test" });
                Console.WriteLine($"Logged in user: {loginResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Unable to login...");   
            }

            try
            {
                AuthorizationResponse errorLoginResponse = await client.LoginAsync(new AuthorizationRequest()
                    {Password = "asdfasdf", Username = "Test"});
            }
            catch (RpcException e)
            {
                Console.WriteLine($"Invalid user credentials.");   
            }
        }
    }
}