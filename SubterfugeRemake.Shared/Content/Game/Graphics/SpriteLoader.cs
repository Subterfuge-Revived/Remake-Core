using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics
{
    class SpriteLoader
    {
        private Dictionary<String, Texture2D> sprites = new Dictionary<string, Texture2D>();

        public SpriteLoader()
        {
            
        }

        public void addSprite(String spriteName, Texture2D sprite)
        {
            this.sprites[spriteName] = sprite;
        }

        public Texture2D getSprite(String spriteName)
        {
            if(this.sprites.ContainsKey(spriteName))
            {
                return this.sprites[spriteName];
            }
            return null;
        }

        public void loadSprites(ContentManager content)
        {

            // Add all sprites to the sprite loader here.
            this.addSprite("SubFill", content.Load<Texture2D>("Assets/Images/Subs/SubFill"));
            this.addSprite("Gift", content.Load<Texture2D>("Assets/Images/Subs/Gift"));
            this.addSprite("TrajectoryMask", content.Load<Texture2D>("Assets/Images/Subs/TrajectoryMask"));

            this.addSprite("Admiral", content.Load<Texture2D>("Assets/Images/Specialists/Admiral"));
            this.addSprite("Assassin", content.Load<Texture2D>("Assets/Images/Specialists/Assassin"));
            this.addSprite("Diplomat", content.Load<Texture2D>("Assets/Images/Specialists/Diplomat"));
            this.addSprite("DoubleAgent", content.Load<Texture2D>("Assets/Images/Specialists/DoubleAgent"));
            this.addSprite("Engineer", content.Load<Texture2D>("Assets/Images/Specialists/Engineer"));
            this.addSprite("Foreman", content.Load<Texture2D>("Assets/Images/Specialists/Foreman"));
            this.addSprite("General", content.Load<Texture2D>("Assets/Images/Specialists/General"));
            this.addSprite("Helmsman", content.Load<Texture2D>("Assets/Images/Specialists/Helmsman"));
            this.addSprite("Hypnotist", content.Load<Texture2D>("Assets/Images/Specialists/Hypnotist"));
            this.addSprite("Infiltrator", content.Load<Texture2D>("Assets/Images/Specialists/Infiltrator"));
            this.addSprite("Inspector", content.Load<Texture2D>("Assets/Images/Specialists/Inspector"));
            this.addSprite("IntelligenceOfficer", content.Load<Texture2D>("Assets/Images/Specialists/IntelligenceOfficer"));
            this.addSprite("King", content.Load<Texture2D>("Assets/Images/Specialists/King"));
            this.addSprite("Lieutenant", content.Load<Texture2D>("Assets/Images/Specialists/Lieutenant"));
            this.addSprite("Martyr", content.Load<Texture2D>("Assets/Images/Specialists/Martyr"));
            this.addSprite("MinisterOfEnergy", content.Load<Texture2D>("Assets/Images/Specialists/MinisterOfEnergy"));
            this.addSprite("Navigator", content.Load<Texture2D>("Assets/Images/Specialists/Navigator"));
            this.addSprite("Pirate", content.Load<Texture2D>("Assets/Images/Specialists/Pirate"));
            this.addSprite("Princess", content.Load<Texture2D>("Assets/Images/Specialists/Princess"));
            this.addSprite("Queen", content.Load<Texture2D>("Assets/Images/Specialists/Queen"));
            this.addSprite("ReveredElder", content.Load<Texture2D>("Assets/Images/Specialists/ReveredElder"));
            this.addSprite("Saboteur", content.Load<Texture2D>("Assets/Images/Specialists/Saboteur"));
            this.addSprite("SecurityChief", content.Load<Texture2D>("Assets/Images/Specialists/SecurityChief"));
            this.addSprite("Sentry", content.Load<Texture2D>("Assets/Images/Specialists/Sentry"));
            this.addSprite("Smuggler", content.Load<Texture2D>("Assets/Images/Specialists/Smuggler"));
            this.addSprite("Thief", content.Load<Texture2D>("Assets/Images/Specialists/Thief"));
            this.addSprite("Tinkerer", content.Load<Texture2D>("Assets/Images/Specialists/Tinkerer"));
            this.addSprite("Tycoon", content.Load<Texture2D>("Assets/Images/Specialists/Tycoon"));
            this.addSprite("WarHero", content.Load<Texture2D>("Assets/Images/Specialists/WarHero"));

            this.addSprite("FactoryFill", content.Load<Texture2D>("Assets/Images/Locations/FactoryFill"));
            this.addSprite("GeneratorFill", content.Load<Texture2D>("Assets/Images/Locations/GeneratorFill"));
            this.addSprite("MineFill", content.Load<Texture2D>("Assets/Images/Locations/MineFill"));

            this.addSprite("Sea", content.Load<Texture2D>("Assets/Images/Sea"));
        }

    }
}
