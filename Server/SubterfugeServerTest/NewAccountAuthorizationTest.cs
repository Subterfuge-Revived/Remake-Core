using System;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole;
using SubterfugeServerConsole.Connections;

namespace Tests
{
    public class NewAccountAuthorizationTest
    {
        SubterfugeClient.SubterfugeClient client;
        // private const String Hostname = "server"; // For docker
        private const String Hostname = "localhost"; // For local
        private const int Port = 5000;
            
        // private const String dbHost = "db"; // For docker
        private const String dbHost = "localhost"; // For local
        private const int dbPort = 6379;
        
        [SetUp]
        public void Setup()
        {
            RedisConnector db = new RedisConnector(dbHost, dbPort.ToString(), true);
            client = new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());
            
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
         public void UserCanLoginAfterRegisteringNewAccount()
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
         public void UserCannotRegisterWithTheSameUsername()
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
         public void UserCannotRegisterWithTheSameEmail()
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

             request.Username = "asdfasdf";
             
             var exception = Assert.Throws<RpcException>(() => client.RegisterAccount(request));
             Assert.That(exception.Status.StatusCode, Is.EqualTo(StatusCode.AlreadyExists));
             Assert.That(exception.Status.Detail, Is.EqualTo("Username already exists."));
         }
    }
}