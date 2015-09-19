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
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace Galen.Ci.EntityFramework.Initialization
{
	public class CreateSecureDatabaseIfNotExists<TContext> : CreateDatabaseIfNotExists<TContext>, ISecureDbWithServiceAccount
		where TContext : DbContext
	{
		public override void InitializeDatabase(TContext context)
		{
			base.InitializeDatabase(context);
			SetupPermissions(context);
		}

		public CreateSecureDatabaseIfNotExists()
		{
			this.ServiceAccountDatabaseUserName=SecureDatabaseInitializationSettings.Default.ServiceAccountDatabaseUserName;
			this.ServiceAccountName=SecureDatabaseInitializationSettings.Default.ServiceAccountName;
			this.ServiceAccountDomain=SecureDatabaseInitializationSettings.Default.ServiceAccountDomain;
			this.ServiceAccountDatabaseUserPassword=
				SecureDatabaseInitializationSettings.Default.ServiceAccountDatabaseUserPassword;
			this.ServiceAccountType=string.IsNullOrEmpty(SecureDatabaseInitializationSettings.Default.ServiceAccountType) ? null :
				(ServiceAccountType?) Enum.Parse(typeof(ServiceAccountType), SecureDatabaseInitializationSettings.Default.ServiceAccountType);
		}

		protected virtual void SetupPermissions(DbContext context)
		{
			if (ServiceAccountType==null)
				return;

			switch (this.ServiceAccountType)
			{
				case Initialization.ServiceAccountType.Sql:
					if ((string.IsNullOrEmpty(ServiceAccountDatabaseUserName)
						  ||string.IsNullOrEmpty(ServiceAccountName)
						  ||string.IsNullOrEmpty(ServiceAccountDatabaseUserPassword)))
					{
						//Consider logging a warning
						return;
					}
					break;
				case Initialization.ServiceAccountType.Windows:
					if ((string.IsNullOrEmpty(ServiceAccountDatabaseUserName)
						  ||string.IsNullOrEmpty(ServiceAccountName)
						  ||string.IsNullOrEmpty(ServiceAccountDomain)))
					{
						//Consider logging a warning
						return;
					}
					break;
			}

			using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
			{
			    var isSqlAzure = CheckIfServerIsSqlAzure(context);

                var fullAccountName = (this.ServiceAccountType==Initialization.ServiceAccountType.Sql)
					? ServiceAccountName
					: $"{ServiceAccountDomain}\\{ServiceAccountName}";

			    if (!isSqlAzure)
			    {
			        switch (this.ServiceAccountType)
			        {
			            case Initialization.ServiceAccountType.Sql:
			                context.Database.ExecuteSqlCommand(
			                    string.Format(
			                        @"IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'{0}') " +
			                        @"BEGIN CREATE LOGIN [{0}] WITH PASSWORD=N'{1}' END;"
			                        , fullAccountName, ServiceAccountDatabaseUserPassword));
			                break;

			            case Initialization.ServiceAccountType.Windows:
			                context.Database.ExecuteSqlCommand(
			                    string.Format(
			                        @"IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'{0}') " +
			                        @"BEGIN CREATE LOGIN [{0}] FROM WINDOWS END;"
			                        , fullAccountName));
			                break;
			        }
                }

                context.Database.ExecuteSqlCommand(
					string.Format(
						@"IF NOT EXISTS (SELECT * FROM [{2}].dbo.sysusers WHERE name = N'{1}') "+
						@"BEGIN CREATE USER [{1}] FOR LOGIN  [{0}] WITH DEFAULT_SCHEMA=[dbo] END;"
						, fullAccountName, ServiceAccountDatabaseUserName, context.Database.Connection.Database));

                context.Database.ExecuteSqlCommand($@"GRANT CONNECT TO [{ServiceAccountDatabaseUserName}];");
                context.Database.ExecuteSqlCommand($"GRANT EXECUTE TO [{ServiceAccountDatabaseUserName}];");
                context.Database.ExecuteSqlCommand($@"EXEC sp_addrolemember 'db_datareader', '{ServiceAccountDatabaseUserName}';");
				context.Database.ExecuteSqlCommand($@"EXEC sp_addrolemember 'db_datawriter', '{ServiceAccountDatabaseUserName}';");
			}
		}

	    private static bool CheckIfServerIsSqlAzure(DbContext context)
	    {
            return context.Database.SqlQuery<string>("SELECT @@VERSION").Single().ToUpper().Contains("SQL AZURE");
        }

	    public string ServiceAccountName
		{ set; get; }
		public string ServiceAccountDomain
		{ set; get; }
		public string ServiceAccountDatabaseUserName
		{ set; get; }
		public string ServiceAccountDatabaseUserPassword
		{ get; set; }
		public ServiceAccountType? ServiceAccountType
		{ get; set; }
	}
}
