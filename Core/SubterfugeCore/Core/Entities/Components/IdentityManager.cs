using System;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class IdentityManager : EntityComponent
    {
        private readonly string _id;
        private string _name;
        
        public IdentityManager(IEntity parent, string id, string name) : base(parent)
        {
            this._id = id;
            this._name = name;
        }
        
        public IdentityManager(IEntity parent, string id = null) : base(parent)
        {
            this._id = id;
            this._name = Guid.NewGuid().ToString();
        }

        public void SetName(string name)
        {
            this._name = name;
        }

        public string GetId()
        {
            return this._id;
        }

        public string GetName()
        {
            return this._name;
        }
    }
}