using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultyThreadingDemo_2
{
    public class HookHelper
    {
        public delegate void CallBackDelegat();
        private CallBackDelegat mCallBackDelegat;

        public HookHelper(CallBackDelegat _callBackDelegat) {
            mCallBackDelegat = _callBackDelegat;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_KEYDOWN = 0x0100;

        private LowLevelKeyboardProcDelegate m_callback;
        private IntPtr m_hHook;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
         int idHook,
         LowLevelKeyboardProcDelegate lpfn,
         IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
         IntPtr hhk,
         int nCode, IntPtr wParam, IntPtr lParam);


        private IntPtr LowLevelKeyboardHookProc(
         int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (IntPtr)WH_KEYDOWN == wParam)
            {
                mCallBackDelegat.Invoke();
                var khs = (KeyboardHookStruct)
                          Marshal.PtrToStructure(lParam,
                          typeof(KeyboardHookStruct));

                Debug.Print("Hook: Code: {0}, WParam: {1},{2},{3},{4} ",
                          nCode, wParam, lParam,
                          khs.VirtualKeyCode,
                          khs.ScanCode, khs.Flags, khs.Time);

                Debug.Print(khs.VirtualKeyCode.ToString());


                if (khs.VirtualKeyCode == 9 &&
                   wParam.ToInt32() == 260 &&
                   khs.ScanCode == 15) //alt+tab
                {
                    System.Console.WriteLine("Alt+Tab pressed!");
                    IntPtr val = new IntPtr(1);
                    return val;
                }
                else
                {
                    return CallNextHookEx(m_hHook, nCode, wParam, lParam);
                }
            }
            else
            {
                return CallNextHookEx(m_hHook, nCode, wParam, lParam);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public readonly int VirtualKeyCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr ExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProcDelegate(
         int nCode, IntPtr wParam, IntPtr lParam);


        public void SetHook()
        {
            m_callback = LowLevelKeyboardHookProc;
            m_hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
             m_callback,
             GetModuleHandle(IntPtr.Zero), 0);
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(m_hHook);
        }
    }
}
