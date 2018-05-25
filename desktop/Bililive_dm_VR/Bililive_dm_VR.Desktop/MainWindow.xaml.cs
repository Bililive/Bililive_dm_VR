using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        private Profile selectedProfile;
        public Profile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                if (selectedProfile == value)
                    return;
                if (selectedProfile != null)
                    selectedProfile.PropertyChanged -= SelectedProfile_PropertyChanged;
                selectedProfile = value;
                if (selectedProfile != null)
                    selectedProfile.PropertyChanged += SelectedProfile_PropertyChanged;
                NotifyPropertyChanged(nameof(SelectedProfile));
            }
        }
        private void SelectedProfile_PropertyChanged(object sender, PropertyChangedEventArgs e) => NotifyPropertyChanged(nameof(SelectedProfile));

        private int _roomid;
        public int RoomId
        {
            get => _roomid;
            set
            {
                if (value == _roomid)
                    return;
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(RoomId));
                _roomid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoomId)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LoadConfig();

            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;

            LoadRpcServer();

            PropertyChanged += MainWindow_PropertyChanged;
            Profiles.CollectionChanged += Profiles_CollectionChanged;

            LoadRenderer();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            server.Shutdown();
        }

        private void Profiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => ProfileComboBox.SelectedIndex = Profiles.Count - 1;

        private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedProfile))
            {
                if (SelectedProfile != null)
                {
                    server.Send(new ProfileCommand() { Profile = SelectedProfile });
                }
            }
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
            server.Connected += (sender, e) => server.Send(new ProfileCommand { Profile = SelectedProfile });
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
                Profiles = new ObservableCollection<Profile> { new Profile() };
            }
            if (Profiles.Count > 0)
                SelectedProfile = Profiles[Profiles.Count - 1];
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
        private void NotifyPropertyChanged(params string[] propertyNames)
        { if (PropertyChanged != null) foreach (string propertyName in propertyNames) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private void ChooseColor(object sender, RoutedEventArgs e)
        {
            var cp = new ColorPicker();
            cp.Picker.SelectedColor = new System.Windows.Media.Color()
            {
                R = (byte)((SelectedProfile.Color >> 24) & 0xff),
                G = (byte)((SelectedProfile.Color >> 16) & 0xff),
                B = (byte)((SelectedProfile.Color >> 8) & 0xff),
                A = 0xff,
            };
            cp.ShowDialog();
            var color = cp.Picker.SelectedColor.Value;
            SelectedProfile.Color = (color.R << 24) + (color.G << 16) + (color.B << 8) + 0xff;
        }

        private void CreateNewProfile(object sender, RoutedEventArgs e)
        {
            Profiles.Add(new Profile());
        }

        private void RemoveCurrentProfile(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile != null && Profiles.Count > 1)
            {
                Profiles.Remove(SelectedProfile);
            }
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    var json = JObject.Parse(new WebClient() { Encoding = Encoding.UTF8, Headers = { { HttpRequestHeader.UserAgent, "Bililive_dm_VR (https://github.com/Bililive/Bililive_dm_VR)" } } }
                    .DownloadString("https://api.live.bilibili.com/room/v1/Room/room_init?id=" + RoomId));
                    if ((json?["code"]?.ToObject<int>() ?? -1) != 0)
                    {
                        MessageBox.Show("获取直播间信息错误" + Environment.NewLine + json?["message"]?.ToObject<string>() ?? json?["msg"]?.ToObject<string>() ?? string.Empty);
                        return;
                    }
                    var real_roomid = json?["data"]?["room_id"]?.ToObject<int>() ?? -1;
                    if (real_roomid > 0)
                        server.Send(new ConnectionCommand { Connect = true, RoomId = real_roomid });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发生了错误" + Environment.NewLine + ex.ToString());
                }
            });
        }

        private void Disconnect(object sender, RoutedEventArgs e)
        {
            server.Send(new ConnectionCommand { Connect = false });
        }
    }
}
