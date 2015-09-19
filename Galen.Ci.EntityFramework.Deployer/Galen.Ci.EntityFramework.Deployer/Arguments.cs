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
using Galen.Ci.EntityFramework.Configuration;
using PowerArgs;

namespace Galen.Ci.EntityFramework.Deployer
{

    public class Arguments
    {
        [ArgShortcut("m")]
        [ArgDescription("Specifies the deployment mode. Defaults to InitializeOrMigrate.")]
        public DeploymentMode Mode { get; set; }

        [ArgRequired]
        [ArgShortcut("ta")]
        [ArgDescription("The assembly containing the migrations to be executed against the databases.")]
        public string TargetAssemblyPath { get; set; }

        [ArgShortcut("da")]
        [ArgDescription("Forces the deployer to use the specified assembly instead of the binaries from deployment history.")]
        public string DeployedAssemblyOverridePath { get; set; }

        [ArgShortcut("am")]
        [ArgDescription("The authentication mode to use when executing initializations, migrations, and seeding.")]
        public AuthenticationMode AuthMode { get; set; }

        [ArgShortcut("sl")]
        [ArgDescription("The database server login name to use if authentication mode is set to Sql.")]
        public string SqlLogin { get; set; }

        [ArgShortcut("sp")]
        [ArgDescription("The database server login password to use if authentication mode is set to Sql.")]
        public string SqlPassword { get; set; }

        // leaving this as DatabaseEndpoints because PowerArgs cries if you change it to a single DatabaseEndpoint
        // it must only support argument types that are in the same assembly
        // so rather than refactor everything i'm just leaving it as is so PowerArgs shuts up, i keep my sanity,
        // and hopefully we can restore the multiple endpoints functionality at a later date
        [ArgRequired]
        [ArgShortcut("d")]
        [ArgDescription("Database and server for deployment in the format of: server|database")]
        public DatabaseEndpoints Database { get; set; }

        [ArgShortcut("st")]
        [ArgDescription("Wraps all migrations for an individual server inside a transaction")]
        public bool RunServerMigrationsInTransaction { get; set; }

        [ArgShortcut("dfs")]
        [ArgDescription("Disables the forced execution of database data seeders post migration")]
        public bool DisabledForcedSeeding { get; set; }

        [ArgShortcut("mct")]
        [ArgDescription("The type name, including namespace, of the migration configuration to use. If not specified, a deployment configuration XML file must be specified via DeploymentConfigurationFilePath.")]
        public string MigrationsConfigurationType { get; set; }

        [ArgShortcut("it")]
        [ArgDescription("The type name, including namespace, of the context initializer to use. If not specified, a deployment configuration XML file must be specified via DeploymentConfigurationFilePath.")]
        public string InitializerType { get; set; }

        [ArgShortcut("isa")]
        [ArgDescription("The service account name to pass to any initializers that implement ISecureDbWithServiceAccount. This may also be specified in the deployment configuration XML file on a per-initializer basis.")]
        public string InitializerServiceAccountName { get; set; }

        [ArgShortcut("isat")]
        [ArgDescription("The service account type to pass to any initializers that implement ISecureDbWithServiceAccount. Possible values: Sql, Windows. This may also be specified in the deployment configuration XML file on a per-initializer basis.")]
        public string InitializerServiceAccountType { get; set; }

        [ArgShortcut("isd")]
        [ArgDescription("The service account domain name to pass to any initializers that implement ISecureDbWithServiceAccount. This may also be specified in the deployment configuration XML file on a per-initializer basis.")]
        public string InitializerServiceAccountDomainName { get; set; }

        [ArgShortcut("isu")]
        [ArgDescription("The database user name to pass to any initializers that implement ISecureDbWithServiceAccount. This may also be specified in the deployment configuration XML file on a per-initializer basis.")]
        public string InitializerServiceAccountDatabaseUser { get; set; }

        [ArgShortcut("isup")]
        [ArgDescription("The database user password to any initializers that implement ISecureDbWithServiceAccount. This may also be specified in the deployment configuration XML file on a per-initializer basis.")]
        public string InitializerServiceAccountDatabaseUserPassword { get; set; }

        [ArgShortcut("dcp")]
        [ArgDescription("Specifies a deployment configuraiton XML file to be used. If not specified, a specific migration configuration type and/or context initializer type must be specified via MigrationsConfigurationType and InitializerType, respectively.")]
        public string DeploymentConfigurationFilePath { get; set; }

        [ArgShortcut("hep")]
        [ArgDescription("The directory to use when extracting the current deployed assembly from Deployment History.  The user account's temp directory is used if this parameter is not specified.")]
        public string DeploymentHistoryExtractPath { get; set; }

        [ArgDefaultValue(false)]
        [ArgShortcut("vb")]
        [ArgDescription("Use verbose logging.")]
        public bool Verbose { get; set; }
    }    
}
