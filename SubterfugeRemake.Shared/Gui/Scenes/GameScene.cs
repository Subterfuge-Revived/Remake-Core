using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore;
using SubterfugeCore.GameEvents;
using SubterfugeFrontend.Shared.Content.Game.UI;
using SubterfugeFrontend.Shared.Content.Game.World;

namespace SubterfugeFrontend.Shared.Content.Gui.Scenes
{
    class GameScene : Scene
    {
        private Match match;
        List<IGuiComponent> renderables = new List<IGuiComponent>();

        public GameScene() : base("Game")
        {
            this.match = new Match(SubterfugeApp.graphics.GraphicsDevice);
            SpriteFont font = SubterfugeApp.FontLoader.getFont("Arial");
            
            Button previousTen = new Button("10<", font, SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Red, new Rectangle(0, 20, 50, 50));
            Button previous = new Button("<", font, SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Red, new Rectangle(50, 20, 50, 50));
            Button next = new Button(">", font, SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Red, new Rectangle(100, 20, 50, 50));
            Button nextTen = new Button(">10", font, SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Red, new Rectangle(150, 20, 50, 50));

            previousTen.Click += PreviousTenClicked;
            previous.Click += PreviousClicked;
            next.Click += NextClicked;
            nextTen.Click += NextTenClicked;

            renderables.Add(previousTen);
            renderables.Add(previous);
            renderables.Add(next);
            renderables.Add(nextTen);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            match.render(spriteBatch, gameTime);

            // Draw HUD controls and debug messages if needed.
            SpriteFont font = SubterfugeApp.FontLoader.getFont("Arial");
            Vector2 StringSize = font.MeasureString("Tick: " + GameServer.timeMachine.getCurrentTick().getTick().ToString());

            spriteBatch.DrawString(
                spriteFont: SubterfugeApp.FontLoader.getFont("Arial"),
                text: "Tick: " +  GameServer.timeMachine.getCurrentTick().getTick().ToString(),
                position: new Vector2(20, 100),
                color: Color.Red,
                rotation: 0,
                origin: StringSize / 2f,
                layerDepth: 1f,
                scale: 1.5f,
                effects: SpriteEffects.None);

            String events = "";
            List<GameEvent> gameEvents = GameServer.timeMachine.getQueuedEvents();
            gameEvents.Sort((a, b) => a.getTick() - b.getTick());
            foreach (GameEvent gameEvent in gameEvents)
            {
                events += gameEvent.getTick().getTick().ToString() + ": " + gameEvent.getEventName() + "\n";
            }
            StringSize = font.MeasureString(events);

            // Output all scheduled actions to the screen for debugging
            spriteBatch.DrawString(
                spriteFont: SubterfugeApp.FontLoader.getFont("Arial"),
                text: events,
                position: new Vector2(400, 20),
                color: Color.Red,
                rotation: 0,
                origin: new Vector2(0, 0),
                layerDepth: 1f,
                scale: 1.5f,
                effects: SpriteEffects.None);

            // Draw some buttons for time machine control.

            foreach (IGuiComponent component in renderables)
            {
                component.Draw(spriteBatch, gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void unloadEventListeners()
        {
            foreach (IGuiComponent guiComponent in this.renderables)
            {
                guiComponent.unregisterEvents();
            }
        }

        public void NextTenClicked(object sender, EventArgs eventArgs)
        {
            GameServer.timeMachine.advance(10);
        }

        public void NextClicked(object sender, EventArgs eventArgs)
        {
            GameServer.timeMachine.advance(1);
        }

        public void PreviousClicked(object sender, EventArgs eventArgs)
        {
            GameServer.timeMachine.rewind(1);
        }

        public void PreviousTenClicked(object sender, EventArgs eventArgs)
        {
            GameServer.timeMachine.rewind(10);
        }
    }
}
