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
        private Queue<BaseCommand> pastCommandQueue = new Queue<BaseCommand>();
        private Queue<BaseCommand> futureCommandQueue = new Queue<BaseCommand>();
        private GameTick currentTick;

        public GameState()
        {
            this.currentTick = new GameTick(new DateTime(), 0);
        }

        public void interpolateTick(int tick)
        {
            GameTick interpolateTick = new GameTick(new DateTime(), tick);
            if(interpolateTick > currentTick)
            {
                while (futureCommandQueue.Peek().getTick() < interpolateTick)
                {
                    // Move commands from the future to the past
                    pastCommandQueue.Enqueue(futureCommandQueue.Dequeue());
                }
            } else
            {
                while (pastCommandQueue.Peek().getTick() > interpolateTick)
                {
                    // Move commands from the past to the future
                    futureCommandQueue.Enqueue(pastCommandQueue.Dequeue());
                }
            }
        }
    }
}
