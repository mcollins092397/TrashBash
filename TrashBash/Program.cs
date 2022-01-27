using System;

namespace TrashBash
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TrashBash())
                game.Run();
        }
    }
}
