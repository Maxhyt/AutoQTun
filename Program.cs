using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace AutoQTun
{
    internal class Program
    {
        public static CvMat Screenshot;
        public static List<CvMat> ImageList = new List<CvMat>();
        public static CvArr Result;
        public static string WindowTitle;
        public static string ProcessName = "LeagueClient";
        public static IntPtr HWND;
        public static Process PROCESS;
        public static CvPoint MinPos;
        public static CvPoint MaxPos;
        public static double MinAcc;
        public static double MaxAcc;
        public static Point Position = new Point(0, 0);

        private static void Main(string[] args)
        {
            try
            {
                Console.Title = Utils.RandomName();
                Utils.Print("AutoQTun v" + Assembly.GetExecutingAssembly().GetName().Version.Major + " lock and loaded.\n", ConsoleColor.Green);

                Utils.Print("Attempting normal bot errection\n", ConsoleColor.Cyan);

                PROCESS = Process.GetProcessesByName(ProcessName).FirstOrDefault();
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
                }
                LoadImages();
                Utils.Print("Everything went well! You can go afk", ConsoleColor.Cyan);
                while (true)
                {
                    if (!Process.GetProcessesByName("League of Legends").Any() && Process.GetProcessesByName(ProcessName).Any())
                    {
                        MainLoop();
                    }
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
            PROCESS.SetWindowPos(0, 0, 800, 600);
            Thread.Sleep(200);
            foreach (var image in ImageList)
            {
                CvMat screen = Utils.TakeScreenshot().ToMat().ToCvMat();
                Screenshot = new CvMat(screen.Rows, screen.Cols, MatrixType.U8C1);
                screen.CvtColor(Screenshot, ColorConversion.BgraToGray);

                Result = Cv.CreateImage(Cv.Size(Screenshot.Width - image.Width + 1, Screenshot.Height - image.Height + 1), BitDepth.F32, 1);

                Cv.MatchTemplate(Screenshot, image, Result, MatchTemplateMethod.CCoeffNormed);
                Cv.Normalize(Result, Result, 0, 1, NormType.MinMax);
                Cv.MinMaxLoc(Result, out MinAcc, out MaxAcc, out MinPos, out MaxPos, null);
                Utils.Print("Accuracy: " + MaxAcc, ConsoleColor.White);
                if (MaxAcc >= 0.75)
                {
                    Position = new Point(MaxPos.X, MaxPos.Y);
                    Utils.MoveMouse(Position);
                    Thread.Sleep(15);
                    Utils.LeftClick();
                    Thread.Sleep(100);
                    MaxAcc = 0;
                }
                Result.Dispose();
            }
        }

        public static void LoadImages()
        {
            string[] files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "data"));
            foreach (string file in files)
            {
                Utils.Print("Loaded " + Path.GetFileName(file), ConsoleColor.Green);
                var img = CvMat.FromFile(file);
                var gray = new CvMat(img.Rows, img.Cols, MatrixType.U8C1);
                img.CvtColor(gray, ColorConversion.BgrToGray);
                ImageList.Add(gray);
            }
        }
    }
}