using System;

namespace GoGoPhilipAdamsRedux
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new MyGame()) game.Run();
        }
    }
}
