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
using System.Data.SqlClient;
using PowerArgs;

namespace Galen.Ci.EntityFramework.Utilities.App.Arguments
{
    public abstract class DatabaseConnectionArgs
    {
        [ArgRequired]
        [ArgDescription("Name of the Sql Server")]
        [ArgShortcut("s")]
        public string ServerName { get; set; }

        [ArgRequired]
        [ArgDescription("Name of the Sql Database")]
        [ArgShortcut("d")]
        public string DatabaseName { get; set; }

        [ArgDescription("Sql login name; required if not using Integrated Security.")]
        [ArgShortcut("l")]
        public string LoginName { get; set; }

        [ArgDescription("Sql login password; required if not using Integrated Security.")]
        [ArgShortcut("p")]
        public string LoginPassword { get; set; }

        [ArgDescription("Specifies to use Integrated Security for logging in to the Sql Server.")]
        public bool IntegratedSecurity { get; set; }

        [ArgIgnore]
        public bool HasLoginName => (!string.IsNullOrEmpty(LoginName));

        [ArgIgnore]
        public bool HasLoginPassword => (!string.IsNullOrEmpty(LoginPassword));

        public string GetConnectionString()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                ApplicationName = "Galen.Ci.EntityFramework.Utilities.App",
                DataSource = ServerName,
                InitialCatalog = DatabaseName,
                IntegratedSecurity = IntegratedSecurity
            };

            if (!IntegratedSecurity)
            {
                connectionStringBuilder.UserID = LoginName;
                connectionStringBuilder.Password = LoginPassword;
            }

            return connectionStringBuilder.ConnectionString;
        }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerName))
            {
                throw new ValidationArgException("Server name is required.");
            }

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                throw new ValidationArgException("Database name is required.");
            }

            if (IntegratedSecurity && HasLoginName)
            {
                throw new ValidationArgException("Login name cannot be specified when using Integrated Security.");
            }

            if (IntegratedSecurity && HasLoginPassword)
            {
                throw new ValidationArgException("Login password cannot be specified when using Integrated Security.");
            }

            if (!IntegratedSecurity && !HasLoginName)
            {
                throw new ValidationArgException("Login name is required or Integrated Security must be specified.");
            }

            if (HasLoginName && !HasLoginPassword)
            {
                throw new ValidationArgException("Login password is required.");
            }
        }
    }
}
