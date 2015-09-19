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
    public class AddRangeMapShardArgsTests
    {
        [TestMethod]
        public void CorrectlyAcceptsValidArguments()
        {
            var sut = new AddRangeMapShardArgs
            {
                ConnectionString = @"Server = (localdb)\mssqllocaldb; Initial Catalog = SomeTestDb; Integrated Security = true; Application Name = Galen.CI.Azure.Sql.Sharding.App.Tests; ",
                MapName = "MyTestListMapName",
                ShardServerName = "TestShard001",
                ShardDatabaseName = "MyShardedDatabase",
                ShardKeyRange = new ShardKeyRange(typeof (int), lowValue: 2000, highValue: 3000)
            };

            sut.Validate();     // should not throw exception
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenIntLowValueIsGreaterThanHighValue()
        {
            var sut = new AddRangeMapShardArgs
            {
                ConnectionString = "required",
                MapName = "required",
                ShardDatabaseName = "required",
                ShardServerName = "required",
                ShardKeyRange = new ShardKeyRange(typeof (int), lowValue: 200, highValue: 100)
            };

            sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationArgException))]
        public void FailsValidationWhenIntLowValueIsSameAsHighValue()
        {
            var sut = new AddRangeMapShardArgs
            {
                ConnectionString = "required",
                MapName = "required",
                ShardDatabaseName = "required",
                ShardServerName = "required",
                ShardKeyRange = new ShardKeyRange(typeof(int), lowValue: 1000, highValue: 1000)
            };

            sut.Validate();
        }
    }
}
