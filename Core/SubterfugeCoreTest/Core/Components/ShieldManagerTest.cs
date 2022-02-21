using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.EventArgs;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class ShieldManagerTest
    {
        private Mock<IEntity> _mockEntity;

        private void MockShieldManagerEntity(
            int shieldCapacity,
            int initialShields = 0
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<ShieldManager>())
                .Returns(new ShieldManager(_mockEntity.Object, shieldCapacity, initialShields));
        }
        
        [TestMethod]
        public void CanInitializeShieldManager()
        {
            MockShieldManagerEntity(10);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void CanSetInitialShieldValue()
        {
            var initialShields = 10;
            var initialShieldCapacity = 20;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }

        [TestMethod]
        public void CanSetInitialShieldCapacity()
        {
            var initialShields = 10;
            MockShieldManagerEntity(initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void CannotSetInitialShieldValueAboveCapacity()
        {
            var initialShields = 100;
            var initialShieldCapacity = 20;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void CanSetShieldValue()
        {
            var initialShieldCapacity = 10;
            MockShieldManagerEntity(initialShieldCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCount = 5;
            _mockEntity.Object.GetComponent<ShieldManager>().SetShields(newShieldCount);
            Assert.AreEqual(newShieldCount, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotSetShieldValueAboveCapacity()
        {
            var initialShieldCapacity = 10;
            MockShieldManagerEntity(initialShieldCapacity);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCount = 25;
            _mockEntity.Object.GetComponent<ShieldManager>().SetShields(newShieldCount);
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanRemoveShields()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToRemove = 40;
            _mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(shieldsToRemove);
            Assert.AreEqual(initialShields - shieldsToRemove, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotRemoveMoreShieldsThanPresent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToRemove = initialShields + 40;
            _mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(shieldsToRemove);
            Assert.AreEqual(0, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void ShieldIsActiveByDefault()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanDeactivateShield()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanReActivateShield()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = 10;
            _mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShields + shieldsToAdd, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotExceedShieldCapacityWhenAddingShields()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = initialShieldCapacity * 2;
            _mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanChangeTheShieldCapacity()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var newShieldCapacity = initialShieldCapacity * 2;
            _mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void DecreasingShieldCapacityWithMaxShieldsAlsoReducesTheShieldValue()
        {
            var initialShieldCapacity = 100;
            var initialShields = initialShieldCapacity;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var newShieldCapacity = initialShieldCapacity - 50;
            _mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(newShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanAddShieldsPastOriginalCapacityAfterCapacityIsIncreased()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = initialShieldCapacity * 2;
            _mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCapacity = initialShieldCapacity * 2;
            _mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(initialShieldCapacity, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            
            shieldsToAdd = 10;
            _mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity + shieldsToAdd, _mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        // Event Handler Tests
        [TestMethod]
        public void DeactivatingShieldsTriggersShieldDisabledEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());

            var eventFired = false;
            OnShieldDisableEventArgs disableArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldDisable += (sender, args) => {
                eventFired = true;
                disableArgs = args;
            };
            
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(disableArgs);
            Assert.AreEqual(disableArgs.ShieldManager, _mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void ReActivatingShieldsTriggersShieldActivationEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(_mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());

            var disableEventFired = false;
            OnShieldDisableEventArgs disableArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldDisable += (sender, args) =>
            {
                disableEventFired = true;
                disableArgs = args;
            };
            
            var enableEventFired = false;
            OnShieldEnableEventArgs enableArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldEnable += (sender, args) =>
            {
                enableEventFired = true;
                enableArgs = args;
            };
            
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            _mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsTrue(disableEventFired);
            Assert.IsNotNull(disableArgs);
            Assert.AreEqual(disableArgs.ShieldManager, _mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(enableEventFired);
            Assert.IsNotNull(enableArgs);
            Assert.AreEqual(enableArgs.ShieldManager, _mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void SettingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            _mockEntity.Object.GetComponent<ShieldManager>().SetShields(1);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.PreviousValue);
            Assert.AreEqual(_mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void AddingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            _mockEntity.Object.GetComponent<ShieldManager>().AddShield(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.PreviousValue);
            Assert.AreEqual(_mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void RemovingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            _mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.PreviousValue);
            Assert.AreEqual(_mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void SettingShieldCapacityTriggersShieldCapacityChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            MockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldCapacityChangeEventArgs capacityChangeArgs = null;
            _mockEntity.Object.GetComponent<ShieldManager>().OnShieldCapacityChange += (sender, args) =>
            {
                eventFired = true;
                capacityChangeArgs = args;
            };
            _mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(capacityChangeArgs);
            Assert.AreEqual(initialShieldCapacity, capacityChangeArgs.PreviousCapacity);
            Assert.AreEqual(_mockEntity.Object.GetComponent<ShieldManager>(), capacityChangeArgs.ShieldManager);
        }
    }
}