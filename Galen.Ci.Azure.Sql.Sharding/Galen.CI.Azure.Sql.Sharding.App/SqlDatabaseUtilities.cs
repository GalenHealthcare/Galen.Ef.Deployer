#region License
// /*
//         The MIT License
// 
//         Copyright (c) 2015 Galen Healthcare Solutions
// 
//         Permission is hereby granted, free of charge, to any person obtaining a copy
//         of this software and associated documentation files (the "Software"), to deal
//         in the Software without restriction, including without limitation the rights
//         to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//         copies of the Software, and to permit persons to whom the Software is
//         furnished to do so, subject to the following conditions:
// 
//         The above copyright notice and this permission notice shall be included in
//         all copies or substantial portions of the Software.
// 
//         THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//         IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//         FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//         AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//         LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//         OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//         THE SOFTWARE.
//  */
#endregion
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Galen.CI.Azure.Sql.Sharding.App
{
    public static class SqlDatabaseUtilities
    {
        private static void ExecuteNonQuery(string sql, string connectionString, int? commandTimeout = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;

                if (commandTimeout.HasValue)
                {
                    command.CommandTimeout = commandTimeout.Value;
                }

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private static bool GetIsSqlAzure(string connectionString)
        {
            string result;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @@VERSION";
                connection.Open();
                result = (string)command.ExecuteScalar();
                connection.Close();
            }

            return result.Contains("SQL Azure");
        }

        private static string GetMasterDatabaseConnectionString(string serverConnectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(serverConnectionString);
            connectionStringBuilder.InitialCatalog = "master";
            return connectionStringBuilder.ConnectionString;
        }

        public static void CreateDatabaseIfNotExists(string databaseName, string serverConnectionString)
        {
            var sql =
                $"IF NOT EXISTS(SELECT NULL FROM sys.databases WHERE name = '{databaseName}') CREATE DATABASE [{databaseName}]";

            var masterDbConnectionString = GetMasterDatabaseConnectionString(serverConnectionString);

            var commandTimeout = GetIsSqlAzure(masterDbConnectionString)
                ? (int?)180     // give Azure 3 minutes to complete the database completion
                : null;

            
            ExecuteNonQuery(sql, masterDbConnectionString, commandTimeout);
        }

        public static void CreateSqlLoginIfNotExists(
            string loginName, 
            string loginPassword,
            string serverConnectionString)
        {
            var masterDbConnectionString = GetMasterDatabaseConnectionString(serverConnectionString);

            var isSqlAzure = GetIsSqlAzure(masterDbConnectionString);
            var loginTable = isSqlAzure
                ? "sys.sql_logins"
                : "dbo.syslogins";

            var loginNameColumn = isSqlAzure
                ? "name"
                : "loginname";

            var sql =
                $"IF NOT EXISTS (SELECT NULL FROM {loginTable} WHERE {loginNameColumn} = N'{loginName}') " +
                $"BEGIN CREATE LOGIN [{loginName}] WITH PASSWORD=N'{loginPassword}' END;";

            ExecuteNonQuery(sql, masterDbConnectionString);
        }

        public static void CreateWindowsLoginIfNotExists(string loginName, string serverConnectionString)
        {
            var sql =
                $"IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'{loginName}') " +
                $"BEGIN CREATE LOGIN [{loginName}] FROM WINDOWS END;";

            ExecuteNonQuery(sql, serverConnectionString);
        }

        public static void CreateDatabaseUserIfNotExists(
            string loginName, 
            string databaseUserName, 
            string connectionString)
        {
            var sql =
                $"IF NOT EXISTS (SELECT * FROM dbo.sysusers WHERE name = N'{databaseUserName}') " +
                $"BEGIN CREATE USER [{databaseUserName}] FOR LOGIN  [{loginName}] WITH DEFAULT_SCHEMA=[dbo] END;";

            ExecuteNonQuery(sql, connectionString);
        }

        public static void GrantUserDatabaseRequiredPermissions(string databaseUserName, string connectionString)
        {
            var grantConnectSql = $"GRANT CONNECT TO [{databaseUserName}];";
            var readerRoleSql = $"EXEC sp_addrolemember 'db_datareader', '{databaseUserName}';";
            var writerRoleSql = $"EXEC sp_addrolemember 'db_datawriter', '{databaseUserName}';";
            var executePermissionSql = $"GRANT EXECUTE TO [{databaseUserName}];";

            using (var connection = new SqlConnection(connectionString))
            {
                var commands = new List<SqlCommand>(3)
                {
                    connection.CreateCommand(),
                    connection.CreateCommand(),
                    connection.CreateCommand(),
                    connection.CreateCommand()
                };

                commands[0].CommandText = grantConnectSql;
                commands[1].CommandText = readerRoleSql;
                commands[2].CommandText = writerRoleSql;
                commands[3].CommandText = executePermissionSql;

                connection.Open();
                commands.ForEach(c => c.ExecuteNonQuery());
                connection.Close();
            }
        }
    }
}
