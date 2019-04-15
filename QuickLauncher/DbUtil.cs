using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickLauncher
{
    class DbUtil
    {
        private static string DbBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Unicorn\\QuickLancher";
        private static string QUICK_COMMAND_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND(UUID TEXT PRIMARY KEY,ALIAS TEXT UNIQUE,PATH TEXT, WORKDIRECTORY TEXT,COMMAND TEXT);";
        private static string QUICK_COMMAND_EVN_CONFIG_TABLE = "CREATE TABLE IF NOT EXISTS QUICK_COMMAND_ENV_CONFIG(Id INTEGER PRIMARY KEY, PARENT_ID TEXT NOT NULL, ENV_KEY TEXT, ENV_VALUE TEXT, FOREIGN KEY(PARENT_ID) REFERENCES QUICK_COMMAND(UUID))";
        private static string SETTING_TABLE = "CREATE TABLE IF NOT EXISTS SETTING(KEY TEXT, VALUE TEXT, PRIMARY KEY(KEY))";
#if DEBUG
        private static string DbPath = "Data Source =" + DbBaseDir + "\\lancherDb-debug.db";
#else
        private static string DbPath = "Data Source =" + DbBaseDir + "\\lancherDb.db";
#endif
        public static SQLiteConnection getConnection()
        {
            if (!Directory.Exists(DbBaseDir))
            {
                Directory.CreateDirectory(DbBaseDir);
            }
            return new SQLiteConnection(DbPath);
        }

        public static void prepareTables()
        {
            var dbConn = getConnection();
            dbConn.Open();

            prepareTable(dbConn, QUICK_COMMAND_TABLE);
            prepareTable(dbConn, QUICK_COMMAND_EVN_CONFIG_TABLE);
            prepareTable(dbConn, SETTING_TABLE);
            dbConn.Close();
        }

        private static void prepareTable(SQLiteConnection conn, string sql)
        {
            SQLiteCommand cmdCreateTable = new SQLiteCommand(sql, conn);
            cmdCreateTable.ExecuteNonQuery();
        }
    }
}
