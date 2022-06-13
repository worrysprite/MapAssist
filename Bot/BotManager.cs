using MapAssist.Helpers;
using MapAssist.Settings;
using MapAssist.Types;
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
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private static Thread botThread;
        private static bool running = false;
        private static RoleBase role = null;
        static long lastCheckTime = 0;

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
                try
                {
                    var gameData = GameMemory.GetGameData();
                    if (gameData != null)
                    {
                        checkLife(gameData);
                    }
                }
                catch
                {
                }
                Thread.Sleep(10);
            }
        }

        private static void checkLife(GameData gameData)
        {
            var now = DateTime.Now.Ticks;
            //if (now - lastCheckTime < TimeSpan.TicksPerMillisecond * 100)
            if (now - lastCheckTime < TimeSpan.TicksPerSecond)
                return;

            lastCheckTime = now;

            if (gameData.Area.IsTown() || gameData.PlayerUnit == null)
                return;

            if (gameData.PlayerUnit.LifePercentage > MapAssistConfiguration.Loaded.LifeProtect)
                return;

            //WindowsExternal.SetForegroundWindow(gameData.MainWindowHandle);
            if (!gameData.MenuOpen.EscMenu)
            {
                WindowsExternal.SendEscapeKey(gameData.MainWindowHandle);
            }

            var windowRect = WindowsExternal.GetWindowRect(gameData.MainWindowHandle);
            WindowsExternal.LeftMouseClick(windowRect.Left + windowRect.Width / 2,
                windowRect.Top + windowRect.Height / 20 * 9);
        }
    }
}
