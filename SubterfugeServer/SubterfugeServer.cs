using System;
using System.Threading.Tasks;
using Grpc.Core;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {
        public override Task<AuthorizationResponse> login(AuthorizationRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Recieved Login Request");
            return Task.FromResult(new AuthorizationResponse {User = new User {Id = 1, Username = "Test"}});
        }

        public override Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request,
            ServerCallContext context)
        {
            Console.WriteLine($"Recieved Register Request");
            return Task.FromResult(new AccountRegistrationResponse {User = new User {Id = 1, Username = "Test"}});
        }
    }
}