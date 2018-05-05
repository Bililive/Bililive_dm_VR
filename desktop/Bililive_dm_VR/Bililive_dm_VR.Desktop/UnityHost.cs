using EasyHook;
using net.r_eg.Conari;
using net.r_eg.Conari.Types;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, EntryPoint = "LoadLibraryW")]
        private extern static IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)]string file);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, EntryPoint = "VirtualProtect")]
        private extern static int VirtualProtect(IntPtr pBaseAddr, int size, int newProtect, out int oldProtect);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true, EntryPoint = "VirtualAlloc")]
        private extern static IntPtr VirtualAlloc(IntPtr pBase, int size, int memtype, int protect);

        [DllImport("UnityPlayer.dll")]
        private static extern int UnityMain(IntPtr hInstance, IntPtr hPrevInstance, [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, int nShowCmd);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.LPTStr)]
        delegate string GetCommandLineDelegate();


        static string CommandLineValue = "";
        static string GetCommandLineHook()
        {
            return CommandLineValue;
        }

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

                // ConariL unityPlayer = new ConariL("UnityPlayer.dll");

                IntPtr hInstance = Marshal.GetHINSTANCE(typeof(UnityHost).Module);
                IntPtr hPrevInstance = IntPtr.Zero;
                string lpCmdLine_raw = "Bililive_dm_VR.Renderer.exe -parentHWND " + hwndHost.ToInt32() + " " + args;
                var lpCmdLine = new UnmanagedString(lpCmdLine_raw, UnmanagedString.SType.Unicode);
                int nShowCmd = 0;

                // var unityMain = unityPlayer.bind<Func<IntPtr, IntPtr, IntPtr, int, int>>("UnityMain");

                new Thread(() =>
                {
                    //mov eax, xxxxxx   
                    //ret C3

                    // IntPtr pBaseAddr = LoadLibraryW("UnityPlayer.dll");
                    // IntPtr pHookAddr = new IntPtr(pBaseAddr.ToInt64() + 0x010fd658);
                    // 
                    // IntPtr pCmdLine = Marshal.AllocCoTaskMem(lpCmdLine_raw.Length * 2 + 2);
                    // Marshal.Copy(Encoding.Unicode.GetBytes(lpCmdLine_raw), 0, pCmdLine, lpCmdLine_raw.Length * 2);
                    // 
                    // byte[] funcBytes = new byte[] {
                    //     0x48, 0xB8,
                    //     (byte)((pCmdLine.ToInt64() >> 0) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 8) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 16) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 24) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 32) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 40) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 48) & 0xff),
                    //     (byte)((pCmdLine.ToInt64() >> 56) & 0xff),
                    //     0xc3
                    // };
                    // 
                    // IntPtr FP = VirtualAlloc(IntPtr.Zero, 12, 0x1000, 0x40);
                    // Marshal.Copy(funcBytes, 0, FP, funcBytes.Length);
                    // 
                    // IntPtr[] getCmdLineFP = new IntPtr[1];
                    // getCmdLineFP[0] = FP;
                    // 
                    // int oldProtect, useless;
                    // VirtualProtect(pHookAddr, 8, 4, out oldProtect);
                    // Marshal.Copy(getCmdLineFP, 0, pHookAddr, 1);
                    // VirtualProtect(pHookAddr, 8, oldProtect, out useless);

                    // CommandLineValue = lpCmdLine_raw;
                    // var hook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "GetCommandLineW"), new GetCommandLineDelegate(GetCommandLineHook), null);
                    // hook.ThreadACL.SetExclusiveACL(new int[] { });

                    int result = UnityMain(hInstance, hPrevInstance, lpCmdLine_raw, nShowCmd);
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
