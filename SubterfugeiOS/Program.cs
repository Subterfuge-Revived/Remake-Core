using System;
using Foundation;
using UIKit;

using SubterfugeCore.Shared;

namespace SubterfugeiOS
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static SubterfugeApp game;

        internal static void RunGame()
        {
            game = new SubterfugeApp();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
    }
}
