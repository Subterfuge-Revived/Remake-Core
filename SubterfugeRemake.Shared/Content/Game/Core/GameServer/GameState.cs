using Microsoft.Xna.Framework;
using SubterfugeCore.Shared.Content.Game.Objects;
using SubterfugeCore.Shared.Content.Game.Objects.Base;
using SubterfugeRemake.Shared.Content.Game.Core.Commands;
using SubterfugeRemake.Shared.Content.Game.Core.Timing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.World
{
    /// <summary>
    /// This class holds information about a game's current state.
    /// </summary>
    class GameState
    {

        private Queue<GameEvent> pastEventQueue = new Queue<GameEvent>();
        private Queue<GameEvent> futureEventQueue = new Queue<GameEvent>();
        private List<GameObject> activeSubs = new List<GameObject>();
        private GameTick currentTick;
        private GameTick startTime;

        public GameState()
        {
            this.currentTick = new GameTick(new DateTime(), 0);
            activeSubs.Add(new Sub());
        }

        public void interpolateTick(int tick)
        {
            GameTick interpolateTick = new GameTick(new DateTime(), tick);
            if(interpolateTick > currentTick)
            {
                while (futureEventQueue.Peek().getTick() < interpolateTick)
                {
                    // Move commands from the future to the past
                    pastEventQueue.Enqueue(futureEventQueue.Dequeue());
                }
            } else
            {
                while (pastEventQueue.Peek().getTick() > interpolateTick)
                {
                    // Move commands from the past to the future
                    futureEventQueue.Enqueue(pastEventQueue.Dequeue());
                }
            }
        }

        public List<GameObject> getSubList()
        {
            return this.activeSubs;
        }

        public void update()
        {
            foreach(GameObject gameObject in this.activeSubs)
            {
                gameObject.setPosition(new Vector2(gameObject.getPosition().X + 1, gameObject.getPosition().Y + 1));
            }
        }
    }
}
