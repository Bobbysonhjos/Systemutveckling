using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DeviceIntelliFan.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace DeviceIntelliFan;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly string _deviceId = "IntelliFan";
    private readonly string _deviceType = "intelliFan";
    private bool _isConnected;
    private bool _isRunning;
    private readonly string _location = "kitchen";
    private readonly string _owner = "Bobby Sonhjos";
    private int _interval = 10000;
    private DeviceClient deviceClient;


    public MainWindow()
    {
        InitializeComponent();
        Initialize();

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += dispatcherTimer_Tick;
        dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
        dispatcherTimer.Start();
    }


    private void dispatcherTimer_Tick(object sender, EventArgs e)
    {
        BtnAction.IsEnabled = _isConnected;
        if (_isConnected)
            tblockConnectionState.Text = "Connected";
        else
            tblockConnectionState.Text = "Not Connected";
    }


    private void Initialize()
    {
        _isConnected = Task.Run(async () =>
        {
            while (!_isConnected)
                try
                {
                    using var http = new HttpClient();
                    var result = await http.PostAsJsonAsync("https://bobbys.azurewebsites.net/api/devices/connect?", new
                    {
                        deviceId = _deviceId,
                        deviceType = _deviceType,
                        location = _location,
                        owner = _owner
                    });

                    if (result.IsSuccessStatusCode || result.StatusCode.ToString() == "Conflict")
                    {
                        var data = JsonConvert.DeserializeObject<DeviceResponse>(
                            await result.Content.ReadAsStringAsync());
                        deviceClient = DeviceClient.CreateFromConnectionString(data.ConnectionString);

                        var twin = await deviceClient.GetTwinAsync();
                        if (twin != null)
                        {
                            try
                            {
                                _interval = twin.Properties.Desired["interval"];
                            }
                            catch 
                            {
                              // ignored 
                            }

                            var reported = new TwinCollection();
                            reported["owner"] = _owner;
                            reported["deviceType"] = _deviceType;
                            reported["location"] = _location;

                            await deviceClient.UpdateReportedPropertiesAsync(reported);
                           
                            
                          

                            return true;
                        }
                    }
                }
                catch
                {
                    // Ignored
                }

            return false;
        }).Result;

    }

    private async Task SendMessageAsync()
    {
        while (true)
        {
            if (_isConnected)
            {
                if (_isRunning)
                {

                    //d2c
                    var json = JsonConvert.SerializeObject(new { isRunning = _isRunning });

                    var message = new Message(Encoding.UTF8.GetBytes(json));

                    message.Properties.Add("deviceName", _deviceId);
                    message.Properties.Add("deviceType", _deviceType);
                    message.Properties.Add("location", _location);
                    message.Properties.Add("owner", _owner);


                    await deviceClient.SendEventAsync(message);

                    //devicetwin (reported)
                    var twinCollection = new TwinCollection();
                    twinCollection["deviceState"] = _isRunning;
                    await deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                }
            }
            await Task.Delay(_interval);
        }
    }



    private void BtnAction_OnClick(object sender, RoutedEventArgs e)
    {
        var iconRotateBladeStoryBoard = (BeginStoryboard)TryFindResource("iconRotateBladeStoryBoard");

        if (!_isRunning)
        {
            iconRotateBladeStoryBoard.Storyboard.Begin();
            _isRunning = true;
            BtnAction.Content = "Stop";
        }
        else
        {
            iconRotateBladeStoryBoard.Storyboard.Stop();
            _isRunning = false;
            BtnAction.Content = "Start";
        }
    }
}