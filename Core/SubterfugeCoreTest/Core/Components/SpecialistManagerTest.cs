using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class SpecialistManagerTest
    {
        private Mock<IEntity> mockEntity;
        private Mock<IEntity> mockSecondEntity;
        
        public void mockSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(new SpecialistManager(mockEntity.Object, specialistCapacity));
        }
        
        public void mockSecondSpecialistManagerEntity(
            int specialistCapacity = 3
        )
        {
            mockSecondEntity = new Mock<IEntity>();
            mockSecondEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(new SpecialistManager(mockSecondEntity.Object, specialistCapacity));
        }
        
        public Specialist createSpecialist(
            string name,
            int priority,
            Player owner
            )
        {
            return new Specialist(name, priority, owner);
        }
        
        [TestMethod]
        public void CanInitializeSpecialistManager()
        {
            mockSpecialistManagerEntity();
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
        }
        
        [TestMethod]
        public void CanInitializeSpecialistManagerWithCapacity()
        {
            var initialCapacity = 5;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
        }
        
        [TestMethod]
        public void SpecialistManagerWithNoSpecialistsReturnsEmptyList()
        {
            var initialCapacity = 5;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Count);
        }
        
        [TestMethod]
        public void SpecialistManagerWithCapacityHasCanAddNewSpecialistsTrue()
        {
            var initialCapacity = 5;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(true, mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void NoCapacityToAddSpecialistIfZeroCapacity()
        {
            var initialCapacity = 0;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(false, mockEntity.Object.GetComponent<SpecialistManager>().CanAddSpecialists());
        }
        
        [TestMethod]
        public void CanAddSpecialistToCarrier()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = createSpecialist("a", 1, new Player("a"));
            bool result = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(result);
            
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CannotAddSpecialistToCarrierIfNoRoom()
        {
            var initialCapacity = 0;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = createSpecialist("a", 1, new Player("a"));
            bool result = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsFalse(result);
            
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CanAddListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void IfCarrierIsFullWhileAddingListOfSpecialistsOnlySomeGetAdded()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(1, addedSpecs);
            
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanRemoveASpecialistAfterItHasBeenAdded()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = createSpecialist("a", 1, new Player("a"));
            bool addResult = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(addResult);
            
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            bool removeResult = mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsTrue(removeResult);
            
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
        }
        
        [TestMethod]
        public void CannotRemoveASpecialistIfNotInCarrier()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            var specialist = createSpecialist("a", 1, new Player("a"));
            bool removeResult = mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsFalse(removeResult);
        }
        
        [TestMethod]
        public void CanRemoveListOfSpecialistsToCarrier()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            int removedSpecialists = mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialists(specialists);
            Assert.AreEqual(2, removedSpecialists);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void RemovingAListOfSpecialistsOnlyRemovesTheOnesInTheCarrier()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            bool addOperation = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialistOne);
            Assert.AreEqual(true, addOperation);
            
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            int removedSpecialists = mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialists(specialists);
            Assert.AreEqual(1, removedSpecialists);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanSetTheCapacity()
        {
            var initialCapacity = 5;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());

            var newCapacity = 6;
            mockEntity.Object.GetComponent<SpecialistManager>().SetCapacity(newCapacity);
            Assert.AreEqual(newCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
        }
        
        [TestMethod]
        public void CanTransferSpecialistsFromOneCarrierToAnotherIfItHasCapacity()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            mockSecondSpecialistManagerEntity(2);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(initialCapacity, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = mockEntity.Object.GetComponent<SpecialistManager>()
                .transferSpecialistsTo(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CannotTransferSpecialistsIfTheSecondCarrierDoesNotHaveCapacity()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            mockSecondSpecialistManagerEntity(0);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var transferResult = mockEntity.Object.GetComponent<SpecialistManager>()
                .transferSpecialistsTo(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanTransferSpecialistsByIdFromOneCarrierToAnotherIfItHasCapacity()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            mockSecondSpecialistManagerEntity(2);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(initialCapacity, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = mockEntity.Object.GetComponent<SpecialistManager>()
                .transferSpecialistsById(mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds);
            
            Assert.IsTrue(transferResult);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CannotTransferSpecialistsByIdFromOneCarrierToAnotherIfNoCapacity()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            mockSecondSpecialistManagerEntity(0);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.IsNotNull(mockSecondEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            var listOfIds = specialists.Select(it => it.GetId()).ToList();

            var transferResult = mockEntity.Object.GetComponent<SpecialistManager>()
                .transferSpecialistsById(mockSecondEntity.Object.GetComponent<SpecialistManager>(), listOfIds);
            
            Assert.IsFalse(transferResult);
            Assert.AreEqual(0, mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsFalse(mockSecondEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanCaptureAllSpecialistsInACarrier()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialistOne = createSpecialist("a", 1, new Player("a"));
            var specialistTwo = createSpecialist("b", 2, new Player("b"));
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
            
            mockEntity.Object.GetComponent<SpecialistManager>().captureAll();
            Assert.IsTrue(specialistOne.IsCaptured());
            Assert.IsTrue(specialistTwo.IsCaptured());
        }
        
        [TestMethod]
        public void CanAddTwoSpecialistsOwnedByDifferentPlayersToACarrier()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            var ownerOne = new Player("1");
            var ownerTwo = new Player("2");
            var specialistOne = createSpecialist("a", 1, ownerOne);
            var specialistTwo = createSpecialist("b", 2, ownerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));
        }
        
        [TestMethod]
        public void CanGetAllSpecialistsOwnedByASpecificPlayer()
        {
            var initialCapacity = 2;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var ownerOne = new Player("1");
            var ownerTwo = new Player("2");
            var specialistOne = createSpecialist("a", 1, ownerOne);
            var specialistTwo = createSpecialist("b", 2, ownerTwo);
            
            var specialists = new List<Specialist>();
            specialists.Add(specialistOne);
            specialists.Add(specialistTwo);
            
            int addedSpecs = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialists(specialists);
            Assert.AreEqual(2, addedSpecs);
            
            Assert.AreEqual(2, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistOne));
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialistTwo));

            List<Specialist> playerOneSpecs =
                mockEntity.Object.GetComponent<SpecialistManager>().GetPlayerSpecialists(ownerOne);
            Assert.AreEqual(1, playerOneSpecs.Count);
            Assert.IsTrue(playerOneSpecs.Contains(specialistOne));
        }
        
        
        // Event Handler Tests
        [TestMethod]
        public void AddingASpecialistTriggersTheOnAddSpecialistEvent()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = createSpecialist("a", 1, new Player("a"));

            OnAddSpecialistEventArgs addSpecArgs = null;
            mockEntity.Object.GetComponent<SpecialistManager>().OnAddSpecialist += (sender, args) =>
            {
                addSpecArgs = args;
            };
            
            bool result = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(result);
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            Assert.IsNotNull(addSpecArgs);
            Assert.AreEqual(specialist , addSpecArgs.AddedSpecialist);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpecialistManager>() , addSpecArgs.AddedTo);
        }
        
        [TestMethod]
        public void RemovingASpecialistTriggersTheOnRemoveSpecialistEvent()
        {
            var initialCapacity = 1;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());

            var specialist = createSpecialist("a", 1, new Player("a"));
            bool addResult = mockEntity.Object.GetComponent<SpecialistManager>().AddSpecialist(specialist);
            Assert.IsTrue(addResult);
            
            Assert.AreEqual(1, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsTrue(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            
            OnRemoveSpecialistEventArgs removeSpecArgs = null;
            mockEntity.Object.GetComponent<SpecialistManager>().OnRemoveSpecialist += (sender, args) =>
            {
                removeSpecArgs = args;
            };
            
            bool removeResult = mockEntity.Object.GetComponent<SpecialistManager>().RemoveSpecialist(specialist);
            Assert.IsTrue(removeResult);
            
            Assert.AreEqual(0, mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialistCount());
            Assert.IsFalse(mockEntity.Object.GetComponent<SpecialistManager>().GetSpecialists().Contains(specialist));
            
            Assert.IsNotNull(removeSpecArgs);
            Assert.AreEqual(specialist , removeSpecArgs.RemovedSpecialist);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpecialistManager>() , removeSpecArgs.RemovedFrom);
        }
        
        [TestMethod]
        public void SettingTheCapacityTriggersOnCapacityChangeEvent()
        {
            var initialCapacity = 5;
            mockSpecialistManagerEntity(initialCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpecialistManager>());
            Assert.AreEqual(initialCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            
            OnSpecialistCapacityChangeEventArgs capacityChangeEvent = null;
            mockEntity.Object.GetComponent<SpecialistManager>().OnSpecialistCapacityChange += (sender, args) =>
            {
                capacityChangeEvent = args;
            };

            var newCapacity = 6;
            mockEntity.Object.GetComponent<SpecialistManager>().SetCapacity(newCapacity);
            Assert.AreEqual(newCapacity, mockEntity.Object.GetComponent<SpecialistManager>().GetCapacity());
            
            Assert.IsNotNull(capacityChangeEvent);
            Assert.AreEqual(initialCapacity , capacityChangeEvent.previousCapacity);
            Assert.AreEqual(newCapacity , capacityChangeEvent.newCapacity);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpecialistManager>() , capacityChangeEvent.SpecialistManager);
        }
    }
}