using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.EventArgs;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class SpeedManagerTest
    {
        private Mock<IEntity> mockEntity;
        
        public void mockSpeedManagerEntity(
            float initialSpeed = 1.0f
        )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<SpeedManager>())
                .Returns(new SpeedManager(mockEntity.Object, initialSpeed));
        }
        
        [TestMethod]
        public void CanInitializeSpeedManager()
        {
            mockSpeedManagerEntity();
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
        }

        [TestMethod]
        public void CanSetTheInitialSpeed()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanIncreaseSpeed()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var increaseBy = 0.5f;
            mockEntity.Object.GetComponent<SpeedManager>().IncreaseSpeed(increaseBy);
            Assert.AreEqual(initialSpeed + increaseBy, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanDecreaseSpeed()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var decreaseBy = 0.5f;
            mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(decreaseBy);
            Assert.AreEqual(initialSpeed - decreaseBy, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        [TestMethod]
        public void CanSetSpeed()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            var newSpeed = 5.0f;
            mockEntity.Object.GetComponent<SpeedManager>().SetSpeed(newSpeed);
            Assert.AreEqual(newSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
        }
        
        // Event Handler tests
        [TestMethod]
        public void IncreasingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var increaseBy = 0.5f;
            mockEntity.Object.GetComponent<SpeedManager>().IncreaseSpeed(increaseBy);
            Assert.AreEqual(initialSpeed + increaseBy, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.previousSpeed);
            Assert.AreEqual(initialSpeed + increaseBy, speedChangeArgs.newSpeed);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
        
        [TestMethod]
        public void DecreasingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var decreaseBy = 0.5f;
            mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(decreaseBy);
            Assert.AreEqual(initialSpeed - decreaseBy, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.previousSpeed);
            Assert.AreEqual(initialSpeed - decreaseBy, speedChangeArgs.newSpeed);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
        
        [TestMethod]
        public void SettingSpeedTriggersSpeedChangedEvent()
        {
            float initialSpeed = 10.0f;
            mockSpeedManagerEntity(initialSpeed);
            Assert.IsNotNull(mockEntity.Object.GetComponent<SpeedManager>());
            Assert.AreEqual(initialSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());

            OnSpeedChangedEventArgs speedChangeArgs = null;
            mockEntity.Object.GetComponent<SpeedManager>().OnSpeedChanged += (sender, args) =>
            {
                speedChangeArgs = args;
            };

            var newSpeed = 5.0f;
            mockEntity.Object.GetComponent<SpeedManager>().DecreaseSpeed(newSpeed);
            Assert.AreEqual(newSpeed, mockEntity.Object.GetComponent<SpeedManager>().GetSpeed());
            
            Assert.IsNotNull(speedChangeArgs);
            Assert.AreEqual(initialSpeed, speedChangeArgs.previousSpeed);
            Assert.AreEqual(newSpeed, speedChangeArgs.newSpeed);
            Assert.AreEqual(mockEntity.Object.GetComponent<SpeedManager>(), speedChangeArgs.SpeedManager);
        }
    }
}