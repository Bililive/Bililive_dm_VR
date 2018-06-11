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
            if (!Environment.Is64BitProcess)
            {
                MessageBox.Show("不支持 32 位操作系统", "B站VR弹幕姬", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Managed", args.Name.Split(',')[0] + ".dll");
                if (File.Exists(fullpath))
                    return Assembly.LoadFrom(fullpath);
                else
                    return null;
            };

            SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Plugins"));

        }
    }
}
