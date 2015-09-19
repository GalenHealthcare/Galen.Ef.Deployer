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
using System.ComponentModel;
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Revivers
{
    public class ShardKeyRangeArgReviver
    {
        private const char PairSeperator = '|';
        private const char ValueSeperator = ',';

        [ArgReviver]
        public static ShardKeyRange Revive(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var argValueSplit = value.Split(new[] { PairSeperator }, StringSplitOptions.RemoveEmptyEntries);
            if (argValueSplit.Length != 2)
            {
                throw new ArgException("Shard key range is not in the expected format of Type|Low,High.");
            }

            var rangeValueSplit = argValueSplit[1].Split(new[] {ValueSeperator}, StringSplitOptions.RemoveEmptyEntries);
            if (rangeValueSplit.Length != 2)
            {
                throw new ArgException("Shard key range is not in the expected format of Type|Low,High.");
            }

            var keyType = ShardKeyArgReviver.GetType(argValueSplit[0]);
            if (keyType == null)
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(keyType);
            var lowValue = converter.ConvertFrom(rangeValueSplit[0]);
            var highValue = converter.ConvertFrom(rangeValueSplit[1]);

            return new ShardKeyRange(keyType, lowValue, highValue);
        }
    }
}
