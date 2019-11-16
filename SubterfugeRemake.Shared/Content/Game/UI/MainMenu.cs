using Microsoft.Xna.Framework;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;

namespace SubterfugeFrontend.Shared.Content.Game.UI
{
    class MainMenu : Screen
    {
        public MainMenu()
        {
            // Set the screen content
            this.Content = new StackPanel
            {
                Margin = 5,
                Orientation = Orientation.Vertical,
                Items = {
                new Button{},
                },
            };
            
        }
    }
}
