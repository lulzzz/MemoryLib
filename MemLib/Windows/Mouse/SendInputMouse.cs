using MemLib.Native;

namespace MemLib.Windows.Mouse {
    public sealed class SendInputMouse : BaseMouse {
        internal SendInputMouse(RemoteWindow window) : base(window) { }

        private static Input CreateInput() {
            return new Input(InputTypes.Mouse);
        }

        #region Overrides of BaseMouse

        public override void MoveTo(int x, int y) {
            x += Window.X;
            y += Window.Y;
            var input = CreateInput();
            input.Mouse.DeltaX = x * 65536 / NativeMethods.GetSystemMetrics(SystemMetrics.CxScreen);
            input.Mouse.DeltaY = y * 65536 / NativeMethods.GetSystemMetrics(SystemMetrics.CyScreen);
            input.Mouse.Flags = MouseFlags.Move | MouseFlags.Absolute;
            input.Mouse.MouseData = 0;
            WindowHelper.SendInput(input);
        }

        public override void PressLeft() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.LeftDown;
            WindowHelper.SendInput(input);
        }

        public override void PressMiddle() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.MiddleDown;
            WindowHelper.SendInput(input);
        }

        public override void PressRight() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.RightDown;
            WindowHelper.SendInput(input);
        }

        public override void ReleaseLeft() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.LeftUp;
            WindowHelper.SendInput(input);
        }

        public override void ReleaseMiddle() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.MiddleUp;
            WindowHelper.SendInput(input);
        }

        public override void ReleaseRight() {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.RightUp;
            WindowHelper.SendInput(input);
        }

        public override void ScrollHorizontally(int delta = 120) {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.HWheel;
            input.Mouse.MouseData = delta;
            WindowHelper.SendInput(input);
        }

        public override void ScrollVertically(int delta = 120) {
            var input = CreateInput();
            input.Mouse.Flags = MouseFlags.Wheel;
            input.Mouse.MouseData = delta;
            WindowHelper.SendInput(input);
        }

        #endregion
    }
}