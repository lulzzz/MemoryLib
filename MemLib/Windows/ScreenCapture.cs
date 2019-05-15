using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MemLib.Windows {
    public class ScreenCapture {
        public Image CaptureScreen() {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        public Image CaptureWindow(IntPtr handle) {
            User32.GetClientRect(handle, out var window);
            if (window.width <= 0 || window.height <= 0) {
                User32.GetWindowRect(handle, out window);
            }
            Image img;
            using (var wingr = Graphics.FromHwnd(handle)) {
                var hdcSrc = wingr.GetHdc();
                var hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
                var hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, window.width, window.height);
                var hOld = Gdi32.SelectObject(hdcDest, hBitmap);
                Gdi32.BitBlt(hdcDest, 0, 0, window.width, window.height, hdcSrc, 0, 0, Gdi32.Srccopy);
                Gdi32.SelectObject(hdcDest, hOld);
                Gdi32.DeleteDC(hdcDest);
                img = Image.FromHbitmap(hBitmap);
                Gdi32.DeleteObject(hBitmap);
            }
            return img;
        }

        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format) {
            var img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        public void CaptureScreenToFile(string filename, ImageFormat format) {
            var img = CaptureScreen();
            img.Save(filename, format);
        }


        private static class Gdi32 {
            public const int Srccopy = 0x00CC0020;

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight,
                IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, int nWidth, int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);
        }

        private static class User32 {
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, out Rect rect);

            [DllImport("user32.dll")]
            public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

            [StructLayout(LayoutKind.Sequential)]
            public struct Rect {
                public int left;
                public int top;
                public int right;
                public int bottom;

                public int width => right - left;
                public int height => bottom - top;
            }
        }
    }
}