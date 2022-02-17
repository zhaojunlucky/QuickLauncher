using System.Collections.Generic;

namespace QuickLauncher.Misc
{
    internal class AutoDetectCommandAutoStartMgr
    {
        public static AutoDetectCommandAutoStartMgr Current => LazyInitializer.Instance;

        private static class LazyInitializer
        {
            static LazyInitializer() { }
            public static readonly AutoDetectCommandAutoStartMgr Instance = new AutoDetectCommandAutoStartMgr();
        }

        private readonly HashSet<string> autoStartItems = new HashSet<string>();
        public AutoDetectCommandAutoStartMgr()
        {
            LoadFromSetting();
        }

        private void LoadFromSetting()
        {
            var settingItem = SettingItemUtils.GetAutoCommandAutoStart();
            foreach (var uuid in settingItem.Value?.Split(":")!)
            {
                if (string.IsNullOrEmpty(uuid))
                {
                    continue;
                }
                autoStartItems.Add(uuid);
            }
        }

        public void ChangeItem(string uuid, bool autoStart)
        {
            if (autoStart)
            {
                autoStartItems.Add(uuid);
            }
            else
            {
                autoStartItems.Remove(uuid);
            }
            
            SaveSettings();
        }

        private void SaveSettings()
        {
            var settingItem = SettingItemUtils.GetAutoCommandAutoStart();
            settingItem.Value = string.Join(":", autoStartItems);
            SettingItemUtils.SaveSettingItem(settingItem);
        }

        public bool IsAutoStart(string uuid)
        {
            return autoStartItems.Contains(uuid);
        }
    }
}
