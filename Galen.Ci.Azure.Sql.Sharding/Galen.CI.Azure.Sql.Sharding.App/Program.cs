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
using Galen.CI.Azure.Sql.Sharding.App.Arguments;
using PowerArgs;
using Serilog;

namespace Galen.CI.Azure.Sql.Sharding.App
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("");

            ConfigureLogging();

            try
            {
                Args.InvokeAction<App>(args);
                Environment.Exit(0);
            }
            catch (ArgException ex)
            {
                Console.WriteLine(ex.Message);
                ShowUsage(ex.Context.SpecifiedAction);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception in Galen.CI.Azure.Sql.Sharding.App", null);
            }

            Environment.Exit(-1);
        }

        private static void ConfigureLogging()
        {
            var loggingConfig = new LoggerConfiguration().WriteTo.ColoredConsole();

            if (!string.IsNullOrEmpty(StaticConfiguration.LoggingEndpoint))
            {
                loggingConfig.WriteTo.Seq(StaticConfiguration.LoggingEndpoint);
            }

            Log.Logger = loggingConfig.CreateLogger();
        }

        private static void ShowUsage(CommandLineAction specifiedAction)
        {
            if (specifiedAction == null)
            {
                ConsoleString.Write("Supported actions: ", ConsoleColor.Cyan);
                Console.WriteLine("Deploy, CreateListShardMap, CreateRangeShardMap, AddListMapShard, AddRangeMapShard");
                Console.WriteLine("");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine("");
            ConsoleString.WriteLine(specifiedAction.DefaultAlias, ConsoleColor.Yellow);
            ConsoleString.WriteLine(new string('-', 80), ConsoleColor.DarkGray);
            Console.WriteLine("");
            Console.WriteLine(specifiedAction.Description);
            Console.WriteLine("");
            ConsoleString.WriteLine(new string('-', 80), ConsoleColor.DarkGray);
            Console.WriteLine("");

            switch (specifiedAction.DefaultAlias)
            {
                case "Deploy":
                    ArgUsage.GetStyledUsage<DeployArgs>("Deploy action").WriteLine();
                    break;
                case "CreateListShardMap":
                case "CreateRangeShardMap":
                    ArgUsage.GetStyledUsage<CreateShardMapArgs>($"{specifiedAction.DefaultAlias} action").WriteLine();
                    break;
                case "AddListMapShard":
                    ArgUsage.GetStyledUsage<AddListMapShardArgs>("AddListMapShard action").WriteLine();
                    break;
                case "AddRangeMapShard":
                    ArgUsage.GetStyledUsage<AddRangeMapShardArgs>("AddRangeMapShard action").WriteLine();
                    break;
                case "AddInt32RangeMapShards":
                    ArgUsage.GetStyledUsage<AddInt32RangeMapShardsArgs>("AddInt32RangeMapShards action").WriteLine();
                    break;
            }
        }
    }
}
