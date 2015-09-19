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
using System.Linq;
using Galen.Ci.EntityFramework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Tests
{
	[TestClass]
	public class DbDeploymentManagerIntegrationTests
	{
		[TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyMigratesUpward()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = $"GalenTest_{Guid.NewGuid():N}";
			string deployedAssemblyPath = TestUtils.BuildTestAssemblyPath(2);
			var expectedMigrationHistory = new[]
			{
					"201404181533201_InitialCreate",
					"201404181719410_AddedAddresInfoToCustomer",
					"201404181726158_MoveAddressInformationIntoContactInfo",
					"201404181729406_AddedRowVersionToDomainObjectBase",
					"201404181740359_AddedMultiplePropertiesToCustomer",
			};

			var assemblyLoader = new AssemblyLoader();
			TestUtils.InitializeDatabase(assemblyLoader, deployedAssemblyPath, "Pinpoint.Test.Data.TestContext", serverName, databaseName);

			//Migration v2 to v3
			var config = new DbDeploymentManagerConfiguration()
			{
				TargetAssemblyPath=TestUtils.BuildTestAssemblyPath(3),
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				MigrationConfig=new MigrationConfigurationInfo
				{
				    Type = "Pinpoint.Test.Data.Migrations.Configuration"
				}
			};

			try
			{
				var sut = new DbDeploymentManager(config, assemblyLoader, new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

			    Assert.AreEqual(1,
			        TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, config.MigrationConfig.Type));

                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, config.MigrationConfig.Type);

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
				Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
			}
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyMigratesDownward()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = $"GalenTest_{Guid.NewGuid():N}";
			string deployedAssemblyPath = TestUtils.BuildTestAssemblyPath(3);
			var expectedMigrationHistory = new[]
			{
				"201404181533201_InitialCreate",
				"201404181719410_AddedAddresInfoToCustomer",
			};

			var assemblyLoader = new AssemblyLoader();
			TestUtils.InitializeDatabase(assemblyLoader, deployedAssemblyPath, "Pinpoint.Test.Data.TestContext", serverName, databaseName);

			//Migration v3 to v1
			var config = new DbDeploymentManagerConfiguration()
			{
				TargetAssemblyPath=TestUtils.BuildTestAssemblyPath(1),
				DeployedAssemblyOverridePath=deployedAssemblyPath,      // we didn't initialize using the deployer, so this has to be specified
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				MigrationConfig=new MigrationConfigurationInfo
					{
				    Type = "Pinpoint.Test.Data.Migrations.Configuration"
					}
			};

			try
			{
				var sut = new DbDeploymentManager(config, assemblyLoader, new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

			    Assert.AreEqual(1,
			        TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, config.MigrationConfig.Type));

                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, config.MigrationConfig.Type);

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
				Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
			}
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}

        [TestMethod]
        [TestCategory("Integration")]
        public void CorrectlyMigratesDownwardUsingDeploymentHistoryCreatedByInitialization()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            var assemblyLoader = new AssemblyLoader();

            // start by initialization to v3
            var initialDeploymentConfig = new DbDeploymentManagerConfiguration
            {
                Mode = DeploymentMode.InitializeOnly,
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(3),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                InitializationConfig = new InitializerConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Initializers.TestContextCreateDatabaseIfNotExists"
                }
            };

            var expectedInitialMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
                "201404181726158_MoveAddressInformationIntoContactInfo",
                "201404181729406_AddedRowVersionToDomainObjectBase",
                "201404181740359_AddedMultiplePropertiesToCustomer"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    initialDeploymentConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedInitialMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedInitialMigrationHistory.SequenceEqual(migrationHistory));
            }
            catch
            {
                // clean up if there was a problem
                TestUtils.DropDatabase(
                    initialDeploymentConfig.Database.ServerName,
                    initialDeploymentConfig.Database.DatabaseName);

                throw;
            }

            // migrate downward from v3 to v1 using Deployment History
            var downwardMigrationConfig = new DbDeploymentManagerConfiguration
            {
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(1),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Migrations.Configuration"
                }
            };

            var expectedDownwardMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
            };

            try
            {
                var sut = new DbDeploymentManager(
                    downwardMigrationConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    downwardMigrationConfig.MigrationConfig.Type);
                Assert.AreEqual(2, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    downwardMigrationConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedDownwardMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedDownwardMigrationHistory.SequenceEqual(migrationHistory));
            }
            finally
            {
                // be sure to clean up
                TestUtils.DropDatabase(downwardMigrationConfig.Database.ServerName, downwardMigrationConfig.Database.DatabaseName);
            }
        }

        [TestMethod]
		[TestCategory("Integration")]
        public void CorrectlyMigratesDownwardUsingDeploymentHistoryCreatedByMigration()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            var assemblyLoader = new AssemblyLoader();

            // start at v1
            var initialDeploymentAssemblyPath = TestUtils.BuildTestAssemblyPath(1);
            TestUtils.InitializeDatabase(assemblyLoader, initialDeploymentAssemblyPath, "Pinpoint.Test.Data.TestContext", serverName, databaseName);

            // migrate upward from v1 to v3
            var upwardMigrationConfig = new DbDeploymentManagerConfiguration
            {
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(3),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Migrations.Configuration"
                }
            };

            var expectedUpwardMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
                "201404181726158_MoveAddressInformationIntoContactInfo",
                "201404181729406_AddedRowVersionToDomainObjectBase",
                "201404181740359_AddedMultiplePropertiesToCustomer"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    upwardMigrationConfig, 
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName, 
                    databaseName,
                    upwardMigrationConfig.MigrationConfig.Type);
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName, 
                    databaseName,
                    upwardMigrationConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedUpwardMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedUpwardMigrationHistory.SequenceEqual(migrationHistory));
            }
            catch
            {
                // be sure to clean up
                TestUtils.DropDatabase(
                    upwardMigrationConfig.Database.ServerName,
                    upwardMigrationConfig.Database.DatabaseName);

                throw;
            }

            // migrate downward from v3 to v1 using Deployment History
            var downwardMigrationConfig = new DbDeploymentManagerConfiguration
            {
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(1),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Migrations.Configuration"
                }
            };

            var expectedDownwardMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
            };

            try
            {
                var sut = new DbDeploymentManager(
                    downwardMigrationConfig, 
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName, 
                    databaseName,
                    downwardMigrationConfig.MigrationConfig.Type);
                Assert.AreEqual(2, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName, 
                    databaseName,
                    downwardMigrationConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedDownwardMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedDownwardMigrationHistory.SequenceEqual(migrationHistory));
            }
            finally
            {
                //Be sure to clean up
                TestUtils.DropDatabase(downwardMigrationConfig.Database.ServerName, downwardMigrationConfig.Database.DatabaseName);
            }
        }

        [TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyMigratesDownwardPastInitialMigration()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = $"GalenTest_{Guid.NewGuid():N}";
			string deployedAssemblyPath = TestUtils.BuildTestAssemblyPath(3);

			var assemblyLoader = new AssemblyLoader();
			TestUtils.InitializeDatabase(assemblyLoader, deployedAssemblyPath, "Pinpoint.Test.Data.TestContext", serverName, databaseName);
			TestUtils.InitializeDatabase(assemblyLoader, deployedAssemblyPath, "Pinpoint.Test.Data.AnotherTestContext", serverName, databaseName);

			//Migration v3 to v1
			var config = new DbDeploymentManagerConfiguration()
			{
				TargetAssemblyPath=TestUtils.BuildTestAssemblyPath(1),
				DeployedAssemblyOverridePath=deployedAssemblyPath,      // we didn't initialize using the deployer so this is required
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				MigrationConfig=new MigrationConfigurationInfo
				{
				    Type = "Pinpoint.Test.Data.AnotherTestContextMigrations.Configuration"
				}
			};

			try
			{
				var sut = new DbDeploymentManager(config, assemblyLoader, new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

			    Assert.AreEqual(1,
			        TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, config.MigrationConfig.Type));

                //We expect all schema related to AnotherTestContext would be deleted
                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, config.MigrationConfig.Type);

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(0, migrationHistory.Count());
			}
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}

	    [TestMethod]
	    [TestCategory("Integration")]
	    public void NoOpMigrationWhenMigratingDownwardPastInitialMigrationUsingDeploymentHistoryWithoutAnOverride()
	    {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            var assemblyLoader = new AssemblyLoader();

            // start by initializing to v3
            var initialDeploymentConfig = new DbDeploymentManagerConfiguration
            {
                Mode = DeploymentMode.InitializeOnly,
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(3),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                InitializationConfig = new InitializerConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Initializers.AnotherTestContextCreateDatabaseIfNotExists"
                }
            };

            var expectedInitialMigrationHistory = new[]
            {
                "201404181743108_InitialCreate"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    initialDeploymentConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.AnotherTestContextMigrations.Configuration");
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.AnotherTestContextMigrations.Configuration");

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedInitialMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedInitialMigrationHistory.SequenceEqual(migrationHistory));
            }
            catch
            {
                // clean up if there was a problem
                TestUtils.DropDatabase(
                    initialDeploymentConfig.Database.ServerName,
                    initialDeploymentConfig.Database.DatabaseName);

                throw;
            }

            // attempt to migrate downward from v3 past initial create using Deployment History
            // without specifying a deployed assembly override
            var downwardMigrationConfig = new DbDeploymentManagerConfiguration
            {
                TargetAssemblyPath = TestUtils.BuildTestAssemblyPath(1),
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.AnotherTestContextMigrations.Configuration"
                }
            };

            try
            {
                var sut = new DbDeploymentManager(
                    downwardMigrationConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                // we expect no changes to migration history
                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName, 
                    databaseName, 
                    downwardMigrationConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedInitialMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedInitialMigrationHistory.SequenceEqual(migrationHistory));

                // no new deployment history should be created because no migration should have occurred
                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.AnotherTestContextMigrations.Configuration");
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);
            }
            finally
            {
                // be sure to clean up
                TestUtils.DropDatabase(downwardMigrationConfig.Database.ServerName, downwardMigrationConfig.Database.DatabaseName);
            }
        }

	    [TestMethod]
	    [TestCategory("Integration")]
	    public void NoOpInitializationWhenInitializingToCurrentDeployedVersion()
	    {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            string currentAssemblyPath = TestUtils.BuildTestAssemblyPath(3);

            var assemblyLoader = new AssemblyLoader();
            TestUtils.InitializeDatabase(assemblyLoader, currentAssemblyPath, "Pinpoint.Test.Data.TestContext", serverName, databaseName);

            var initializeConfig = new DbDeploymentManagerConfiguration
            {
                Mode = DeploymentMode.InitializeOnly,
                TargetAssemblyPath = currentAssemblyPath,      // use the current version as the target
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                InitializationConfig = new InitializerConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Initializers.TestContextCreateDatabaseIfNotExists"
                }
            };

            var expectedMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
                "201404181726158_MoveAddressInformationIntoContactInfo",
                "201404181729406_AddedRowVersionToDomainObjectBase",
                "201404181740359_AddedMultiplePropertiesToCustomer"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    initializeConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                // we expect no changes to migration history
                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));

                // no new deployment history should be created because no initialization nor migration should have occurred
                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");
                Assert.AreEqual(0, actualDeploymentHistoryRowCount);
            }
            finally
            {
                // be sure to clean up
                TestUtils.DropDatabase(initializeConfig.Database.ServerName, initializeConfig.Database.DatabaseName);
            }
        }

	    [TestMethod]
	    [TestCategory("Integration")]
	    public void NoOpMigrationWhenMigratingToCurrentDeployedVersion()
	    {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            var assemblyLoader = new AssemblyLoader();

	        var currentAssemblyPath = TestUtils.BuildTestAssemblyPath(3);

            // initialization to v3 - this forces the creation of deployment history
            var initialDeploymentConfig = new DbDeploymentManagerConfiguration
            {
                Mode = DeploymentMode.InitializeOnly,
                TargetAssemblyPath = currentAssemblyPath,
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                InitializationConfig = new InitializerConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Initializers.TestContextCreateDatabaseIfNotExists"
                }
            };

            var expectedMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
                "201404181726158_MoveAddressInformationIntoContactInfo",
                "201404181729406_AddedRowVersionToDomainObjectBase",
                "201404181740359_AddedMultiplePropertiesToCustomer"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    initialDeploymentConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    "Pinpoint.Test.Data.Migrations.Configuration");

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
            }
            catch
            {
                // clean up if there was a problem
                TestUtils.DropDatabase(
                    initialDeploymentConfig.Database.ServerName,
                    initialDeploymentConfig.Database.DatabaseName);

                throw;
            }

            // run a "migration" to the same version as what is deployed
            var reMigrationConfig = new DbDeploymentManagerConfiguration
            {
                TargetAssemblyPath = currentAssemblyPath,
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Migrations.Configuration"
                }
            };

            try
            {
                var sut = new DbDeploymentManager(
                    reMigrationConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                // nothing should be different in deployment history
                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    reMigrationConfig.MigrationConfig.Type);
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                // nothing should be different in migration history
                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    reMigrationConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
            }
            finally
            {
                // be sure to clean up
                TestUtils.DropDatabase(reMigrationConfig.Database.ServerName, reMigrationConfig.Database.DatabaseName);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void NoOpWhenInitializingOrMigratingToCurrentDeployedVersion()
        {
            const string serverName = @"(localdb)\mssqllocaldb";
            var databaseName = $"GalenTest_{Guid.NewGuid():N}";

            var assemblyLoader = new AssemblyLoader();

            var currentAssemblyPath = TestUtils.BuildTestAssemblyPath(3);

            // start at v3 - (this creates deployment history)
            var initializeOrMigrateConfig = new DbDeploymentManagerConfiguration
            {
                Mode = DeploymentMode.InitializeOrMigrate,
                TargetAssemblyPath = currentAssemblyPath,
                Database = new DatabaseEndpoint { ServerName = serverName, DatabaseName = databaseName },
                InitializationConfig = new InitializerConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Initializers.TestContextCreateDatabaseIfNotExists"
                },
                MigrationConfig = new MigrationConfigurationInfo
                {
                    Type = "Pinpoint.Test.Data.Migrations.Configuration"
                }
            };

            var expectedMigrationHistory = new[]
            {
                "201404181533201_InitialCreate",
                "201404181719410_AddedAddresInfoToCustomer",
                "201404181726158_MoveAddressInformationIntoContactInfo",
                "201404181729406_AddedRowVersionToDomainObjectBase",
                "201404181740359_AddedMultiplePropertiesToCustomer"
            };

            try
            {
                var sut = new DbDeploymentManager(
                    initializeOrMigrateConfig,
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    initializeOrMigrateConfig.MigrationConfig.Type);
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    initializeOrMigrateConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
            }
            catch
            {
                // clean up if there was a problem
                TestUtils.DropDatabase(
                    initializeOrMigrateConfig.Database.ServerName,
                    initializeOrMigrateConfig.Database.DatabaseName);

                throw;
            }

            try
            {
                // run another InitializeOrMigrate to the same version as what is deployed
                // (this is a close representation of how our release management works)
                var sut = new DbDeploymentManager(
                    initializeOrMigrateConfig,      // <-- reuse; same as before, nothing has changed
                    assemblyLoader,
                    new SqlClientDbConnectionInfoBuilder());
                sut.Deploy();

                // nothing should be different in deployment history
                var actualDeploymentHistoryRowCount = TestUtils.GetDeploymentHistoryRowCount(
                    serverName,
                    databaseName,
                    initializeOrMigrateConfig.MigrationConfig.Type);
                Assert.AreEqual(1, actualDeploymentHistoryRowCount);

                // nothing should be different in migration history
                var migrationHistory = TestUtils.GetMigrationHistory(
                    serverName,
                    databaseName,
                    initializeOrMigrateConfig.MigrationConfig.Type);

                Assert.IsNotNull(migrationHistory);
                Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
                Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));
            }
            finally
            {
                // be sure to clean up
                TestUtils.DropDatabase(initializeOrMigrateConfig.Database.ServerName, initializeOrMigrateConfig.Database.DatabaseName);
            }
        }

        [TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyInitializesADatabase()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = $"GalenTest_{Guid.NewGuid():N}";
			var expectedMigrationHistory = new[]
			{
					"201404181533201_InitialCreate",
					"201404181719410_AddedAddresInfoToCustomer",
					"201404181726158_MoveAddressInformationIntoContactInfo",
					"201404181729406_AddedRowVersionToDomainObjectBase",
					"201404181740359_AddedMultiplePropertiesToCustomer",
			};

			var config = new DbDeploymentManagerConfiguration()
			{
				Mode=DeploymentMode.InitializeOnly,
				TargetAssemblyPath=TestUtils.BuildTestAssemblyPath(3),
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				InitializationConfig=new InitializerConfigurationInfo
				{
					Type = "Pinpoint.Test.Data.Initializers.TestContextCreateDatabaseIfNotExists"
				}
			};

			try
			{
				var sut = new DbDeploymentManager(config, new AssemblyLoader(), new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, "Pinpoint.Test.Data.Migrations.Configuration");

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
				Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));

			    Assert.AreEqual(1,
			        TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, "Pinpoint.Test.Data.Migrations.Configuration"));
			}
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyInitializesADatabaseAndVerifySeeding()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = string.Format("TestContext_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty));
			var expectedMigrationHistory = new[]
			{
					"201506161504528_InitialCreate"
			};

			var config = new DbDeploymentManagerConfiguration()
			{
				Mode=DeploymentMode.InitializeOnly,
				TargetAssemblyPath=TestUtils.BuildTestContextTestAssemblyPath(1),
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				InitializationConfig=new InitializerConfigurationInfo
				{
					Type = "Galen.Ci.EntityFramework.Initialization.CreateSecureSeededDatabaseIfNotExists`2[[Galen.Ci.EntityFramework.Tests.TestContext.Data.TestDbContext, Galen.Ci.EntityFramework.Tests.TestContext], [Galen.Ci.EntityFramework.Tests.TestContext.Data.TestDataSeeder, Galen.Ci.EntityFramework.Tests.TestContext]], Galen.Ci.EntityFramework.Initialization"
				}
			};

			try
			{
				var sut = new DbDeploymentManager(config, new AssemblyLoader(), new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, "Galen.Ci.EntityFramework.Tests.TestContext.Data.Migrations.Configuration");

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
				Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));

				var startingRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				//Now that we are deployed, let's delete some data and see if the auto-seeding re-adds it
				TestUtils.ExecuteSqlCommand(serverName, databaseName, "DELETE FROM dbo.BasicEntities WHERE ID = 2");
				var postDeleteRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				Assert.IsTrue(postDeleteRows.Count()==(startingRows.Count()-1));

				sut=new DbDeploymentManager(config, new AssemblyLoader(), new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

				var postSeedingRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				Assert.AreEqual(startingRows.Count(), postSeedingRows.Count());

                Assert.AreEqual(1, TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, "Galen.Ci.EntityFramework.Tests.TestContext.Data.Migrations.Configuration"));
            }
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CorrectlyInitializesADatabaseAndDoesNotSeed()
		{
			string serverName = @"(localdb)\mssqllocaldb";
			string databaseName = string.Format("TestContext_{0}", Guid.NewGuid().ToString().Replace("-", string.Empty));
			var expectedMigrationHistory = new[]
			{
					"201506161504528_InitialCreate"
			};

			var config = new DbDeploymentManagerConfiguration()
			{
				Mode=DeploymentMode.InitializeOnly,
				TargetAssemblyPath=TestUtils.BuildTestContextTestAssemblyPath(1),
				Database=new DatabaseEndpoint { ServerName=serverName, DatabaseName=databaseName },
				InitializationConfig=new InitializerConfigurationInfo
				{
					Type = "Galen.Ci.EntityFramework.Initialization.CreateSecureSeededDatabaseIfNotExists`2[[Galen.Ci.EntityFramework.Tests.TestContext.Data.TestDbContext, Galen.Ci.EntityFramework.Tests.TestContext], [Galen.Ci.EntityFramework.Tests.TestContext.Data.TestDataSeeder, Galen.Ci.EntityFramework.Tests.TestContext]], Galen.Ci.EntityFramework.Initialization",
					DisableForcedSeeding = true //Disable seeding
				}
			};

			try
			{
				var sut = new DbDeploymentManager(config, new AssemblyLoader(), new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

                var migrationHistory = TestUtils.GetMigrationHistory(serverName, databaseName, "Galen.Ci.EntityFramework.Tests.TestContext.Data.Migrations.Configuration");

				Assert.IsNotNull(migrationHistory);
				Assert.AreEqual(expectedMigrationHistory.Length, migrationHistory.Count());
				Assert.IsTrue(expectedMigrationHistory.SequenceEqual(migrationHistory));

				var startingRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				//Now that we are deployed, let's delete some data and see if the auto-seeding re-adds it
				TestUtils.ExecuteSqlCommand(serverName, databaseName, "DELETE FROM dbo.BasicEntities WHERE ID = 2");
				var postDeleteRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				Assert.IsTrue(postDeleteRows.Count()==(startingRows.Count()-1));

				sut=new DbDeploymentManager(config, new AssemblyLoader(), new SqlClientDbConnectionInfoBuilder());
				sut.Deploy();

				var postSeedingRows = TestUtils.GetRows(serverName, databaseName, "dbo.BasicEntities");

				Assert.IsTrue(postSeedingRows.Count()==(startingRows.Count()-1));

                Assert.AreEqual(1, TestUtils.GetDeploymentHistoryRowCount(serverName, databaseName, "Galen.Ci.EntityFramework.Tests.TestContext.Data.Migrations.Configuration"));
			}
			finally
			{
				//Be sure to clean up
				TestUtils.DropDatabase(config.Database.ServerName, config.Database.DatabaseName);
			}
		}
	}
}
