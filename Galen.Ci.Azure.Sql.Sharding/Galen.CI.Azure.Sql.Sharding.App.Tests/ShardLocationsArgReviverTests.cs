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
using Galen.CI.Azure.Sql.Sharding.App.Revivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Tests
{
    [TestClass]
    public class ShardLocationsArgReviverTests
    {
        [TestMethod]
        public void CorrectlyRevivesSingleShardLocation()
        {
            const string expectedServerName = @"(localdb)\mssqllocaldb";
            const string expectedDatabaseName = "MyTestShard001";

            var actualShardLocations = ShardLocationsArgReviver.Revive(null, $"{expectedServerName}|{expectedDatabaseName}");
            Assert.IsNotNull(actualShardLocations);
            Assert.IsNotNull(actualShardLocations.Locations);
            Assert.AreEqual(1, actualShardLocations.Locations.Length);

            var actualLocation = actualShardLocations.Locations.Single();
            Assert.AreEqual(expectedServerName, actualLocation.ServerName);
            Assert.AreEqual(expectedDatabaseName, actualLocation.DatabaseName);
        }

        [TestMethod]
        public void CorrectlyRevivesSingleShardLocationWithErrantPairSeperator()
        {
            const string expectedServerName = @"(localdb)\mssqllocaldb";
            const string expectedDatabaseName = "MyTestShard001";

            var actualShardLocations = ShardLocationsArgReviver.Revive(null, $"{expectedServerName}|{expectedDatabaseName}|");
            Assert.IsNotNull(actualShardLocations);
            Assert.IsNotNull(actualShardLocations.Locations);
            Assert.AreEqual(1, actualShardLocations.Locations.Length);

            var actualLocation = actualShardLocations.Locations.Single();
            Assert.AreEqual(expectedServerName, actualLocation.ServerName);
            Assert.AreEqual(expectedDatabaseName, actualLocation.DatabaseName);
        }

        [TestMethod]
        public void CorrectlyRevivesMutlipleShardLocations()
        {
            const string expectedServerName01 = @"(localdb)\mssqllocaldb";
            const string expectedServerName02 = @"(localdb)\.\SharedLocalDb";
            const string expectedDatabaseName01 = "MyTestShard001";
            const string expectedDatabaseName02 = "MyTestShard002";

            var argValue =
                $"{expectedServerName01}|{expectedDatabaseName01},{expectedServerName02}|{expectedDatabaseName02}";

            var actualShardLocations = ShardLocationsArgReviver.Revive(null, argValue);
            Assert.IsNotNull(actualShardLocations);
            Assert.IsNotNull(actualShardLocations.Locations);
            Assert.AreEqual(2, actualShardLocations.Locations.Length);

            var actualLocation01 = actualShardLocations.Locations[0];
            Assert.AreEqual(expectedServerName01, actualLocation01.ServerName);
            Assert.AreEqual(expectedDatabaseName01, actualLocation01.DatabaseName);

            var actualLocation02 = actualShardLocations.Locations[1];
            Assert.AreEqual(expectedServerName02, actualLocation02.ServerName);
            Assert.AreEqual(expectedDatabaseName02, actualLocation02.DatabaseName);
        }

        [TestMethod]
        public void CorrectlyReturnsNullForNullOrEmptyOrWhiteSpaceValue()
        {
            var actualNull = ShardLocationsArgReviver.Revive(null, null);
            Assert.IsNull(actualNull);

            var actualEmpty = ShardLocationsArgReviver.Revive(null, "");
            Assert.IsNull(actualNull);

            var actualWhiteSpace = ShardLocationsArgReviver.Revive(null, "\t");
            Assert.IsNull(actualNull);
        }

        [TestMethod]
        public void CorrectlyReturnsEmptyLocationsArrayForEmtpyLocationValueAndDoesNotThrowException()
        {
            var actualEmptyLocations = ShardLocationsArgReviver.Revive(null, ",");
            Assert.IsNotNull(actualEmptyLocations);
            Assert.IsNotNull(actualEmptyLocations.Locations);
            Assert.IsFalse(actualEmptyLocations.Locations.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionForMissingServerName()
        {
            var _ = ShardLocationsArgReviver.Revive(null, "|MyTestShard001");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionForMissingDatabaseName()
        {
            var _ = ShardLocationsArgReviver.Revive(null, @"(localdb)\mssqllocaldb|");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionForMissingServerNameWhenMultipleLocationsSpecified()
        {
            var _ = ShardLocationsArgReviver.Revive(null, @"(localdb)\mssqllocaldb|MyTestShard001,|MyTestShard002");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionForMissingDatabaseNameWhenMultipleLocationsSpecified()
        {
            var _ = ShardLocationsArgReviver.Revive(null, @"(localdb)\mssqllocaldb|,(localdb)\mssqllocaldb|MyTestShard002");
        }
    }
}
