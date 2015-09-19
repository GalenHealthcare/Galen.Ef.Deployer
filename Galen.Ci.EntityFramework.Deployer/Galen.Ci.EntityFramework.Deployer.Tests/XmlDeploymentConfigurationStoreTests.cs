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
using System.IO;
using Galen.Ci.EntityFramework.Configuration;
using Galen.Ci.EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Deployer.Tests
{
	[TestClass]
	public class XmlDeploymentConfigurationStoreTests
	{
		[TestMethod]
		public void SerializationShouldGenerateCorrectXml()
		{
			string expectedXml =
				ResourceHelper.ReadString(
					"Galen.Ci.EntityFramework.Deployer.Tests.Data.SerializationShouldGenerateCorrectXml_Expected.xml");

			string actualXml;

			var config = new DeploymentConfiguration()
			{
				MigrationConfigurationInfo = new MigrationConfigurationInfo
				{
					Type = "Galen.Enterprise.Data.Migrations.SomeContext.Configuration"
				},

				InitializerConfigurationInfo=new InitializerConfigurationInfo
				{
					Type = "Galen.Enterprise.Data.Initializers.SomeContextCreateDatabaseIfNotExists",
					ServiceAccount = new ServiceAccountInfo()
					{
						Name = "SomeWindowsAccountName",
						Domain = "SomeDomainName",
						DatabaseUser = "SomeDbUserName",
						AccountType = "Windows"
					}
				}
			};

			using (var memoryStream = new MemoryStream())
			{
				var sut = new DeploymentConfigurationXmlStore(memoryStream);
				sut.Save(config);

				memoryStream.Position=0;
				using (var sr = new StreamReader(memoryStream))
				{
					actualXml=sr.ReadToEnd();
				}
			}

			Assert.AreEqual(expectedXml, actualXml);
		}

		[TestMethod]
		public void DeserializationShouldResultInCorrectConfiguration()
		{
			byte[] sourceData = ResourceHelper.ReadBytes("Galen.Ci.EntityFramework.Deployer.Tests.Data.DeserializationShouldResultInCorrectConfiguration_Source.xml");
			DeploymentConfiguration actualConfig;
			var expectedConfig = new DeploymentConfiguration()
			{
				MigrationConfigurationInfo=new MigrationConfigurationInfo
		        {
			        Type = "Galen.Enterprise.Data.Migrations.SomeContext.Configuration"
		        },

				InitializerConfigurationInfo=new InitializerConfigurationInfo
				{
					Type = "Galen.Enterprise.Data.Initializers.SomeContextCreateDatabaseIfNotExists",
					ServiceAccount = new ServiceAccountInfo()
					{
						Name = "SomeWindowsAccountName",
						Domain = "SomeDomainName",
						DatabaseUser = "SomeDbUserName",
						AccountType = "Windows"

					}
				}
			};

			using (var stream = new MemoryStream(sourceData))
			{
				var sut = new DeploymentConfigurationXmlStore(stream);
				actualConfig=sut.Load();
			}

			Assert.IsNotNull(actualConfig);
			Assert.IsNotNull(actualConfig.InitializerConfigurationInfo);

		    Assert.IsNotNull(actualConfig.MigrationConfigurationInfo);
		    Assert.AreEqual(expectedConfig.MigrationConfigurationInfo.Type, actualConfig.MigrationConfigurationInfo.Type);

			Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.Type, actualConfig.InitializerConfigurationInfo.Type);

			Assert.IsTrue((actualConfig.InitializerConfigurationInfo.ServiceAccount!=null
							&&expectedConfig.InitializerConfigurationInfo.ServiceAccount!=null)
							||(actualConfig.InitializerConfigurationInfo.ServiceAccount==null
								&&expectedConfig.InitializerConfigurationInfo.ServiceAccount==null), "Service account info mismatch");

			if (actualConfig.InitializerConfigurationInfo.ServiceAccount!=null)
			{
				Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.Name, actualConfig.InitializerConfigurationInfo.ServiceAccount.Name);
				Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.Domain, actualConfig.InitializerConfigurationInfo.ServiceAccount.Domain);
				Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUser, actualConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUser);
				Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.AccountType, actualConfig.InitializerConfigurationInfo.ServiceAccount.AccountType);
				Assert.AreEqual(expectedConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUserPassword, actualConfig.InitializerConfigurationInfo.ServiceAccount.DatabaseUserPassword);
			}
		}
	}
}
