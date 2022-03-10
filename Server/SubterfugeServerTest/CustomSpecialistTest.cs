using System;
using System.Linq;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using Tests.AuthTestingHelper;
using ValueType = SubterfugeRemakeService.ValueType;

namespace Tests
{
    public class CustomSpecialistTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            MongoConnector.FlushCollections();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.loginToAccount("userOne");
        }
        
        /**
         * Tests the following rpcs:
         *       rpc SubmitCustomSpecialist(SubmitCustomSpecialistRequest) returns (SubmitCustomSpecialistResponse) {}
         *       rpc GetCustomSpecialists(GetCustomSpecialistsRequest) returns (GetCustomSpecialistsResponse) {}
         *       rpc GetPlayerCustomSpecialists(GetPlayerCustomSpecialistsRequest) returns (GetPlayerCustomSpecialistsResponse) {}
         *       rpc CreateSpecialistPackage(CreateSpecialistPackageRequest) returns (CreateSpecialistPackageResponse) {}
         *       rpc GetSpecialistPackages(GetSpecialistPackagesRequest) returns (GetSpecialistPackagesResponse) {}
         *       rpc GetPlayerSpecialistPackages(GetPlayerSpecialistPackagesRequest) returns (GetPlayerSpecialistPackagesResponse) {}
         */
        
        [Test]
        public void PlayerCanCreateACustomSpecialist()
        {
            SubmitCustomSpecialistRequest request = createSpecialistRequest("MyCustomSpecialist");

            SubmitCustomSpecialistResponse response = client.SubmitCustomSpecialist(request);
            Assert.IsTrue(response.Status.IsSuccess);
            Assert.NotNull(response.SpecialistConfigurationId);
        }
        
        [Test]
        public void PlayerCanViewCustomSpecialistAfterCreating()
        {
            String specName = "MyCustomSpecialist";
            SubmitCustomSpecialistResponse response = submitCustomSpecialist(specName);
            
            string specUuid = response.SpecialistConfigurationId;
            
            GetCustomSpecialistsResponse specResponse = client.GetCustomSpecialists(new GetCustomSpecialistsRequest());
            Assert.IsTrue(specResponse.CustomSpecialists.Count == 1);
            Assert.AreEqual(specUuid, specResponse.CustomSpecialists[0].Id);
            Assert.AreEqual(specName, specResponse.CustomSpecialists[0].SpecialistName);
            Assert.AreEqual(1, specResponse.CustomSpecialists[0].Priority);
            Assert.AreEqual(EffectModifier.Driller, specResponse.CustomSpecialists[0].SpecialistEffects[0].EffectModifier);
            // Ensure some of the specialist details are the same!
        }
        
        [Test]
        public void CanGetAllSpecialistsCreatedByAPlayer()
        {
            String specName = "MyCustomSpecialist";
            submitCustomSpecialist(specName);
            
            String secondSpecName = "MyCustomSpecialist2";
            submitCustomSpecialist(secondSpecName);
            
            
            GetPlayerCustomSpecialistsResponse playerSpecsResponse = client.GetPlayerCustomSpecialists(new GetPlayerCustomSpecialistsRequest()
            {
                PlayerId = authHelper.getAccountId("userOne")
            });
            Assert.IsTrue(playerSpecsResponse.PlayerSpecialists.Count == 2);
            Assert.IsTrue(playerSpecsResponse.PlayerSpecialists.Count(it => it.SpecialistName == specName) == 1);
            Assert.IsTrue(playerSpecsResponse.PlayerSpecialists.Count(it => it.SpecialistName == secondSpecName) == 1);
        }
        
        [Test]
        public void CanViewSpecialistsCreatedByAnyPlayer()
        {
            String userOneSpecName = "UserOneSpecialists";
            submitCustomSpecialist(userOneSpecName);

            authHelper.loginToAccount("userTwo");

            String userTwoSpecialist = "MyCustomSpecialist";
            submitCustomSpecialist(userTwoSpecialist);

            GetCustomSpecialistsResponse specResponse = client.GetCustomSpecialists(new GetCustomSpecialistsRequest());
            Assert.IsTrue(specResponse.CustomSpecialists.Count == 2);
            Assert.IsTrue(specResponse.CustomSpecialists.Count(it => it.SpecialistName == userOneSpecName) == 1);
            Assert.IsTrue(specResponse.CustomSpecialists.Count(it => it.SpecialistName == userTwoSpecialist) == 1);
            Assert.IsTrue(specResponse.CustomSpecialists.Count(it => it.Creator.Username == "userOne") == 1);
            Assert.IsTrue(specResponse.CustomSpecialists.Count(it => it.Creator.Username == "userTwo") == 1);
        }
        
        [Test]
        public void CanCreateASpecialistPackage()
        {
            SubmitCustomSpecialistResponse response = submitCustomSpecialist("MyCustomSpecialist");

            CreateSpecialistPackageResponse packageResponse = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "myPackage",
                        SpecialistIds = {response.SpecialistConfigurationId},
                    }
                });
            
            Assert.IsTrue(packageResponse.Status.IsSuccess);
            Assert.NotNull(packageResponse.SpecialistPackageId);
        }
        
        [Test]
        public void CanViewAPlayersSpecialistPackages()
        {
            SubmitCustomSpecialistResponse response = submitCustomSpecialist("MyCustomSpecialist");

            CreateSpecialistPackageResponse packageResponse = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "myPackage",
                        SpecialistIds = {response.SpecialistConfigurationId},
                    }
                });
            
            Assert.IsTrue(packageResponse.Status.IsSuccess);
            Assert.NotNull(packageResponse.SpecialistPackageId);
            
            GetPlayerSpecialistPackagesResponse playerPackages = client.GetPlayerSpecialistPackages(new GetPlayerSpecialistPackagesRequest()
            {
                PlayerId = authHelper.getAccountId("userOne")
            });
            
            Assert.True(playerPackages.PlayerPackages.Count == 1);
            Assert.True(playerPackages.PlayerPackages[0].Creator.Username == "userOne");
            Assert.True(playerPackages.PlayerPackages[0].SpecialistIds.Count == 1);
            Assert.True(playerPackages.PlayerPackages[0].SpecialistIds.Count(it => it == response.SpecialistConfigurationId) == 1);
        }
        
        [Test]
        public void CanAddMultipleSpecialistsToAPackage()
        {
            SubmitCustomSpecialistResponse response = submitCustomSpecialist("MyCustomSpecialist");
            SubmitCustomSpecialistResponse responseTwo = submitCustomSpecialist("SecondCustomSpec");

            CreateSpecialistPackageResponse packageResponse = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "myPackage",
                        SpecialistIds = {response.SpecialistConfigurationId, responseTwo.SpecialistConfigurationId},
                    }
                });
            
            Assert.IsTrue(packageResponse.Status.IsSuccess);
            Assert.NotNull(packageResponse.SpecialistPackageId);
            
            GetPlayerSpecialistPackagesResponse playerPackages = client.GetPlayerSpecialistPackages(new GetPlayerSpecialistPackagesRequest()
            {
                PlayerId = authHelper.getAccountId("userOne")
            });
            
            Assert.True(playerPackages.PlayerPackages.Count == 1);
            Assert.True(playerPackages.PlayerPackages[0].Creator.Username == "userOne");
            Assert.True(playerPackages.PlayerPackages[0].SpecialistIds.Count == 2);
            Assert.True(playerPackages.PlayerPackages[0].SpecialistIds.Count(it => it == response.SpecialistConfigurationId) == 1);
            Assert.True(playerPackages.PlayerPackages[0].SpecialistIds.Count(it => it == responseTwo.SpecialistConfigurationId) == 1);
        }
        
        [Test]
        public void CanAddASpecialistPackageToAPackage()
        {
            SubmitCustomSpecialistResponse response = submitCustomSpecialist("MyCustomSpecialist");
            SubmitCustomSpecialistResponse responseTwo = submitCustomSpecialist("SecondCustomSpec");

            CreateSpecialistPackageResponse packageResponse = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "myPackage",
                        SpecialistIds = {response.SpecialistConfigurationId},
                    }
                });
            
            CreateSpecialistPackageResponse packageInPackageResponse = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "myPackage In A Package",
                        SpecialistIds = {responseTwo.SpecialistConfigurationId},
                        SpecialistPackageIds = { packageResponse.SpecialistPackageId }
                    }
                });

            GetPlayerSpecialistPackagesResponse playerPackages = client.GetPlayerSpecialistPackages(new GetPlayerSpecialistPackagesRequest()
            {
                PlayerId = authHelper.getAccountId("userOne")
            });
            
            Assert.True(playerPackages.PlayerPackages.Count == 2);
            Assert.True(playerPackages.PlayerPackages.Count(it => it.Id == packageInPackageResponse.SpecialistPackageId) == 1);
            Assert.True(playerPackages.PlayerPackages.Count(it => it.Id == packageResponse.SpecialistPackageId) == 1);
        }
        
        [Test]
        public void CanViewOtherPlayersPackages()
        {
            SubmitCustomSpecialistResponse response = submitCustomSpecialist("MyCustomSpecialist");
            SubmitCustomSpecialistResponse responseTwo = submitCustomSpecialist("SecondCustomSpec");

            CreateSpecialistPackageResponse playerOnePackage = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "PlayerOnePackage",
                        SpecialistIds = { response.SpecialistConfigurationId },
                    }
                });

            authHelper.loginToAccount("userTwo");
            
            CreateSpecialistPackageResponse playerTwoPackage = client.CreateSpecialistPackage(
                new CreateSpecialistPackageRequest()
                {
                    SpecialistPackage = new SpecialistPackage()
                    {
                        PackageName = "PlayerTwoPackage",
                        SpecialistIds = { responseTwo.SpecialistConfigurationId },
                    }
                });

            GetSpecialistPackagesResponse specialistPackagesResponse = client.GetSpecialistPackages(new GetSpecialistPackagesRequest());
            
            Assert.True(specialistPackagesResponse.SpecialistPackages.Count == 2);
            Assert.True(specialistPackagesResponse.SpecialistPackages.Count(it => it.Id == playerTwoPackage.SpecialistPackageId) == 1);
            Assert.True(specialistPackagesResponse.SpecialistPackages.Count(it => it.Id == playerOnePackage.SpecialistPackageId) == 1);
        }

        private SubmitCustomSpecialistResponse submitCustomSpecialist(String specialistName)
        {
            return client.SubmitCustomSpecialist(createSpecialistRequest(specialistName));
        }

        private SubmitCustomSpecialistRequest createSpecialistRequest(String specialistName) {
            return new SubmitCustomSpecialistRequest()
            {
                Configuration = new SpecialistConfiguration()
                {
                    Priority = 1,
                    SpecialistName = specialistName,
                    SpecialistEffects =
                    {
                        new SpecialistEffectConfiguration()
                        {
                            EffectModifier = EffectModifier.Driller,
                            EffectTarget = EffectTarget.Friendly,
                            EffectTrigger = EffectTrigger.PreCombat,
                            Value = new EffectValue()
                            {
                                Value = 15,
                                ValueType = ValueType.Numeric,
                            },
                        }
                    },
                    PromotesFrom = "",
                }
            };
        }

    }
}