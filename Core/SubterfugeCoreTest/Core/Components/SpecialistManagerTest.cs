using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Entities.Specialists.Specialists;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class SpecialistManagerTest
    {
        private Mock<IEntity> _mockEntity;
        private Mock<IEntity> _mockSecondEntity;

        private Player playerOne = new Player(new SimpleUser() { Id = "1" });
        private Player playerTwo = new Player(new SimpleUser() { Id = "2" });
        
        private List<Player> players;
        private TimeMachine _timeMachine;

        private void MockSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            _mockEntity = new Mock<IEntity>();
            var specialistManager = new SpecialistManager(_mockEntity.Object, specialistCapacity);
            specialistManager.AllowHireFromLocation();
            
            _mockEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(specialistManager);
        }

        private void MockSecondSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            _mockSecondEntity = new Mock<IEntity>();
            var specialistManager = new SpecialistManager(_mockSecondEntity.Object, specialistCapacity);
            specialistManager.AllowHireFromLocation();
            _mockSecondEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(specialistManager);
        }

        [TestInitialize]
        public void Setup()
        {
            players = new List<Player>()
            {
                new Player(new SimpleUser() { Id = "1" })
            };
            _timeMachine = new TimeMachine(new GameState(players));
        }

        [TestMethod]
        public void CanInitializeSpecialistManager()
        {
            MockSpecialistManagerEntity();
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
        }
        
        [TestMethod]
        public void CanInitializeSpecialistManagerWithCapacity()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
        }
        
        [TestMethod]
        public void SpecialistManagerWithNoSpecialistsReturnsEmptyList()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Count);
        }
        
        [TestMethod]
        public void SpecialistManagerWithCapacityHasCanAddNewSpecialistsTrue()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void NoCapacityToAddSpecialistIfZeroCapacity()
        {
            var initialCapacity = 0;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void CanAddSpecialistToCarrier()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialist = new NoOpSpecialist(playerOne);
            bool result = _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialist, _timeMachine);
            Assert.IsTrue(result);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CannotAddSpecialistToCarrierIfNoRoom()
        {
            var initialCapacity = 0;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialist = new NoOpSpecialist(playerOne);
            bool result = _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialist, _timeMachine);
            Assert.IsFalse(result);
            
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CanAddListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void IfCarrierIsFullWhileAddingListOfSpecialistsOnlySomeGetAdded()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }

        [TestMethod]
        public void CannotRemoveASpecialistIfNotInCarrier()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            MockSecondSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            
            var specialist = new NoOpSpecialist(playerOne);
            bool removeResult = _mockEntity.Object.GetComponent<SpecialistManager>().TransferSpecialistsById(
                _mockSecondEntity.Object.GetComponent<SpecialistManager>(), 
                new List<string>() { specialist.GetId() },
                _timeMachine
            );
            Assert.IsTrue(removeResult);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
        }
        
        [TestMethod]
        public void CanRemoveListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            _mockEntity.Object.GetComponent<SpecialistManager>().KillAll();
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void RemovingAListOfSpecialistsOnlyRemovesTheOnesInTheCarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            bool addOperation = _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            Assert.AreEqual(true, addOperation);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            _mockEntity.Object.GetComponent<SpecialistManager>().KillAll();
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanSetTheCapacity()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());

            var capacityDelta = 6;
            _mockEntity.Object.GetComponent<SpecialistManager>().AlterCapacity(capacityDelta);
            Assert.AreEqual(initialCapacity + capacityDelta, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
        }
        
        [TestMethod]
        public void CanTransferSpecialistsFromOneCarrierToAnotherIfItHasCapacity()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            MockSecondSpecialistManagerEntity(2);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(initialCapacity, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsTo(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), _timeMachine);
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CannotTransferSpecialistsIfTheSecondCarrierDoesNotHaveCapacity()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            MockSecondSpecialistManagerEntity(0);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsTo(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), _timeMachine);
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanTransferSpecialistsByIdFromOneCarrierToAnotherIfItHasCapacity()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            MockSecondSpecialistManagerEntity(2);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(initialCapacity, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsById(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds, _timeMachine);
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CannotTransferSpecialistsByIdFromOneCarrierToAnotherIfNoCapacity()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            MockSecondSpecialistManagerEntity(0);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsById(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds, _timeMachine);
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanCaptureAllSpecialistsInACarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            _mockEntity.Object.GetComponent<SpecialistManager>().CaptureAllForward(null, null);
            Assert.IsTrue(specialistOne.IsCaptured());
            Assert.IsTrue(specialistTwo.IsCaptured());
        }
        
        [TestMethod]
        public void CanAddTwoSpecialistsOwnedByDifferentPlayersToACarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            
            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanGetAllSpecialistsOwnedByASpecificPlayer()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            var specialistOne = new NoOpSpecialist(playerOne);
            var specialistTwo = new NoOpSpecialist(playerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);

            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistOne, _timeMachine);
            _mockEntity.Object.GetComponent<SpecialistManager>().HireSpecialist(specialistTwo, _timeMachine);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            List<Specialist> playerOneSpecs =
                _mockEntity.Object.GetComponent<SpecialistManager>().GetPlayerSpecialists(playerOne);
            Assert.AreEqual(1, playerOneSpecs.Count);
            Assert.IsTrue(playerOneSpecs.Contains(specialistOne));
        }
    }
}