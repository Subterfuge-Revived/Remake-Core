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
        private Mock<IEntity> mockEntity;
        
        public void mockShieldManagerEntity(
            int shieldCapacity,
            int initialShields = 0
        )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<ShieldManager>())
                .Returns(new ShieldManager(mockEntity.Object, shieldCapacity, initialShields));
        }
        
        [TestMethod]
        public void CanInitializeShieldManager()
        {
            mockShieldManagerEntity(10);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void CanSetInitialShieldValue()
        {
            var initialShields = 10;
            var initialShieldCapacity = 20;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }

        [TestMethod]
        public void CanSetInitialShieldCapacity()
        {
            var initialShields = 10;
            mockShieldManagerEntity(initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void CannotSetInitialShieldValueAboveCapacity()
        {
            var initialShields = 100;
            var initialShieldCapacity = 20;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void CanSetShieldValue()
        {
            var initialShieldCapacity = 10;
            mockShieldManagerEntity(initialShieldCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCount = 5;
            mockEntity.Object.GetComponent<ShieldManager>().SetShields(newShieldCount);
            Assert.AreEqual(newShieldCount, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotSetShieldValueAboveCapacity()
        {
            var initialShieldCapacity = 10;
            mockShieldManagerEntity(initialShieldCapacity);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(0, mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCount = 25;
            mockEntity.Object.GetComponent<ShieldManager>().SetShields(newShieldCount);
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanRemoveShields()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToRemove = 40;
            mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(shieldsToRemove);
            Assert.AreEqual(initialShields - shieldsToRemove, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotRemoveMoreShieldsThanPresent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToRemove = initialShields + 40;
            mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(shieldsToRemove);
            Assert.AreEqual(0, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void ShieldIsActiveByDefault()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanDeactivateShield()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanReActivateShield()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = 10;
            mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShields + shieldsToAdd, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotExceedShieldCapacityWhenAddingShields()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = initialShieldCapacity * 2;
            mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanChangeTheShieldCapacity()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var newShieldCapacity = initialShieldCapacity * 2;
            mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
        }
        
        [TestMethod]
        public void DecreasingShieldCapacityWithMaxShieldsAlsoReducesTheShieldValue()
        {
            var initialShieldCapacity = 100;
            var initialShields = initialShieldCapacity;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var newShieldCapacity = initialShieldCapacity - 50;
            mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(newShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanAddShieldsPastOriginalCapacityAfterCapacityIsIncreased()
        {
            var initialShields = 10;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.AreEqual(initialShields, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());

            var shieldsToAdd = initialShieldCapacity * 2;
            mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());

            var newShieldCapacity = initialShieldCapacity * 2;
            mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(newShieldCapacity);
            Assert.AreEqual(newShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShieldCapacity());
            Assert.AreEqual(initialShieldCapacity, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
            
            shieldsToAdd = 10;
            mockEntity.Object.GetComponent<ShieldManager>().AddShield(shieldsToAdd);
            Assert.AreEqual(initialShieldCapacity + shieldsToAdd, mockEntity.Object.GetComponent<ShieldManager>().GetShields());
        }
        
        // Event Handler Tests
        [TestMethod]
        public void DeactivatingShieldsTriggersShieldDisabledEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());

            var eventFired = false;
            OnShieldDisableEventArgs disableArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldDisable += (sender, args) => {
                eventFired = true;
                disableArgs = args;
            };
            
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsFalse(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(disableArgs);
            Assert.AreEqual(disableArgs.ShieldManager, mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void ReActivatingShieldsTriggersShieldActivationEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(mockEntity.Object.GetComponent<ShieldManager>().IsShieldActive());

            var disableEventFired = false;
            OnShieldDisableEventArgs disableArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldDisable += (sender, args) =>
            {
                disableEventFired = true;
                disableArgs = args;
            };
            
            var enableEventFired = false;
            OnShieldEnableEventArgs enableArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldEnable += (sender, args) =>
            {
                enableEventFired = true;
                enableArgs = args;
            };
            
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            mockEntity.Object.GetComponent<ShieldManager>().ToggleShield();
            Assert.IsTrue(disableEventFired);
            Assert.IsNotNull(disableArgs);
            Assert.AreEqual(disableArgs.ShieldManager, mockEntity.Object.GetComponent<ShieldManager>());
            Assert.IsTrue(enableEventFired);
            Assert.IsNotNull(enableArgs);
            Assert.AreEqual(enableArgs.ShieldManager, mockEntity.Object.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void SettingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 100;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            mockEntity.Object.GetComponent<ShieldManager>().SetShields(1);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.previousValue);
            Assert.AreEqual(mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void AddingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            mockEntity.Object.GetComponent<ShieldManager>().AddShield(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.previousValue);
            Assert.AreEqual(mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void RemovingShieldValueTriggersShieldValueChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldValueChangeEventArgs valueChangeArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldValueChange += (sender, args) =>
            {
                eventFired = true;
                valueChangeArgs = args;
            };
            mockEntity.Object.GetComponent<ShieldManager>().RemoveShields(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(valueChangeArgs);
            Assert.AreEqual(initialShields, valueChangeArgs.previousValue);
            Assert.AreEqual(mockEntity.Object.GetComponent<ShieldManager>(), valueChangeArgs.ShieldManager);
        }
        
        [TestMethod]
        public void SettingShieldCapacityTriggersShieldCapacityChangeEvent()
        {
            var initialShields = 50;
            var initialShieldCapacity = 100;
            mockShieldManagerEntity(initialShieldCapacity, initialShields);
            Assert.IsNotNull(mockEntity.Object.GetComponent<ShieldManager>());

            var eventFired = false;
            OnShieldCapacityChangeEventArgs capacityChangeArgs = null;
            mockEntity.Object.GetComponent<ShieldManager>().OnShieldCapacityChange += (sender, args) =>
            {
                eventFired = true;
                capacityChangeArgs = args;
            };
            mockEntity.Object.GetComponent<ShieldManager>().SetShieldCapacity(10);
            
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(capacityChangeArgs);
            Assert.AreEqual(initialShieldCapacity, capacityChangeArgs.previousCapacity);
            Assert.AreEqual(mockEntity.Object.GetComponent<ShieldManager>(), capacityChangeArgs.ShieldManager);
        }
    }
}