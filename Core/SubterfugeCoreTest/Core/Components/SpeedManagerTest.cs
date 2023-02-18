using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.EventArgs;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class SpeedManagerTest
    {
        private Mock<IEntity> _mockEntity;
        
        public void MockSpeedManagerEntity(
            float initialSpeed = 1.0f
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(_mockEntity.Object, initialSpeed));
        }
        
        [TestMethod]
        public void CanInitializeSpeedManager()
        {
            MockSpeedManagerEntity();
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
        }

        [TestMethod]
        public void CanSetTheInitialSpeed()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanIncreaseSpeed()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var increaseBy = 0.5f;
            _mockEntity.Object.GetComponent<SpeedManager>().IncreaseSpeed(increaseBy);
            Assert.AreEqual(initialSpeed + increaseBy, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanDecreaseSpeed()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var decreaseBy = 0.5f;
            _mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(decreaseBy);
            Assert.AreEqual(initialSpeed - decreaseBy, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanSetSpeed()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var newSpeed = 5.0f;
            _mockEntity.Object.GetComponent<SpeedManager>().SetSpeed(newSpeed);
            Assert.AreEqual(newSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        // Event Handler tests
        [TestMethod]
        public void IncreasingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            _mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var increaseBy = 0.5f;
            _mockEntity.Object.GetComponent<SpeedManager>().IncreaseSpeed(increaseBy);
            Assert.AreEqual(initialSpeed + increaseBy, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.PreviousSpeed);
            Assert.AreEqual(initialSpeed + increaseBy, speedChangeArgs.NewSpeed);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
        
        [TestMethod]
        public void DecreasingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            _mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var decreaseBy = 0.5f;
            _mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(decreaseBy);
            Assert.AreEqual(initialSpeed - decreaseBy, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.PreviousSpeed);
            Assert.AreEqual(initialSpeed - decreaseBy, speedChangeArgs.NewSpeed);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
        
        [TestMethod]
        public void SettingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            MockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            _mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var newSpeed = 5.0f;
            _mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(newSpeed);
            Assert.AreEqual(newSpeed, _mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.PreviousSpeed);
            Assert.AreEqual(newSpeed, speedChangeArgs.NewSpeed);
            Assert.AreEqual(_mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
    }
}