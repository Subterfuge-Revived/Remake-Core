using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;
using SubterfugeFrontend.Shared.Content.Game.Events;
using SubterfugeFrontend.Shared.Content.Game.Events.Base;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using Color = Microsoft.Xna.Framework.Color;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects
{
    class TexturedOutpost : TexturedGameObject, IListener
    {

        public TexturedOutpost(GameObject gameObject) : base(gameObject, SubterfugeApp.SpriteLoader.getSprite("GeneratorFill"),
            100, 100)
        {
            this.registerListener();
        }


        public void onEvent(Event e)
        {
            if(e.getEventType() == EventType.OnTouchPressEvent)
            {
                TouchPressEvent touchPress = (TouchPressEvent) e;

                // Get the absolut position of the press
                TouchLocation location = touchPress.getTouchLocation();
                Vector2 absolutePosition = Camera.getAbsoluePosition(location.Position);

                // Determine if the press was on the outpost.
                if (this.getBoundingBox().Contains(new PointF((int)absolutePosition.X, (int)absolutePosition.Y)))
                {
                    // Set the selected outpost and wait for a release event.
                    Console.WriteLine("Outpost Selected!!!");

                }
                
            }
        }

        public void registerListener()
        {
            EventObserver.addEventHandler(this);
        }

        public override void render(SpriteBatch spriteBatch)
        {
            Color playerColor;

            switch (((Outpost)this.gameObject).getOwner().getId())
            {
                case 1:
                    playerColor = Color.Blue;
                    break;
                case 2:
                    playerColor = Color.Red;
                    break;
                default:
                    playerColor = Color.White;
                    break;
            }

            spriteBatch.Draw(
                texture: this.getTexture(),
                destinationRectangle: Camera.getRelativeLocation(this),
                color: playerColor,
                sourceRectangle: this.getTexture().Bounds,
                effects: new SpriteEffects(),
                layerDepth: 1,
                rotation: 0,
                origin: this.getTexture().Bounds.Size.ToVector2() / 2f);


            SpriteFont font = SubterfugeApp.FontLoader.getFont("Arial");
            string drillerCount = ((Outpost)this.gameObject).getDrillerCount().ToString();
            Vector2 stringSize = font.MeasureString(drillerCount);


            spriteBatch.DrawString(
                spriteFont: SubterfugeApp.FontLoader.getFont("Arial"),
                text: drillerCount,
                position: Camera.getRelativePosition(this.getPosition()),
                color: Color.White,
                rotation: 0,
                origin: stringSize / 2f,
                layerDepth: 1f,
                scale: 1.5f,
                effects: SpriteEffects.None);
        }

        public void unregisterListener()
        {
            EventObserver.removeEventHandler(this);
        }
    }
}
