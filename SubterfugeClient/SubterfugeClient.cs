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
            AuthorizationResponse response = client.Login(new AuthorizationRequest() { Password = "Test", Username = "Joe" });
            Console.WriteLine(response.User.Username);
        }
    }
}