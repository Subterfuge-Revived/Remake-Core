using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class SpecialistManagerTest
    {
        private Mock<IEntity> _mockEntity;
        private Mock<IEntity> _mockSecondEntity;

        private void MockSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(new SpecialistManager(_mockEntity.Object, specialistCapacity));
        }

        private void MockSecondSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            _mockSecondEntity = new Mock<IEntity>();
            _mockSecondEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(new SpecialistManager(_mockSecondEntity.Object, specialistCapacity));
        }

        private static Specialist CreateSpecialist(
            string name,
            int priority,
            Player owner
            )
        {
            return new Specialist(owner, null);
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Count);
        }
        
        [TestMethod]
        public void SpecialistManagerWithCapacityHasCanAddNewSpecialistsTrue()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void NoCapacityToAddSpecialistIfZeroCapacity()
        {
            var initialCapacity = 0;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void CanAddSpecialistToCarrier()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = CreateSpecialist("a", 1, new Player("a"));
            bool result = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(result);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CannotAddSpecialistToCarrierIfNoRoom()
        {
            var initialCapacity = 0;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = CreateSpecialist("a", 1, new Player("a"));
            bool result = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsFalse(result);
            
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CanAddListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(1, addedSpecs);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanRemoveASpecialistAfterItHasBeenAdded()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = CreateSpecialist("a", 1, new Player("a"));
            bool addResult = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(addResult);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            bool removeResult = _mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsTrue(removeResult);
            
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CannotRemoveASpecialistIfNotInCarrier()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            var specialist = CreateSpecialist("a", 1, new Player("a"));
            bool removeResult = _mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsFalse(removeResult);
        }
        
        [TestMethod]
        public void CanRemoveListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            int removedSpecialists = _mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialists(specialists);
            Assert.AreEqual(2, removedSpecialists);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            bool addOperation = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialistOne);
            Assert.AreEqual(true, addOperation);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            int removedSpecialists = _mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialists(specialists);
            Assert.AreEqual(1, removedSpecialists);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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

            var newCapacity = 6;
            _mockEntity.Object.GetComponent<SpecialistManager>().SetCapacity(newCapacity);
            Assert.AreEqual(newCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsTo(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsTo(_mockSecondEntity.Object.GetComponent<SpecialistManager>());
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsById(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds);
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = _mockEntity.Object.GetComponent<SpecialistManager>()
                .TransferSpecialistsById(_mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds);
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, _mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(_mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = CreateSpecialist("a", 1, new Player("a"));
            var specialistTwo = CreateSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            _mockEntity.Object.GetComponent<SpecialistManager>().CaptureAll();
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            var ownerOne = new Player("1");
            var ownerTwo = new Player("2");
            var specialistOne = CreateSpecialist("a", 1, ownerOne);
            var specialistTwo = CreateSpecialist("b", 2, ownerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
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
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var ownerOne = new Player("1");
            var ownerTwo = new Player("2");
            var specialistOne = CreateSpecialist("a", 1, ownerOne);
            var specialistTwo = CreateSpecialist("b", 2, ownerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            List<Specialist> playerOneSpecs =
                _mockEntity.Object.GetComponent<SpecialistManager>().GetPlayerSpecialists(ownerOne);
            Assert.AreEqual(1, playerOneSpecs.Count);
            Assert.IsTrue(playerOneSpecs.Contains(specialistOne));
        }
        
        
        // Event Handler Tests
        [TestMethod]
        public void AddingASpecialistTriggersTheOnAddSpecialistEvent()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = CreateSpecialist("a", 1, new Player("a"));

            OnAddSpecialistEventArgs addSpecArgs = null;
            _mockEntity.Object.GetComponent<SpecialistManager>().OnAddSpecialist += (sender, args) =>
            {
                addSpecArgs = args;
            };
            
            bool result = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            Assert.IsNotNull(addSpecArgs);
            Assert.AreEqual(specialist , addSpecArgs.AddedSpecialist);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpecialistManager>() , addSpecArgs.AddedTo);
        }
        
        [TestMethod]
        public void RemovingASpecialistTriggersTheOnRemoveSpecialistEvent()
        {
            var initialCapacity = 1;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = CreateSpecialist("a", 1, new Player("a"));
            bool addResult = _mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(addResult);
            
            Assert.AreEqual(1, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            
            OnRemoveSpecialistEventArgs removeSpecArgs = null;
            _mockEntity.Object.GetComponent<SpecialistManager>().OnRemoveSpecialist += (sender, args) =>
            {
                removeSpecArgs = args;
            };
            
            bool removeResult = _mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsTrue(removeResult);
            
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(_mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            Assert.IsNotNull(removeSpecArgs);
            Assert.AreEqual(specialist , removeSpecArgs.RemovedSpecialist);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpecialistManager>() , removeSpecArgs.RemovedFrom);
        }
        
        [TestMethod]
        public void SettingTheCapacityTriggersOnCapacityChangeEvent()
        {
            var initialCapacity = 5;
            MockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            
            OnSpecialistCapacityChangeEventArgs capacityChangeEvent = null;
            _mockEntity.Object.GetComponent<SpecialistManager>().OnSpecialistCapacityChange += (sender, args) =>
            {
                capacityChangeEvent = args;
            };

            var newCapacity = 6;
            _mockEntity.Object.GetComponent<SpecialistManager>().SetCapacity(newCapacity);
            Assert.AreEqual(newCapacity, _mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            
            Assert.IsNotNull(capacityChangeEvent);
            Assert.AreEqual(initialCapacity , capacityChangeEvent.PreviousCapacity);
            Assert.AreEqual(newCapacity , capacityChangeEvent.NewCapacity);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpecialistManager>() , capacityChangeEvent.SpecialistManager);
        }
    }
}