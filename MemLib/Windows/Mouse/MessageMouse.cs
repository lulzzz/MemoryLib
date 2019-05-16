using MemLib.Native;

namespace MemLib.Windows.Mouse {
    public sealed class MessageMouse : BaseMouse {
        public Point VirtualCursor { get; private set; }
        private bool IsLeftDown { get; set; }
        private bool IsMiddleDown { get; set; }
        private bool IsRightDown { get; set; }

        public MessageMouse(RemoteWindow window) : base(window) {
            VirtualCursor = new Point();
        }

        private static uint MakeParam(int loWord, int hiWord) => (uint)((hiWord << 16) + loWord);
        private uint GetPosParam() => MakeParam(VirtualCursor.X, VirtualCursor.Y);

        private uint GetMouseKeysDown() {
            var key = Keys.None;
            if (IsLeftDown) key |= Keys.LButton;
            if (IsMiddleDown) key |= Keys.MButton;
            if (IsRightDown) key |= Keys.RButton;
            return (uint) key;
        }

        #region Overrides of BaseMouse

        public override void MoveTo(int x, int y) {
            Window.PostMessage(WindowsMessages.MouseMove, GetMouseKeysDown(), MakeParam(x, y));
            VirtualCursor = new Point {X = x, Y = y};
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
            Window.PostMessage(WindowsMessages.MouseHWheel, MakeParam(0, delta), GetPosParam());
        }

        public override void ScrollVertically(int delta = 120) {
            Window.PostMessage(WindowsMessages.MouseWheel, MakeParam(0, delta), GetPosParam());
        }

        #endregion
    }
}