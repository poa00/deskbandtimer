﻿using AwqatSalaat.Data;
using AwqatSalaat.Helpers;
using AwqatSalaat.Services.AlAdhan;
using AwqatSalaat.Services.IslamicFinder;
using System;

using Settings = AwqatSalaat.Properties.Settings;

namespace AwqatSalaat.ViewModels
{
    public class WidgetSettingsViewModel : ObservableObject
    {
        private bool isOpen = !Settings.Default.IsConfigured;
        private (
            PrayerTimesService service,
            School school,
            string method,
            string countryCode,
            string city,
            string zipCode,
            decimal latitude,
            decimal longitude,
            LocationDetectionMode locationDetection)? _serviceSettingsBackup;

        public static Country[] AvailableCountries => CountriesProvider.GetCountries();

        public bool IsOpen { get => isOpen; set => Open(value); }
        public bool UseArabic { get => Settings.DisplayLanguage == "ar"; set => SetLanguage("ar"); }
        public bool UseEnglish { get => Settings.DisplayLanguage == "en"; set => SetLanguage("en"); }
        public Settings Settings => Settings.Default;
        public RelayCommand Save { get; }
        public RelayCommand Cancel { get; }
        public LocatorViewModel Locator { get; } = new LocatorViewModel();

        public event Action<bool> Updated;

        public WidgetSettingsViewModel()
        {
            Save = new RelayCommand(SaveExecute);
            Cancel = new RelayCommand(CancelExecute, o => Settings.IsConfigured);

            if (!Settings.IsConfigured)
            {
                Settings.Upgrade();
            }

            if (string.IsNullOrEmpty(Settings.DisplayLanguage))
            {
                Settings.DisplayLanguage = LocaleManager.Default.Current;
            }

            Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.DisplayLanguage))
                {
                    if (UseArabic)
                    {
                        OnPropertyChanged(nameof(UseArabic));
                    }
                    else
                    {
                        OnPropertyChanged(nameof(UseEnglish));
                    }
                }
            };
        }

        private void SaveExecute(object obj)
        {
            var currentServiceSettings = (
                    Settings.Service,
                    Settings.School,
                    Settings.MethodString,
                    Settings.CountryCode,
                    Settings.City,
                    Settings.ZipCode,
                    Settings.Latitude,
                    Settings.Longitude,
                    Settings.LocationDetection
                    );
            bool serviceSettingsChanged = _serviceSettingsBackup != currentServiceSettings;
            Settings.IsConfigured = true;
            Settings.Save();
            IsOpen = false;
            Cancel.RaiseCanExecuteChanged();
            Updated?.Invoke(serviceSettingsChanged);
        }

        private void CancelExecute(object obj)
        {
            Settings.Reload();
            Locator.SearchQuery = null;
            SetLanguage(Settings.DisplayLanguage);
            IsOpen = false;
            Cancel.RaiseCanExecuteChanged();
        }

        private void Open(bool value)
        {
            SetProperty(ref isOpen, value, nameof(IsOpen));

            if (value)
            {
                _serviceSettingsBackup = (
                    Settings.Service,
                    Settings.School,
                    Settings.MethodString,
                    Settings.CountryCode,
                    Settings.City,
                    Settings.ZipCode,
                    Settings.Latitude,
                    Settings.Longitude,
                    Settings.LocationDetection
                    );
            }
            else
            {
                _serviceSettingsBackup = null;
            }
        }

        private void SetLanguage(string lang)
        {
            LocaleManager.Default.Current = lang;
        }
    }
}