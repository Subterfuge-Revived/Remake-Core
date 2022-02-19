using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Components
{
    public class VisionManager : EntityComponent
    {
        private float _visionRange;
        private PositionManager position;
        private List<Entity> _lastCheckedEntitiesInRange = new List<Entity>();
        
        public event EventHandler<OnVisionRangeChangeEventArgs> OnVisionRangeChange;
        public event EventHandler<OnEntityEnterVisionRangeEventArgs> OnEntityEnterVisionRange;
        public event EventHandler<OnEntityLeaveVisionRangeEventArgs> OnEntityLeaveVisionRange;
        
        public VisionManager(Entity parent, float visionRange) : base(parent)
        {
            this._visionRange = visionRange;
            this.position = parent.GetComponent<PositionManager>();
        }

        /// <summary>
        /// Gets the range of vision for the current object
        /// </summary>
        /// <returns>The object's vision range</returns>
        public float GetVisionRange()
        {
            return this._visionRange;
        }

        /// <summary>
        /// Sets the vision range to the specified value.
        /// </summary>
        /// <param name="newVisionRange">The new vision range.</param>
        public void SetVisionRange(float newVisionRange, GameState state, GameTick tick)
        {
            var previousVisionRange = this._visionRange;
            this._visionRange = newVisionRange;
            
            OnVisionRangeChange(this, new OnVisionRangeChangeEventArgs()
            {
                NewVisionRange = newVisionRange,
                PreviousVisionRange = previousVisionRange,
                VisionManager = this,
            });
            
            // Update what the vision manager can see.
            GetEntitiesInVisionRange(state, tick);
        }


        /// <summary>
        /// Determines if the position is in vision of this object.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>If the object is in the vision range.</returns>
        public bool IsInVisionRange(GameTick tick, PositionManager positionManager)
        {
            var position = this.position.GetPositionAt(tick);
            return Vector2.Distance(position.ToVector2(), positionManager.GetPositionAt(tick).ToVector2()) > this._visionRange;
        }

        public List<Entity> GetEntitiesInVisionRange(GameState state, GameTick tick)
        {
            var currentPosition = this.position.GetPositionAt(tick);
            var currentEntitiesInRange = state.EntitesInRange(this._visionRange, currentPosition);
            
            // Check entities that have left the vision range (or that have died/etc.)
            foreach (Entity e in _lastCheckedEntitiesInRange)
            {
                if (!currentEntitiesInRange.Contains(e))
                {
                    OnEntityLeaveVisionRange?.Invoke(this, new OnEntityLeaveVisionRangeEventArgs()
                    {
                        EntityLeavingVision = e,
                        VisionManager = this,
                    });
                }
            }
            
            // Check new entities in range
            foreach (Entity e in currentEntitiesInRange)
            {
                if (!_lastCheckedEntitiesInRange.Contains(e))
                {
                    OnEntityEnterVisionRange?.Invoke(this, new OnEntityEnterVisionRangeEventArgs()
                    {
                        EntityInVision = e,
                        VisionManager = this,
                    });
                }
            }

            _lastCheckedEntitiesInRange = currentEntitiesInRange;
            return _lastCheckedEntitiesInRange;
        }
    }
}