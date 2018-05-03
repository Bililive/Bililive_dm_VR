using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;

namespace Bililive_dm_VR.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly BinarySerializer binarySerializer = new BinarySerializer()
        {
            Encoding = Encoding.UTF8,
            Endianness = Endianness.Big,
        };

        private RpcServer server;

        public ObservableCollection<Profile> Profiles { get; set; }

        private int _roomid;
        public int RoomId
        {
            get => _roomid;
            set
            {
                if (value == _roomid)
                    return;
                _roomid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoomId)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            LoadConfig();

            LoadRpcServer();

            LoadRenderer();
        }

        private void LoadRpcServer()
        {
            server = new RpcServer(binarySerializer);
        }

        private void LoadConfig()
        {
            try
            {
                var config = binarySerializer.Deserialize<ConfigFile>(File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, "config.bin")));
                RoomId = config.RoomId;
                Profiles = new ObservableCollection<Profile>(config.Profiles);
            }
            catch (Exception)
            {
                Profiles = new ObservableCollection<Profile>();
                Profiles.Add(new Profile()
                {
                    Name = "默认",
                    MountDevice = MountDevice.RightController,
                    MountLocation = MountLocation.BelowFlipped,
                    AnimationType = AnimationType.AlphaAndScale,
                    // TODO 
                });
            }
        }

        private void LoadRenderer()
        {
            bool runUnity = true;
#if DEBUG
            runUnity = false;
#endif
            if (runUnity)
                Unity.Children.Add(new UnityHost(Unity.ActualWidth, Unity.ActualHeight, server.PipeName));
        }



        public event PropertyChangedEventHandler PropertyChanged;
    }
}
