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

namespace Galen.Ci.EntityFramework.Deployer
{
    public class ConfigurationArgumentMapper
    {
        private readonly IDeploymentConfigurationStore m_ConfigStore;

        public ConfigurationArgumentMapper(IDeploymentConfigurationStore configStore)
        {
            m_ConfigStore = configStore;
        }

        public DbDeploymentManagerConfiguration FromArguments(Arguments args)
        {
            var config = m_ConfigStore.Load();
            return new DbDeploymentManagerConfiguration()
            {
                Database = args.Database.Endpoints.Single(),            // we currently only support 1 endpoint, but refactoring things gave PowerArgs a hissy fit because classes weren't where it wanted them
                AuthMode = args.AuthMode,
                SqlLogin = args.SqlLogin,
                SqlPassword = args.SqlPassword,
                DeployedAssemblyOverridePath = args.DeployedAssemblyOverridePath,
                TargetAssemblyPath = args.TargetAssemblyPath,
                RunServerMigrationsInTransaction = args.RunServerMigrationsInTransaction,
                Mode = args.Mode,
                MigrationConfig = config.MigrationConfigurationInfo,
                InitializationConfig = config.InitializerConfigurationInfo,
                DeploymentHistoryExtractPath = args.DeploymentHistoryExtractPath
            };
        }
    }
}
