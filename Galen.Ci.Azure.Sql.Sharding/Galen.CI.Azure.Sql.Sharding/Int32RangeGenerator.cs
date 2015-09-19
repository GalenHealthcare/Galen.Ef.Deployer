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
using System.Collections.Generic;

namespace Galen.CI.Azure.Sql.Sharding
{
    public static class Int32RangeGenerator
    {
        public static Dictionary<int, Int32Range> GetRanges(int numShards)
        {
            if (numShards < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numShards), numShards, "numShards cannot be less than 1.");
            }

            var ranges = new Dictionary<int, Int32Range>(numShards);

            const uint totalNumbersInRangeType = uint.MaxValue;
            var rangeSize = (uint)Math.Ceiling(totalNumbersInRangeType / (decimal)numShards);

            int shardNumber = 1;
            int lowValue = int.MinValue;
            while (shardNumber < numShards)
            {
                var highValue = (int)(lowValue + rangeSize);
                var shardRange = new Int32Range(lowValue, highValue);
                ranges.Add(shardNumber, shardRange);

                shardNumber++;
                lowValue = highValue;
            }

            // last range
            if (lowValue < int.MaxValue)
            {
                var highValue = (lowValue + rangeSize);
                if (highValue > int.MaxValue)
                {
                    highValue = int.MaxValue;
                }
                var lastShardRange = new Int32Range(lowValue, (int)highValue);
                ranges.Add(numShards, lastShardRange);
            }

            return ranges;
        }
    }
}
