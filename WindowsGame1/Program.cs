using System;
using Drawing;

namespace CloudberryKingdom
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if WINDOWS
#if PC
#endif
        [STAThread]
#endif

        static void Main(string[] args)
        {
            CloudberryKingdomGame.ProcessArgs(args);

            AppDomain.CurrentDomain.UnhandledException +=new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

#if GAME
            using (CloudberryKingdomGame game = new CloudberryKingdomGame())
#else
            using (Game_Editor game = new Game_Editor())            
#endif
            {
                game.Run();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Tools.Log(e.ExceptionObject.ToString());
        } 
    }
}

