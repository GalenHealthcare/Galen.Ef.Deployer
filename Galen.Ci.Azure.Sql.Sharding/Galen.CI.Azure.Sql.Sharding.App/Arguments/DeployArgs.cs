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
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Arguments
{
    public class DeployArgs
    {
        [ArgRequired]
        [ArgDescription("Connection string for the shard map manager database.")]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgDescription("Name for a login that will be created, if it does not already exist, on the shard map manager server.")]
        [ArgShortcut("ln")]
        public string LoginName { get; set; }

        [ArgDescription("Password for the created login.")]
        [ArgShortcut("lp")]
        public string LoginPassword { get; set; }

        [ArgDescription("User that will be created, if it does not already exist, in the shard map manager database and granted read/write access.")]
        [ArgShortcut("dun")]
        public string DatabaseUserName { get; set; }

        [ArgDescription("Specifies that LoginName is a Windows user account.")]
        [ArgShortcut("windows")]
        public bool UseWindowsLogin { get; set; }

        [ArgIgnore]
        public bool HasLoginName => (!string.IsNullOrEmpty(LoginName));

        [ArgIgnore]
        public bool HasLoginPassword => (!string.IsNullOrEmpty(LoginPassword));

        [ArgIgnore]
        public bool HasDatabaseUserName => (!string.IsNullOrEmpty(DatabaseUserName));

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ValidationArgException("Connection string is required for deployment.");
            }

            if (UseWindowsLogin && !HasLoginName)
            {
                throw new ValidationArgException("Login name is required when UseWindowsLogin is specified.");
            }

            if (HasLoginPassword && !HasLoginName)
            {
                throw new ValidationArgException("Login name is required when LoginPassword is specified.");
            }

            if (HasLoginName && !UseWindowsLogin && !HasLoginPassword)
            {
                throw new ValidationArgException("LoginPassword is required when creating a login and not using Windows login.");
            }

            if (UseWindowsLogin && HasLoginPassword)
            {
                throw new ValidationArgException("LoginPassword cannot be specified when using Windows login.");
            }

            if (HasDatabaseUserName && !HasLoginName)
            {
                throw new ValidationArgException("LoginName is required when creating database user.");
            }
        }
    }
}
