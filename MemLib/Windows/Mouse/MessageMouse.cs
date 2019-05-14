using System;
using MemLib.Native;

namespace MemLib.Windows.Mouse {
    public sealed class MessageMouse : BaseMouse {
        public Point Cursor { get; private set; }
        public bool IsLeftDown { get; private set; }
        public bool IsMiddleDown { get; private set; }
        public bool IsRightDown { get; private set; }

        public MessageMouse(RemoteWindow window) : base(window) {
            NativeMethods.GetCursorPos(out var pos);
            NativeMethods.ScreenToClient(window.Handle, ref pos);
            Cursor = pos;
        }

        private static uint MakeLParam(int loWord, int hiWord) {
            return (uint)((hiWord << 16) + loWord);
        }

        private uint GetPosParam() => MakeLParam(Cursor.X, Cursor.Y);

        private uint GetKeysDown() {
            var key = Keys.None;
            if (IsLeftDown) key |= Keys.LButton;
            if (IsMiddleDown) key |= Keys.MButton;
            if (IsRightDown) key |= Keys.RButton;
            return (uint) key;
        }

        #region Overrides of BaseMouse

        public override void MoveTo(int x, int y) {
            Window.PostMessage(WindowsMessages.MouseMove, GetKeysDown(), MakeLParam(x, y));
            Cursor = new Point {X = x, Y = y};
        }

        public override void PressLeft() {
            Window.PostMessage(WindowsMessages.LButtonDown, (uint)Keys.LButton, GetPosParam());
            IsLeftDown = true;
        }

        public override void PressMiddle() {
            Window.PostMessage(WindowsMessages.MButtonDown, (uint)Keys.MButton, GetPosParam());
            IsMiddleDown = true;
        }

        public override void PressRight() {
            Window.PostMessage(WindowsMessages.RButtonDown, (uint)Keys.RButton, GetPosParam());
            IsRightDown = true;
        }

        public override void ReleaseLeft() {
            Window.PostMessage(WindowsMessages.LButtonUp, (uint)Keys.LButton, GetPosParam());
            IsLeftDown = false;
        }

        public override void ReleaseMiddle() {
            Window.PostMessage(WindowsMessages.MButtonUp, (uint)Keys.MButton, GetPosParam());
            IsMiddleDown = false;
        }

        public override void ReleaseRight() {
            Window.PostMessage(WindowsMessages.RButtonUp, (uint)Keys.RButton, GetPosParam());
            IsRightDown = false;
        }

        public override void ScrollHorizontally(int delta = 120) {
            throw new NotImplementedException();
        }

        public override void ScrollVertically(int delta = 120) {
            throw new NotImplementedException();
        }

        #endregion
    }
}