using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickLauncher
{
    sealed class QuickCommandContext : DbContext
    {
        private static volatile QuickCommandContext instance;
        private static object syncRoot = new Object();

        public DbSet<QuickCommand> QuickCommands { get; set; }
        public DbSet<QuickCommandEnvConfig> QuickCommandEnvConfigs { get; set; }
        public DbSet<SettingItem> SettingItems { get; set; }

        private QuickCommandContext(DbConnection dbConn)
            :base(dbConn,false)
        {
            this.Configuration.AutoDetectChangesEnabled = true;
        }

        public static QuickCommandContext Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new QuickCommandContext(DbUtil.getConnection());
                    }
                }

                return instance;
            }
        }

    }
}
