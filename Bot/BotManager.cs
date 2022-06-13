using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapAssist.Bot
{
    internal static class BotManager
    {
        private static Thread botThread;
        private static bool running = false;
        private static RoleBase role = null;

        public static void Start()
        {
            running = true;
            botThread = new Thread(Run);
            botThread.Start();
        }

        public static void Dispose()
        {
            running = false;
            botThread.Join();
        }

        private static void Run()
        {
            while (running)
            {
                

                Thread.Sleep(10);
            }
        }

        
    }
}
