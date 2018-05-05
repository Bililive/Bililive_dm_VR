using net.r_eg.Conari;
using net.r_eg.Conari.Types;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Bililive_dm_VR.Desktop
{
    internal class UnityHost : HwndHost
    {
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
        private const int HOST_ID = 0x00000002;

        private IntPtr hwndHost;
        private int hostHeight, hostWidth;

        private string args;

        private Process process;
        private IntPtr unityHWND = IntPtr.Zero;

        public UnityHost(double width, double height, string arguments)
        {
            hostHeight = (int)height;
            hostWidth = (int)width;
            args = arguments;
        }

        // [DllImport("UnityPlayer.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        // private static extern int UnityMain(IntPtr hInstance, IntPtr hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, int nShowCmd);

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hwndHost = IntPtr.Zero;

            hwndHost = CreateWindowEx(0, "static", "",
                                      WS_CHILD | WS_VISIBLE,
                                      0, 0,
                                      hostWidth, hostHeight,
                                      hwndParent.Handle,
                                      (IntPtr)HOST_ID,
                                      IntPtr.Zero,
                                      0);


            try
            {

                ConariL unityPlayer = new ConariL("UnityPlayer.dll");

                IntPtr hInstance = Marshal.GetHINSTANCE(typeof(UnityHost).Module);
                IntPtr hPrevInstance = IntPtr.Zero;
                string lpCmdLine_raw = "Bililive_dm_VR.Renderer.exe -parentHWND" + hwndHost.ToInt32() + " " + args;
                var lpCmdLine = new UnmanagedString(lpCmdLine_raw, UnmanagedString.SType.Unicode);
                int nShowCmd = 0;

                var unityMain = unityPlayer.bind<Func<IntPtr, IntPtr, IntPtr, int, int>>("UnityMain");

                new Thread(() =>
                {
                    int result = unityMain(hInstance, hPrevInstance, lpCmdLine.Pointer, nShowCmd);
                    MessageBox.Show("Unity Quit: " + result);
                })
                {
                    Name = "UnityMain",
                    IsBackground = true,
                }.Start();

                // process = Process.Start(new ProcessStartInfo()
                // {
                //     FileName = "Bililive_dm_VR.Renderer.exe",
                //     Arguments = "-parentHWND " + hwndHost.ToInt32() + " " + args,
                //     UseShellExecute = true,
                //     CreateNoWindow = true
                // });
                // 
                // process.WaitForInputIdle();
                // Doesn't work for some reason ?!
                //unityHWND = process.MainWindowHandle;
                EnumChildWindows(hwndHost, WindowEnum, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "GameHost: Could not find game executable.");
            }

            return new HandleRef(this, hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            try
            {
                process.CloseMainWindow();

                System.Threading.Thread.Sleep(1000);
                while (process.HasExited == false)
                    process.Kill();
            }
            catch (Exception)
            { }

            DestroyWindow(hwnd.Handle);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            return 0;
        }

        //PInvoke declarations
        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                      string lpszClassName,
                                                      string lpszWindowName,
                                                      int style,
                                                      int x, int y,
                                                      int width, int height,
                                                      IntPtr hwndParent,
                                                      IntPtr hMenu,
                                                      IntPtr hInst,
                                                      [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);
    }
}
