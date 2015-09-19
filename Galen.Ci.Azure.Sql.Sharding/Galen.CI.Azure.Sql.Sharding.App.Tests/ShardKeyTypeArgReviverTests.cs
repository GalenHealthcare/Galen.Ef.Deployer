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

namespace Galen.CI.Azure.Sql.Sharding.App.Tests
{
    [TestClass]
    public class ShardKeyTypeArgReviverTests
    {
        [TestMethod]
        public void CorrectRevivesAllSupportedTypesIgnoringCase()
        {
            var actualGuidType = ShardKeyTypeArgReviver.Revive(null, "GUID");
            Assert.IsNotNull(actualGuidType);
            Assert.AreEqual(typeof (Guid), actualGuidType);

            var actualIntType = ShardKeyTypeArgReviver.Revive(null, "INT");
            Assert.IsNotNull(actualIntType);
            Assert.AreEqual(typeof (int), actualIntType);
        }

        [TestMethod]
        public void DoesNotReviveNullOrEmptyOrWhiteSpaceOrInvalidOrUnknownOrUnsupportedTypesAndDoesNotThrowException()
        {
            var actualNullType = ShardKeyTypeArgReviver.Revive(null, null);
            Assert.IsNull(actualNullType);

            var actualEmptyStringType = ShardKeyTypeArgReviver.Revive(null, "");
            Assert.IsNull(actualEmptyStringType);

            var actualWhiteSpaceType = ShardKeyTypeArgReviver.Revive(null, "\t");
            Assert.IsNull(actualWhiteSpaceType);

            var actualInvalidType = ShardKeyTypeArgReviver.Revive(null, "Invalid");     // this maps to 0 in the enum
            Assert.IsNull(actualInvalidType);

            var actualUnknownType = ShardKeyTypeArgReviver.Revive(null, "Widget");      // not a real thing
            Assert.IsNull(actualUnknownType);

            var actualUnsupportedType = ShardKeyTypeArgReviver.Revive(null, "Long");    // not currently supported by Sharding.App (though it is supported by SqlAzure Sharding)
            Assert.IsNull(actualUnsupportedType);
        }
    }
}
