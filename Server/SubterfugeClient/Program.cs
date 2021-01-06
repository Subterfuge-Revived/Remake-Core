using System;
using System.Threading.Tasks;
using Grpc.Core;
using SubterfugeRemakeService;

namespace SubterfugeClient
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            string FriendId = "";
            SubterfugeClient client = new SubterfugeClient("localhost", "5000");
            
            try
            {
                await client.AuthorizedHealthCheckAsync(new AuthorizedHealthCheckRequest());
                Console.WriteLine($"Authorized request successful");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }

            try
            {
                AccountRegistrationResponse registerResponse = client.RegisterAccount(new AccountRegistrationRequest()
                    {Email = "test@test.com", Password = "Test", Username = "Test"});
                
                Console.WriteLine($"Created user: {registerResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot register. {e.Status.Detail}");   
            }
            
            try
            {
                AccountRegistrationResponse registerResponse = client.RegisterAccount(new AccountRegistrationRequest()
                    {Email = "test@test.com", Password = "Test1", Username = "Test1"});
                Console.WriteLine($"Created user: {registerResponse.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot register. {e.Status.Detail}");   
            }
            
            try
            {
                AuthorizationResponse response = await client.LoginAsync(new AuthorizationRequest() { Password = "Test", Username = "asdfasdfasdf" });
                Console.WriteLine($"Success, login as Test:{response.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot login. {e.Status.Detail}");
            }
            
            try
            {
                AuthorizationResponse response = await client.LoginAsync(new AuthorizationRequest() { Password = "Test1", Username = "Test1" });
                Console.WriteLine($"Success, login as Test1: {response.User.Id}");
                FriendId = response.User.Id;
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot login. {e.Status.Detail}");
            }

            try
            {
                AuthorizationResponse response = await client.LoginAsync(new AuthorizationRequest() { Password = "Test", Username = "Test" });
                Console.WriteLine($"Success, login as Test: {response.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot login. {e.Status.Detail}");
            }

            try
            {
                await client.AuthorizedHealthCheckAsync(new AuthorizedHealthCheckRequest());
                Console.WriteLine($"Authorized request successful");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"Cannot authorize request. {e.Status.Detail}");
            }

            try
            {
                SendFriendRequestResponse response = await client.SendFriendRequestAsync(new SendFriendRequestRequest()
                {
                    FriendId = FriendId
                });
                Console.WriteLine($"Freind request sent to {FriendId}.");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"Cannot send friend request. {e.Status.Detail}");
            }
            
            try
            {
                AuthorizationResponse response = await client.LoginAsync(new AuthorizationRequest()
                {
                    Username = "Test1",
                    Password = "Test1",
                });
                Console.WriteLine($"Logged in as Test1: {response.User.Id}");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"Cannot login as Test1. {e.Status.Detail}");
            }

            string friendToAdd = "";
            try
            {
                ViewFriendRequestsResponse response = await client.ViewFriendRequestsAsync(new ViewFriendRequestsRequest());
                Console.WriteLine($"Your incoming friend requests are: {response.IncomingFriends}");
                friendToAdd = response.IncomingFriends[0].Id;
            }
            catch (RpcException e)
            {
                Console.WriteLine($"Cannot get incoming friends requests. {e.Status.Detail}");
            }
            
            try
            {
                await client.AcceptFriendRequestAsync(new AcceptFriendRequestRequest()
                {
                    FriendId = friendToAdd
                });
                Console.WriteLine($"Added Friend.");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }
            
            try
            {
                ViewFriendsResponse response = await client.ViewFriendsAsync(new ViewFriendsRequest());
                Console.WriteLine($"Your Friends: {response.Friends}");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }
            
            try
            {
                AuthorizationResponse response = await client.LoginAsync(new AuthorizationRequest() { Password = "Test", Username = "Test" });
                Console.WriteLine($"Success, login as Test: {response.User.Id}");
            } catch (RpcException e)
            {
                Console.WriteLine($"Cannot login. {e.Status.Detail}");
            }

            string FriendToRemove = "";
            try
            {
                ViewFriendsResponse response = await client.ViewFriendsAsync(new ViewFriendsRequest());
                FriendToRemove = response.Friends[0].Id;
                Console.WriteLine($"Your Friends: {response.Friends}");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }
            
            try
            {
                RemoveFriendResponse response = await client.RemoveFriendAsync(new RemoveFriendRequest()
                {
                    FriendId = FriendToRemove
                });
                Console.WriteLine($"Removed friend");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }
            
            try
            {
                ViewFriendsResponse response = await client.ViewFriendsAsync(new ViewFriendsRequest());
                Console.WriteLine($"Your Friends: {response.Friends}");
            }
            catch (RpcException e)
            {
                Console.WriteLine($"{e.Status.Detail}");
            }
            
        }
    }
}