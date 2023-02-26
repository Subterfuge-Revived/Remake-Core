using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;

namespace Subterfuge.Remake.Test.Core.Components
{
    [TestClass]
    public class IdentityManagerTest
    {
        private Mock<IEntity> _mockEntity;

        private void MockIdentityManagerEntity(
            string id,
            string name = null
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<IdentityManager>())
                .Returns(new IdentityManager(_mockEntity.Object, id, name));
        }

        private void MockIdentityManagerEntityWithoutId(
            string name = null
        )
        {
            _mockEntity = new Mock<IEntity>();
            _mockEntity.Setup(it => it.GetComponent<IdentityManager>())
                .Returns(new IdentityManager(_mockEntity.Object, Guid.NewGuid().ToString(), name));
        }
        
        [TestMethod]
        public void CanInitializeIdentityManager()
        {
            MockIdentityManagerEntityWithoutId();
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
        }
        
        [TestMethod]
        public void GuidIsGeneratedIfNotSet()
        {
            MockIdentityManagerEntityWithoutId();
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>().GetId());
        }
        
        [TestMethod]
        public void NameCanBeNull()
        {
            MockIdentityManagerEntityWithoutId();
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
            Assert.IsNull(_mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetCustomName()
        {
            var name = "CustomName";
            MockIdentityManagerEntityWithoutId(name);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(name, _mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetCustomId()
        {
            var id = Guid.NewGuid().ToString();
            var name = "CustomName";
            MockIdentityManagerEntity(id, name);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(id, _mockEntity.Object.GetComponent<IdentityManager>().GetId());
            Assert.AreEqual(name, _mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
        
        [TestMethod]
        public void CanSetNameAfterCreation()
        {
            var id = Guid.NewGuid().ToString();
            var name = "CustomName";
            MockIdentityManagerEntity(id, name);
            Assert.IsNotNull(_mockEntity.Object.GetComponent<IdentityManager>());
            Assert.AreEqual(name, _mockEntity.Object.GetComponent<IdentityManager>().GetName());

            var newName = "NewName";
            _mockEntity.Object.GetComponent<IdentityManager>().SetName(newName);
            Assert.AreEqual(newName, _mockEntity.Object.GetComponent<IdentityManager>().GetName());
        }
    }
}