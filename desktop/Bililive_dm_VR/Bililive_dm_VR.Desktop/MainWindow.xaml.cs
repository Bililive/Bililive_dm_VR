using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            binarySerializer.MemberSerializing += OnMemberSerializing;
            binarySerializer.MemberSerialized += OnMemberSerialized;
            binarySerializer.MemberDeserializing += OnMemberDeserializing;
            binarySerializer.MemberDeserialized += OnMemberDeserialized;


            LoadConfig();

            Closed += MainWindow_Closed;

            LoadRpcServer();

            LoadRenderer();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                binarySerializer.Serialize(File.OpenWrite(Path.Combine(Environment.CurrentDirectory, "config.bin")), new ConfigFile()
                {
                    RoomId = RoomId,
                    Profiles = Profiles.ToList(),
                });
            }
            catch (Exception)
            { }
        }

        private void LoadRpcServer()
        {
            server = new RpcServer(binarySerializer);
        }

        private void LoadConfig()
        {
            try
            {
                var s = new FileStream(new FileInfo(Path.Combine(Environment.CurrentDirectory, "config.bin")).FullName, FileMode.Open);
                var config = binarySerializer.Deserialize<ConfigFile>(s);
                RoomId = config.RoomId;
                Profiles = new ObservableCollection<Profile>(config.Profiles);
            }
            catch (Exception)
            {
                Profiles = new ObservableCollection<Profile>
                {
                    new Profile()
                    {
                        Name = "默认",
                        MountDevice = MountDevice.RightController,
                        MountLocation = MountLocation.BelowFlipped,
                        AnimateOnGaze = AnimationType.AlphaAndScale,
                        // TODO 
                    }
                };
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


        private static void OnMemberSerializing(object sender, MemberSerializingEventArgs e)
        {
            Debug.Write(string.Join("    ", Enumerable.Range(0, e.Context.Depth).Select(x => "")));
            Debug.WriteLine("S-Start: {0} ({1}) @ {2}", e.MemberName, e.Context.Value ?? "null", e.Offset);
        }

        private static void OnMemberSerialized(object sender, MemberSerializedEventArgs e)
        {
            Debug.Write(string.Join("    ", Enumerable.Range(0, e.Context.Depth).Select(x => "")));
            Debug.WriteLine("S-End: {0} ({1}) @ {2}", e.MemberName, e.Value ?? "null", e.Offset);
        }

        private static void OnMemberDeserializing(object sender, MemberSerializingEventArgs e)
        {
            Debug.Write(string.Join("    ", Enumerable.Range(0, e.Context.Depth).Select(x => "")));
            Debug.WriteLine("D-Start: {0} ({1}) @ {2}", e.MemberName, e.Context.Value ?? "null", e.Offset);

        }

        private static void OnMemberDeserialized(object sender, MemberSerializedEventArgs e)
        {
            Debug.Write(string.Join("    ", Enumerable.Range(0, e.Context.Depth).Select(x => "")));
            Debug.WriteLine("D-End: {0} ({1}) @ {2}", e.MemberName, e.Value ?? "null", e.Offset);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            server.Send(new ConnectionCommand() { Connect = true, RoomId = 123 });
        }
    }
}
