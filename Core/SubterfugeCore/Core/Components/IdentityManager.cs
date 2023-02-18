using System;
using Subterfuge.Remake.Core.Entities;

namespace Subterfuge.Remake.Core.Components
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
        
        public IdentityManager(IEntity parent, string name = null) : base(parent)
        {
            this._id = Guid.NewGuid().ToString();
            this._name = name;
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