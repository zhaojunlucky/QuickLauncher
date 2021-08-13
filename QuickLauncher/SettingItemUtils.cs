using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QuickLauncher
{
    public class SettingItemUtils
    {
        private static Dictionary<string, SettingItem> cache = new Dictionary<string, SettingItem>();
        public static SettingItem GetByKey(string key, string defaulValue, bool cacheAble=false)
        {
            SettingItem item = null;
            if (cacheAble && cache.ContainsKey(key))
            {
                item = cache[key];
            }
            if (item == null)
            {
                item = getSettingItem(key);
                if (cacheAble)
                {
                    cache[key] = item;
                }
            }
            
            if (item == null && defaulValue != null)
            {
                item = new SettingItem { Key = key, Value = defaulValue };
                SaveSettingItem(item);
                if (cacheAble)
                {
                    cache[key] = item;
                }
            }
            
            return item;
        }

        public static void SaveSettingItem(SettingItem item)
        {
            var dbContext = QuickCommandContext.Instance;
            var dbItem = getSettingItem(item.Key);
            if (dbItem == null)
            {
                dbContext.SettingItems.Add(item);
            }
            else
            {
                dbItem.Value = item.Value;
            }
            dbContext.SaveChanges();
        }

        private static SettingItem getSettingItem(string key)
        {
            var dbContext = QuickCommandContext.Instance;
            var query = from b in dbContext.SettingItems
                        where b.Key == key
                        select b;
            return query.Count() == 1 ? query.First() : null;
        }

        public static SettingItem GetViewMode()
        {
            return GetByKey("view.viewmode", "TV", true);
        }

        public static SettingItem GetDbVersion()
        {
            return GetByKey("db.version", "0", true);
        }
    }
}
