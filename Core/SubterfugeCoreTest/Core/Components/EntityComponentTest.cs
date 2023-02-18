using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class EntityComponentTest
    {
        class ReturnSevenComponent : EntityComponent
        {
            public ReturnSevenComponent(Entity parent) : base(parent) { }

            public int Return()
            {
                return 7;
            }
        }
        class TestReturnIntComponent : EntityComponent
        {
            private int returnMe;
            
            public TestReturnIntComponent(Entity parent, int returnMe) : base(parent)
            {
                this.returnMe = returnMe;
            }

            public int Return()
            {
                return returnMe;
            }
        }

        private class TestEntity : Entity
        {
            public TestEntity(int returnInt)
            {
                AddComponent(new TestReturnIntComponent(this, returnInt));
            }
        }

        private class DuplicateComponentEntity : Entity
        {
            public DuplicateComponentEntity(int firstReturn, int secondReturn)
            {
                AddComponent(new TestReturnIntComponent(this, firstReturn));
                AddComponent(new TestReturnIntComponent(this, secondReturn));
            }
        }

        private class ManyComponentEntity : Entity
        {
            public ManyComponentEntity(int returnValue)
            {
                AddComponent(new TestReturnIntComponent(this, returnValue));
                AddComponent(new ReturnSevenComponent(this));
            }
        }
        
        class TestNullComponent : Entity { }

        [TestMethod]
        public void CanAddComponentsToAnEntity()
        {
            var returnValue = 7;
            var testEntity = new TestEntity(returnValue);
            Assert.IsTrue(testEntity.HasComponent<TestReturnIntComponent>());
            Assert.IsNotNull(testEntity.GetComponent<TestReturnIntComponent>());
            Assert.AreEqual(returnValue, testEntity.GetComponent<TestReturnIntComponent>().Return());
        }
        
        [TestMethod]
        public void ComponentCanReferenceTheParentEntity()
        {
            var returnValue = 7;
            var testEntity = new TestEntity(returnValue);
            Assert.IsTrue(testEntity.HasComponent<TestReturnIntComponent>());
            Assert.IsNotNull(testEntity.GetComponent<TestReturnIntComponent>());
            Assert.AreEqual(testEntity, testEntity.GetComponent<TestReturnIntComponent>().Parent);
        }
        
        [TestMethod]
        public void AttemptingToGetAComponentThatDoesntExistReturnsNull()
        {
            var testEntity = new TestNullComponent();
            Assert.IsFalse(testEntity.HasComponent<TestReturnIntComponent>());
            Assert.IsNull(testEntity.GetComponent<TestReturnIntComponent>());
        }

        [TestMethod]
        public void TwoComponentsOfTheSameTypeOverwrite()
        {
            var testEntity = new DuplicateComponentEntity(1, 2);
            Assert.IsTrue(testEntity.HasComponent<TestReturnIntComponent>());
            Assert.IsNotNull(testEntity.GetComponent<TestReturnIntComponent>());
            Assert.AreEqual(2, testEntity.GetComponent<TestReturnIntComponent>().Return());
        }
        
        [TestMethod]
        public void CanGetMultipleComponentsFromEntity()
        {
            var testEntity = new ManyComponentEntity(1);
            Assert.IsTrue(testEntity.HasComponent<TestReturnIntComponent>());
            Assert.IsNotNull(testEntity.GetComponent<TestReturnIntComponent>());
            Assert.AreEqual(1, testEntity.GetComponent<TestReturnIntComponent>().Return());
            
            
            // Assert.IsTrue(testEntity.HasComponent<ReturnSevenComponent>());
            Assert.IsNotNull(testEntity.GetComponent<ReturnSevenComponent>());
            Assert.AreEqual(7, testEntity.GetComponent<ReturnSevenComponent>().Return());
        }
    }
}