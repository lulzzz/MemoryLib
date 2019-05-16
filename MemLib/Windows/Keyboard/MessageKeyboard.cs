using MemLib.Native;

namespace MemLib.Windows.Keyboard {
    public sealed class MessageKeyboard : BaseKeyboard {
        public MessageKeyboard(RemoteWindow window) : base(window) { }

        private static uint MakeKeyParam(Keys key, bool keyUp, bool fRepeat, uint cRepeat, bool altDown, bool fExtended) {
            var result = cRepeat;
            result |= NativeMethods.MapVirtualKey((uint)key, TranslationTypes.VirtualKeyToScanCode) << 16;
            if (fExtended) result |= 0x1000000;
            if (altDown) result |= 0x20000000;
            if (fRepeat) result |= 0x40000000;
            if (keyUp) result |= 0x80000000;
            return result;
        }

        private static uint MakeKeyParam(Keys key, bool keyUp) {
            return MakeKeyParam(key, keyUp, keyUp, 1, false, false);
        }

        private void Message(WindowsMessages message, uint wParam, uint lParam) {
            Window.PostMessage(message, wParam, lParam);
        }

        #region Overrides of BaseKeyboard

        public override void Press(Keys key) {
            Message(WindowsMessages.KeyDown, (uint)key, MakeKeyParam(key, false));
        }

        public override void Release(Keys key) {
            base.Release(key);
            Message(WindowsMessages.KeyUp, (uint)key, MakeKeyParam(key, true));
        }

        public override void Write(char character) {
            Message(WindowsMessages.Char, character, 0);
        }

        #endregion
    }
}