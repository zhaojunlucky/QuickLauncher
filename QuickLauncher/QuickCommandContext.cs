using Microsoft.EntityFrameworkCore;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        private QuickCommandContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(DbUtil.getConnection());
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
                            instance = new QuickCommandContext();
                    }
                }

                return instance;
            }
        }

    }
}
