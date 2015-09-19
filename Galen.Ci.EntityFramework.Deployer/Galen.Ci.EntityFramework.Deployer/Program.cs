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
using System.IO;
using Galen.Ci.EntityFramework.Configuration;
using PowerArgs;
using Serilog;

namespace Galen.Ci.EntityFramework.Deployer
{
	class Program
	{
		private static void Main(string[] args)
		{
            ConfigureLogging();

		    try
		    {
		        var arguments = Args.Parse<Arguments>(args);

		        if (arguments.Verbose)
		        {
		            ConfigureLogging(true);
		        }

		        ValidateArguments(arguments);

		        var dbMigrationManager = InitializeDbMigrationManager(arguments);
		        dbMigrationManager.Deploy();

		        Environment.Exit(0);
		    }
		    catch (ArgException ae)
		    {
		        Log.Error(ae, "Invalid arguments passed to deployer");
		        ArgUsage.GetStyledUsage<Arguments>().Write();
		    }
		    catch (Exception ex)
		    {
                Log.Error(ex, "Unknown error during deployment");
		    }

			Environment.Exit(-1);
		}

		private static void ValidateArguments(Arguments args)
		{
			if (args.Mode==DeploymentMode.SeedOnly)
			{
				if (string.IsNullOrEmpty(args.DeploymentConfigurationFilePath)&&
					string.IsNullOrEmpty(args.InitializerType))
				{
					throw new InvalidOperationException(
						"Either DeploymentConfigurationFilePath or InitializerType must be specified");
				}
			}
			else
			{
				if (args.Mode!=DeploymentMode.InitializeOnly)
				{
					if (string.IsNullOrEmpty(args.DeploymentConfigurationFilePath)&&
						string.IsNullOrEmpty(args.MigrationsConfigurationType))
					{
						throw new InvalidOperationException(
							"Either DeploymentConfigurationFilePath or MigrationsConfigurationType must be specified");
					}
				}

				if (args.Mode!=DeploymentMode.MigrationOnly)
				{
					if (string.IsNullOrEmpty(args.DeploymentConfigurationFilePath)&&
						string.IsNullOrEmpty(args.InitializerType))
					{
						throw new InvalidOperationException(
							"Either DeploymentConfigurationFilePath or InitializerType must be specified");
					}
				}
			}
		}

		private static void ConfigureLogging(bool isEnableVerbose = false)
		{
			var loggingConfig = new LoggerConfiguration().WriteTo.ColoredConsole();

		    if (isEnableVerbose)
		    {
		        loggingConfig.MinimumLevel.Verbose();
		    }
		    else
		    {
                loggingConfig.MinimumLevel.Warning();
            }

		    if (!string.IsNullOrEmpty(StaticConfiguration.LoggingEndpoint))
			{
				loggingConfig.WriteTo.Seq(StaticConfiguration.LoggingEndpoint);
			}

			Log.Logger=loggingConfig.CreateLogger();
		}

		private static DbDeploymentManager InitializeDbMigrationManager(Arguments arguments)
		{
			var configMapper = new ConfigurationArgumentMapper(
				(string.IsNullOrEmpty(arguments.DeploymentConfigurationFilePath)
					? (IDeploymentConfigurationStore) new DeploymentConfigurationInMemoryStore(
						new DeploymentConfiguration
						{
							MigrationConfigurationInfo = new MigrationConfigurationInfo
							{
								Type = arguments.MigrationsConfigurationType
							},

							InitializerConfigurationInfo=new InitializerConfigurationInfo
							{
								Type = arguments.InitializerType,
								DisableForcedSeeding = arguments.DisabledForcedSeeding,
								ServiceAccount = string.IsNullOrEmpty(arguments.InitializerServiceAccountName)
									? null 
                                    : new ServiceAccountInfo()
									{
										Name = arguments.InitializerServiceAccountName,
										AccountType = arguments.InitializerServiceAccountType,
										Domain = arguments.InitializerServiceAccountDomainName,
										DatabaseUser = arguments.InitializerServiceAccountDatabaseUser,
										DatabaseUserPassword = arguments.InitializerServiceAccountDatabaseUserPassword
									}
							}
						})
					: new DeploymentConfigurationXmlStore(EnsureAbsolutePath(arguments.DeploymentConfigurationFilePath))));

			var config = configMapper.FromArguments(arguments);

			config.TargetAssemblyPath=EnsureAbsolutePath(config.TargetAssemblyPath);
			config.DeployedAssemblyOverridePath=EnsureAbsolutePath(config.DeployedAssemblyOverridePath);

			return new DbDeploymentManager(config,
				new AssemblyLoader(),
				new SqlClientDbConnectionInfoBuilder());
		}

		private static string EnsureAbsolutePath(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				if (path.StartsWith(@".\"))
				{
					path=Directory.GetCurrentDirectory()+path.Substring(1, path.Length-1);
				}
			}

			return path;
		}
	}
}
