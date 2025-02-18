using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BummerFix
{
    internal class MessageBoxWinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public static void Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            uint buttonType = 0;
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    buttonType = 0x00000000; // MB_OK
                    break;
                case MessageBoxButtons.OKCancel:
                    buttonType = 0x00000001; // MB_OKCANCEL
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    buttonType = 0x00000002; // MB_ABORTRETRYIGNORE
                    break;
                case MessageBoxButtons.YesNoCancel:
                    buttonType = 0x00000003; // MB_YESNOCANCEL
                    break;
                case MessageBoxButtons.YesNo:
                    buttonType = 0x00000004; // MB_YESNO
                    break;
                case MessageBoxButtons.RetryCancel:
                    buttonType = 0x00000005; // MB_RETRYCANCEL
                    break;
            }

            MessageBox(IntPtr.Zero, text, caption, buttonType);
        }
    }

    public enum MessageBoxButtons
    {
        OK,
        OKCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel
    }
}
