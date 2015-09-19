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
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Testing
{
    [Serializable]
    public class MigrationAssertFailedExecption : AssertFailedException
    {
        private const string DefaultMessage = "Migration to {0} failed.";

        public string TargetMigrationId { get; }

        public MigrationAssertFailedExecption(string targetMigrationId)
            : base(string.Format(DefaultMessage, targetMigrationId))
        {
            TargetMigrationId = targetMigrationId;
        }

        public MigrationAssertFailedExecption(string targetMigrationId, string message) : base(message)
        {
            TargetMigrationId = targetMigrationId;
        }

        public MigrationAssertFailedExecption(string targetMigrationId, string message, Exception inner)
            : base(message, inner)
        {
            TargetMigrationId = targetMigrationId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected MigrationAssertFailedExecption(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            TargetMigrationId = info.GetString("TargetMigrationId");
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("TargetMigrationId", TargetMigrationId);

            base.GetObjectData(info, context);
        }
    }
}
