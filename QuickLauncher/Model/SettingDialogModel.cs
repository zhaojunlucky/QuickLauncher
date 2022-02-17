using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Utility;
using Utility.HotKey;
using Utility.Model;

namespace QuickLauncher.Model
{
    class SettingDialogModel : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private readonly SettingItem mainWindowHotKey;
        private readonly SettingItem enableAutoDetect;
        private HotKey hotKey;
        private readonly MetroWindow owningWindow;
        private readonly string currentHotKey;
        private RawHotKey rawHotKey;
        public SettingDialogModel(MetroWindow window)
        {
            owningWindow = window;
            mainWindowHotKey = SettingItemUtils.GetMainWindowOpenHotkey();
            enableAutoDetect = SettingItemUtils.GetEnableAutoDetect();
            currentHotKey = mainWindowHotKey.Value;
            InitHotKey();
        }

        internal void Save()
        {
            if (mainWindowHotKey.Value == currentHotKey)
            {
                return;
            }
            SafeSave(mainWindowHotKey);
            var main = (MainWindow)Application.Current.MainWindow;
            main?.RegisterHotKeys(true);
        }

        internal void ResetOrClear()
        {
            var curInput = MainWindowOpenHotkey.ToString();
            if (curInput.Length == 0 || curInput.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                MainWindowOpenHotkey = new HotKey(rawHotKey.Key, rawHotKey.HotKeyModifiers);
                return;
            }
            // reset
            MainWindowOpenHotkey = curInput != currentHotKey ? new HotKey(rawHotKey.Key, rawHotKey.HotKeyModifiers) : new HotKey(Key.None, ModifierKeys.None);
        }

        private void InitHotKey()
        {
            try
            {
                rawHotKey = RawHotKey.Parse(mainWindowHotKey.Value);
                hotKey = new HotKey(rawHotKey.Key, rawHotKey.HotKeyModifiers);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                DialogUtil.ShowError(owningWindow, e.Message);
            }
        }

        public bool IsAutoStart
        {
            get
            {
                try
                {
                    return AutoStartUtil.IsAutoStart();
                }
                catch (Exception r)
                {
                    Trace.TraceError(r.Message);
                    Trace.TraceError(r.StackTrace);
                    DialogUtil.ShowError(owningWindow, r.Message);
                    return false;
                }
            }
            set
            {
                try
                {
                    AutoStartUtil.SetAutoStart(value);
                }
                catch (Exception r)
                {
                    Trace.TraceError(r.Message);
                    Trace.TraceError(r.StackTrace);
                    DialogUtil.ShowError(owningWindow, (r.InnerException ?? r).Message);
                }

            }
        }

        public bool EnableAutoDetect
        {
            get => enableAutoDetect.Value == "1";
            set
            {
                enableAutoDetect.Value = value ? "1" : "0";
                RaisePropertyChanged("EnableAutoDetect");
                SafeSave(enableAutoDetect);
                PromptRestart();
            }
        }

        private async void PromptRestart()
        {
            var result = await DialogUtil.ShowYesNo("Confirm", owningWindow, "Restart to take effect?");
            if (result == MessageDialogResult.Affirmative)
            {
                AppUtil.RestartApp("PromptRestart", "/Restart");
            }
        }

        

        private string CheckMainWindowOpenHotkey()
        {
            try
            {
                var key = "CheckMainWindowOpenHotkey";
                HotkeyManager.Current.RegisterHotKey(key, mainWindowHotKey.Value, (sender, e) => { });
                HotkeyManager.Current.UnRegisterHotKey(key);
            }
            catch (HotkeyAlreadyRegisteredException e)
            {
                Trace.TraceError(e.ToString());
                return $"{hotKey} is already registered";

            }
            return null;
        }

        private void SafeSave(SettingItem settingItem)
        {
            try
            {
                SettingItemUtils.SaveSettingItem(settingItem);
            }
            catch (Exception r)
            {
                Trace.TraceError(r.Message);
                Trace.TraceError(r.StackTrace);
                DialogUtil.ShowError(owningWindow, (r.InnerException ?? r).Message);
            }
        }

        public HotKey MainWindowOpenHotkey
        {
            get => hotKey;
            set
            {
                hotKey = value;
                var hotKeyStr = value != null ? value.ToString() : "";
                mainWindowHotKey.Value = hotKeyStr.Equals("None", StringComparison.OrdinalIgnoreCase) ? "" : hotKeyStr;
                RaisePropertyChanged("MainWindowOpenHotkey");
            }
        }

        public string Error => string.Empty;

        public string this[string name]
        {
            get
            {
                if (name == "MainWindowOpenHotkey" && mainWindowHotKey.Value.Length > 0 && currentHotKey != mainWindowHotKey.Value)
                {
                    return CheckMainWindowOpenHotkey();
                }
                return null;
            }
        }
    }
}
