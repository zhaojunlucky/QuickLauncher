using MahApps.Metro.Controls;
using QuickLauncher.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Utility;
using Utility.HotKey;
using Utility.Model;

namespace QuickLauncher.Model
{
    class SettingDialogModel : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private SettingItem viewMode;
        private SettingItem mainWindowHotKey;
        private HotKey hotKey;
        private MetroWindow owningWindow;
        private string currentHotKey;
        private RawHotKey rawHotKey;
        public SettingDialogModel(MetroWindow window)
        {
            owningWindow = window;
            viewMode = SettingItemUtils.GetViewMode();
            mainWindowHotKey = SettingItemUtils.GetMainWindowOpenHotkey();
            currentHotKey = mainWindowHotKey.Value;
            InitHotKey();
        }

        internal void Save()
        {
            if (mainWindowHotKey.Value != currentHotKey)
            {
                SafeSave(mainWindowHotKey);
                MainWindow main = (MainWindow)Application.Current.MainWindow;
                main.RegisterHotKeys(true);
            }
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
            if (curInput != currentHotKey)
            {
                MainWindowOpenHotkey = new HotKey(rawHotKey.Key, rawHotKey.HotKeyModifiers);
            }
            else // clear
            {
                MainWindowOpenHotkey = new HotKey(Key.None, ModifierKeys.None);
            }
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
                DialogUtil.showError(owningWindow, e.Message);
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
                    DialogUtil.showError(owningWindow, r.Message);
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
                    DialogUtil.showError(owningWindow, r.InnerException.Message);
                }

            }
        }

        private string CheckMainWindowOpenHotkey()
        {
            try
            {
                var key = "CheckMainWindowOpenHotkey";
                HotkeyManager.Current.RegisterHotKey(key, mainWindowHotKey.Value, (object sender, HotkeyEventArgs e) => { });
                HotkeyManager.Current.UnRegisterHotKey(key);
            }
            catch (HotkeyAlreadyRegisteredException e)
            {
                Trace.TraceError(e.ToString());
                return String.Format("{0} is already registered", hotKey.ToString());
                
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
                DialogUtil.showError(owningWindow, r.InnerException.Message);
            }
        }

        public bool ViewMode
        {
            get
            {
                return viewMode.Value == "TV";
            }
            set
            {
                viewMode.Value = value ? "TV" : "DV";

                SafeSave(viewMode);
            }
        }

        public HotKey MainWindowOpenHotkey
        {
            get
            {
                return hotKey;
            }
            set
            {
                hotKey = value;
                var hotKeyStr = value != null ? value.ToString() : "";
                mainWindowHotKey.Value = hotKeyStr.Equals("None", StringComparison.OrdinalIgnoreCase)? "" : hotKeyStr;
                RaisePropertyChanged("MainWindowOpenHotkey");
            }
        }

        public string Error => String.Empty;

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
