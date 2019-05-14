using System;
using System.Collections.Generic;
using System.Linq;
using MemLib.Native;

namespace MemLib.Windows {
    public sealed class WindowManager : IDisposable {
        private readonly RemoteProcess m_Process;
        private IEnumerable<IntPtr> ChildWindowHandles => WindowHelper.EnumChildWindows(MainWindowHandle);
        private IEnumerable<IntPtr> WindowHandles => new List<IntPtr>(ChildWindowHandles) {MainWindowHandle};

        public IntPtr MainWindowHandle => m_Process.Native.MainWindowHandle;
        public RemoteWindow MainWindow => new RemoteWindow(m_Process, MainWindowHandle);
        public IEnumerable<RemoteWindow> ChildWindows => ChildWindowHandles.Select(handle => new RemoteWindow(m_Process, handle));

        public IEnumerable<RemoteWindow> this[string windowTitle] => GetWindowsByTitle(windowTitle);

        internal WindowManager(RemoteProcess process) {
            m_Process = process;
        }

        public RemoteWindow GetWindowFromPoint(int x, int y) {
            var hwnd = NativeMethods.WindowFromPoint(new Point {X = x, Y = y});
            return hwnd == IntPtr.Zero ? null : new RemoteWindow(m_Process, hwnd);
        }

        public IEnumerable<RemoteWindow> GetWindowsByTitle(string windowTitle) {
            return WindowHandles
                .Where(handle => WindowHelper.GetWindowText(handle).Equals(windowTitle, StringComparison.Ordinal))
                .Select(handle => new RemoteWindow(m_Process, handle));
        }

        public IEnumerable<RemoteWindow> GetWindowsByTitleContains(string windowTitle) {
            return WindowHandles
                .Where(handle => WindowHelper.GetWindowText(handle).Contains(windowTitle))
                .Select(handle => new RemoteWindow(m_Process, handle));
        }

        public IEnumerable<RemoteWindow> GetWindowsByClassName(string className) {
            return WindowHandles
                .Where(handle => WindowHelper.GetClassName(handle).Equals(className, StringComparison.Ordinal))
                .Select(handle => new RemoteWindow(m_Process, handle));
        }

        #region IDisposable

        void IDisposable.Dispose() { }

        ~WindowManager() {
            ((IDisposable) this).Dispose();
        }

        #endregion
    }
}