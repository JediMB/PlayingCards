global using System.Runtime.Versioning;

namespace PlayingCardsApp
{
    [SupportedOSPlatform("windows")]

    internal class Program
    {        
        static void Main()
        {
            Logic logic = Logic.Instance;

            logic.RunLoop();
        }
    }
}