using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BazthalLib
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Represents a callback function that processes low-level mouse input events.
        /// </summary>
        /// <param name="nCode">A code that the hook procedure uses to determine how to process the message. If less than zero, the hook
        /// procedure must pass the message to the CallNextHookEx function without further processing.</param>
        /// <param name="wParam">The identifier of the mouse message. This parameter can be one of the mouse messages, such as WM_MOUSEMOVE
        /// or WM_LBUTTONDOWN.</param>
        /// <param name="lParam">A pointer to a MOUSEHOOKSTRUCT structure that contains details about the mouse event.</param>
        /// <returns>An IntPtr that indicates whether the hook procedure handled the message. If the procedure returns a non-zero
        /// value, the message is considered handled and will not be passed to the next hook procedure.</returns>
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Sets a low-level mouse hook procedure.
        /// </summary>
        /// <remarks>The hook procedure is installed in the context of the calling thread and will be
        /// called in response to mouse events. Ensure to unhook the procedure using the appropriate method to avoid
        /// resource leaks.</remarks>
        /// <param name="proc">The callback function to be called whenever a mouse event is processed.</param>
        /// <returns>A handle to the hook procedure. If the function fails, the return value is <see cref="IntPtr.Zero"/>.</returns>
        public static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        /// <summary>
        /// Represents the low-level mouse input event hook constant.
        /// </summary>
        /// <remarks>This constant is used to set a hook procedure that monitors low-level mouse input
        /// events.</remarks>
        public const int WH_MOUSE_LL = 14;

        /// <summary>
        /// Represents a point in a two-dimensional coordinate system.
        /// </summary>
        /// <remarks>The <see cref="POINT"/> structure is commonly used to define the X and Y coordinates
        /// of a point.</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int x, y; }
        /// <summary>
        /// Contains information about a low-level keyboard input event.
        /// </summary>
        /// <remarks>This structure is used with the WH_MOUSE_LL hook to monitor mouse input events.  It
        /// provides details such as the mouse position, additional mouse data, and event timing.</remarks>
        public struct MSLLHOOKSTRUCT { public POINT pt; public int mouseData, flags, time; public IntPtr dwExtraInfo; }


        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public static int WS_EX_TOOLWINDOW { get; internal set; }

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}
