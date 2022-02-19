using System;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Components
{
    public class IdentityManager : EntityComponent
    {
        private string id;
        private string name;
        
        public IdentityManager(Entity parent, string id, string name) : base(parent)
        {
            this.id = id;
            this.name = name;
        }
        
        public IdentityManager(Entity parent, string name) : base(parent)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
        }

        public IdentityManager(Entity parent) : base(parent)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = null;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetId()
        {
            return this.id;
        }

        public string GetName()
        {
            return this.name;
        }
    }
}