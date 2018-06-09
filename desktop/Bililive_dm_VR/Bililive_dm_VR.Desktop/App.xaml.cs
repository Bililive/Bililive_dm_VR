using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Bililive_dm_VR.Desktop
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        public App()
        {
            Environment.SetEnvironmentVariable("PATH",
                Environment.GetEnvironmentVariable("PATH") + ";" +
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bililive_dm_VR.Renderer_Data\Managed")
            );

            SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Bililive_dm_VR.Renderer_Data\Plugins"));

        }
    }
}
