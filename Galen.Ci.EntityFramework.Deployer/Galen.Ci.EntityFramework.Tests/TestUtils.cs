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
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace Galen.Ci.EntityFramework.Tests
{
	[TestClass]
	internal class TestUtils
	{
		private static string m_ExecutionPath;

		[AssemblyInitialize]
		public static void AssemblyInit(TestContext context)
		{
			m_ExecutionPath=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			//Required to mitgate strange EF bug: http://entityframework.codeplex.com/workitem/1590
			var _ = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
			Log.Logger=new LoggerConfiguration().WriteTo.Trace().WriteTo.Seq("http://localhost:5341").CreateLogger();
		}

		public static string BuildTestAssemblyPath(int version)
		{
			return Path.Combine(m_ExecutionPath, string.Format(@"Data\Assemblies\v{0}\Pinpoint.Test.Data.dll", version));
		}

		public static string BuildTestContextTestAssemblyPath(int version)
		{
			return Path.Combine(m_ExecutionPath, string.Format(@"Data\Assemblies\TestContext\v{0}\Galen.Ci.EntityFramework.Tests.TestContext.dll", version));
		}

		public static void InitializeDatabase(
            AssemblyLoader assemblyLoader, 
            string contextAssemblyPath, 
            string contextName, 
            string serverName,
			string databaseName)
		{
			assemblyLoader.Load(MigrationsSource.Deployed, contextAssemblyPath);

			using (
				var context =
					Assembly.LoadFile(contextAssemblyPath).CreateInstance(contextName) as DbContext)
			{
				context.Database.Connection.ConnectionString=
					string.Format(
						"Server={0};Initial Catalog={1};Integrated Security=true;Application Name=Galen.Ci.EntityFramework.Tests;",
						serverName, databaseName);
				context.Database.Initialize(false);
			}
		}

		public static void CreateDatabase(string serverName, string databaseName)
		{
			using (var conn = new SqlConnection(
				string.Format("Server={0};Integrated Security=true;Application Name=Galen.Ci.EntityFramework.Tests;", serverName)))
			{
				var createdDb = conn.CreateCommand();
				createdDb.CommandText=string.Format("CREATE DATABASE {0}", databaseName);
				conn.Open();
				createdDb.ExecuteNonQuery();
			}
		}

		public static void ExecuteSqlCommand(string serverName, string databaseName, string sql)
		{
			using (var conn = new SqlConnection(
				string.Format("Data Source={0};Initial Catalog={1};Application Name=Galen.Ci.EntityFramework.Tests;", serverName, databaseName)))
			{
				var createdDb = conn.CreateCommand();
				createdDb.CommandText=sql;
				conn.Open();
				createdDb.ExecuteNonQuery();
			}
		}

		public static IEnumerable<object[]> GetRows(string serverName, string databaseName, string tableName)
		{
			var rows = new List<object[]>();

			using (var conn = new SqlConnection(
				string.Format("Data Source={0};Initial Catalog={1};Application Name=Galen.Ci.EntityFramework.Tests;", serverName, databaseName)))
			{
				var createdDb = conn.CreateCommand();
				createdDb.CommandText=string.Format("SELECT * FROM {0}", tableName);
				conn.Open();
				using (var reader = createdDb.ExecuteReader())
				{
					while (reader.Read())
					{
						var rowValues = new object[reader.FieldCount];
						reader.GetValues(rowValues);
						rows.Add(rowValues);
					}
				}
			}

			return rows.ToArray();
		}

		public static void DropDatabase(string serverName, string databaseName)
		{
			using (var conn = new SqlConnection(
				string.Format("Data Source={0};Application Name=Galen.Ci.EntityFramework.Tests;", serverName)))
			{
				var createdDb = conn.CreateCommand();
				createdDb.CommandText=string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];", databaseName);
				conn.Open();
				createdDb.ExecuteNonQuery();
			}
		}

		public static IEnumerable<string> GetMigrationHistory(string serverName, string databaseName, string contextKey)
		{
			var migrationIds = new List<string>();

			using (var conn = new SqlConnection(
				string.Format("Server={0};Initial Catalog={1};Integrated Security=true;Application Name=Galen.Ci.EntityFramework.Tests;", serverName, databaseName)))
			{
				var createdDb = conn.CreateCommand();
				createdDb.CommandText=
					string.Format("SELECT [MigrationId] FROM [dbo].[__MigrationHistory] WHERE [ContextKey] = '{0}'",
						contextKey);

				conn.Open();
				using (var reader = createdDb.ExecuteReader())
				{
					while (reader.Read())
					{
						migrationIds.Add(reader.GetString(0));
					}
				}
			}

			return migrationIds.ToArray();
		}

        public static int GetDeploymentHistoryRowCount(string serverName, string databaseName, string contextKey)
        {
            var connectionString =
                $"Server={serverName};Initial Catalog={databaseName};Integrated Security=true;Application Name=Galen.Ci.EntityFramework.Tests;";

            using (var conn = new SqlConnection(connectionString))
            {
                var createdDb = conn.CreateCommand();
                createdDb.CommandText =
                    $"SELECT COUNT(*) FROM [dbo].[__DeploymentHistory] WHERE [ContextKey] = '{contextKey}'";

                conn.Open();
                using (var reader = createdDb.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }

            return 0;
        }
	}
}
