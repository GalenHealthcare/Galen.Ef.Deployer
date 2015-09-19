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
using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Testing
{
    [TestClass]
    public abstract class MigrationsTestAgainstEmptyDatabase : MigrationsTest
    {
        private const string DefaultApplicationName = "Galen.Ci.EntityFramework.Testing.MigrationsTestAgainstEmptyDatabase";

        protected MigrationsTestAgainstEmptyDatabase(string serverName, string applicationName = DefaultApplicationName)
            : base(serverName, $"GalenTest_{Guid.NewGuid():N}", applicationName)
        {
        }

        protected MigrationsTestAgainstEmptyDatabase(
            string serverName, 
            string userId, 
            string password, 
            string applicationName = DefaultApplicationName) : 
            base(serverName, $"GalenTest_{Guid.NewGuid():N}", userId, password, applicationName)
        {
        }

        private static void ExecuteNonQuery(string connectionString, string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        [TestInitialize]
        public void CreateTestDatabase()
        {
            var sql = $"CREATE DATABASE {DatabaseName}";
            var connectionString = GetConnectionString("master");

            ExecuteNonQuery(connectionString, sql);
        }

        [TestCleanup]
        public void DropTestDatabase()
        {
            var sql = $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DatabaseName}];";
            var connectionString = GetConnectionString("master");

            ExecuteNonQuery(connectionString, sql);
        }
    }
}
