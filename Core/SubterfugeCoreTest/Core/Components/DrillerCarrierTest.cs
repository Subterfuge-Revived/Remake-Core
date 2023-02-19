using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class DrillerCarrierTest
    {
        private Mock<IEntity> _mockEntity;
        private Player playerOne = new Player(new SimpleUser() { Id = "1" });
        private Player playerTwo = new Player(new SimpleUser() { Id = "2" });

        private void MockDrillerCarrierEntity(
            int initialDrillers,
            Player owner
            )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<DrillerCarrier>())
                .Returns(new DrillerCarrier(_mockEntity.Object, initialDrillers, owner));
        }

        [TestMethod]
        public void CanInitializeDrillerCarrier()
        {
            MockDrillerCarrierEntity(0, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
        }
        
        [TestMethod]
        public void DrillerCarrierSetsInitialDrillers()
        {
            var initialDrillers = 10;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            initialDrillers = 20;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(initialDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void DrillerCarrierSetsInitialOwner()
        {
            MockDrillerCarrierEntity(0, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(playerOne, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            
            MockDrillerCarrierEntity(0, playerTwo);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(playerTwo, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
        }
        
        [TestMethod]
        public void CanAddDrillersToDrillerCarrier()
        {
            var initialDrillers = 0;
            var drillersToAdd = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            _mockEntity.Object.GetComponent<DrillerCarrier>().AddDrillers(drillersToAdd);
            Assert.AreEqual(initialDrillers + drillersToAdd,
                _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanRemoveDrillersFromDrillerCarrier()
        {
            var initialDrillers = 100;
            var drillersToRemove = 40;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            _mockEntity.Object.GetComponent<DrillerCarrier>().RemoveDrillers(drillersToRemove);
            Assert.AreEqual(initialDrillers - drillersToRemove, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void RemovingMoreThanTheTotalDrillerCountCapturesTheCarrier()
        {
            var initialDrillers = 50;
            var drillersToRemove = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            _mockEntity.Object.GetComponent<DrillerCarrier>().RemoveDrillers(drillersToRemove);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void CanSetDrillerCountOfDrillerCarrier()
        {
            var initialDrillers = 100;
            var newDrillers = 40;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            _mockEntity.Object.GetComponent<DrillerCarrier>().SetDrillerCount(newDrillers);
            Assert.AreEqual(newDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void DrillerCarrierHasDrillers()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(75));
            Assert.IsTrue(_mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(100));
            Assert.IsFalse(_mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(101));
            Assert.IsFalse(_mockEntity.Object.GetComponent<DrillerCarrier>().HasDrillers(600));
        }
        
        [TestMethod]
        public void CanSetNewOwnerAndDrillerCountOfCarrier()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(playerOne, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(initialDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            var newDrillerCount = 20;
            _mockEntity.Object.GetComponent<DrillerCarrier>().SetNewOwner(playerTwo, newDrillerCount);
            Assert.AreEqual(playerTwo, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(newDrillerCount, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanSetOwner()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(playerOne, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
            
            _mockEntity.Object.GetComponent<DrillerCarrier>().SetOwner(playerTwo);
            Assert.AreEqual(playerTwo, _mockEntity.Object.GetComponent<DrillerCarrier>().GetOwner());
        }
        
        [TestMethod]
        public void CanSetCaptured()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());

            _mockEntity.Object.GetComponent<DrillerCarrier>().SetCaptured(true);
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
            
            _mockEntity.Object.GetComponent<DrillerCarrier>().SetCaptured(false);
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<DrillerCarrier>().IsCaptured());
        }
        
        [TestMethod]
        public void CanDestroy()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(initialDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            _mockEntity.Object.GetComponent<DrillerCarrier>().Destroy();
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanAddDrillersAfterBeingDestroyed()
        {
            var initialDrillers = 100;
            MockDrillerCarrierEntity(initialDrillers, playerOne);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<DrillerCarrier>());
            Assert.AreEqual(false, _mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(initialDrillers, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());

            _mockEntity.Object.GetComponent<DrillerCarrier>().Destroy();
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            _mockEntity.Object.GetComponent<DrillerCarrier>().AddDrillers(50);
            Assert.AreEqual(true, _mockEntity.Object.GetComponent<DrillerCarrier>().IsDestroyed());
            Assert.AreEqual(50, _mockEntity.Object.GetComponent<DrillerCarrier>().GetDrillerCount());
        }

    }
}