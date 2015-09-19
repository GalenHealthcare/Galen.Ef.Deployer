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
using PowerArgs;

namespace Galen.Ci.EntityFramework.Utilities.App.Arguments
{
    public class ExtractArgs : DatabaseConnectionArgs
    {
        [ArgRequired]
        [ArgDescription("Name of the schema")]
        [ArgShortcut("schema")]
        public string SchemaName { get; set; }

        [ArgRequired]
        [ArgDescription("Path to the directory where the deployment history will be extracted.")]
        [ArgShortcut("td")]
        public string TargetDirectory { get; set; }

        [ArgRequired]
        [ArgDescription("Deployment Id to extract.")]
        [ArgShortcut("id")]
        public string DeploymentId { get; set; }

        [ArgDescription("Turns off verification of extracted files hashes.")]
        [ArgShortcut("nv")]
        [ArgDefaultValue(false)]
        public bool NoVerify { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(SchemaName))
            {
                throw new ValidationArgException("Schema name is required.");
            }

            if (string.IsNullOrWhiteSpace(TargetDirectory))
            {
                throw new ValidationArgException("Target directory is required.");
            }

            if (string.IsNullOrWhiteSpace(DeploymentId))
            {
                throw new ValidationArgException("Deployment Id is required.");
            }
        }
    }
}
