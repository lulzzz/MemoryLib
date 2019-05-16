using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MemLib.Native;

namespace MemLib.Windows {
    [DebuggerStepThrough]
    internal static class WindowHelper {
        public static IEnumerable<IntPtr> EnumChildWindows(IntPtr parentHandle){
            var list = new List<IntPtr>();
            NativeMethods.EnumChildWindows(parentHandle, (wnd, param) => {
                list.Add(wnd);
                return true;
            }, IntPtr.Zero);
            return list;
        }

        public static WindowPlacement GetWindowPlacement(IntPtr windowHandle) {
            WindowPlacement placement;
            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));

            if (!NativeMethods.GetWindowPlacement(windowHandle, out placement))
                throw new Win32Exception("Couldn't get the window placement.");

            return placement;
        }

        public static string GetWindowText(IntPtr windowHandle) {
            var capacity = NativeMethods.GetWindowTextLength(windowHandle);
            if (capacity == 0)
                return string.Empty;
            var stringBuilder = new StringBuilder(capacity + 1);
            if (NativeMethods.GetWindowText(windowHandle, stringBuilder, stringBuilder.Capacity) == 0)
                return string.Empty;
            return stringBuilder.ToString();
        }

        public static string GetClassName(IntPtr windowHandle) {
            var sb = new StringBuilder(1024);
            if (NativeMethods.GetClassName(windowHandle, sb, sb.Capacity) == 0)
                return string.Empty;
            return sb.ToString();
        }

        public static bool SetForegroundWindow(IntPtr windowHandle) {
            if (NativeMethods.GetForegroundWindow() == windowHandle)
                return true;
            NativeMethods.ShowWindow(windowHandle, WindowStates.Restore);
            return NativeMethods.SetForegroundWindow(windowHandle);
        }

        public static bool SetWindowPlacement(IntPtr windowHandle, int left, int top, int height, int width) {
            var placement = GetWindowPlacement(windowHandle);
            placement.NormalPosition.Left = left;
            placement.NormalPosition.Top = top;
            placement.NormalPosition.Height = height;
            placement.NormalPosition.Width = width;
            return SetWindowPlacement(windowHandle, placement);
        }

        public static bool SetWindowPlacement(IntPtr windowHandle, WindowPlacement placement){
            if (Debugger.IsAttached && placement.ShowCmd == WindowStates.ShowNormal)
                placement.ShowCmd = WindowStates.Restore;
            return NativeMethods.SetWindowPlacement(windowHandle, ref placement);
        }

        public static int GetWindowProcessId(IntPtr windowHandle) {
            NativeMethods.GetWindowThreadProcessId(windowHandle, out var processId);
            return processId;
        }
    }
}