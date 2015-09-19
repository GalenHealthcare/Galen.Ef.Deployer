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
using Galen.CI.Azure.Sql.Sharding.App.Arguments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Tests
{
    [TestClass]
    public class DeployArgsTests
    {
        [TestMethod]
        public void CorrectlyAcceptsValidArguments()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; "
            };

            // should not throw exception
            // connection string by itself is valid
            sut.Validate();

            // should not throw exception
            // connection string w/both a login name and password is valid when not using Windows login
            sut.LoginName = "MySqlLogin";
            sut.LoginPassword = "MySqlLoginPassword";
            sut.Validate();

            // should not throw exception
            // connection string w/login name and no password is valid when using Windows login
            sut.LoginPassword = null;
            sut.UseWindowsLogin = true;
            sut.Validate();

            // should not throw exception
            // connection string w/database user name is valid when login name is provided with no password and using Windows login
            sut.DatabaseUserName = "MyDbU";
            sut.Validate();

            // should not throw exception
            // connection string w/database user name is valid when login name and password are provided and not using Windows login
            sut.LoginPassword = "MySqlLoginPassword";
            sut.UseWindowsLogin = false;
            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenUsingWindowsLoginWithoutLoginName()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                UseWindowsLogin = true
            };

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenLogingPasswordProvidedWithoutLoginName()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                LoginPassword = "MySqlLoginPassword"
            };

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenLoginNameProvidedWithoutPasswordAndNotUsingWindowsLogin()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                LoginName = "MySqlLogin"
            };

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenLoginPasswordProvidedAndUsingWindowsLogin()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                LoginName = "MySqlLogin",
                LoginPassword = "MySqlLoginPassword",
                UseWindowsLogin = true
            };

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenDatabaseUserNameProvidedWithoutLoginName()
        {
            var sut = new DeployArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                DatabaseUserName = "MyDbU"
            };

            sut.Validate();
        }
    }
}
