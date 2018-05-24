using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;
using Bitmap = System.Drawing.Bitmap;

namespace AutoQTun
{
    internal class Program
    {
        private static Dictionary<string,Mat> ImageList = new Dictionary<string, Mat>();
        private static Dictionary<string, string> settings = new Dictionary<string, string>();
        private static string WindowTitle;
        private static string ProcessName = "LeagueClient";
        private static IntPtr HWND;
        private static Process PROCESS;
        private static Point positionHolder = new Point(0, 0);

        private static void Main(string[] args)
        {
            try
            {
                Console.Title = Utils.RandomName();
                Utils.Print("AutoQTun v" + Assembly.GetExecutingAssembly().GetName().Version.Major + " lock and loaded.\n", ConsoleColor.Green);

                Utils.Print("Attempting normal bot errection\n", ConsoleColor.Cyan);

                /*PROCESS = Process.GetProcessesByName(ProcessName).FirstOrDefault();
                if (PROCESS != null)
                {
                    HWND = PROCESS.MainWindowHandle;
                    WindowTitle = PROCESS.MainWindowTitle;
                }
                else
                {
                    Utils.Print("Please start up lol and login", ConsoleColor.Red);
                    Console.ReadLine();
                    return;
                }*/
                LoadSettings();
                LoadImages();
                Utils.Print("Everything went well! You can go afk", ConsoleColor.Cyan);
                while (true)
                {
                    //if (!Process.GetProcessesByName("League of Legends").Any() && Process.GetProcessesByName(ProcessName).Any())
                    //{
                        MainLoop();
                    //}
                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Utils.Print(e.ToString(), ConsoleColor.Red);
                Console.ReadLine();
            }
        }

        public static void MainLoop()
        {
            //PROCESS.SetWindowPos(0, 0, Convert.ToInt32(settings["resolution"].Split('x')[0]), Convert.ToInt32(settings["resolution"].Split('x')[1]));
            //Thread.Sleep(200);
            Dictionary<string, Mat>[] lists = new Dictionary<string, Mat>[]
            {
                new Dictionary<string, Mat>(),
                new Dictionary<string, Mat>()
            };

            for (int i = 0; i < ImageList.Count / 2; i++)
            {
                lists[0].Add(ImageList.ElementAt(i).Key, ImageList.ElementAt(i).Value);
            }

            for (int i = ImageList.Count / 2; i < ImageList.Count; i++)
            {
                lists[1].Add(ImageList.ElementAt(i).Key, ImageList.ElementAt(i).Value);
            }

            foreach (var list in lists)
            {
                Task.Run(() => Process(list));
            }
        }

        private static async Task Process(Dictionary<string,Mat> list)
        {
            foreach (var image in list.Values)
            {
                Mat screenshot = Utils.TakeScreenshot();
                Point match = Utils.CompareImages(screenshot, image);
                screenshot.Dispose();
                if (!match.Equals(Point.Empty))
                {
                    Utils.Print("Found match at: " + match, ConsoleColor.Cyan);
                    Utils.MoveMouse(match);
                    Utils.LeftClick();
                    Utils.MoveMouse(positionHolder);
                    return;
                }
                await Task.Delay(1000);
            }
        }

        private static void LoadImages()
        {
            string[] files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "data"));
            foreach (string file in files)
            {
                if (settings["mode"].ToLower().Equals("bot") && Path.GetFileName(file).ToLower().Contains("pvp"))
                    continue;
                else if (settings["mode"].ToLower().Equals("normal") && (Path.GetFileName(file).ToLower().Contains("bot") || Path.GetFileName(file).ToLower().Contains("rank")))
                    continue;
                else if (settings["mode"].ToLower().Equals("ranksolo") && (Path.GetFileName(file).ToLower().Contains("bot") || Path.GetFileName(file).ToLower().Contains("normal") || Path.GetFileName(file).ToLower().Contains("flex")))
                    continue;
                else if (settings["mode"].ToLower().Equals("rankflex") && (Path.GetFileName(file).ToLower().Contains("bot") || Path.GetFileName(file).ToLower().Contains("normal") || Path.GetFileName(file).ToLower().Contains("solo")))
                    continue;
                
                using (Mat mat = new Mat(file, ImreadModes.GrayScale))
                {
                    ImageList.Add(Path.GetFileName(file), mat);
                }
                
                Utils.Print("Loaded " + Path.GetFileName(file), ConsoleColor.Green);
            }
        }

        private static void LoadSettings()
        {
            try
            {
                string[] file = File.ReadAllLines(@"settings.ini");
                foreach (string line in file)
                {
                    if (line.Contains("##"))
                        continue;
                    string[] tmp = line.Split('=');
                    settings.Add(tmp[0], tmp[1]);
                }
                Utils.Print("Settings loaded.", ConsoleColor.Yellow);
                Utils.Print("Mode: " + settings["mode"], ConsoleColor.Yellow);
                Utils.Print("Champion: " + settings["champ"], ConsoleColor.Yellow);
                Utils.Print("Resolution: " + settings["resolution"], ConsoleColor.Yellow);
            }
            catch (Exception e)
            {
                Utils.Print("Settings load failed\n" + e, ConsoleColor.Red);
                Console.ReadLine();
            }
        }
    }
}