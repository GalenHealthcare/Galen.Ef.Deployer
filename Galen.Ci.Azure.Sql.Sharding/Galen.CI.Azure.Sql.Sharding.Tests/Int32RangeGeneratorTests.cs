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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.CI.Azure.Sql.Sharding.Tests
{
    [TestClass]
    public class Int32RangeGeneratorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ThrowsExceptionForNumShardsLessThan1()
        {
            Int32RangeGenerator.GetRanges(0);
        }

        [TestMethod]
        public void CorrectlyGeneratesRangeForOneShard()
        {
            var ranges = Int32RangeGenerator.GetRanges(1);
            Assert.IsNotNull(ranges);
            Assert.AreEqual(1, ranges.Count);

            var actualFullRange = ranges.Single();
            Assert.AreEqual(1, actualFullRange.Key);
            Assert.AreEqual(int.MinValue, actualFullRange.Value.LowValue);
            Assert.AreEqual(int.MaxValue, actualFullRange.Value.HighValue);
        }

        [TestMethod]
        public void CorrectlyGeneratesRanges()
        {
            foreach (var numShards in Enumerable.Range(2, 10000))
            {
                var ranges = Int32RangeGenerator.GetRanges(numShards);
                Assert.IsNotNull(ranges);
                Assert.AreEqual(numShards, ranges.Count);

                var actualFirstRange = ranges.First();
                Assert.AreEqual(1, actualFirstRange.Key);
                Assert.AreEqual(int.MinValue, actualFirstRange.Value.LowValue);

                for (int shardNum = 2; shardNum < ranges.Count; shardNum++)
                {
                    var previousRange = ranges[shardNum - 1];
                    var actualRange = ranges[shardNum];
                    Assert.AreEqual(previousRange.HighValue, actualRange.LowValue);
                }

                var actualLastRange = ranges.Last();
                Assert.AreEqual(ranges.Count, actualLastRange.Key);
                Assert.AreEqual(int.MaxValue, actualLastRange.Value.HighValue);

                var actualNextToLastRange = ranges[ranges.Count - 1];
                Assert.AreEqual(actualNextToLastRange.HighValue, actualLastRange.Value.LowValue);
            }
        }
    }
}
