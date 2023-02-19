using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class PositionManagerTest
    {
        private Mock<IEntity> _mockDestinationEntity;
        private Mock<IEntity> _mockChaserEntity;
        private Mock<IEntity> _mockChaserChaserEntity;
        private void MockDestinationPositionManagerEntity(
            float speed,
            RftVector initialLocation,
            IEntity destination = null,
            int launchTick = 0
        )
        {
            _mockDestinationEntity = new Mock<IEntity>();
            _mockDestinationEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockDestinationEntity.Object, speed));
            _mockDestinationEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockDestinationEntity.Object, initialLocation, destination, new GameTick(launchTick)));
        }
        
        private void MockChaserPositionManagerEntity(
            float speed,
            RftVector initialLocation,
            int launchTick = 0
        )
        {
            _mockChaserEntity = new Mock<IEntity>();
            _mockChaserEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockChaserEntity.Object, speed));
            _mockChaserEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockChaserEntity.Object, initialLocation, _mockDestinationEntity.Object, new GameTick(launchTick)));
        }
        
        private void MockChasersChaserPositionManagerEntity(
            float speed,
            RftVector initialLocation,
            int launchTick = 0
        )
        {
            _mockChaserChaserEntity = new Mock<IEntity>();
            _mockChaserChaserEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockChaserChaserEntity.Object, speed));
            _mockChaserChaserEntity.Setup(it => it.GetComponent<PositionManager>())
                .Returns(new PositionManager(_mockChaserChaserEntity.Object, initialLocation, _mockChaserEntity.Object, new GameTick(launchTick)));
        }

        [TestInitialize]
        public void Setup()
        {
            RftVector.Map = new Rft(1000, 1000);
        }

        /*
        [TestMethod]
        public void CanInstantiateAPositionManager()
        {
            MockDestinationPositionManagerEntity(0.0f, new RftVector(1, 0));
        }

        [TestMethod]
        public void PositionOfNonMovingPositionManagerAtAnyTickIsTheInitialLocation()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            Assert.AreEqual(0, positionManager.GetSpawnTick().GetTick());
            Assert.AreEqual(initialLocation.X, positionManager.GetPositionAt(new GameTick(0)).X);
            Assert.AreEqual(initialLocation.Y, positionManager.GetPositionAt(new GameTick(0)).Y);
            Assert.AreEqual(initialLocation.X, positionManager.GetPositionAt(new GameTick(14)).X);
            Assert.AreEqual(initialLocation.Y, positionManager.GetPositionAt(new GameTick(144)).Y);
            Assert.AreEqual(initialLocation.X, positionManager.GetPositionAt(new GameTick(463)).X);
            Assert.AreEqual(initialLocation.Y, positionManager.GetPositionAt(new GameTick(6875)).Y);
        }
        
        [TestMethod]
        public void TheDirectionOfANonMovingPositionIsAZeroVector()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            Assert.AreEqual(0, positionManager.GetDirection().X);
            Assert.AreEqual(0, positionManager.GetDirection().Y);
        }
        
        [TestMethod]
        public void TheDestinationOfANonMovingPositionIsNull()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            Assert.IsNull(positionManager.GetDestination());
        }
        
        [TestMethod]
        public void APositionWithANullDestinationHaANullExpectedArrival()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            
            Assert.IsNull(positionManager.GetExpectedArrival());
        }
        
        [TestMethod]
        public void ExpectedDestinationOfNonMovingPositionIsItsInitialLocation()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(initialLocation.X, positionManager.GetExpectedDestination().X);
            Assert.AreEqual(initialLocation.Y, positionManager.GetExpectedDestination().Y);
        }
        
        [TestMethod]
        public void TheInterceptionPointOfANonMovingPositionIsItsInitialLocation()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);
            var positionManager = _mockDestinationEntity.Object.GetComponent<PositionManager>();
            Assert.AreEqual(0, positionManager.GetSpawnTick().GetTick());
            Assert.AreEqual(initialLocation.X, positionManager.GetInterceptionPosition(new RftVector(100, 0), 1.0f).X);
            Assert.AreEqual(initialLocation.Y, positionManager.GetInterceptionPosition(new RftVector(100, 0), 1.0f).Y);
        }
        
        [TestMethod]
        public void AMovingEntityCanTargetAStaticPosition()
        {
            var initialLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, initialLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
        }
        
        [TestMethod]
        public void AMovingEntitysDestinationLocationIsTheSameLocationAsTheStaticPosition()
        {
            var destinationLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetExpectedDestination().Y);
            
            // Test Two
            destinationLocation = new RftVector(75, 4);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(56, 76);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetExpectedDestination().Y);
        }
        
        [TestMethod]
        public void AChaserStartingAtZeroZeroHasDirectionSetToTheDestinationLocation()
        {
            var destinationLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetDirection().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetDirection().Y);
            
            // Test Two
            destinationLocation = new RftVector(54, -13);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetDirection().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetDirection().Y);
        }
        
        [TestMethod]
        public void AChasersOfSpeedOneHasExpectedArrivalOfOneTickMorePerDistanceUnitToItsDestination()
        {
            // Test One, 1 unit away at speed 1 should take 1 tick
            var destinationLocation = new RftVector(1, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(1, chaserPositionManager.GetExpectedArrival().GetTick());
            
            // Test Two, 100 units away at speed 1 should take 100 ticks
            destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(100, chaserPositionManager.GetExpectedArrival().GetTick());
        }
        
        [TestMethod]
        public void AChasersSpeedEffectsTheArrivalTime()
        {
            // Test One, At speed 1, 100 units takes 100 ticks
            var destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(100, chaserPositionManager.GetExpectedArrival().GetTick());
            
            // Test Two, At speed 2, 100 units takes 50 ticks
            destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(2.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(50, chaserPositionManager.GetExpectedArrival().GetTick());
            
            // Test Two, At speed 4, 100 units takes 25 ticks
            destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(4.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(25, chaserPositionManager.GetExpectedArrival().GetTick());
            
            // Test Three, At speed 10, 100 units takes 10 ticks
            destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(10.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(10, chaserPositionManager.GetExpectedArrival().GetTick());
        }
        
        [TestMethod]
        public void CanGetChasersPositionAtAnyTickDuringTravel()
        {
            // Test One, 100 units over 100 ticks. Halfway is 50
            var destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            // Ensure interpolation works along the line
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(100, chaserPositionManager.GetExpectedArrival().GetTick());
            for (var x = 0; x <= 100; x++)
            {
                Assert.AreEqual(x, chaserPositionManager.GetPositionAt(new GameTick(x)).X);
                Assert.AreEqual(0, chaserPositionManager.GetPositionAt(new GameTick(x)).Y);    
            }

            // Test Two, 100 units over 50 ticks.
            destinationLocation = new RftVector(100, 0);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            MockChaserPositionManagerEntity(2.0f, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(50, chaserPositionManager.GetExpectedArrival().GetTick());
            for (var x = 0; x <= 50; x++)
            {
                Assert.AreEqual(x*2, chaserPositionManager.GetPositionAt(new GameTick(x)).X);
                Assert.AreEqual(0, chaserPositionManager.GetPositionAt(new GameTick(x)).Y);    
            }
            
            // Test Three, random distance.
            destinationLocation = new RftVector(131, 121);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            chaserLocation = new RftVector(0, 0);
            var speed = 1.2f;
            MockChaserPositionManagerEntity(speed, chaserLocation);
            chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();

            var totalDistance = destinationLocation.Magnitude();
            var normalizedDirection = destinationLocation.Normalize();
            var expectedTicks = Math.Floor(totalDistance / speed);
            var lastTick = (int)Math.Floor(totalDistance / speed);
            
            Assert.AreEqual(_mockDestinationEntity.Object, chaserPositionManager.GetDestination());
            Assert.AreEqual(expectedTicks, chaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(normalizedDirection.X, chaserPositionManager.GetDirection().Normalize().X);
            Assert.AreEqual(normalizedDirection.Y, chaserPositionManager.GetDirection().Normalize().Y);
            for (var x = 0; x <= expectedTicks; x++)
            {
                Assert.AreEqual((normalizedDirection * x * speed).X, chaserPositionManager.GetPositionAt(new GameTick(x)).X, 0.01);
                Assert.AreEqual((normalizedDirection * x * speed).Y, chaserPositionManager.GetPositionAt(new GameTick(x)).Y, 0.01);    
            }
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetPositionAt(new GameTick(lastTick)).X, speed/2);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetPositionAt(new GameTick(lastTick)).Y, speed/2);
        }
        
        [TestMethod]
        public void ChaserCanChaseAChaser()
        {
            var destinationLocation = new RftVector(1, 1);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(-1, 1);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();

            var chasersChaserInitialPosition = new RftVector(0, 0);
            MockChasersChaserPositionManagerEntity(1.0f, chasersChaserInitialPosition);
            var chaserChaserPositionManager = _mockChaserChaserEntity.Object.GetComponent<PositionManager>();
            Assert.AreEqual(_mockChaserEntity.Object, chaserChaserPositionManager.GetDestination());
        }
        */
        [TestMethod]
        public void ChaserOfChaserHasCorrectInterceptionPoint()
        {
            var destinationLocation = new RftVector(100, 100);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(-100, 100);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            // 200 units total.
            Assert.AreEqual(200, chaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetExpectedDestination().Y);
            // At tick 100, this unit will be at (0, 100)
            Assert.AreEqual(0, chaserPositionManager.GetPositionAt(new GameTick(100)).X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetPositionAt(new GameTick(100)).Y);
            
            // Therefore, if a chaser starts at (0, 0), it can intercept this sub at (0, 100) in 100 ticks.
            

            var chasersChaserInitialPosition = new RftVector(0, 0);
            MockChasersChaserPositionManagerEntity(1.0f, chasersChaserInitialPosition);
            var chaserChaserPositionManager = _mockChaserChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.IsTrue(_mockChaserEntity.Object.GetComponent<SpeedManager>().GetSpeed() > 0.001);
            Assert.AreEqual(_mockChaserEntity.Object, chaserChaserPositionManager.GetDestination());
            
            Assert.AreEqual(100, chaserChaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(0, chaserChaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(100, chaserChaserPositionManager.GetExpectedDestination().Y);
            Assert.AreEqual(0, chaserChaserPositionManager.GetUnitDirection().X);
            Assert.AreEqual(100, chaserChaserPositionManager.GetUnitDirection().Y);
        }
        
        [TestMethod]
        public void ChaserOfChasersSpeedEffectsTheInterceptionPoint()
        {
            var destinationLocation = new RftVector(100, 200);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(-100, 200);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            // 200 units total.
            Assert.AreEqual(200, chaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetExpectedDestination().Y);
            // At tick 100, this unit will be at (0, 200)
            Assert.AreEqual(0, chaserPositionManager.GetPositionAt(new GameTick(100)).X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetPositionAt(new GameTick(100)).Y);
            
            // Therefore, if a chaser starts at (0, 0), with speed 2 it can intercept this sub at (0, 200) in 100 ticks.
            

            var chasersChaserInitialPosition = new RftVector(0, 0);
            MockChasersChaserPositionManagerEntity(2.0f, chasersChaserInitialPosition);
            var chaserChaserPositionManager = _mockChaserChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.IsTrue(_mockChaserEntity.Object.GetComponent<SpeedManager>().GetSpeed() > 0.001);
            Assert.AreEqual(_mockChaserEntity.Object, chaserChaserPositionManager.GetDestination());
            
            Assert.AreEqual(100, chaserChaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(0, chaserChaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(200, chaserChaserPositionManager.GetExpectedDestination().Y);
            Assert.AreEqual(0, chaserChaserPositionManager.GetUnitDirection().X);
            Assert.AreEqual(200, chaserChaserPositionManager.GetUnitDirection().Y);
        }
        
        [TestMethod]
        public void ChaserCanChaseFromBehindIfItIsFaster()
        {
            var destinationLocation = new RftVector(100, 200);
            MockDestinationPositionManagerEntity(0.0f, destinationLocation);

            var chaserLocation = new RftVector(-100, 200);
            MockChaserPositionManagerEntity(1.0f, chaserLocation);
            var chaserPositionManager = _mockChaserEntity.Object.GetComponent<PositionManager>();
            // 200 units total.
            Assert.AreEqual(200, chaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(destinationLocation.X, chaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetExpectedDestination().Y);
            // At tick 100, this unit will be at (0, 200)
            Assert.AreEqual(0, chaserPositionManager.GetPositionAt(new GameTick(100)).X);
            Assert.AreEqual(destinationLocation.Y, chaserPositionManager.GetPositionAt(new GameTick(100)).Y);
            
            // Therefore, if a chaser starts at the same position but a bit behind at double speed, it can intercept this sub at (0, 200) in 100 ticks.
            var chasersChaserInitialPosition = new RftVector(-200, 200);
            MockChasersChaserPositionManagerEntity(2.0f, chasersChaserInitialPosition);
            var chaserChaserPositionManager = _mockChaserChaserEntity.Object.GetComponent<PositionManager>();
            
            Assert.IsTrue(_mockChaserEntity.Object.GetComponent<SpeedManager>().GetSpeed() > 0.001);
            Assert.AreEqual(_mockChaserEntity.Object, chaserChaserPositionManager.GetDestination());
            
            Assert.AreEqual(100, chaserChaserPositionManager.GetExpectedArrival().GetTick());
            Assert.AreEqual(0, chaserChaserPositionManager.GetExpectedDestination().X);
            Assert.AreEqual(200, chaserChaserPositionManager.GetExpectedDestination().Y);
            Assert.AreEqual(200, chaserChaserPositionManager.GetUnitDirection().X);
            Assert.AreEqual(0, chaserChaserPositionManager.GetUnitDirection().Y);
        }
    }
}