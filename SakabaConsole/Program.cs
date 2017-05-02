using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SakabaConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var boss = new Boss();
            await boss.InitializeAsync();

            var battle = new Battle(boss);
            await battle.Start();
        }
    }
}
