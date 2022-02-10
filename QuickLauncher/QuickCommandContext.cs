using Microsoft.EntityFrameworkCore;
using QuickLauncher.Model;
using System;

namespace QuickLauncher
{
    sealed class QuickCommandContext : DbContext
    {
        private static volatile QuickCommandContext _instance;
        private static readonly object SyncRoot = new Object();

        public DbSet<QuickCommand> QuickCommands { get; set; }
        public DbSet<QuickCommandEnvConfig> QuickCommandEnvConfigs { get; set; }
        public DbSet<SettingItem> SettingItems { get; set; }

        private QuickCommandContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(DbUtil.GetConnection());
        }

        public static QuickCommandContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        _instance ??= new QuickCommandContext();
                    }
                }

                return _instance;
            }
        }

    }
}
