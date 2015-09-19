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
    public class AddInt32RangeMapShardsArgsTests
    {
        [TestMethod]
        public void CorrectlyAcceptsValidArguments()
        {
            var sut = new AddInt32RangeMapShardsArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests;",
                MapName = "MyTestIntRangeMapName",
                ShardLocations = new ShardLocations
                {
                    Locations = new []
                    {
                        new ShardLocation
                        {
                            ServerName = @"(localdb)\.\SharedLocalDb",
                            DatabaseName = "MyTestShardDb001"
                        }
                    }
                }
            };

            sut.Validate();     // should not throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenAnyShardLocationsAreNull()
        {
            var sut = new AddInt32RangeMapShardsArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests;",
                MapName = "MyTestIntRangeMapName",
                ShardLocations = new ShardLocations()
            };

            var validShardLocation = new ShardLocation
            {
                ServerName = @"(localdb)\.\SharedLocalDb",
                DatabaseName = "MyTestShardDb001"
            };

            sut.ShardLocations.Locations = new [] {validShardLocation, null};

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenAnyShardLocationIsMissingServerName()
        {
            var sut = new AddInt32RangeMapShardsArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests;",
                MapName = "MyTestIntRangeMapName",
                ShardLocations = new ShardLocations()
            };

            var shardLocations = new[]
            {
                new ShardLocation
                {
                    ServerName = @"(localdb)\.\SharedLocalDb",
                    DatabaseName = "MyTestShardDb001"
                },
                new ShardLocation
                {
                    DatabaseName = "MyTestShardDb002"
                }
            };

            sut.ShardLocations.Locations = shardLocations;

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenAnyShardLocationIsMissingDatabaseName()
        {
            var sut = new AddInt32RangeMapShardsArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests;",
                MapName = "MyTestIntRangeMapName",
                ShardLocations = new ShardLocations()
            };

            var shardLocations = new[]
            {
                new ShardLocation
                {
                    ServerName = @"(localdb)\.\SharedLocalDb",
                    DatabaseName = "MyTestShardDb001"
                },
                new ShardLocation
                {
                    ServerName = @"(localdb)\mssqllocaldb"
                }
            };

            sut.ShardLocations.Locations = shardLocations;

            sut.Validate();
        }
    }
}
