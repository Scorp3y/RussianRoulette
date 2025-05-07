using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace RestoreConsole
{
    internal class RestoreConsole
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void SecondMain(string[] args)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string backupPath = Path.Combine(basePath, "Backup");
            string wallpaperPath = Path.Combine(backupPath, "wallpaper.bmp");
            string shortcutsPath = Path.Combine(backupPath, "shortcuts");
            string metadataPath = Path.Combine(backupPath, "shortcut_metadata.json");

            if (!File.Exists(wallpaperPath) || !Directory.Exists(shortcutsPath) || !File.Exists(metadataPath))
            {
                Console.WriteLine("❌ Резервные данные не найдены. Восстановление невозможно.");
                return;
            }

            Console.WriteLine("🔧 Восстановление начато...");
            Console.WriteLine("🖼 Восстановление обоев...");
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach (var lnk in Directory.GetFiles(desktopPath, "*.lnk"))
            {
                try { File.Delete(lnk); } catch { }
            }


            Console.WriteLine("📂 Восстановление ярлыков...");
            var metadata = JsonSerializer.Deserialize<ShortcutMetadata[]>(File.ReadAllText(metadataPath));
            foreach (var item in metadata)
            {
                string sourceLnk = Path.Combine(shortcutsPath, item.FileName);
                string destLnk = Path.Combine(desktopPath, item.OriginalName);
                File.Copy(sourceLnk, destLnk, true);

                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(destLnk);
                shortcut.IconLocation = item.IconPath;
                shortcut.Save();
            }

            Console.WriteLine("✅ Восстановление завершено!");
            Console.ReadKey();
        }

        class ShortcutMetadata
        {
            public string OriginalName { get; set; }
            public string FileName { get; set; }
            public string IconPath { get; set; }
        }
    }
}
