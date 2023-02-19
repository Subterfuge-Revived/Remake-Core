using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameState;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test.Core.Components
{
    /*
    [TestClass]
    public class VisionManagerTest
    {
        private Mock<IEntity> _mockEntity;
        private Mock<IEntity> _mockSecondEntity;
        
        public void MockVisionManagerEntity(
            RftVector initialLocation,
            IEntity destination,
            GameTick startTime,
            float visionRange = 1.0f
        )
        {
            
            _mockEntity = new Mock<IEntity>();
            _mockSecondEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockEntity.Object, 1.0f));
            _mockEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockEntity.Object, initialLocation, destination, startTime));
            _mockEntity.Setup(it => it.GetComponent<VisionManager>())
                .Returns(new VisionManager(_mockEntity.Object, visionRange));
        }
        
        [TestMethod]
        public void CanInitializeVisionManager()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            
            MockVisionManagerEntity(initialLocation, destination, new GameTick());
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
        }
        
        [TestMethod]
        public void CanSetInitialVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
        }
        
        [TestMethod]
        public void CanChangeTheVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            
            

            float newVisionRange = 1.0f;
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            _mockEntity.Object.GetComponent<VisionManager>().SetVisionRange(newVisionRange, mockGameState.Object, new GameTick());
            
            Assert.AreEqual(newVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
        }
        
        [TestMethod]
        public void PositionInsideOfVisionRangeIsDetectedInVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(_mockEntity.Object.GetComponent<VisionManager>().IsInVisionRange(new GameTick(), _mockSecondEntity.Object.GetComponent<PositionManager>()));
        }
        
        [TestMethod]
        public void GetEntitesInVisionRangeReturnsEntitesInVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> secondEntity = new List<IEntity> { _mockSecondEntity.Object };
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
        }
        
        [TestMethod]
        public void PositionOutsideOfVisionRangeIsDetectedOutOfVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionOutsideVisionRange = new RftVector(1.1f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionOutsideVisionRange, destination, new GameTick()));
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(_mockEntity.Object.GetComponent<VisionManager>().IsInVisionRange(new GameTick(), _mockSecondEntity.Object.GetComponent<PositionManager>()));
        }
        
        [TestMethod]
        public void GetEntitesInVisionRangeDoesNotReturnsEntitesOutsideVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> emptyList = new List<IEntity>();
            List<IEntity> secondEntity = new List<IEntity> { _mockSecondEntity.Object };
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(emptyList);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
        }
        
        // Test Vision event handlers
        [TestMethod]
        public void WhenVisionRangeIsChangedVisionRangeChangeEventIsRaised()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());

            OnVisionRangeChangeEventArgs visionRangeChangeArgs = null;
            _mockEntity.Object.GetComponent<VisionManager>().OnVisionRangeChange += (sender, args) =>
            {
                visionRangeChangeArgs = args;
            };
            

            float newVisionRange = 1.0f;
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            _mockEntity.Object.GetComponent<VisionManager>().SetVisionRange(newVisionRange, mockGameState.Object, new GameTick());
            
            Assert.AreEqual(newVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsNotNull(visionRangeChangeArgs);
            Assert.AreEqual(initialVisionRange, visionRangeChangeArgs.PreviousVisionRange);
            Assert.AreEqual(newVisionRange, visionRangeChangeArgs.NewVisionRange);
            Assert.AreEqual(_mockEntity.Object.GetComponent<VisionManager>(), visionRangeChangeArgs.VisionManager);
        }
        
        [TestMethod]
        public void WhenANewEntityEntersTheVisionRangeAnOnEntityEnterVisionRangeEventIsTriggered()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> emptyList = new List<IEntity>();
            List<IEntity> secondEntity = new List<IEntity> { _mockSecondEntity.Object };
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(emptyList);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
            
            // Update to have the entity in rage.
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);

            OnEntityEnterVisionRangeEventArgs enterVisionArgs = null;
            _mockEntity.Object.GetComponent<VisionManager>().OnEntityEnterVisionRange += (sender, args) =>
            {
                enterVisionArgs = args;
            };
            
            Assert.IsTrue(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
            Assert.IsNotNull(enterVisionArgs);
            Assert.AreEqual(_mockSecondEntity.Object, enterVisionArgs.EntityInVision);
            Assert.AreEqual(_mockEntity.Object.GetComponent<VisionManager>(), enterVisionArgs.VisionManager);
        }

        [TestMethod]
        public void WhenAnEntityLeavesTheVisionRangeAnOnEntityLeaveVisionRangeEventIsTriggered()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            MockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            _mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockSecondEntity.Object,0.0f));
            _mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> secondEntity = new List<IEntity> { _mockSecondEntity.Object };
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(_mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, _mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
            
            // Make entity leave vision range
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            
            OnEntityLeaveVisionRangeEventArgs leaveVisionArgs = null;
            _mockEntity.Object.GetComponent<VisionManager>().OnEntityLeaveVisionRange += (sender, args) =>
            {
                leaveVisionArgs = args;
            };

            Assert.IsFalse(_mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(_mockSecondEntity.Object));
            Assert.IsNotNull(leaveVisionArgs);
            Assert.AreEqual(_mockSecondEntity.Object, leaveVisionArgs.EntityLeavingVision);
            Assert.AreEqual(_mockEntity.Object.GetComponent<VisionManager>(), leaveVisionArgs.VisionManager);
        }
    }
    */
}