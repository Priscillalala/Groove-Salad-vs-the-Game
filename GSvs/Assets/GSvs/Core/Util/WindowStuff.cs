using RoR2;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GSvs.Core.Util
{
    // https://discussions.unity.com/t/solved-how-to-remove-the-title-bar-of-a-game/754827/2
    public static class WindowStuff
    {
        [ConCommand(commandName = "move_window")]
        public static void MoveWindow(ConCommandArgs args)
        {
            Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, new Vector2Int(args.GetArgInt(0), args.GetArgInt(1)));
        }

        #region DLLstuff
        const int SWP_HIDEWINDOW = 0x80; //hide window flag.
        const int SWP_SHOWWINDOW = 0x40; //show window flag.
        const int SWP_NOMOVE = 0x0002; //don't move the window flag.
        const int SWP_NOSIZE = 0x0001; //don't resize the window flag.
        const uint WS_SIZEBOX = 0x00040000;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 0x00800000; //window with border
        const int WS_DLGFRAME = 0x00400000; //window with double border but no title
        const int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar
        const int WS_SYSMENU = 0x00080000;      //window with no borders etc.
        const int WS_MAXIMIZEBOX = 0x00010000;
        const int WS_MINIMIZEBOX = 0x00020000;  //window with minimizebox

        [DllImport("user32.dll")]
        static extern System.IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(
            System.IntPtr hWnd, // window handle
            System.IntPtr hWndInsertAfter, // placement order of the window
            short X, // x position
            short Y, // y position
            short cx, // width
            short cy, // height
            uint uFlags // window flags.
        );

        [DllImport("user32.dll")]
        static extern System.IntPtr SetWindowLong(
             System.IntPtr hWnd, // window handle
             int nIndex,
             uint dwNewLong
        );

        [DllImport("user32.dll")]
        static extern System.IntPtr GetWindowLong(
            System.IntPtr hWnd,
            int nIndex
        );

        static System.IntPtr hWnd;
        static  System.IntPtr HWND_TOP = new System.IntPtr(0);
        static  System.IntPtr HWND_TOPMOST = new System.IntPtr(-1);
        static  System.IntPtr HWND_NOTOPMOST = new System.IntPtr(-2);

        #endregion

        [ConCommand(commandName = "show_window_borders")]
        public static void ShowWindowBordersCommand(ConCommandArgs args)
        {
            hWnd = GetActiveWindow(); //Gets the currently active window handle for use in the user32.dll functions.
            ShowWindowBorders(args.GetArgBool(0));
        }

        public static void ShowWindowBorders(bool value)
        {
            if (Application.isEditor) return; //We don't want to hide the toolbar from our editor!

            int style = GetWindowLong(hWnd, GWL_STYLE).ToInt32(); //gets current style

            if (value)
            {
                SetWindowLong(hWnd, GWL_STYLE, (uint)(style | WS_CAPTION | WS_SIZEBOX)); //Adds caption and the sizebox back.
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW); //Make the window normal.
            }
            else
            {
                SetWindowLong(hWnd, GWL_STYLE, (uint)(style & ~(WS_CAPTION | WS_SIZEBOX))); //removes caption and the sizebox from current style.
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW); //Make the window render above toolbar.
            }
        }
    }
}