using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class SubLauncherTest
    {
        private Mock<IEntity> _mockEntity;
        private Player initialOwner = new Player(new SimpleUser(){ Id = "1" });

        private List<Player> players;
        private TimeMachine _timeMachine;
        
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
        
        [TestInitialize]
        public void Setup()
        {
            players = new List<Player>()
            {
                initialOwner
            };
            _timeMachine = new TimeMachine(new GameState(players));
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

            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(launchEventData),
                },
                Id = "asdf",
                IssuedBy = initialOwner.PlayerInstance,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0), _timeMachine); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0), _timeMachine);
            
            _timeMachine.GetState().GetOutposts().Add(destination);
            _timeMachine.GetState().GetOutposts().Add(source);

            var launchedSub = _mockEntity.Object.GetComponent<SubLauncher>().LaunchSub(_timeMachine, launchEvent);
            
            // Verify the sub was added to the state
            Assert.IsNotNull(launchedSub);
            Assert.AreEqual(destination, launchedSub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, launchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, launchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, launchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
        }

        [TestMethod]
        public void CanUndoASubLaunch()
        {
            MockSubLauncherEntity(10, initialOwner, 3);

            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(launchEventData),
                },
                Id = "asdf",
                IssuedBy = initialOwner.PlayerInstance,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0), _timeMachine); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0), initialOwner, _timeMachine);
            var initialDrillerCount = source.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            OnSubLaunchEventArgs subLaunchEventData = null;
            OnSubLaunchEventArgs undoSubLaunch = null;
            source.GetComponent<SubLauncher>().OnSubLaunched += (sender, args) =>
            {
                if (args.Direction == TimeMachineDirection.FORWARD)
                {
                    subLaunchEventData = args;   
                }
                else
                {
                    undoSubLaunch = args;
                }
            };
            
            _timeMachine.GetState().GetOutposts().Add(destination);
            _timeMachine.GetState().GetOutposts().Add(source);
            
            var launchedSub = source.GetComponent<SubLauncher>().LaunchSub(_timeMachine, launchEvent);

            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            
            // Verify the sub was added to the state
            Assert.IsNotNull(launchedSub);
            Assert.AreEqual(launchEventData.DrillerCount, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(initialDrillerCount - launchEvent.GetEventData().DrillerCount, source.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            source.GetComponent<SubLauncher>().UndoLaunch(_timeMachine, launchEvent);
            
            // Verify the sub that was launched, is removed from the state, and the outposts have their original driller counts.
            Assert.IsNotNull(undoSubLaunch);
            Assert.AreEqual(initialDrillerCount, source.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
        }
        
        // Event Handler Tests
        [TestMethod]
        public void LaunchingASubTriggersAnOnLaunchEvent()
        {
            MockSubLauncherEntity(10, initialOwner, 3);

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(launchEventData),
                },
                Id = "asdf",
                IssuedBy = initialOwner.PlayerInstance,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0), _timeMachine);
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0), initialOwner, _timeMachine);
            
            _timeMachine.GetState().GetOutposts().Add(destination);
            _timeMachine.GetState().GetOutposts().Add(source);

            OnSubLaunchEventArgs subLaunchEventData = null;
            source.GetComponent<SubLauncher>().OnSubLaunched += (source, launchArgs) =>
            {
                if (launchArgs.Direction == TimeMachineDirection.FORWARD)
                {
                    subLaunchEventData = launchArgs;
                }
            };

            var sub = source.GetComponent<SubLauncher>().LaunchSub(_timeMachine, launchEvent);
            
            Assert.IsNotNull(sub);
            Assert.AreEqual(destination, sub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, sub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, sub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, sub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());

            // Verify event data
            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            Assert.AreEqual(launchEventData.DrillerCount,
                subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
        }


        [TestMethod]
        public void UndoingALaunchTriggersAnUndoLaunchEvent()
        {
            MockSubLauncherEntity(10, initialOwner, 3);

            var mockGameState = new Mock<IGameState>();
            var launchEventData = new LaunchEventData()
            {
                DestinationId = "1",
                DrillerCount = 5,
                SourceId = "2"
            };
            var launchEvent = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(launchEventData),
                },
                Id = "asdf",
                IssuedBy = initialOwner.PlayerInstance,
                RoomId = "1",
                TimeIssued = DateTime.FromFileTimeUtc(1234123412341234),
            });

            RftVector.Map = new Rft(100, 100);
            var destination = new Generator(launchEventData.DestinationId, new RftVector(0, 0), _timeMachine); 
            var source = new Generator(launchEventData.SourceId, new RftVector(0, 0), initialOwner, _timeMachine); 
            
            _timeMachine.GetState().GetOutposts().Add(destination);
            _timeMachine.GetState().GetOutposts().Add(source);
            
            OnSubLaunchEventArgs subLaunchEventData = null;
            OnSubLaunchEventArgs undoSubLaunch = null;
            source.GetComponent<SubLauncher>().OnSubLaunched += (sender, args) =>
            {
                if (args.Direction == TimeMachineDirection.FORWARD)
                {
                    subLaunchEventData = args;   
                }
                else
                {
                    undoSubLaunch = args;
                }
            };
            
            
            var launchedSub = source.GetComponent<SubLauncher>().LaunchSub(_timeMachine, launchEvent);
            // Verify the sub was added to the state
            Assert.IsNotNull(launchedSub);
            Assert.AreEqual(destination, launchedSub.GetComponent<PositionManager>().GetDestination());
            Assert.AreEqual(launchEventData.DrillerCount, launchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, launchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, launchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            
            // Verify event data
            Assert.IsNotNull(subLaunchEventData);
            Assert.AreEqual(destination, subLaunchEventData.Destination);
            Assert.AreEqual(source, subLaunchEventData.Source);
            Assert.IsNotNull(subLaunchEventData.LaunchedSub);
            Assert.AreEqual(launchEventData.DrillerCount, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(initialOwner, subLaunchEventData.LaunchedSub.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(0, subLaunchEventData.LaunchedSub.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount());
            
            source.GetComponent<SubLauncher>().UndoLaunch(_timeMachine, launchEvent);
            
            // Verify the sub that was launched is removed from the state
            // Verify event data
            Assert.IsNotNull(undoSubLaunch);
            Assert.AreEqual(subLaunchEventData.LaunchedSub, undoSubLaunch.LaunchedSub);
            Assert.AreEqual(subLaunchEventData.Source, undoSubLaunch.Source);
            Assert.AreEqual(subLaunchEventData.LaunchEvent, undoSubLaunch.LaunchEvent);
        }
    }
}