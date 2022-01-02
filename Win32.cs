using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MacroBotV0._1Language
{
    class Win32
    {

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const int KEYEVENTF_KEYDOWN = 0x0000; // New definition
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag


        public static void PressKey(int keyCode)
        {
            // Hold Control down and press A
            keybd_event((byte)keyCode, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event((byte)keyCode, 0, KEYEVENTF_KEYUP, 0);
        }

        public static POINT GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }

        public void CursorClick()
        {
            POINT cursorPos = GetCursorPosition();
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)cursorPos.x, (uint)cursorPos.y, 0, 0);
        }

    }
}
/*
GlobalSymbolTable.Set("VK_0", new Number(0x30         ));
GlobalSymbolTable.Set("VK_1", new Number(0x31         ));
GlobalSymbolTable.Set("VK_2", new Number(0x32         ));
GlobalSymbolTable.Set("VK_3", new Number(0x33         ));
GlobalSymbolTable.Set("VK_4", new Number(0x34         ));
GlobalSymbolTable.Set("VK_5", new Number(0x35         ));
GlobalSymbolTable.Set("VK_6", new Number(0x36         ));
GlobalSymbolTable.Set("VK_7", new Number(0x37         ));
GlobalSymbolTable.Set("VK_8", new Number(0x38         ));
GlobalSymbolTable.Set("VK_9", new Number(0x39         ));
GlobalSymbolTable.Set("VK_A", new Number(0x41         ));
GlobalSymbolTable.Set("VK_B", new Number(0x42	      ));
GlobalSymbolTable.Set("VK_C", new Number(0x43	      ));
GlobalSymbolTable.Set("VK_D", new Number(0x44	      ));
GlobalSymbolTable.Set("VK_E", new Number(0x45	      ));
GlobalSymbolTable.Set("VK_F", new Number(0x46	      ));
GlobalSymbolTable.Set("VK_G", new Number(0x47	      ));
GlobalSymbolTable.Set("VK_H", new Number(0x48	      ));
GlobalSymbolTable.Set("VK_I", new Number(0x49	      ));
GlobalSymbolTable.Set("VK_J", new Number(0x4A	      ));
GlobalSymbolTable.Set("VK_K", new Number(0x4B	      ));
GlobalSymbolTable.Set("VK_L", new Number(0x4C	      ));
GlobalSymbolTable.Set("VK_M", new Number(0x4D	      ));
GlobalSymbolTable.Set("VK_N", new Number(0x4E	      ));
GlobalSymbolTable.Set("VK_O", new Number(0x4F	      ));
GlobalSymbolTable.Set("VK_P", new Number(0x50	      ));
GlobalSymbolTable.Set("VK_Q", new Number(0x51	      ));
GlobalSymbolTable.Set("VK_R", new Number(0x52	      ));
GlobalSymbolTable.Set("VK_S", new Number(0x53	      ));
GlobalSymbolTable.Set("VK_T", new Number(0x54	      ));
GlobalSymbolTable.Set("VK_U", new Number(0x55	      ));
GlobalSymbolTable.Set("VK_V", new Number(0x56	      ));
GlobalSymbolTable.Set("VK_W", new Number(0x57	      ));
GlobalSymbolTable.Set("VK_X", new Number(0x58	      ));
GlobalSymbolTable.Set("VK_Y", new Number(0x59	      ));
GlobalSymbolTable.Set("VK_Z", new Number(0x5A	      ));
GlobalSymbolTable.Set("VK_NUMPAD0",	new Number(0x60   ));
GlobalSymbolTable.Set("VK_NUMPAD1",	new Number(0x61   ));
GlobalSymbolTable.Set("VK_NUMPAD2",	new Number(0x62   ));
GlobalSymbolTable.Set("VK_NUMPAD3",	new Number(0x63   ));
GlobalSymbolTable.Set("VK_NUMPAD4",	new Number(0x64   ));
GlobalSymbolTable.Set("VK_NUMPAD5",	new Number(0x65   ));
GlobalSymbolTable.Set("VK_NUMPAD6",	new Number(0x66   ));
GlobalSymbolTable.Set("VK_NUMPAD7",	new Number(0x67   ));
GlobalSymbolTable.Set("VK_NUMPAD8",	new Number(0x68   ));
GlobalSymbolTable.Set("VK_NUMPAD9",	new Number(0x69   ));
GlobalSymbolTable.Set("VK_MULTIPLY", new Number(0x6A  ));
GlobalSymbolTable.Set("VK_ADD",	new Number(0x6B       ));
GlobalSymbolTable.Set("VK_SUBTRACT", new Number(0x6D  ));
GlobalSymbolTable.Set("VK_DECIMAL", new Number(0x6E   ));
GlobalSymbolTable.Set("VK_DIVIDE", new Number(0x6F    ));
GlobalSymbolTable.Set("VK_F1",	new Number(0x70       ));
GlobalSymbolTable.Set("VK_F2",	new Number(0x71       ));
GlobalSymbolTable.Set("VK_F3",	new Number(0x72       ));
GlobalSymbolTable.Set("VK_F4",	new Number(0x73       ));
GlobalSymbolTable.Set("VK_F5",	new Number(0x74       ));
GlobalSymbolTable.Set("VK_F6",	new Number(0x75       ));
GlobalSymbolTable.Set("VK_F7",	new Number(0x76       ));
GlobalSymbolTable.Set("VK_F8",	new Number(0x77       ));
GlobalSymbolTable.Set("VK_F9",	new Number(0x78       ));
GlobalSymbolTable.Set("VK_F10",	new Number(0x79       ));
GlobalSymbolTable.Set("VK_F11",	new Number(0x7A       ));
GlobalSymbolTable.Set("VK_F12",	new Number(0x7B       ));

*/