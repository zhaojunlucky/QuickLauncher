﻿using Microsoft.Data.Sqlite;
using QuickLauncher.Misc;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickLauncher
{
    class DbUtil
    {
        internal static string DbBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Unicorn\\QuickLancher";
        private static string QUICK_COMMAND_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND(UUID TEXT PRIMARY KEY,ALIAS TEXT UNIQUE,PATH TEXT, WORKDIRECTORY TEXT,COMMAND TEXT, CustomIcon BLOB, AUTO_START INTEGER);";
        private static string QUICK_COMMAND_EVN_CONFIG_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND_ENV_CONFIG(Id INTEGER PRIMARY KEY, PARENT_ID TEXT NOT NULL, ENV_KEY TEXT, ENV_VALUE TEXT, FOREIGN KEY(PARENT_ID) REFERENCES QUICK_COMMAND(UUID))";
        private static string SETTING_TABLE = "CREATE TABLE IF NOT EXISTS SETTING(KEY TEXT, VALUE TEXT, PRIMARY KEY(KEY))";
#if DEBUG
        private static string DbPath = "Data Source =" + DbBaseDir + "\\lancherDb-debug.db";
#else
        private static string DbPath = "Data Source =" + DbBaseDir + "\\lancherDb.db";
#endif
        public static SqliteConnection getConnection()
        {
            return new SqliteConnection(DbPath);
        }

        public static void CheckDb()
        {
            if (!Directory.Exists(DbBaseDir))
            {
                Directory.CreateDirectory(DbBaseDir);
                Trace.TraceInformation("create db folder");
            }

            if (!File.Exists(DbPath))
            {
                Trace.TraceInformation("initialize a new db");
                prepareTables();
                Trace.TraceInformation("create tables");
            }
        }

        public static void DoUpgradeDb()
        {
            var dbVerItem = SettingItemUtils.GetDbVersion();
            int v = Convert.ToInt32(dbVerItem.Value);
            Trace.TraceInformation("current database version {0}", v);
            UpgradeSQL upgradeSQL = GlobalSetting.Instance.GetUpgradeSQL();
            
            int newerVersion = upgradeSQL.SQLS.Count;
            Trace.TraceInformation("newer database version {0}", newerVersion);
            if (newerVersion > v)
            {
                Trace.TraceInformation("upgrade database version from {0} to {1}", v, newerVersion);
                using (var dbConn = getConnection())
                {
                    dbConn.Open();
                    using (var transaction = dbConn.BeginTransaction())
                    {
                        for (int i = v; i < newerVersion; i++)
                        {
                            List<string> sqls = upgradeSQL.SQLS[i];
                            foreach (string sql in sqls)
                            {
                                Trace.TraceInformation("run sql {0}", sql);
                                var cmd = new SqliteCommand(sql, dbConn, transaction);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        Trace.TraceInformation("write current database version to db");
                        var verSQL = string.Format("INSERT OR REPLACE INTO SETTING(KEY, VALUE) VALUES ('db.version', '{0}');", newerVersion);
                        var verCmd = new SqliteCommand(verSQL, dbConn, transaction);
                        verCmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
            }
            else
            {
                Trace.TraceInformation("database version is already the latest");
            }
        }

        public static void prepareTables()
        {
            var dbConn = getConnection();
            dbConn.Open();
            
            try
            {
                ExecuteNonQuery(dbConn, QUICK_COMMAND_TABLE);
                ExecuteNonQuery(dbConn, QUICK_COMMAND_EVN_CONFIG_TABLE);
                ExecuteNonQuery(dbConn, SETTING_TABLE);
            }
            finally
            {
                dbConn.Close();
            }
        }

        private static void ExecuteNonQuery(SqliteConnection conn, string sql)
        {
            SqliteCommand cmdCreateTable = new SqliteCommand(sql, conn);
            cmdCreateTable.ExecuteNonQuery();
        }
    }
}
