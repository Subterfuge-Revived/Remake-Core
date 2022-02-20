using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class DrillerCarrierTest
    {
        private Mock<IEntity> mockEntity;

        public void mockDrillerCarrierEntity(
            int initialDrillers,
            Player owner
            )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<DrillerCarrier>())
                .Returns(new DrillerCarrier(mockEntity.Object, initialDrillers, owner));
        }

        [TestMethod]
        public void CanInitializeDrillerCarrier()
        {
            mockDrillerCarrierEntity(0, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
        }
        
        [TestMethod]
        public void DrillerCarrierSetsInitialDrillers()
        {
            var initialDrillers = 10;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            initialDrillers = 20;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void DrillerCarrierSetsInitialOwner()
        {
            var initialOwner = new Player("1");
            mockDrillerCarrierEntity(0, initialOwner);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialOwner, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            
            initialOwner = new Player("2");
            mockDrillerCarrierEntity(0, initialOwner);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialOwner, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
        }
        
        [TestMethod]
        public void CanAddDrillersToDrillerCarrier()
        {
            var initialDrillers = 0;
            var drillersToAdd = 100;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            mockEntity.Object.GetComponent<DrillerCarrier>().AddDrillers(drillersToAdd);
            Assert.AreEqual(initialDrillers + drillersToAdd, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanRemoveDrillersFromDrillerCarrier()
        {
            var initialDrillers = 100;
            var drillersToRemove = 40;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            mockEntity.Object.GetComponent<DrillerCarrier>().RemoveDrillers(drillersToRemove);
            Assert.AreEqual(initialDrillers - drillersToRemove, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(false, mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void RemovingMoreThanTheTotalDrillerCountCapturesTheCarrier()
        {
            var initialDrillers = 50;
            var drillersToRemove = 100;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            mockEntity.Object.GetComponent<DrillerCarrier>().RemoveDrillers(drillersToRemove);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(true, mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void CanSetDrillerCountOfDrillerCarrier()
        {
            var initialDrillers = 100;
            var newDrillers = 40;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            mockEntity.Object.GetComponent<DrillerCarrier>().SetDrillerCount(newDrillers);
            Assert.AreEqual(newDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void DrillerCarrierHasDrillers()
        {
            var initialDrillers = 100;
            mockDrillerCarrierEntity(initialDrillers, new Player("1"));
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.IsTrue(mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(75));
            Assert.IsTrue(mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(100));
            Assert.IsFalse(mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(101));
            Assert.IsFalse(mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(600));
        }
        
        [TestMethod]
        public void CanSetNewOwnerAndDrillerCountOfCarrier()
        {
            var initialDrillers = 100;
            var initialPlayer = new Player("1");
            mockDrillerCarrierEntity(initialDrillers, initialPlayer);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialPlayer, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(initialDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            var newDrillerCount = 20;
            var newOwner = new Player("2");
            mockEntity.Object.GetComponent<DrillerCarrier>().SetNewOwner(newOwner, newDrillerCount);
            Assert.AreEqual(newOwner, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(newDrillerCount, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanSetOwner()
        {
            var initialDrillers = 100;
            var initialPlayer = new Player("1");
            mockDrillerCarrierEntity(initialDrillers, initialPlayer);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialPlayer, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());

            var newOwner = new Player("2");
            mockEntity.Object.GetComponent<DrillerCarrier>().SetOwner(newOwner);
            Assert.AreEqual(newOwner, mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
        }
        
        [TestMethod]
        public void CanSetCaptured()
        {
            var initialDrillers = 100;
            var initialPlayer = new Player("1");
            mockDrillerCarrierEntity(initialDrillers, initialPlayer);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());

            mockEntity.Object.GetComponent<DrillerCarrier>().SetCaptured(true);
            Assert.AreEqual(true, mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
            
            mockEntity.Object.GetComponent<DrillerCarrier>().SetCaptured(false);
            Assert.AreEqual(false, mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void CanDestroy()
        {
            var initialDrillers = 100;
            var initialPlayer = new Player("1");
            mockDrillerCarrierEntity(initialDrillers, initialPlayer);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(initialDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            mockEntity.Object.GetComponent<DrillerCarrier>().Destroy();
            Assert.AreEqual(true, mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanAddDrillersAfterBeingDestroyed()
        {
            var initialDrillers = 100;
            var initialPlayer = new Player("1");
            mockDrillerCarrierEntity(initialDrillers, initialPlayer);
            Assert.IsNotNull(mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(initialDrillers, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            mockEntity.Object.GetComponent<DrillerCarrier>().Destroy();
            Assert.AreEqual(true, mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            mockEntity.Object.GetComponent<DrillerCarrier>().AddDrillers(50);
            Assert.AreEqual(true, mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(50, mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }

    }
}