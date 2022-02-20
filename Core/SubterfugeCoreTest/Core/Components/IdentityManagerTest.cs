using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest.Core.Components
{
    [TestClass]
    public class IdentityManagerTest
    {
        private Mock<IEntity> mockEntity;
        
        public void mockIdentityManagerEntity(
            string id,
            string name = null
        )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<IdentityManager>())
                .Returns(new IdentityManager(mockEntity.Object, id, name));
        }

        public void mockIdentityManagerEntity(
            string name = null
        )
        {
            mockEntity = new Mock<IEntity>();
            mockEntity.Setup(it => it.GetComponent<IdentityManager>())
                .Returns(new IdentityManager(mockEntity.Object, Guid.NewGuid().ToString(), name));
        }
        
        [TestMethod]
        public void CanInitializeIdentityManager()
        {
            mockIdentityManagerEntity();
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
        }
        
        [TestMethod]
        public void GuidIsGeneratedIfNotSet()
        {
            mockIdentityManagerEntity();
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>().GetId());
        }
        
        [TestMethod]
        public void NameCanBeNull()
        {
            mockIdentityManagerEntity();
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
            Assert.IsNull(mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetCustomName()
        {
            var name = "CustomName";
            mockIdentityManagerEntity(name);
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(name, mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetCustomId()
        {
            var id = Guid.NewGuid().ToString();
            var name = "CustomName";
            mockIdentityManagerEntity(id, name);
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(id, mockEntity.Object.GetComponent<IdentityManager>().GetId());
            Assert.AreEqual(name, mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetNameAfterCreation()
        {
            var id = Guid.NewGuid().ToString();
            var name = "CustomName";
            mockIdentityManagerEntity(id, name);
            Assert.IsNotNull(mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(name, mockEntity.Object.GetComponent<IdentityManager>().GetName());

            var newName = "NewName";
            mockEntity.Object.GetComponent<IdentityManager>().SetName(newName);
            Assert.AreEqual(newName, mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
    }
}