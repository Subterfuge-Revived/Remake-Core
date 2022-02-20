using System;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Components
{
    public class IdentityManager : EntityComponent
    {
        private string id;
        private string name;
        
        public IdentityManager(IEntity parent, string id, string name) : base(parent)
        {
            this.id = id;
            this.name = name;
        }
        
        public IdentityManager(IEntity parent, string name = null) : base(parent)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
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