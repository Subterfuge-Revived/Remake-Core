using System;
using System.Threading.Tasks;
using Grpc.Core;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {
        public override Task<RoomDataResponse> GetRoomData(RoomDataRequest request, ServerCallContext context)
        {
            return base.GetRoomData(request, context);
        }

        public override Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            return base.CreateNewRoom(request, context);
        }

        public override Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            return base.JoinRoom(request, context);
        }

        public override Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            return base.LeaveRoom(request, context);
        }

        public override Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            return base.StartGameEarly(request, context);
        }

        public override Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            return base.GetGameRoomEvents(request, context);
        }

        public override Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            return base.SubmitGameEvent(request, context);
        }

        public override Task<UpdateGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            return base.UpdateGameEvent(request, context);
        }

        public override Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            return base.DeleteGameEvent(request, context);
        }

        public override Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, ServerCallContext context)
        {
            return base.CreateMessageGroup(request, context);
        }

        public override Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            return base.SendMessage(request, context);
        }

        public override Task<GetMessageGroupsResponse> GetMessageGroups(GetMessageGroupsRequest request, ServerCallContext context)
        {
            return base.GetMessageGroups(request, context);
        }

        public override Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, ServerCallContext context)
        {
            return base.BlockPlayer(request, context);
        }

        public override Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            return base.UnblockPlayer(request, context);
        }

        public override Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            return base.ViewBlockedPlayers(request, context);
        }

        public override Task<AuthorizationResponse> Login(AuthorizationRequest request, ServerCallContext context)
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