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
    public class ShardKeyRangeArgReviverTests
    {
        [TestMethod]
        public void CorrectlyRevivesIntRangeIgnoringCase()
        {
            const int expectedLowValue = 100;
            const int expectedHighValue = 300;

            var actualIntRange = ShardKeyRangeArgReviver.Revive(null, $"INT|{expectedLowValue},{expectedHighValue}");
            Assert.IsNotNull(actualIntRange);
            Assert.IsNotNull(actualIntRange.KeyType);
            Assert.IsNotNull(actualIntRange.LowValue);
            Assert.IsNotNull(actualIntRange.HighValue);

            Assert.AreEqual(typeof(int), actualIntRange.KeyType);
            Assert.AreEqual(expectedLowValue, (int)actualIntRange.LowValue);
            Assert.AreEqual(expectedHighValue, (int)actualIntRange.HighValue);
        }

        [TestMethod]
        public void DoesNotReviveNullOrEmptyOrWhiteSpaceOrInvalidOrUnknownOrUnsupportedTypesAndDoesNotThrowException()
        {
            var actualNullValue = ShardKeyRangeArgReviver.Revive(null, null);
            Assert.IsNull(actualNullValue);

            var actualEmptyValue = ShardKeyRangeArgReviver.Revive(null, "");
            Assert.IsNull(actualEmptyValue);

            var actualWhiteSpaceValue = ShardKeyRangeArgReviver.Revive(null, "\t");
            Assert.IsNull(actualWhiteSpaceValue);

            var actualInvalidValue = ShardKeyRangeArgReviver.Revive(null, "Invalid|4000,6000");                 // Invalid maps to 0 in the enum
            Assert.IsNull(actualInvalidValue);

            var actualUnknownValue = ShardKeyRangeArgReviver.Revive(null, "Widget|Sprocket,Whistle");           // Widget not a real thing
            Assert.IsNull(actualUnknownValue);

            var actualUnsupportedType = ShardKeyRangeArgReviver.Revive(null, "Long|2147483648,3245216237");     // Long not currently supported by Sharding.App (though it is supported by SqlAzure Sharding)
            Assert.IsNull(actualUnsupportedType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingType()
        {
            ShardKeyRangeArgReviver.Revive(null, "|15000,16000");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingValues()
        {
            ShardKeyRangeArgReviver.Revive(null, "int|");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingLowValue()
        {
            ShardKeyRangeArgReviver.Revive(null, "int|,9000");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenMissingHighValue()
        {
            ShardKeyRangeArgReviver.Revive(null, "int|7500,");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenTooManyValues()
        {
            ShardKeyRangeArgReviver.Revive(null, "int|7500,9000,11250");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgException))]
        public void ThrowsExceptionWhenFormatIsInvalid()
        {
            ShardKeyRangeArgReviver.Revive(null, "int|7500|9000");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ThrowsExceptionWhenTypeAndValueAreMismatched()
        {
            ShardKeyRangeArgReviver.Revive(null, "guid|1600,2000");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ThrowsExceptionWhenValuesAreMismatchedTypes()
        {
            ShardKeyRangeArgReviver.Revive(null, "guid|C9E34B32E0F7468C8D4C525232D11F0C,2700");
        }
    }
}
