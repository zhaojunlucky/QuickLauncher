using Microsoft.Data.Sqlite;
using QuickLauncher.Config;
using QuickLauncher.Misc;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace QuickLauncher
{
    class DbUtil
    {

        private static readonly string QUICK_COMMAND_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND(UUID TEXT PRIMARY KEY,ALIAS TEXT UNIQUE,PATH TEXT, WORKDIRECTORY TEXT,COMMAND TEXT, CustomIcon BLOB);";
        private static readonly string QUICK_COMMAND_EVN_CONFIG_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND_ENV_CONFIG(Id INTEGER PRIMARY KEY, PARENT_ID TEXT NOT NULL, ENV_KEY TEXT, ENV_VALUE TEXT, FOREIGN KEY(PARENT_ID) REFERENCES QUICK_COMMAND(UUID))";
        private static readonly string SETTING_TABLE = "CREATE TABLE IF NOT EXISTS SETTING(KEY TEXT, VALUE TEXT, PRIMARY KEY(KEY))";

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(QlConfig.DbConnStr);
        }

        public static void CheckDb()
        {
            if (!Directory.Exists(QlConfig.AppConfigBaseDir))
            {
                Directory.CreateDirectory(QlConfig.AppConfigBaseDir);
                Trace.TraceInformation("create db folder");
            }

            if (!File.Exists(QlConfig.DbFilePath))
            {
                Trace.TraceInformation("initialize a new db");
                PrepareTables();
                Trace.TraceInformation("created tables");
            }
            DoUpgradeDb();
        }

        public static void DoUpgradeDb()
        {
            var dbVerItem = SettingItemUtils.GetDbVersion();
            int v = Convert.ToInt32(dbVerItem.Value);
            Trace.TraceInformation("current database version {0}", v);
            UpgradeSql upgradeSql = GlobalSetting.Instance.GetUpgradeSql();

            int newerVersion = upgradeSql.Sqls.Count;
            Trace.TraceInformation("newer database version {0}", newerVersion);
            if (newerVersion > v)
            {
                Trace.TraceInformation("upgrade database version from {0} to {1}", v, newerVersion);
                using var dbConn = GetConnection();
                dbConn.Open();
                using var transaction = dbConn.BeginTransaction();
                for (int i = v; i < newerVersion; i++)
                {
                    List<string> sqls = upgradeSql.Sqls[i];
                    foreach (string sql in sqls)
                    {
                        Trace.TraceInformation("run sql {0}", sql);
                        var cmd = new SqliteCommand(sql, dbConn, transaction);
                        cmd.ExecuteNonQuery();
                    }
                }
                Trace.TraceInformation("write current database version to db");
                var verSql = $"INSERT OR REPLACE INTO SETTING(KEY, VALUE) VALUES ('db.version', '{newerVersion}');";
                var verCmd = new SqliteCommand(verSql, dbConn, transaction);
                verCmd.ExecuteNonQuery();
                transaction.Commit();
            }
            else
            {
                Trace.TraceInformation("database version is already the latest");
            }
        }

        public static void PrepareTables()
        {
            var dbConn = GetConnection();
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
