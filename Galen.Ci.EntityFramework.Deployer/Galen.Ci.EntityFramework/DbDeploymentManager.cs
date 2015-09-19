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
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Transactions;
using Galen.Ci.EntityFramework.Configuration;
using Galen.Ci.EntityFramework.Initialization;
using Galen.Ci.EntityFramework.Utilities;
using Serilog;

namespace Galen.Ci.EntityFramework
{
    public class DbDeploymentManager
    {
        private readonly DbDeploymentManagerConfiguration m_Config;
        private readonly IAssemblyLoader m_AssemblyLoader;
        private readonly IDbConnectionInfoBuilder m_ConnectionInfoBuilder;
        private readonly Lazy<Assembly> m_TargetAssembly;
        private readonly Lazy<Assembly> m_DeployedAssembly;
        private readonly Lazy<ContextTypeInitializerInfo> m_InitializerInfo;
        private bool m_HasValidated = false;

        private static Version GetVersion()
        {
            var thisAssembly = typeof (DbDeploymentManager).Assembly;
            return thisAssembly.GetName().Version;
        }

        public DbDeploymentManager(DbDeploymentManagerConfiguration config, IAssemblyLoader assemblyLoader, IDbConnectionInfoBuilder dbConnectionInfoBuilder)
        {
            if (config.Database == null)
            {
                throw new InvalidOperationException("No database endpoint provided");
            }

            m_Config = config;
            m_AssemblyLoader = assemblyLoader;
            m_ConnectionInfoBuilder = dbConnectionInfoBuilder;
            m_TargetAssembly = new Lazy<Assembly>(LazyLoadTargetAssembly);
            m_DeployedAssembly = new Lazy<Assembly>(LazyLoadDeployedAssembly);
            m_InitializerInfo = new Lazy<ContextTypeInitializerInfo>(LazyLoadInitializerInfo);

            if (string.IsNullOrWhiteSpace(m_Config.DeploymentHistoryExtractPath))
            {
                m_Config.DeploymentHistoryExtractPath = Path.GetTempPath();
                Log.Debug(
                    "Deployment History extract path not specified.  Defaulting to temp directory {tempDirectory}.",
                    m_Config.DeploymentHistoryExtractPath);
            }
        }

        private Assembly TargetAssembly => (m_TargetAssembly.Value);
        private Assembly LazyLoadTargetAssembly()
        {
            return m_AssemblyLoader.Load(MigrationsSource.Target, m_Config.TargetAssemblyPath);
        }

        private Assembly DeployedAssembly => (m_DeployedAssembly.Value);
        private Assembly LazyLoadDeployedAssembly()
        {
            var isOverrideDeploymentHistory = !string.IsNullOrEmpty(m_Config.DeployedAssemblyOverridePath);
            if (isOverrideDeploymentHistory)
            {
                Log.Information(
                    "Override deployed assembly path {overridePath} specified.  Deployment History will not be used!", 
                    m_Config.DeployedAssemblyOverridePath);
                return m_AssemblyLoader.Load(MigrationsSource.Deployed, m_Config.DeployedAssemblyOverridePath);
            }

            if (m_Config.Mode == DeploymentMode.InitializeOnly || m_Config.Mode == DeploymentMode.SeedOnly)
            {
                return null;
            }

            string deploymentHistoryAssemblyPath = null;
            var targetContextKeySchema = GetContextKeySchema(TargetAssembly, m_Config.MigrationConfig.Type);
            if (targetContextKeySchema == null)
            {
                Log.Warning(
                    "Failed to determine context key and schema name for config type {configType} in target assembly {assemblyPath}.  " +
                    "The cause is most likely an attempt to migrate downward to a version in which {configType} does not exist.  " +
                    "A deployed assembly override must be specified in these cases.  Downward migrations will either not occur or will fail.",
                    m_Config.MigrationConfig.Type,
                    TargetAssembly.CodeBase);
                return null;
            }

            if (!GetIsTargetDatabaseExists())
            {
                Log.Debug(
                    "Database {endpointDatabase} does not exist on {endPointServer}.  Deployment History will not be used.",
                    m_Config.Database.DatabaseName, 
                    m_Config.Database.ServerName);
                return null;
            }

            var factory = DbProviderFactories.GetFactory(m_ConnectionInfoBuilder.ProviderName);
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = m_ConnectionInfoBuilder.BuildConnectionString(
                    m_Config.Database,
                    m_Config.AuthMode,
                    m_Config.SqlLogin,
                    m_Config.SqlPassword);

                connection.Open();

                Log.Debug(
                    "Extracting current deployment history assemblies " +
                    "for {contextKey} in {schemaName} schema on {endPointServer}\\{endpointDatabase} to {extractPath}.",
                    targetContextKeySchema.ContextKey,
                    targetContextKeySchema.SchemaName,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName,
                    m_Config.DeploymentHistoryExtractPath);

                deploymentHistoryAssemblyPath = DeploymentHistory.ExtractCurrent(
                    targetContextKeySchema.ContextKey, 
                    connection, 
                    targetContextKeySchema.SchemaName,
                    m_Config.DeploymentHistoryExtractPath);
                connection.Close();
            }

            if (string.IsNullOrEmpty(deploymentHistoryAssemblyPath))
            {
                Log.Warning("No {targetSchemaName} Deployment History available for {targetContextKey} on {endPointServer}\\{endpointDatabase}.",
                    targetContextKeySchema.SchemaName,
                    targetContextKeySchema.ContextKey,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName);
                return null;
            }

            return m_AssemblyLoader.Load(MigrationsSource.Deployed, deploymentHistoryAssemblyPath);
        }

        private ContextTypeInitializerInfo InitializerInfo => (m_InitializerInfo.Value);
        private ContextTypeInitializerInfo LazyLoadInitializerInfo()
        {
            var hasInitializer = (m_Config.InitializationConfig != null &&
                                  !string.IsNullOrEmpty(m_Config.InitializationConfig.Type));
            if (!hasInitializer)
            {
                Log.Debug("No initializer specified");
                return null;
            }

            Log.Debug("Loading initializer");

            var initializer = ConstructInitializer();
            if (initializer == null)
            {
                throw new InvalidOperationException($"Invalid initializer type specified: {m_Config.InitializationConfig.Type}");
            }

            var securedInitializer = initializer as ISecureDbWithServiceAccount;
            var isSecuredInitializer = (securedInitializer != null);
            var hasServiceAccount = (m_Config.InitializationConfig.ServiceAccount != null);

            if (!hasServiceAccount && isSecuredInitializer)
            {
                Log.Warning(
                    "The context initializer {initializer} implements ISecureDbWithServiceAccount, but " +
                    "no service account info was configured.", initializer);
            }
            else if (isSecuredInitializer)
            {
                var serviceAccount = m_Config.InitializationConfig.ServiceAccount;
                LogInitializerServiceAccount(initializer, serviceAccount);
                if (m_Config.InitializationConfig.ServiceAccount.AccountType == null)
                {
                    Log.Warning(
                        "The service account type was null for {initializer}. " +
                        "This will result in other service account information being ignored.",
                        initializer);
                }

                securedInitializer.ServiceAccountName = serviceAccount.Name;
                securedInitializer.ServiceAccountDomain = serviceAccount.Domain;
                securedInitializer.ServiceAccountDatabaseUserName = serviceAccount.DatabaseUser;
                securedInitializer.ServiceAccountDatabaseUserPassword = serviceAccount.DatabaseUserPassword;
                securedInitializer.ServiceAccountType = string.IsNullOrEmpty(serviceAccount.AccountType)
                    ? null
                    : (ServiceAccountType?)Enum.Parse(typeof(ServiceAccountType), serviceAccount.AccountType);
            }
            else
            {
                Log.Information("No service account information found.");
            }

            var initializerContextType = initializer.GetType().BaseType
                .GetGenericArguments()
                .Single(t => t.IsSubclassOf(typeof(DbContext)));

            Log.Debug("Initializer found {initializer} for context {contextType}", initializer, initializerContextType);

            return new ContextTypeInitializerInfo(initializerContextType, initializer);
        }

        private static void LogInitializerServiceAccount(object initializer, ServiceAccountInfo serviceAccount)
        {
            Log.Information(
                "Service account information found for {initializer}. " +
                "Name={Name}, " +
                "Type={AccountType}, " +
                "Domain={Domain}, " +
                "DatabaseUser={DatabaseUser}",
                initializer,
                serviceAccount.Name,
                serviceAccount.AccountType,
                serviceAccount.Domain,
                serviceAccount.DatabaseUser);
        }

        public void Deploy()
        {
            ValidateConfiguredAssemblies();

            var isPerformInitialization = m_Config.Mode == DeploymentMode.InitializeOnly ||
                                          m_Config.Mode == DeploymentMode.InitializeOrMigrate;

            var isPerformMigration = m_Config.Mode != DeploymentMode.InitializeOnly &&
                                     m_Config.Mode != DeploymentMode.SeedOnly;

            TransactionScope multiMigrationTxScope = null;
            try
            {
                MigrationExecutionInfo pendingMigration = null;
                if (isPerformMigration)
                {
                    pendingMigration = GetPendingMigration();
                    isPerformMigration = (pendingMigration != null);
                }

                var isInitializationPerformed = isPerformInitialization && HandleDatabaseInitialization();

                if (isPerformMigration && pendingMigration.Migration.ContextKeySchema != null)
                {
                    SetupDeploymentHistory(pendingMigration.Migration.ContextKeySchema.SchemaName);
                }
                else if (isPerformInitialization)
                {
                    var contextKeySchema = GetTargetAssemblyContextKeySchema();
                    SetupDeploymentHistory(contextKeySchema.SchemaName);
                }

                if (m_Config.RunServerMigrationsInTransaction)
                {
                    multiMigrationTxScope = new TransactionScope(TransactionScopeOption.Required);
                }

                if (isPerformMigration)
                {
                    var isMigrationPerformed = HandleDatabaseMigration(pendingMigration);
                    if (isMigrationPerformed)
                    {
                        HandleDeploymentHistory(
                            pendingMigration.Migration.ContextKeySchema.SchemaName,
                            pendingMigration.Migration.ContextKeySchema.ContextKey);
                    }
                }
                else if (isInitializationPerformed)
                {
                    var contextKeySchema = GetTargetAssemblyContextKeySchema();
                    HandleDeploymentHistory(contextKeySchema.SchemaName, contextKeySchema.ContextKey);
                }

                multiMigrationTxScope?.Complete();

                if (!isInitializationPerformed)
                {
                    HandleDataSeeding();
                }
            }
            finally
            {
                multiMigrationTxScope.SafeDispose();
            }
        }

        private void HandleDataSeeding()
        {
            if (InitializerInfo?.Initializer == null)
            {
                Log.Debug("Seeding skipped as no initializer available");
                return;
            }

            Log.Information(
                "Handling database seeding for {contextType} on {endPointServer}\\{endpointDatabase}",
                InitializerInfo.ContextType,
                m_Config.Database.ServerName,
                m_Config.Database.DatabaseName);

            var forcedSeedingDisabled = m_Config.InitializationConfig.DisableForcedSeeding;

            if (forcedSeedingDisabled && m_Config.Mode == DeploymentMode.SeedOnly)
            {
                forcedSeedingDisabled = false;
            }

            Log.Debug(
                "Initailizer ({initializer}) to be used for seeding found for {contextType} on {endPointServer}\\{endpointDatabase}. Disable forced seeding is set to {forcedSeedingDisabled}",
                InitializerInfo.Initializer,
                InitializerInfo.ContextType,
                m_Config.Database.ServerName,
                m_Config.Database.DatabaseName,
                forcedSeedingDisabled);

            if (forcedSeedingDisabled)
            {
                return;
            }

            var seederType = InitializerInfo.Initializer.GetType()
                .GetGenericArguments()
                .SingleOrDefault(at => at.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (ISeedData<>)));

            if (seederType != null)
            {
                Log.Information(
                    "Performing data seeding for {contextType} using seed {seederType} on {endPointServer}\\{endpointDatabase}",
                    InitializerInfo.ContextType,
                    seederType,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName);

                using (var targetContext = (DbContext) Activator.CreateInstance(InitializerInfo.ContextType))
                {
                    if (InitializerInfo.Initializer != null)
                    {
                        // shut off the initializer as we are only doing seeding now (regardless of mode)
                        Log.Debug("Setting Database Initializer to null before seeding.");
                        typeof(Database).GetMethod("SetInitializer")
                            .MakeGenericMethod(InitializerInfo.ContextType)
                            .Invoke(null, new object[] { null });
                    }

                    targetContext.Database.Connection.ConnectionString =
                        m_ConnectionInfoBuilder.BuildConnectionString(m_Config.Database, m_Config.AuthMode, m_Config.SqlLogin, m_Config.SqlPassword);

                    var dataSeeder = Activator.CreateInstance(seederType);
                    seederType.GetMethod("Seed").Invoke(dataSeeder, new object[] {targetContext});
                }
            }
            else
            {
                Log.Information(
                    "No seed data specified on {initializer} for {contextType} on {endPointServer}\\{endpointDatabase}",
                    InitializerInfo.Initializer,
                    InitializerInfo.ContextType,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName);
            }
        }

        private void HandleDeploymentHistory(string schemaName, string contextKey)
        {
            var factory = DbProviderFactories.GetFactory(m_ConnectionInfoBuilder.ProviderName);
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = m_ConnectionInfoBuilder.BuildConnectionString(
                    m_Config.Database,
                    m_Config.AuthMode,
                    m_Config.SqlLogin,
                    m_Config.SqlPassword);

                connection.Open();

                Log.Information(
                    "Creating {contextKey} deployment history in {schemaName} schema " +
                    "on {endPointServer}\\{endpointDatabase} using {targetAssemblyPath}.",
                    contextKey,
                    schemaName,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName,
                    m_Config.TargetAssemblyPath);

                DeploymentHistory.Create(
                    contextKey,
                    GetVersion().ToString(),
                    m_Config.TargetAssemblyPath,
                    connection,
                    schemaName);

                connection.Close();
            }
        }

        /// <returns>true when migration is performed; false otherwise.</returns>
        private bool HandleDatabaseMigration(MigrationExecutionInfo pendingMigration)
        {
            // create a new DbMigrator using a specific target database because
            // the GenericMigrator in pendingMigration can't be used directly as
            // it doesn't have the specific database information, and the connection 
            // can't be changed after the fact
            var targetedMigrator = GetDbMigrator(
                pendingMigration.Migration.Assembly,
                pendingMigration.Migration.ConfigurationType, 
                m_ConnectionInfoBuilder.Build(m_Config.Database, m_Config.AuthMode, m_Config.SqlLogin, m_Config.SqlPassword));

            if (targetedMigrator == null)
            {
                Log.Warning(
                    "Cannot perform migration on {endPointServer}\\{endpointDatabase} to {targetMigrationId} " +
                    "for configuration {configType} using migration source {migrationSource} assembly.",
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName,
                    string.IsNullOrEmpty(pendingMigration.TargetMigrationId)
                        ? "LATEST"
                        : pendingMigration.TargetMigrationId,
                    pendingMigration.Migration.ConfigurationType,
                    pendingMigration.Migration.Source);
                return false;
            }

            Log.Information(
                "Migrating {endPointServer}\\{endpointDatabase} to {targetMigrationId} for " +
                "configuration {configType} using a migration source of {migrationSource} assembly",
                m_Config.Database.ServerName,
                m_Config.Database.DatabaseName,
                string.IsNullOrEmpty(pendingMigration.TargetMigrationId)
                    ? "LATEST"
                    : pendingMigration.TargetMigrationId,
                pendingMigration.Migration.ConfigurationType,
                pendingMigration.Migration.Source);

            using (var migrationTransaction = new TransactionScope(TransactionScopeOption.Required))
            {
                if (string.IsNullOrEmpty(pendingMigration.TargetMigrationId))
                {
                    //Null target migration id means we're going to the latest version
                    targetedMigrator.Update();
                }
                else
                {
                    targetedMigrator.Update(pendingMigration.TargetMigrationId);
                }

                migrationTransaction.Complete();
            }

            return true;
        }

        /// <returns>true when initialization was performed and false otherwise.</returns>
        private bool HandleDatabaseInitialization()
        {
            //Must suppress transactions as we can't create a DB in the same
            //tx as when we create the schema
            using (var supressTranScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                if (AnyVersionOfContextHasBeenDeployed())
                {
                    return false;
                }

                Log.Information(
                    "Context has yet to be deployed. Running initializer for {contextType} on {endPointServer}\\{endpointDatabase}",
                    InitializerInfo.ContextType,
                    m_Config.Database.ServerName,
                    m_Config.Database.DatabaseName);

                using (var targetContext = (DbContext) Activator.CreateInstance(InitializerInfo.ContextType))
                {
                    if (InitializerInfo.Initializer != null)
                    {
                        //Have to use reflection here because initializers often
                        //don't have their TContext generic parmater marked as covariant (in)
                        typeof (Database).GetMethod("SetInitializer")
                            .MakeGenericMethod(InitializerInfo.ContextType)
                            .Invoke(null, new[] { InitializerInfo.Initializer });
                    }

                    targetContext.Database.Connection.ConnectionString =
                        m_ConnectionInfoBuilder.BuildConnectionString(m_Config.Database, m_Config.AuthMode, m_Config.SqlLogin, m_Config.SqlPassword);

                    targetContext.Database.Initialize(false);
                    return true;
                }
            }
        }

        private bool GetIsTargetDatabaseExists()
        {
            
            const string sql = "SELECT 1 FROM sys.databases WHERE name = @DatabaseName";

            var factory = DbProviderFactories.GetFactory(m_ConnectionInfoBuilder.ProviderName);
            using (var connection = factory.CreateConnection())
            {
                var masterDatabase = new DatabaseEndpoint
                {
                    DatabaseName = "master",
                    ServerName = m_Config.Database.ServerName
                };

                connection.ConnectionString = m_ConnectionInfoBuilder.BuildConnectionString(
                    masterDatabase,
                    m_Config.AuthMode,
                    m_Config.SqlLogin,
                    m_Config.SqlPassword);


                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    var databaseNameParam = command.CreateParameter();
                    databaseNameParam.DbType = DbType.String;
                    databaseNameParam.Direction = ParameterDirection.Input;
                    databaseNameParam.ParameterName = "@DatabaseName";
                    databaseNameParam.Value = m_Config.Database.DatabaseName;
                    command.Parameters.Add(databaseNameParam);

                    connection.Open();

                    var result = (int?)command.ExecuteScalar();
                    connection.Close();

                    return (result == 1);
                };
            }
        }

        private bool AnyVersionOfContextHasBeenDeployed()
        {
            using (var targetContext = (DbContext)Activator.CreateInstance(InitializerInfo.ContextType))
            {
                targetContext.Database.Connection.ConnectionString =
                    m_ConnectionInfoBuilder.BuildConnectionString(m_Config.Database, m_Config.AuthMode, m_Config.SqlLogin, m_Config.SqlPassword);

                if (!targetContext.Database.Exists()) return false;

                try
                {
                    //Throws an exception if no model meta data is found,
                    //which is the case if the context hasn't been deloyed
                    targetContext.Database.CompatibleWithModel(true);
                }
                catch (NotSupportedException)
                {
                    return false;
                }

                return true;
            }
        }

        private MigrationExecutionInfo GetPendingMigration()
        {
            ValidateConfiguredAssemblies();

            var hasDeployedAssembly = (this.DeployedAssembly != null);
            var deployedMigrationInfo = hasDeployedAssembly
                ? GetMigrationInfo(MigrationsSource.Deployed)
                : null;

            var targetMigrationInfo = GetMigrationInfo(MigrationsSource.Target);

            if (targetMigrationInfo.Source == MigrationsSource.Target
                && targetMigrationInfo.GenericMigrator == null
                && hasDeployedAssembly)
            {
                //The target assembly does not contain a matching migrator / migration config
                //This means we are rolling back past to a time before this context existed
                //So we should use the deployed assembly migration with a target of "0"

                var isDeployedAssemblyHasConfiguration = 
                    deployedMigrationInfo.GenericMigrator.Configuration.GetType().FullName ==
                    targetMigrationInfo.ConfigurationType;

                if (!isDeployedAssemblyHasConfiguration)
                {
                    throw new InvalidOperationException("Deployed migration missing required migration configuration");
                }

                return new MigrationExecutionInfo
                {
                    Migration = deployedMigrationInfo,
                    TargetMigrationId = "0" //Before initial create
                };
            }
            else
            {
                var isDeployedAssemblyHasConfiguration = (deployedMigrationInfo != null) &&
                    (deployedMigrationInfo.GenericMigrator.Configuration.GetType().FullName ==
                     targetMigrationInfo.GenericMigrator.Configuration.GetType().FullName)
                    &&
                    (deployedMigrationInfo.GenericMigrator.Configuration.ContextType.FullName ==
                     targetMigrationInfo.GenericMigrator.Configuration.ContextType.FullName);

                if (!isDeployedAssemblyHasConfiguration)
                {
                    // there is no matching deployed migration, so just use the target
                    return new MigrationExecutionInfo {Migration = targetMigrationInfo};
                }

                if (targetMigrationInfo.GenericMigrator != null)
                {
                    string targetMigrationId = targetMigrationInfo.GenericMigrator
                        .GetLocalMigrations()
                        .OrderBy(i => i)
                        .LastOrDefault();

                    if (string.IsNullOrEmpty(targetMigrationId))
                    {
                        return null;
                    }

                    string lastDeployedMigrationId = deployedMigrationInfo.GenericMigrator
                        .GetLocalMigrations()
                        .OrderBy(i => i)
                        .LastOrDefault();

                    Log.Debug(
                        "Last migration deployed is {lastDeployedMigrationId}.  Latest target migration is {targetMigrationId}.",
                        lastDeployedMigrationId,
                        targetMigrationId);

                    var migrationComparisonResult = targetMigrationId.CompareTo(lastDeployedMigrationId);
                    if (migrationComparisonResult == 0)
                    {
                        // target migration is equal to the latest deployed migration
                        // so there is no migration to execute
                        return null;
                    }

                    var isUpwardMigration = (migrationComparisonResult > 0);
                    return new MigrationExecutionInfo
                    {
                        TargetMigrationId = targetMigrationId,
                        Migration = isUpwardMigration
                            ? targetMigrationInfo
                            : deployedMigrationInfo
                    };
                }

                return null;
            }
        }

        private MigrationInfo GetMigrationInfo(MigrationsSource source)
        {
            var assembly = (source == MigrationsSource.Target)
                ? this.TargetAssembly 
                : this.DeployedAssembly;

            if (assembly == null)
            {
                throw new InvalidOperationException("Source assembly could not be loaded");
            }

            var migrationConfigurationType = m_Config.MigrationConfig.Type;
            return new MigrationInfo(
                source,
                assembly,
                migrationConfigurationType,
                GetContextKeySchema(assembly, migrationConfigurationType),
                GetDbMigrator(assembly, migrationConfigurationType));
        }

        private ContextKeySchemaInfo GetTargetAssemblyContextKeySchema()
        {
            var configurationTypeForContext =
                from dt in TargetAssembly.DefinedTypes
                let t = dt.AsType()
                where
                    t.IsSubclassOf(typeof(DbMigrationsConfiguration)) &&
                    !dt.IsAbstract &&
                    (dt.GenericTypeArguments.Contains(InitializerInfo.ContextType) ||
                     t.BaseType.GenericTypeArguments.Contains(InitializerInfo.ContextType))
                select dt;

            var configurationType = configurationTypeForContext.Single();
            return GetContextKeySchema(TargetAssembly, configurationType.FullName);
        }

        private static ContextKeySchemaInfo GetContextKeySchema(Assembly assembly, string configurationType)
        {
            var targetAssemblyConfig = (DbMigrationsConfiguration)assembly.CreateInstance(configurationType);
            if (targetAssemblyConfig == null)
            {
                Log.Debug(
                    "Unable to create DbMigrationsConfiguration for {configurationType} using assembly {assembly}.",
                    configurationType, 
                    assembly.CodeBase);
                return null;
            }

            // modified from EF6 System.Data.Entity.Migrations.Infrastructure.MigrationAssembly constructor
            // https://entityframework.codeplex.com/SourceControl/latest#src/EntityFramework/Migrations/Infrastructure/MigrationAssembly.cs
            var schemaNames =
                from dt in assembly.DefinedTypes
                let t = dt.AsType()
                let ti = t.GetTypeInfo()
                where 
                    t.IsSubclassOf(typeof(DbMigration)) &&
                    typeof(IMigrationMetadata).IsAssignableFrom(t) &&
                    !ti.IsAbstract &&
                    !ti.IsGenericType &&
                    t.Namespace == targetAssemblyConfig.MigrationsNamespace
                let defaultSchema = new ResourceManager(t).GetString("DefaultSchema")
                select string.IsNullOrEmpty(defaultSchema) ? "dbo" : defaultSchema;

            // we're only supporting one schema per configuration type
            var schemaName = schemaNames.Distinct().Single();
            return new ContextKeySchemaInfo(targetAssemblyConfig.ContextKey, schemaName);
        }

        private DbMigrator GetDbMigrator(Assembly assembly, string configurationType, DbConnectionInfo targetDatabase = null)
        {
            var targetAssemblyConfig = (DbMigrationsConfiguration)assembly.CreateInstance(configurationType);

            if (targetAssemblyConfig == null)
            {
                Log.Warning("The assembly at the path {assemblyPath} does not contain " +
                            "the migration configuration type {configType}. This may be due to a " +
                            "migration from or to a version where that config type doesn't exist. " +
                            "A target migration of '0' will be used, which will rollback all migrations.", assembly.CodeBase, configurationType);
            }
            else
            {
                if (targetDatabase != null)
                {
                    targetAssemblyConfig.TargetDatabase = targetDatabase;
                }
                else
                {
                    //Force local db to allow for meta data resolution
                    //We can probably get rid of this by implementing: 
                    //http://stackoverflow.com/questions/4741499/how-to-configure-providermanifesttoken-for-ef-code-first
                    targetAssemblyConfig.TargetDatabase = m_ConnectionInfoBuilder.Build(new DatabaseEndpoint()
                    {
                        ServerName = @"(localdb)\mssqllocaldb"
                    }, AuthenticationMode.Integrated);
                }
            }

            return targetAssemblyConfig != null ? new DbMigrator(targetAssemblyConfig) : null;
        }

        private void SetupDeploymentHistory(string schemaName)
        {
            Log.Information(
                "Adding Deployment History table for schema {schemaName} on {endPointServer}\\{endpointDatabase} if it does not exist",
                schemaName,
                m_Config.Database.ServerName,
                m_Config.Database.DatabaseName);

            var factory = DbProviderFactories.GetFactory(m_ConnectionInfoBuilder.ProviderName);
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = m_ConnectionInfoBuilder.BuildConnectionString(
                    m_Config.Database,
                    m_Config.AuthMode,
                    m_Config.SqlLogin,
                    m_Config.SqlPassword);

                connection.Open();
                DeploymentHistory.Setup(connection, schemaName);
                connection.Close();
            }
        }

        private object ConstructInitializer()
        {
            var isGalenInitializationType = m_Config.InitializationConfig.Type.EndsWith("Galen.Ci.EntityFramework.Initialization");
            if (!isGalenInitializationType)
            {
                Log.Debug(
                    "Configured initializer type of {type} is being loaded from target assembly {targetAssembly}.",
                    m_Config.InitializationConfig.Type,
                    TargetAssembly.FullName);

                return TargetAssembly.CreateInstance(m_Config.InitializationConfig.Type);
            }

            Log.Debug(
                "Configured initializer type of {type} is provided by CI framework. Loading types accordingly.",
                m_Config.InitializationConfig.Type);

            var assemblyQualifiedName = new ParsedAssemblyQualifiedName(m_Config.InitializationConfig.Type);

            var isSecureSeededInitializer =
                assemblyQualifiedName.GenericParameters.Count == 2 &&
                assemblyQualifiedName.CSharpStyleName.Value.Contains("CreateSecureSeededDatabaseIfNotExists");

            if (isSecureSeededInitializer)
            {
                Log.Debug(
                    "Configured initializer of type {type} is a CI based secure seeded initializer. Loading generic types...",
                    m_Config.InitializationConfig.Type);
                return GetSecureSeededInitializer(assemblyQualifiedName);
            }

            var isSecureInitializer =
                assemblyQualifiedName.GenericParameters.Count == 1 &&
                assemblyQualifiedName.CSharpStyleName.Value.Contains("CreateSecureDatabaseIfNotExists");

            if (isSecureInitializer)
            {
                Log.Debug(
                    "Configured initializer of type {type} is a CI based secure non-seeded initializer. Loading generic types...",
                    m_Config.InitializationConfig.Type);
                return GetSecureInitializer(assemblyQualifiedName.GenericParameters.Single().TypeName);
            }

            return !assemblyQualifiedName.GenericParameters.Any() 
                ? Type.GetType(m_Config.InitializationConfig.Type) 
                : null;
        }

        private object GetSecureSeededInitializer(ParsedAssemblyQualifiedName assemblyQualifiedName)
        {
            // pre-load generic types from correct locations
            var seedDataAssemblyPath = Path.Combine(
                Path.GetDirectoryName(m_Config.TargetAssemblyPath),
                $"{assemblyQualifiedName.GenericParameters[1].ShortAssemblyName}.dll");

            var seedDataAssembly = Assembly.LoadFrom(seedDataAssemblyPath);
            var seedDataType = seedDataAssembly.GetType(assemblyQualifiedName.GenericParameters[1].TypeName);

            var targetContextType = TargetAssembly.GetType(assemblyQualifiedName.GenericParameters[0].TypeName);
            var initType = typeof(CreateSecureSeededDatabaseIfNotExists<,>).MakeGenericType(
                targetContextType,
                seedDataType);

            return Activator.CreateInstance(initType);
        }

        private object GetSecureInitializer(string dbContextTypeName)
        {
            // pre-load generic types from correct locations
            var targetContextType = TargetAssembly.GetType(dbContextTypeName);
            var initType = typeof (CreateSecureDatabaseIfNotExists<>).MakeGenericType(targetContextType);

            return Activator.CreateInstance(initType);
        }

        private void ValidateConfiguredAssemblies()
        {
            if (m_HasValidated)
                return;

            if (this.TargetAssembly == null)
                throw new InvalidOperationException("Unable to load target assembly");

            if (this.DeployedAssembly == null)
                Log.Warning("No deployed assembly could be loaded. Downward migrations may fail.");

            m_HasValidated = true;
        }
    }
}
