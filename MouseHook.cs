using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BazthalLib
{
    public static class MouseHook
    {

        private static IntPtr _hookID = IntPtr.Zero;
        private static NativeMethods.LowLevelMouseProc _proc;
        private static Action<Point> _callback;


     /// <summary>
     /// Initializes the hook and starts listening for input events.
     /// </summary>
     /// <remarks>The <paramref name="callback"/> parameter is called whenever an input event is detected,
     /// providing the event's coordinates. Ensure that the callback method is efficient to avoid performance
     /// issues.</remarks>
     /// <param name="callback">A callback method that is invoked with the coordinates of the input event.</param>
        public static void Start(Action<Point> callback)
        {
            _proc = HookCallback;
            _callback = callback;
            _hookID = NativeMethods.SetHook(_proc);
        }

        /// <summary>
        /// Stops the global Mouse  hook by unhooking the Windows hook.
        /// </summary>
        /// <remarks>This method should be called to release the hook when it is no longer needed, 
        /// preventing resource leaks and ensuring that the application does not continue  to intercept Mouse
        /// events.</remarks>
        public static void Stop()
        {
            NativeMethods.UnhookWindowsHookEx(_hookID);
        }

        /// <summary>
        /// Processes low-level mouse input events and invokes a callback when a left mouse button down event is
        /// detected.
        /// </summary>
        /// <remarks>This method is intended to be used as a callback for a mouse hook. It checks if the
        /// event is a left mouse button down event and, if so, invokes a user-defined callback with the mouse
        /// coordinates. After processing, it stops the hook.</remarks>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message. If less than zero, the hook
        /// procedure must pass the message to the <see cref="NativeMethods.CallNextHookEx"/> function without further
        /// processing.</param>
        /// <param name="wParam">The identifier of the mouse message. This method specifically checks for the left mouse button down event.</param>
        /// <param name="lParam">A pointer to a <see cref="NativeMethods.MSLLHOOKSTRUCT"/> structure containing information about the mouse
        /// event.</param>
        /// <returns>A pointer to the next hook procedure in the chain, as returned by <see
        /// cref="NativeMethods.CallNextHookEx"/>.</returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                _callback?.Invoke(new Point(hookStruct.pt.x, hookStruct.pt.y));
                Stop();
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WM_LBUTTONDOWN = 0x0201;
    }





}