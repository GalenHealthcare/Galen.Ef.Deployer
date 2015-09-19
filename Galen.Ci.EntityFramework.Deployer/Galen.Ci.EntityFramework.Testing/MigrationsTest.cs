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
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Testing
{
    [TestClass]
    public abstract class MigrationsTest
    {
        private const string DbConnectionProvider = "System.Data.SqlClient";
        private const string DefaultApplicationName = "Galen.Ci.EntityFramework.Testing.MigrationTests";

        private readonly string m_ServerName;
        private readonly string m_ApplicationName;
        private readonly string m_UserId;
        private readonly string m_Password;
        private readonly bool m_IsUseIntegratedSecurity;

        private readonly Lazy<DbConnectionInfo> m_DbConnectionInfo;

        private AppDomain m_AppDomain;
        private MigrationTestRunnerProxy m_MigrationTestRunnerProxy;

        protected MigrationsTest(
            string serverName, 
            string databaseName, 
            string applicationName = DefaultApplicationName)
        {
            m_ServerName = serverName;
            DatabaseName = databaseName;
            m_ApplicationName = applicationName;
            m_IsUseIntegratedSecurity = true;

            m_DbConnectionInfo = new Lazy<DbConnectionInfo>(LazyLoadDbConnectionInfo);
        }

        protected MigrationsTest(
            string serverName,
            string databaseName,
            string userId,
            string password,
            string applicationName = DefaultApplicationName)
        {
            m_ServerName = serverName;
            DatabaseName = databaseName;
            m_UserId = userId;
            m_Password = password;
            m_ApplicationName = applicationName;
            m_IsUseIntegratedSecurity = false;

            m_DbConnectionInfo = new Lazy<DbConnectionInfo>(LazyLoadDbConnectionInfo);
        }

        protected string DatabaseName { get; }

        protected DbConnectionInfo DbConnectionInfo => (m_DbConnectionInfo.Value);
        private DbConnectionInfo LazyLoadDbConnectionInfo()
        {
            var connectionString = GetConnectionString(DatabaseName);
            return new DbConnectionInfo(connectionString, DbConnectionProvider);
        }

        protected IMigrationTestRunner Runner => (m_MigrationTestRunnerProxy);

        protected void InitializeRunner(string assemblyPath)
        {
            if (m_MigrationTestRunnerProxy != null)
            {
                throw new InvalidOperationException("Runner can only be initialized once per test method.");
            }

            var domainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            m_AppDomain = AppDomain.CreateDomain(
                $"MigrationTests_{Guid.NewGuid():N}", 
                AppDomain.CurrentDomain.Evidence,
                domainSetup);

            var proxyType = typeof(MigrationTestRunnerProxy);
            m_MigrationTestRunnerProxy = (MigrationTestRunnerProxy) m_AppDomain.CreateInstanceAndUnwrap(
                proxyType.Assembly.FullName,
                proxyType.FullName,
                false,
                0,
                null,
                new object[] {assemblyPath},
                null,
                null);
        }

        protected string GetConnectionString(string databaseName)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = m_ServerName,
                InitialCatalog = databaseName,
                IntegratedSecurity = m_IsUseIntegratedSecurity,
                ApplicationName = m_ApplicationName
            };

            if (!m_IsUseIntegratedSecurity)
            {
                connectionStringBuilder.UserID = m_UserId;
                connectionStringBuilder.Password = m_Password;
            }

            return connectionStringBuilder.ConnectionString;
        }

        [TestCleanup]
        public void UnloadMigrationAssembly()
        {
            AppDomain.Unload(m_AppDomain);
        }
    }
}
