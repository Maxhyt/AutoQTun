using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;

namespace AutoQTun
{
    public static class Utils
    {
        private static Random rand = new Random();

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 2u,
            LEFTUP = 4u,
            MIDDLEDOWN = 32u,
            MIDDLEUP = 64u,
            MOVE = 1u,
            ABSOLUTE = 32768u,
            RIGHTDOWN = 8u,
            RIGHTUP = 16u,
            WHEEL = 2048u,
            XDOWN = 128u,
            XUP = 256u
        }

        public enum SpecialWindowHandles
        {
            HWND_TOP,
            HWND_BOTTOM,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2
        }

        public enum SetWindowPosFlags : uint
        {
            SWP_ASYNCWINDOWPOS = 16384u,
            SWP_DEFERERASE = 8192u,
            SWP_DRAWFRAME = 32u,
            SWP_FRAMECHANGED = 32u,
            SWP_HIDEWINDOW = 128u,
            SWP_NOACTIVATE = 16u,
            SWP_NOCOPYBITS = 256u,
            SWP_NOMOVE = 2u,
            SWP_NOOWNERZORDER = 512u,
            SWP_NOREDRAW = 8u,
            SWP_NOREPOSITION = 512u,
            SWP_NOSENDCHANGING = 1024u,
            SWP_NOSIZE = 1u,
            SWP_NOZORDER = 4u,
            SWP_SHOWWINDOW = 64u
        }

        public enum RedrawWindowFlags
        {
            RDW_INVALIDATE = 1,
            RDW_INTERNALPAINT,
            RDW_ERASE = 4,
            RDW_VALIDATE = 8,
            RDW_NOINTERNALPAINT = 16,
            RDW_NOERASE = 32,
            RDW_NOCHILDREN = 64,
            RDW_ALLCHILDREN = 128,
            RDW_UPDATENOW = 256,
            RDW_ERASENOW = 512,
            RDW_FRAME = 1024,
            RDW_NOFRAME = 2048
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            SetWindowPosFlags uFlags);

        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lpRectUpdate, IntPtr hrgnUpdate, uint flags);

        public static Mat TakeScreenshot()
        {
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            using (Graphics graphic = Graphics.FromImage(screenshot))
            {
                graphic.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            }
            
            return BitmapConverter.ToMat(screenshot);
        }

        public static Point CompareImages(Mat pic1, Mat pic2)
        {
            

            return Point.Empty;
        }

        public static void SetWindowPos(this Process Process, int x, int y, int width, int height)
        {
            Utils.SetWindowPos(Process.MainWindowHandle, (IntPtr) 0, x, y, width, height, Utils.SetWindowPosFlags.SWP_SHOWWINDOW);
            Utils.RedrawWindow(Process.MainWindowHandle, (IntPtr) 0, (IntPtr) 0, 256u);
            Utils.SetForegroundWindow(Process.MainWindowHandle);
            Utils.ShowWindow(Process.MainWindowHandle, 6);
            Utils.ShowWindow(Process.MainWindowHandle, 9);
        }

        public static void LeftClick()
        {
            Utils.mouse_event(2u, 0u, 0u, 0u, 0);
            Thread.Sleep(rand.Next(20, 50));
            Utils.mouse_event(4u, 0u, 0u, 0u, 0);
            Thread.Sleep(rand.Next(100, 150));
        }

        public static bool MoveMouse(System.Drawing.Point target, bool human = true, int steps = 100)
        {
            Cursor.Position = target;
            return Cursor.Position == target;
        }

        public static void SendKeys(string keys, int delay = 300)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                System.Windows.Forms.SendKeys.Send(keys[i].ToString());
                Thread.Sleep(rand.Next(delay - 30, delay + 30));
            }
        }

        public static void Print(string text, ConsoleColor color)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[" + DateTime.Now + "] ");
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        public static string RandomName()
        {
            char[] name = new char[32];

            for (int i = 0; i < name.Length; i++)
            {
                int type = rand.Next(1, 4);
                switch (type)
                {
                    case 1:
                        name[i] = (char)rand.Next(48, 58);
                        break;
                    case 2:
                        name[i] = (char)rand.Next(65, 91);
                        break;
                    case 3:
                        name[i] = (char)rand.Next(97, 123);
                        break;
                }
            }

            return Convert.ToBase64String(Convert.FromBase64CharArray(name, 0, name.Length));
        }
    }
}