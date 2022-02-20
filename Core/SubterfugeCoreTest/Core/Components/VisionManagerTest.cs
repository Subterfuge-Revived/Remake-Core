using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class VisionManagerTest
    {
        private Mock<IEntity> mockEntity;
        private Mock<IEntity> mockSecondEntity;
        
        public void mockVisionManagerEntity(
            RftVector initialLocation,
            IEntity destination,
            GameTick startTime,
            float visionRange = 1.0f
        )
        {
            mockEntity = new Mock<IEntity>();
            mockSecondEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockEntity.Object, 1.0f));
            mockEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockEntity.Object, initialLocation, destination, startTime));
            mockEntity.Setup(it => it.GetComponent<VisionManager>())
                .Returns(new VisionManager(mockEntity.Object, visionRange));
        }
        
        [TestMethod]
        public void CanInitializeVisionManager()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            
            mockVisionManagerEntity(initialLocation, destination, new GameTick());
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
        }
        
        [TestMethod]
        public void CanSetInitialVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
        }
        
        [TestMethod]
        public void CanChangeTheVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            
            

            float newVisionRange = 1.0f;
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            mockEntity.Object.GetComponent<VisionManager>().SetVisionRange(newVisionRange, mockGameState.Object, new GameTick());
            
            Assert.AreEqual(newVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
        }
        
        [TestMethod]
        public void PositionInsideOfVisionRangeIsDetectedInVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(mockEntity.Object.GetComponent<VisionManager>().IsInVisionRange(new GameTick(), mockSecondEntity.Object.GetComponent<PositionManager>()));
        }
        
        [TestMethod]
        public void GetEntitesInVisionRangeReturnsEntitesInVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> secondEntity = new List<IEntity>(); 
            secondEntity.Add(mockSecondEntity.Object);
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
        }
        
        [TestMethod]
        public void PositionOutsideOfVisionRangeIsDetectedOutOfVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionOutsideVisionRange = new RftVector(1.1f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionOutsideVisionRange, destination, new GameTick()));
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(mockEntity.Object.GetComponent<VisionManager>().IsInVisionRange(new GameTick(), mockSecondEntity.Object.GetComponent<PositionManager>()));
        }
        
        [TestMethod]
        public void GetEntitesInVisionRangeDoesNotReturnsEntitesOutsideVisionRange()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> emptyList = new List<IEntity>();
            List<IEntity> secondEntity = new List<IEntity>(); 
            secondEntity.Add(mockSecondEntity.Object);
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(emptyList);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
        }
        
        // Test Vision event handlers
        [TestMethod]
        public void WhenVisionRangeIsChangedVisionRangeChangeEventIsRaised()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 10.0f;
            
            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());

            OnVisionRangeChangeEventArgs visionRangeChangeArgs = null;
            mockEntity.Object.GetComponent<VisionManager>().OnVisionRangeChange += (sender, args) =>
            {
                visionRangeChangeArgs = args;
            };
            

            float newVisionRange = 1.0f;
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            mockEntity.Object.GetComponent<VisionManager>().SetVisionRange(newVisionRange, mockGameState.Object, new GameTick());
            
            Assert.AreEqual(newVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsNotNull(visionRangeChangeArgs);
            Assert.AreEqual(initialVisionRange, visionRangeChangeArgs.PreviousVisionRange);
            Assert.AreEqual(newVisionRange, visionRangeChangeArgs.NewVisionRange);
            Assert.AreEqual(mockEntity.Object.GetComponent<VisionManager>(), visionRangeChangeArgs.VisionManager);
        }
        
        [TestMethod]
        public void WhenANewEntityEntersTheVisionRangeAnOnEntityEnterVisionRangeEventIsTriggered()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> emptyList = new List<IEntity>();
            List<IEntity> secondEntity = new List<IEntity>(); 
            secondEntity.Add(mockSecondEntity.Object);
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(emptyList);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsFalse(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
            
            // Update to have the entity in rage.
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);

            OnEntityEnterVisionRangeEventArgs enterVisionArgs = null;
            mockEntity.Object.GetComponent<VisionManager>().OnEntityEnterVisionRange += (sender, args) =>
            {
                enterVisionArgs = args;
            };
            
            Assert.IsTrue(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
            Assert.IsNotNull(enterVisionArgs);
            Assert.AreEqual(mockSecondEntity.Object, enterVisionArgs.EntityInVision);
            Assert.AreEqual(mockEntity.Object.GetComponent<VisionManager>(), enterVisionArgs.VisionManager);
        }

        [TestMethod]
        public void WhenAnEntityLeavesTheVisionRangeAnOnEntityLeaveVisionRangeEventIsTriggered()
        {
            RftVector.Map = new Rft(100, 100);
            var destination = new Generator("A", new RftVector(0, 0));
            var initialLocation = new RftVector(0, 0);
            var initialVisionRange = 1.0f;

            mockVisionManagerEntity(initialLocation, destination, new GameTick(), initialVisionRange);
            
            var positionInsideVisionRange = new RftVector(0.5f, 0.0f);
            mockSecondEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockSecondEntity.Object,0.0f));
            mockSecondEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(mockSecondEntity.Object, positionInsideVisionRange, destination, new GameTick()));
            List<IEntity> secondEntity = new List<IEntity>(); 
            secondEntity.Add(mockSecondEntity.Object);
            var mockGameState = new Mock<IGameState>();
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(secondEntity);
            mockGameState.Setup(it => it.GetAllGameObjects()).Returns(secondEntity);
            
            Assert.IsNotNull(mockEntity.Object.GetComponent<VisionManager>());
            Assert.AreEqual(initialVisionRange, mockEntity.Object.GetComponent<VisionManager>().GetVisionRange());
            Assert.IsTrue(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
            
            // Make entity leave vision range
            mockGameState.Setup(it => it.EntitesInRange(It.IsAny<float>(), It.IsAny<RftVector>())).Returns(new List<IEntity>());
            
            OnEntityLeaveVisionRangeEventArgs leaveVisionArgs = null;
            mockEntity.Object.GetComponent<VisionManager>().OnEntityLeaveVisionRange += (sender, args) =>
            {
                leaveVisionArgs = args;
            };

            Assert.IsFalse(mockEntity.Object.GetComponent<VisionManager>().GetEntitiesInVisionRange(mockGameState.Object, new GameTick()).Contains(mockSecondEntity.Object));
            Assert.IsNotNull(leaveVisionArgs);
            Assert.AreEqual(mockSecondEntity.Object, leaveVisionArgs.EntityLeavingVision);
            Assert.AreEqual(mockEntity.Object.GetComponent<VisionManager>(), leaveVisionArgs.VisionManager);
        }
    }
}