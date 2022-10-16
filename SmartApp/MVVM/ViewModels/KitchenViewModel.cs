using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using SmartApp.MVVM.Models;
using SmartApp.Services;

namespace SmartApp.MVVM.ViewModels
{
    internal class KitchenViewModel : ObservableObject
    {
        private readonly RegistryManager registryManager = RegistryManager.CreateFromConnectionString("HostName=BobbyS.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=k8PxzcTC8WN9Kl3X0XmoDTLsPTQlT84/PN4bZA/vfHc=");
        private WeatherService _weatherService;
        private List<DeviceItem> _tempList;

        public KitchenViewModel()
        {
            _weatherService = new WeatherService();
            _tempList = new List<DeviceItem>();
            DeviceItems = new ObservableCollection<DeviceItem>();
            InitializeAsync().ConfigureAwait(false);


        }
        private async Task UpdateDeviceItemsAsync()
        {
            _tempList.Clear();

            foreach (var item in DeviceItems)
            {
                var device = await registryManager.GetDeviceAsync(item.DeviceId);
                if (device == null)
                    _tempList.Add(item);
            }

            foreach (var item in _tempList)
            {
                DeviceItems.Remove(item);
            }
        }

        private async Task UpdateWeatherAsync()
        {
            var data = await _weatherService.GetWeatherDataAsync();
            CurrentWeatherCondition = data.WeatherCondition;
            CurrentTemperature = data.Temperature;
        }

        private async Task InitializeAsync()
        {
            await UpdateWeatherAsync();

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            await PopulateDeviceTaskAsync();

            while (await timer.WaitForNextTickAsync())
            {
                await PopulateDeviceTaskAsync();
                await UpdateDeviceItemsAsync();
            }

        }


        public string Title { get; set; } = "Kitchen and Dining";

        private string _currentWeatherCondition;

        public string CurrentWeatherCondition
        {
            get { return _currentWeatherCondition; }
            set
            {
                _currentWeatherCondition = value;
                OnPropertyChanged();
            }
        }

        private int _currentTemperature;

        public int CurrentTemperature
        {
            get { return _currentTemperature; }
            set
            {
                _currentTemperature = value;
                OnPropertyChanged();
            }
        }



        private ObservableCollection<DeviceItem>? _deviceItems;
        public ObservableCollection<DeviceItem>? DeviceItems
        {
            get => _deviceItems;
            set
            {
                _deviceItems = value;
                OnPropertyChanged();
            }
        }



        private async Task PopulateDeviceTaskAsync()
        {
            var result = registryManager.CreateQuery("select * from devices where properties.reported.location = 'kitchen'");

            if (result.HasMoreResults)
            {
                foreach (Twin twin in await result.GetNextAsTwinAsync())
                {
                    var device = DeviceItems.FirstOrDefault(x => x.DeviceId == twin.DeviceId);
                    if (device == null)
                    {
                        device = new DeviceItem
                        {
                            DeviceId = twin.DeviceId,
                        };
                        try { device.DeviceName = twin.Properties.Reported["deviceName"]; }
                        catch { device.DeviceName = device.DeviceId; }
                        try { device.DeviceType = twin.Properties.Reported["deviceType"]; }
                        catch { }

                        switch (device.DeviceType.ToLower())
                        {
                            case "fan":
                                device.IconActive = "\uf863";
                                device.IconInActive = "\uf863";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            case "light":
                                device.IconActive = "\uf672";
                                device.IconInActive = "\uf0eb";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            default:
                                device.IconActive = "\uf2db";
                                device.IconInActive = "\uf2db";
                                device.StateActive = "ENABLE";
                                device.StateInActive = "DISABLE";
                                break;
                        }

                        DeviceItems.Add(device);
                    }
                    else { }

                }

            }
            else
            {
                DeviceItems.Clear();
            }
        }

    }

}








