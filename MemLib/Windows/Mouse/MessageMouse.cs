using System;
using MemLib.Native;

namespace MemLib.Windows.Mouse {
    public sealed class MessageMouse : BaseMouse {
        public Point Cursor { get; private set; }
        public bool IsLeftDown { get; private set; }
        public bool IsMiddleDown { get; private set; }
        public bool IsRightDown { get; private set; }

        public MessageMouse(RemoteWindow window) : base(window) {
            Cursor = NativeMethods.GetCursorPos(out var pos) ? pos : new Point();
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
            Window.NPostMessage(WindowsMessages.MouseMove, GetKeysDown(), MakeLParam(x, y));
            Cursor = new Point {X = x, Y = y};
        }

        public override void PressLeft() {
            Window.NPostMessage(WindowsMessages.LButtonDown, 1, GetPosParam());
            IsLeftDown = true;
        }

        public override void PressMiddle() {
            Window.NPostMessage(WindowsMessages.MButtonDown, 1, GetPosParam());
            IsMiddleDown = true;
        }

        public override void PressRight() {
            Window.NPostMessage(WindowsMessages.RButtonDown, 1, GetPosParam());
            IsRightDown = true;
        }

        public override void ReleaseLeft() {
            Window.NPostMessage(WindowsMessages.LButtonUp, 0, GetPosParam());
            IsLeftDown = false;
        }

        public override void ReleaseMiddle() {
            Window.NPostMessage(WindowsMessages.MButtonUp, 0, GetPosParam());
            IsMiddleDown = false;
        }

        public override void ReleaseRight() {
            Window.NPostMessage(WindowsMessages.RButtonUp, 0, GetPosParam());
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