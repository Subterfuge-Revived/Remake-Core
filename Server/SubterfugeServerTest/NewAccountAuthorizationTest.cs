using System;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class NewAccountAuthorizationTest
    {
        SubterfugeClient.SubterfugeClient client;
        
        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
        }

        [Test]
        public void AnyoneCanAccessHealthCheckEndpoint()
        {
            HealthCheckResponse response = client.HealthCheck(new HealthCheckRequest());
            Assert.IsTrue(response != null);
        }

        [Test]
         public void UserCanRegisterNewAccount()
         {
             String username = "Username";
             String password = "Password";
             
             AccountRegistrationRequest request = new AccountRegistrationRequest()
             {
                 Email = "Test@test.com",
                 Password = password,
                 Username = username,
             };
 
             AccountRegistrationResponse response = client.RegisterAccount(request);
 
             Assert.IsTrue(response.Token != null);
             Assert.IsTrue(response.User.Id != null);
             Assert.AreEqual(response.User.Username, username);
         }
         
         [Test]
         public void UserIsLoggedInAfterRegistering()
         {
             String username = "Username";
             String password = "Password";
             
             AccountRegistrationRequest request = new AccountRegistrationRequest()
             {
                 Email = "Test@test.com",
                 Password = password,
                 Username = username,
             };
 
             AccountRegistrationResponse response = client.RegisterAccount(request);
 
             Assert.IsTrue(response.Token != null);
             Assert.IsTrue(response.User.Id != null);
             Assert.AreEqual(response.User.Username, username);

             AuthorizedHealthCheckResponse AuthResponse = client.AuthorizedHealthCheck(new AuthorizedHealthCheckRequest());
             
             Assert.IsTrue(AuthResponse != null);
         }
        
         [Test]
         public void PlayerCanLoginAfterRegisteringNewAccount()
         {
             String username = "Username";
             String password = "Password";
    
             AccountRegistrationRequest request = new AccountRegistrationRequest()
             {
                 Email = "Test@test.com",
                 Password = password,
                 Username = username,
             };

             AccountRegistrationResponse response = client.RegisterAccount(request);

             String userId = response.User.Id;

             Assert.IsTrue(response.Token != null);
             Assert.IsTrue(response.User.Id != null);
             Assert.AreEqual(response.User.Username, username);
             
             AuthorizationRequest authRequest = new AuthorizationRequest()
             {
                 Password = password,
                 Username = username,
             };

             AuthorizationResponse authResponse = client.Login(authRequest);
             
             Assert.IsTrue(authResponse.Token != null);
             Assert.IsTrue(authResponse.User.Id == userId);
             Assert.AreEqual(authResponse.User.Username, username);
             
             AuthorizedHealthCheckResponse AuthResponse = client.AuthorizedHealthCheck(new AuthorizedHealthCheckRequest());
             
             Assert.IsTrue(AuthResponse != null);
         }

         [Test]
         public void PlayerCannotRegisterWithTheSameUsername()
         {
             String username = "Username";
             String password = "Password";
    
             AccountRegistrationRequest request = new AccountRegistrationRequest()
             {
                 Email = "Test@test.com",
                 Password = password,
                 Username = username,
             };

             AccountRegistrationResponse response = client.RegisterAccount(request);

             String userId = response.User.Id;

             Assert.IsTrue(response.Token != null);
             Assert.IsTrue(response.User.Id != null);
             Assert.IsTrue(response.User.Username == username);

             request.Email = "otherEmail@test.com";
             
             var exception = Assert.Throws<RpcException>(() => client.RegisterAccount(request));
             Assert.That(exception.Status.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
             Assert.That(exception.Status.Detail, Is.EqualTo("Username already exists."));
         }

         [Test]
         public void SuperUserAccountHasAdminClaims()
         {
             SuperUser superUser = RedisUserModel.CreateSuperUser().Result;
             
             // Fetch the user from the database.
             RedisUserModel user = RedisUserModel.GetUserFromGuid(superUser.userModel.UserModel.Id).Result;
             // Ensure the user has admin power
             Assert.IsTrue(user.HasClaim(UserClaim.Admin));
             Assert.IsTrue(user.HasClaim(UserClaim.Dev));
             Assert.IsTrue(user.HasClaim(UserClaim.EmailVerified));
         }
        
    }
}