﻿using QuickLauncher.Model;
using System.Collections.Generic;
using System.Linq;


namespace QuickLauncher
{
    public class SettingItemUtils
    {
        private static readonly Dictionary<string, SettingItem> Cache = new Dictionary<string, SettingItem>();
        public static SettingItem GetByKey(string key, string defaultValue, bool cacheAble = false)
        {
            SettingItem item = null;
            if (cacheAble && Cache.ContainsKey(key))
            {
                item = Cache[key];
            }
            if (item == null)
            {
                item = GetSettingItem(key);
                if (cacheAble)
                {
                    Cache[key] = item;
                }
            }

            if (item == null && defaultValue != null)
            {
                item = new SettingItem { Key = key, Value = defaultValue };
                SaveSettingItem(item);
                if (cacheAble)
                {
                    Cache[key] = item;
                }
            }

            return item;
        }

        internal static SettingItem GetEnableAutoDetect()
        {
            return GetByKey("auto.detect.commands", "0", true);
        }

        internal static SettingItem GetEnableReminder()
        {
            return GetByKey("reminder.enable", "0", true);
        }

        internal static SettingItem GetReminderInterval()
        {
            return GetByKey("reminder.interval", "40", true);
        }

        internal static SettingItem GetReminderNote()
        {
            return GetByKey("reminder.note", "", true);
        }

        public static void SaveSettingItem(SettingItem item)
        {
            var dbContext = QuickCommandContext.Instance;
            var dbItem = GetSettingItem(item.Key);
            if (dbItem == null)
            {
                dbContext.SettingItems.Add(item);
            }
            else
            {
                dbItem.Value = item.Value;
            }
            dbContext.SaveChanges();
            if (Cache.ContainsKey(item.Key))
            {
                Cache[item.Key] = item;
            }
        }

        private static SettingItem GetSettingItem(string key)
        {
            var dbContext = QuickCommandContext.Instance;
            var query = from b in dbContext.SettingItems
                        where b.Key == key
                        select b;
            return query.Count() == 1 ? query.First() : null;
        }

        public static SettingItem GetDbVersion()
        {
            return GetByKey("db.version", "0", true);
        }

        public static SettingItem GetMainWindowOpenHotkey()
        {
            return GetByKey("hotkey.main.window", "", true);
        }

        public static SettingItem GetAutoCommandAutoStart()
        {
            return GetByKey("auto.command.auto.start", "", true);
        }
    }
}
