using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace RussianRoulette
{
    internal class Program
    {
      

        private delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);

        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_CLOSE_EVENT = 2,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType) => true;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        const int SW_MAXIMIZE = 3;
        const int GWL_STYLE = -16;
        const int WS_CAPTION = 0x00C00000;
        const int WS_SYSMENU = 0x00080000;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_FRAMECHANGED = 0x0020;
        const uint SC_CLOSE = 0xF060;
        const uint MF_BYCOMMAND = 0x00000000;

        static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void Main(string[] args)
        {

            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, SW_MAXIMIZE);

            int style = GetWindowLong(handle, GWL_STYLE);
            style &= ~(WS_CAPTION | WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);

            Console.OutputEncoding = Encoding.UTF8;

            if (!IsRunningAsAdmin())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ Пожалуйста, перезапустите программу от имени администратора для корректной работы.");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.Title = "РУССКАЯ РУЛЕТКА: СИСТЕМНОЕ ИСПЫТАНИЕ";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
       ______
    .-'      '-.
   /            \
  |              |
  |,  .-.  .-.  ,|
  | )(_o/  \o_)( |
  |/     /\     \|
  (_     ^^     _)
   \__|IIIIII|__/
    | \IIIIII/ |
    \          /
     `--------`

>> Добро пожаловать в 'РУССКУЮ РУЛЕТКУ: СИСТЕМНОЕ ИСПЫТАНИЕ'

Ты сделал свой выбор — и пути назад больше нет.
С этого момента ты — участник. 

Игра не спросит, готов ли ты. Игра просто начнётся.

Каждое нажатие клавиши — как спуск курка.
Один из них активирует необратимое.

🎲 Желаешь сыграть? Уже поздно отступать...");

            Console.WriteLine(@"
1 - Да
2 - Да");

            Console.ReadKey();
            if (args.Length > 0 && args[0] == "child")
            {
                Console.Clear();
                Console.Title = "МИГАНИЕ...";
                ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
                Random rnd = new Random();
                for (int i = 0; i < 5; i++)
                {
                    Console.BackgroundColor = colors[rnd.Next(colors.Length)];
                    Console.Clear();
                    Thread.Sleep(1000);
                }
            }
            Game(args);
        }

        static void Game(string[] args)
        {
            Console.Clear();
            Console.WriteLine(@"
       ______
    .-'      '-.
   /            \
  |              |
  |,  .-.  .-.  ,|
  | )(_o/  \o_)( |
  |/     /\     \|
  (_     ^^     _)
   \__|IIIIII|__/
    | \IIIIII/ |
    \          /
     `--------`
'РУССКАЯ РУЛЕТКА: СИСТЕМНОЕ ИСПЫТАНИЕ'");

            int num;
            while (true)
            {
                Console.Write("Выбери число от 1 до 3: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out num) && num >= 1 && num <= 3)
                    break;
                Console.WriteLine("Введено некорректное значение! Попробуй снова. ⚠");
            }

            Random rnd = new Random();
            int rndNum = rnd.Next(1, 4);
            Console.WriteLine("Повезло :( ");
            Console.WriteLine("Пуля была в числе: " + rndNum);

            if (num == rndNum)
                Died();
            else
                Win();
        }

        static void Win()
        {
            DesktopIcons.SetDesktopIconsVisible(true);
            Console.WriteLine("Ты выиграл...");
            Thread.Sleep(5000);
        }

        static void Died()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("УПС... Ты проиграл :)");
            Thread.Sleep(100);

            ConsoleColor[] colors = (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor));
            Random rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                Console.BackgroundColor = colors[rnd.Next(colors.Length)];
                Console.Clear();
                Thread.Sleep(5);
                
            }

            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            for (int i = 0; i < 10; i++)
            {
                Console.Clear();
                Process.Start(exePath, "child");
                
            }

            Process.Start(new ProcessStartInfo("shutdown", "/s /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }


        
    }

    class DesktopIcons
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static void SetDesktopIconsVisible(bool visible)
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr desktopHandle = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (desktopHandle == IntPtr.Zero)
            {
                IntPtr workerw = FindWindow("WorkerW", null);
                desktopHandle = FindWindowEx(workerw, IntPtr.Zero, "SHELLDLL_DefView", null);
            }

            if (desktopHandle != IntPtr.Zero)
            {
                ShowWindow(desktopHandle, visible ? SW_SHOW : SW_HIDE);
            }
        }
    }
}
