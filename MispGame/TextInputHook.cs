﻿//Stolen without guilt from http://www.benryves.com/index.php?module=journal&mode=filtered&single_post=3024426
//Thanks tons, Ben.

/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * XnaTextInput.TextInputHandler - benryves@benryves.com                                     *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This is quick and very, VERY dirty.                                                       *
 * It uses Win32 message hooks to grab messages (as we don't get a nicely wrapped WndProc).  *
 * I couldn't get WH_KEYBOARD to work (accessing the data via its pointer resulted in access *
 * violation exceptions), nor could I get WH_CALLWNDPROC to work.                            *
 * Maybe someone who actually knows what they're  doing can work something out that's not so *
 * kludgy.                                                                                   *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * This quite obviously relies on a Win32 nastiness, so this is for Windows XNA games only!  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

#if !XBOX

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms; // This class exposes WinForms-style key events.

namespace XnaTextInput
{

    /// <summary>
    /// A class to provide text input capabilities to an XNA application via Win32 hooks.
    /// </summary>
    class TextInputHandler : IDisposable
    {
        /// <summary>
        /// Types of hook that can be installed using the SetWindwsHookEx function.
        /// </summary>
        public enum HookId
        {
            WH_CALLWNDPROC = 4,
            WH_CALLWNDPROCRET = 12,
            WH_CBT = 5,
            WH_DEBUG = 9,
            WH_FOREGROUNDIDLE = 11,
            WH_GETMESSAGE = 3,
            WH_HARDWARE = 8,
            WH_JOURNALPLAYBACK = 1,
            WH_JOURNALRECORD = 0,
            WH_KEYBOARD = 2,
            WH_KEYBOARD_LL = 13,
            WH_MAX = 11,
            WH_MAXHOOK = WH_MAX,
            WH_MIN = -1,
            WH_MINHOOK = WH_MIN,
            WH_MOUSE_LL = 14,
            WH_MSGFILTER = -1,
            WH_SHELL = 10,
            WH_SYSMSGFILTER = 6,
        };

        /// <summary>
        /// Window message types.
        /// </summary>
        /// <remarks>Heavily abridged, naturally.</remarks>
        public enum WindowMessage
        {
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_CHAR = 0x102,
        };

        /// <summary>
        /// A delegate used to create a hook callback.
        /// </summary>
        public delegate int GetMsgProc(int nCode, int wParam, ref Message msg);

        /// <summary>
        /// Install an application-defined hook procedure into a hook chain.
        /// </summary>
        /// <param name="idHook">Specifies the type of hook procedure to be installed.</param>
        /// <param name="lpfn">Pointer to the hook procedure.</param>
        /// <param name="hmod">Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
        /// <param name="dwThreadId">Specifies the identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure. Otherwise returns 0.</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA")]
        public static extern IntPtr SetWindowsHookEx(HookId idHook, GetMsgProc lpfn, IntPtr hmod, int dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <param name="hHook">Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hHook">Ignored.</param>
        /// <param name="ncode">Specifies the hook code passed to the current hook procedure.</param>
        /// <param name="wParam">Specifies the wParam value passed to the current hook procedure.</param>
        /// <param name="lParam">Specifies the lParam value passed to the current hook procedure.</param>
        /// <returns>This value is returned by the next hook procedure in the chain.</returns>
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int hHook, int ncode, int wParam, ref Message lParam);

        /// <summary>
        /// Translates virtual-key messages into character messages.
        /// </summary>
        /// <param name="lpMsg">Pointer to an Message structure that contains message information retrieved from the calling thread's message queue.</param>
        /// <returns>If the message is translated (that is, a character message is posted to the thread's message queue), the return value is true.</returns>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref Message lpMsg);


        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The thread identifier of the calling thread.</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        /// <summary>Handle for the created hook.</summary>
        private readonly IntPtr HookHandle;

        private readonly GetMsgProc ProcessMessagesCallback;

        /// <summary>Create an instance of the TextInputHandler.</summary>
        /// <param name="whnd">Handle of the window you wish to receive messages (and thus keyboard input) from.</param>
        public TextInputHandler(IntPtr whnd)
        {
            // Create the delegate callback:
            this.ProcessMessagesCallback = new GetMsgProc(ProcessMessages);
            // Create the keyboard hook:
            this.HookHandle = SetWindowsHookEx(HookId.WH_GETMESSAGE, this.ProcessMessagesCallback, IntPtr.Zero, GetCurrentThreadId());
        }

        public void Dispose()
        {
            // Remove the hook.
            if (this.HookHandle != IntPtr.Zero) UnhookWindowsHookEx(this.HookHandle);
        }

        private int ProcessMessages(int nCode, int wParam, ref Message msg)
        {
            // Check if we must process this message (and whether it has been retrieved via GetMessage):
            if (nCode == 0 && wParam == 1)
            {

                // We need character input, so use TranslateMessage to generate WM_CHAR messages.
                TranslateMessage(ref msg);

                // If it's one of the keyboard-related messages, raise an event for it:
                switch ((WindowMessage)msg.Msg)
                {
                    case WindowMessage.WM_CHAR:
                        this.OnKeyPress(new KeyPressEventArgs((char)msg.WParam));
                        break;
                    case WindowMessage.WM_KEYDOWN:
                        this.OnKeyDown(new KeyEventArgs((Keys)msg.WParam));
                        break;
                    case WindowMessage.WM_KEYUP:
                        this.OnKeyUp(new KeyEventArgs((Keys)msg.WParam));
                        break;
                }

            }

            // Call next hook in chain:
            return CallNextHookEx(0, nCode, wParam, ref msg);
        }

        public event KeyEventHandler KeyUp;
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (this.KeyUp != null) this.KeyUp(this, e);
        }

        public event KeyEventHandler KeyDown;
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (this.KeyDown != null) this.KeyDown(this, e);
        }

        public event KeyPressEventHandler KeyPress;
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.KeyPress != null) this.KeyPress(this, e);
        }

    }
}

#endif