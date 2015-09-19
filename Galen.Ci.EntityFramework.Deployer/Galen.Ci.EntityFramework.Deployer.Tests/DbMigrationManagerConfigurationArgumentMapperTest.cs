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
using System.Linq;
using Galen.Ci.EntityFramework.Configuration;
using Galen.Ci.EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Deployer.Tests
{
	[TestClass]
	public class DbMigrationManagerConfigurationArgumentMapperTest
	{
		[TestMethod]
		public void ArgumentsShouldBeCorrectlyMappedToConfiguration()
		{
			var connections = new[]
			{
				new DatabaseEndpoint() {DatabaseName = "Db1", ServerName = "Server1"}
			};

			var arguments = new Arguments()
			{
				Database=new DatabaseEndpoints()
				{
					Endpoints=connections
				},
				DeployedAssemblyOverridePath=@"c:\some dir\some other dir\MyAssembly1.dll",
				TargetAssemblyPath=@".\MyAssembly1.dll",
				RunServerMigrationsInTransaction=false,
				Mode=DeploymentMode.MigrationOnly,
                DeploymentHistoryExtractPath=@"c:\some dir\deployment history"
			};

			var migration = new MigrationConfigurationInfo
			{
				Type = "Galen.Enterprise.Data.MigrationConfigs.SomeContext.Configuration"
			};

			var initializer = new InitializerConfigurationInfo
			{
				Type = "Galen.Enterprise.Data.Initializers.SomeContextCreateDatabaseIfNotExists",
				ServiceAccount = new ServiceAccountInfo()
				{
					Name = "SomeWindowsAccountName",
					Domain = "SomeDomainName",
					DatabaseUser = "SomeDbUserName",
					AccountType = "Windows"

				}
			};

			var mockConfigStore = new Moq.Mock<IDeploymentConfigurationStore>();
			mockConfigStore.Setup(m => m.Load())
				.Returns(new DeploymentConfiguration()
				{
					MigrationConfigurationInfo=migration,
					InitializerConfigurationInfo=initializer
				});

			var expected = new DbDeploymentManagerConfiguration
			{
				Database=connections.First(),
				MigrationConfig=migration,
				InitializationConfig=initializer,
				DeployedAssemblyOverridePath=@"c:\some dir\some other dir\MyAssembly1.dll",
				TargetAssemblyPath=@".\MyAssembly1.dll",
				RunServerMigrationsInTransaction=false,
				Mode=DeploymentMode.MigrationOnly,
				AuthMode=AuthenticationMode.Integrated,
                DeploymentHistoryExtractPath = @"c:\some dir\deployment history"
            };

			var sut = new ConfigurationArgumentMapper(mockConfigStore.Object);
			var actual = sut.FromArguments(arguments);

			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.DeployedAssemblyOverridePath, actual.DeployedAssemblyOverridePath);
			Assert.AreEqual(expected.TargetAssemblyPath, actual.TargetAssemblyPath);
			Assert.AreEqual(expected.RunServerMigrationsInTransaction, actual.RunServerMigrationsInTransaction);
			Assert.AreEqual(expected.Mode, actual.Mode);
		    Assert.AreEqual(expected.DeploymentHistoryExtractPath, actual.DeploymentHistoryExtractPath);
			Assert.IsNull(actual.SqlLogin);
			Assert.IsNull(actual.SqlPassword);

			Assert.IsNotNull(actual.Database);
            Assert.AreEqual(expected.Database.ServerName, actual.Database.ServerName);
            Assert.AreEqual(expected.Database.DatabaseName, actual.Database.DatabaseName);

			Assert.IsNotNull(actual.MigrationConfig);
            Assert.AreEqual(migration.Type, actual.MigrationConfig.Type);

			Assert.IsNotNull(actual.InitializationConfig);
			Assert.IsNotNull(actual.InitializationConfig.ServiceAccount);
			Assert.AreEqual(initializer.Type, actual.InitializationConfig.Type);
			Assert.AreEqual(initializer.ServiceAccount.AccountType, actual.InitializationConfig.ServiceAccount.AccountType);
			Assert.AreEqual(initializer.ServiceAccount.Name, actual.InitializationConfig.ServiceAccount.Name);
			Assert.AreEqual(initializer.ServiceAccount.Domain, actual.InitializationConfig.ServiceAccount.Domain);
			Assert.AreEqual(initializer.ServiceAccount.DatabaseUser, actual.InitializationConfig.ServiceAccount.DatabaseUser);
			Assert.AreEqual(initializer.ServiceAccount.DatabaseUserPassword, actual.InitializationConfig.ServiceAccount.DatabaseUserPassword);
		}


		[TestMethod]
		public void TestConfigurationXmlShouldConfigureDeployerCorrectly()
		{
			var connections = new[]
			{
				new DatabaseEndpoint() {DatabaseName = "Db1", ServerName = "Server1"},
			};

			var arguments = new Arguments()
			{
				Database=new DatabaseEndpoints()
				{
					Endpoints=connections
				},
				DeployedAssemblyOverridePath=@"c:\some dir\some other dir\MyAssembly1.dll",
				TargetAssemblyPath=@".\MyAssembly1.dll",
				RunServerMigrationsInTransaction=false,
				Mode=DeploymentMode.MigrationOnly
			};

			var expectedConfig = new DeploymentConfiguration()
			{
				MigrationConfigurationInfo=new MigrationConfigurationInfo
				{
					Type = "Galen.Enterprise.Security.Core.Data.Migrations.SecurityDbContext.Configuration"
				},

				InitializerConfigurationInfo=new InitializerConfigurationInfo
				{
					Type = "Galen.Enterprise.Security.Core.Data.Initializers.SecurityDbContextCreateIfNotExistsWithTestData",
					DisableForcedSeeding = true,
					ServiceAccount = new ServiceAccountInfo()
					{
						Name = "EnterpriseServiceAccount",
						AccountType = "Sql",
						DatabaseUser = "ESA",
						DatabaseUserPassword = "SuperSecretPassword12345!",
					}
				}
			};

			var sut = new ConfigurationArgumentMapper(new DeploymentConfigurationXmlStore(ResourceHelper.ReadStream(
				"Galen.Ci.EntityFramework.Deployer.Tests.Data.TestConfiguration.xml")));
			var actualConfig = sut.FromArguments(arguments);

			Assert.IsNotNull(actualConfig);
			Assert.IsNotNull(actualConfig.InitializationConfig);

		    Assert.IsNotNull(actualConfig.MigrationConfig);
		    Assert.AreEqual(expectedConfig.MigrationConfigurationInfo.Type, actualConfig.MigrationConfig.Type);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.Type, actualConfig.InitializationConfig.Type);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.DisableForcedSeeding, actualConfig.InitializationConfig.DisableForcedSeeding);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.Name, actualConfig.InitializationConfig.ServiceAccount.Name);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.Domain, actualConfig.InitializationConfig.ServiceAccount.Domain);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUser, actualConfig.InitializationConfig.ServiceAccount.DatabaseUser);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.AccountType, actualConfig.InitializationConfig.ServiceAccount.AccountType);
			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUserPassword, actualConfig.InitializationConfig.ServiceAccount.DatabaseUserPassword);
		}
	}

}