using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Core.GameState;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class SubLauncherTest
    {
        private Mock<IEntity> _mockEntity;
        private Player initialOwner = new Player("1");
        
        public void MockSubLauncherEntity(
            int initialDrillers,
            Player owner,
            int specialistCapacity
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<DrillerCarrier>())
                .Returns(new DrillerCarrier(_mockEntity.Object, initialDrillers, owner));
            _mockEntity.Setup(it => it.GetComponent<SpecialistManager>())
                .Returns(new SpecialistManager(_mockEntity.Object, specialistCapacity));
            _mockEntity.Setup(it => it.GetComponent<SubLauncher>())
                .Returns(new SubLauncher(_mockEntity.Object));
        }
        
        [TestMethod]
        public void CanInitializeSubLauncher()
        {
            MockSubLauncherEntity(10, initialOwner, 3);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SubLauncher>());
        }
        
        [TestMethod]
        public void CanLaunchASub()
        {
            MockSubLauncherEntity(10, initialOwner, 3);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SubLauncher>());

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameEventData()
            {
                EventData = launchEventData,
                Id = "asdf",
                IssuedBy = initialOwner.ToUser(),
                OccursAtTick = 1,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0)); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0)); 
            mockGameState.Setup(it => it.GetEntity(launchEventData.SourceId)).Returns(source);
            mockGameState.Setup(it => it.GetEntity(launchEventData.DestinationId)).Returns(destination);

            Sub capturedSub = null;
            mockGameState.Setup(it => it.AddSub(It.IsAny<Sub>())).Callback<Sub>(sub => { capturedSub = sub; });
            
            _mockEntity.Object.GetComponent<SubLauncher>().LaunchSub(mockGameState.Object, launchEvent);
            
            // Verify the sub was added to the state
            mockGameState.Verify(it => it.AddSub(It.IsAny<Sub>()), Times.Once);
            Assert.IsNotNull(capturedSub);
            Assert.AreEqual(destination, capturedSub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, capturedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, capturedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, capturedSub.GetComponent<SpecialistManager>().GetSpecialistCount());
        }

        [TestMethod]
        public void CanUndoASubLaunch()
        {
            MockSubLauncherEntity(10, initialOwner, 3);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SubLauncher>());

            OnSubLaunchEventArgs subLaunchEventData = null;
            _mockEntity.Object.GetComponent<SubLauncher>().OnSubLaunch += (sender, args) =>
            {
                subLaunchEventData = args;
            };

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameEventData()
            {
                EventData = launchEventData,
                Id = "asdf",
                IssuedBy = initialOwner.ToUser(),
                OccursAtTick = 1,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0)); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0)); 
            mockGameState.Setup(it => it.GetEntity(launchEventData.SourceId)).Returns(source);
            mockGameState.Setup(it => it.GetEntity(launchEventData.DestinationId)).Returns(destination);
            
            _mockEntity.Object.GetComponent<SubLauncher>().LaunchSub(mockGameState.Object, launchEvent);

            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            
            // Verify the sub was added to the state
            mockGameState.Verify(it => it.AddSub(subLaunchEventData.LaunchedSub), Times.Once);
            
            Assert.AreEqual(launchEventData.DrillerCount, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetSpecialistCount());

            _mockEntity.Object.GetComponent<SubLauncher>().UndoLaunch(mockGameState.Object, launchEvent);
            
            // Verify the sub that was launched is removed from the state
            mockGameState.Verify(it => it.RemoveSub(subLaunchEventData.LaunchedSub), Times.Once);
        }
        
        // Event Handler Tests
        [TestMethod]
        public void LaunchingASubTriggersAnOnLaunchEvent()
        {
            MockSubLauncherEntity(10, initialOwner, 3);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SubLauncher>());

            OnSubLaunchEventArgs subLaunchEventData = null;
            _mockEntity.Object.GetComponent<SubLauncher>().OnSubLaunch += (sender, args) =>
            {
                subLaunchEventData = args;
            };

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameEventData()
            {
                EventData = launchEventData,
                Id = "asdf",
                IssuedBy = initialOwner.ToUser(),
                OccursAtTick = 1,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0));
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0));
            mockGameState.Setup(it => it.GetEntity(launchEventData.SourceId)).Returns(source);
            mockGameState.Setup(it => it.GetEntity(launchEventData.DestinationId)).Returns(destination);

            Sub capturedSub = null;
            mockGameState.Setup(it => it.AddSub(It.IsAny<Sub>())).Callback<Sub>(sub => { capturedSub = sub; });

            _mockEntity.Object.GetComponent<SubLauncher>().LaunchSub(mockGameState.Object, launchEvent);
            // Verify the sub was added to the state
            mockGameState.Verify(it => it.AddSub(subLaunchEventData.LaunchedSub), Times.Once);
            Assert.IsNotNull(capturedSub);
            Assert.AreEqual(destination, capturedSub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, capturedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, capturedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, capturedSub.GetComponent<SpecialistManager>().GetSpecialistCount());

            // Verify event data
            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            Assert.AreEqual(launchEventData.DrillerCount,
                subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetSpecialistCount());
        }


        [TestMethod]
        public void UndoingALaunchTriggersAnUndoLaunchEvent()
        {
            MockSubLauncherEntity(10, initialOwner, 3);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SubLauncher>());

            OnSubLaunchEventArgs subLaunchEventData = null;
            _mockEntity.Object.GetComponent<SubLauncher>().OnSubLaunch += (sender, args) =>
            {
                subLaunchEventData = args;
            };
            
            OnUndoSubLaunchEventArgs undoSubLaunchEvent = null;
            _mockEntity.Object.GetComponent<SubLauncher>().OnUndoSubLaunch += (sender, args) =>
            {
                undoSubLaunchEvent = args;
            };

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameEventData()
            {
                EventData = launchEventData,
                Id = "asdf",
                IssuedBy = initialOwner.ToUser(),
                OccursAtTick = 1,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0)); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0)); 
            mockGameState.Setup(it => it.GetEntity(launchEventData.SourceId)).Returns(source);
            mockGameState.Setup(it => it.GetEntity(launchEventData.DestinationId)).Returns(destination);
            
            Sub capturedSub = null;
            mockGameState.Setup(it => it.AddSub(It.IsAny<Sub>())).Callback<Sub>(sub => { capturedSub = sub; });
            
            _mockEntity.Object.GetComponent<SubLauncher>().LaunchSub(mockGameState.Object, launchEvent);
            // Verify the sub was added to the state
            mockGameState.Verify(it => it.AddSub(subLaunchEventData.LaunchedSub), Times.Once);
            Assert.IsNotNull(capturedSub);
            Assert.AreEqual(destination, capturedSub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, capturedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, capturedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, capturedSub.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            // Verify event data
            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            Assert.AreEqual(launchEventData.DrillerCount, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetSpecialistCount());
            
            _mockEntity.Object.GetComponent<SubLauncher>().UndoLaunch(mockGameState.Object, launchEvent);
            
            // Verify the sub that was launched is removed from the state
            mockGameState.Verify(it => it.RemoveSub(subLaunchEventData.LaunchedSub), Times.Once);
            // Verify event data
            Assert.IsNotNull(undoSubLaunchEvent);
            Assert.AreEqual(subLaunchEventData.LaunchedSub, undoSubLaunchEvent.LaunchedSub);
            Assert.AreEqual(subLaunchEventData.Destination, undoSubLaunchEvent.Destination);
            Assert.AreEqual(subLaunchEventData.Source, undoSubLaunchEvent.Source);
            Assert.AreEqual(subLaunchEventData.LaunchEvent, undoSubLaunchEvent.LaunchEvent);
        }
    }
}