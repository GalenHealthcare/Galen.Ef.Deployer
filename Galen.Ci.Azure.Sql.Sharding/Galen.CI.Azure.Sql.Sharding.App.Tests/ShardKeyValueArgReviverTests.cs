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
using Galen.CI.Azure.Sql.Sharding.App.Revivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Tests
{
    [TestClass]
    public class ShardKeyValueArgReviverTests
    {
        [TestMethod]
        public void CorrectlyRevivesGuidValueContainingDashesIgnoringCase()
        {
            var expectedGuid = Guid.Parse("46975B4A-2d77-4cd9-B559-16E13600FCDB");
            var actualGuidShardKeyValue = ShardKeyValueArgReviver.Revive(null, $"GUID|{expectedGuid}");
            Assert.IsNotNull(actualGuidShardKeyValue);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyType);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyValue);
            Assert.AreEqual(typeof (Guid), actualGuidShardKeyValue.KeyType);
            Assert.AreEqual(expectedGuid, (Guid)actualGuidShardKeyValue.KeyValue);
        }

        [TestMethod]
        public void CorrectlyRevivesGuidValueWithoutDashesIgnoringCase()
        {
            var expectedGuid = Guid.Parse("661282B387C84f5aa66167D79CB866A3");
            var actualGuidShardKeyValue = ShardKeyValueArgReviver.Revive(null, $"GUID|{expectedGuid}");
            Assert.IsNotNull(actualGuidShardKeyValue);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyType);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyValue);
            Assert.AreEqual(typeof(Guid), actualGuidShardKeyValue.KeyType);
            Assert.AreEqual(expectedGuid, (Guid)actualGuidShardKeyValue.KeyValue);
        }

        [TestMethod]
        public void CorrectlyRevivesGuidValueInsideBracesDashesIgnoringCase()
        {
            var expectedGuid = Guid.Parse("{5A88452F-2534-40d4-a439-1a856e8e4d7a}");
            var actualGuidShardKeyValue = ShardKeyValueArgReviver.Revive(null, $"GUID|{expectedGuid}");
            Assert.IsNotNull(actualGuidShardKeyValue);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyType);
            Assert.IsNotNull(actualGuidShardKeyValue.KeyValue);
            Assert.AreEqual(typeof(Guid), actualGuidShardKeyValue.KeyType);
            Assert.AreEqual(expectedGuid, (Guid)actualGuidShardKeyValue.KeyValue);
        }

        [TestMethod]
        public void CorrectlyRevivesIntValueIgnoringCase()
        {
            const int expectedInt = 66;
            var actualIntShardKeyValue = ShardKeyValueArgReviver.Revive(null, $"int|{expectedInt}");
            Assert.IsNotNull(actualIntShardKeyValue);
            Assert.IsNotNull(actualIntShardKeyValue.KeyType);
            Assert.IsNotNull(actualIntShardKeyValue.KeyValue);
            Assert.AreEqual(typeof(int), actualIntShardKeyValue.KeyType);
            Assert.AreEqual(expectedInt, (int)actualIntShardKeyValue.KeyValue);
        }

        [TestMethod]
        public void DoesNotReviveNullOrEmptyOrWhiteSpaceOrInvalidOrUnknownOrUnsupportedTypesAndDoesNotThrowException()
        {
            var actualNullValue = ShardKeyValueArgReviver.Revive(null, null);
            Assert.IsNull(actualNullValue);

            var actualEmptyValue = ShardKeyValueArgReviver.Revive(null, "");
            Assert.IsNull(actualEmptyValue);

            var actualWhiteSpaceValue = ShardKeyValueArgReviver.Revive(null, "\t");
            Assert.IsNull(actualWhiteSpaceValue);

            var actualInvalidValue = ShardKeyValueArgReviver.Revive(null, "Invalid|4000");           // Invalid maps to 0 in the enum
            Assert.IsNull(actualInvalidValue);

            var actualUnknownValue = ShardKeyValueArgReviver.Revive(null, "Widget|Sprocket");        // Widget not a real thing
            Assert.IsNull(actualUnknownValue);

            var actualUnsupportedType = ShardKeyValueArgReviver.Revive(null, "Long|2147483648");     // Long not currently supported by Sharding.App (though it is supported by SqlAzure Sharding)
            Assert.IsNull(actualUnsupportedType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingType()
        {
            ShardKeyValueArgReviver.Revive(null, "|E47704F6BF2B4950A343BAC47997CC0C");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingValue()
        {
            ShardKeyValueArgReviver.Revive(null, "Int|");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenTooManyValues()
        {
            ShardKeyValueArgReviver.Revive(null, "Int|100|200");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenFormatIsInvalid()
        {
            ShardKeyValueArgReviver.Revive(null, "int 2000");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ThrowsExceptionWhenTypeAndValueAreMismatched()
        {
            ShardKeyValueArgReviver.Revive(null, "guid|126");
        }
    }
}