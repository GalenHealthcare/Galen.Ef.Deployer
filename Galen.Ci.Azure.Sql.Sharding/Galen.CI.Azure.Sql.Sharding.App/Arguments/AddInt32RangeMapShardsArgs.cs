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
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Arguments
{
    public class AddInt32RangeMapShardsArgs
    {
        [ArgRequired]
        [ArgDescription("Connection string for the shard map manager database.")]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgRequired]
        [ArgDescription("Name of the range shard map.")]
        [ArgShortcut("mn")]
        public string MapName { get; set; }

        [ArgRequired]
        [ArgDescription("Shard locations in the format of Server1|Database1,Server2|Database2")]
        [ArgShortcut("sl")]
        public ShardLocations ShardLocations { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ValidationArgException("Connection string is required for adding Int32 range map shards.");
            }

            if (string.IsNullOrWhiteSpace(MapName))
            {
                throw new ValidationArgException("Map name is required for adding Int32 range map shards.");
            }

            if (ShardLocations?.Locations == null || ShardLocations.Locations.Length < 1)
            {
                throw new ValidationArgException("Shard locations were not provided, are invalid, or are in an incorrect format.");
            }

            var hasNullLocations = ShardLocations.Locations.Any(l => l == null);
            if (hasNullLocations)
            {
                throw new ValidationArgException("One or more shard locations were not provided, are invalid, or are in an incorrect format.");
            }

            var hasInvalidLocations = ShardLocations.Locations
                .Any(l => string.IsNullOrWhiteSpace(l.ServerName) || string.IsNullOrWhiteSpace(l.DatabaseName));
            if (hasInvalidLocations)
            {
                throw new ValidationArgException("One or more shard locations were not provided, are invalid, or are in an incorrect format.");
            }
        }
    }
}
