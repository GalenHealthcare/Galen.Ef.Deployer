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
using System.Data.SqlClient;
using Galen.Ci.EntityFramework.Utilities.App.Arguments;
using PowerArgs;
using Serilog;

namespace Galen.Ci.EntityFramework.Utilities.App
{
    public class App
    {
        [ArgActionMethod]
        [ArgDescription("Extracts deployment history binaries.")]
        public void Extract(ExtractArgs args)
        {
            args.Validate();

            var connectionString = args.GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Log.Information(
                    LoggingMessageTemplates.Extract, 
                    args.SchemaName, 
                    args.DeploymentId,
                    args.TargetDirectory, 
                    args.DatabaseName, 
                    args.ServerName, 
                    args.NoVerify);

                DeploymentHistory.Extract(
                    args.DeploymentId, 
                    connection, 
                    args.SchemaName, 
                    args.TargetDirectory,
                    args.NoVerify);

                connection.Close();
            }
        }
    }
}
