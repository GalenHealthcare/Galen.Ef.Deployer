﻿#region License
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
using PowerArgs;

namespace Galen.CI.Azure.Sql.Sharding.App.Arguments
{
    public class AddListMapShardArgs
    {
        [ArgRequired]
        [ArgDescription("Connection string for the shard map manager database.")]
        [ArgShortcut("cs")]
        public string ConnectionString { get; set; }

        [ArgRequired]
        [ArgDescription("Name of the list shard map.")]
        [ArgShortcut("mn")]
        public string MapName { get; set; }

        [ArgRequired]
        [ArgDescription("Shard key in the format of Type|Value")]
        [ArgShortcut("sk")]
        public ShardKeyValue ShardKey { get; set; }

        [ArgRequired]
        [ArgDescription("Name of the server that has the database being added as a shard.")]
        [ArgShortcut("ssn")]
        public string ShardServerName { get; set; }

        [ArgRequired]
        [ArgDescription("Name of the database to add as a shard.")]
        [ArgShortcut("sdn")]
        public string ShardDatabaseName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ValidationArgException("Connection string is required for adding a list map shard.");
            }

            if (string.IsNullOrWhiteSpace(MapName))
            {
                throw new ValidationArgException("Map name is required for adding a list map shard.");
            }

            if (ShardKey == null)
            {
                throw new ValidationArgException("Shard key was not provided, is invalid, or is in an incorrect format.");
            }

            if (ShardKey.KeyType == null)
            {
                throw new ValidationArgException("Shard key type was not provided, is invalid, or could not be parsed.");
            }

            if (ShardKey.KeyValue == null)
            {
                throw new ValidationArgException("Shard key value was not provided, is invalid, or could not be parsed.");
            }

            if (string.IsNullOrWhiteSpace(ShardServerName))
            {
                throw new ValidationArgException("Shard server name is required for adding a list map shard.");
            }

            if (string.IsNullOrWhiteSpace(ShardDatabaseName))
            {
                throw new ValidationArgException("Shard database name is required for adding a list map shard.");
            }
        }
    }
}
