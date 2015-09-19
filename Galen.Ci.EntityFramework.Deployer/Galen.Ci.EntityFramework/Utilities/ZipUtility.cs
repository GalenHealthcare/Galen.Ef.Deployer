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
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Galen.Ci.EntityFramework.Utilities
{
    public static class ZipUtility
    {
        private const string ConstDefaultHashName = "MD5";
        private const int StreamReadBufferSize = 4096;
        private const string HashEntryFormat = "{0} *{1}";

        public static readonly string DefaultHashName = ConstDefaultHashName;
        
        public static void ZipRecursive(
            string rootDirectoryPath,
            DirectoryInfo currentDirectoryInfo, 
            ZipArchive archive, 
            TextWriter hashWriter, 
            string hashName = ConstDefaultHashName)
        {
            rootDirectoryPath = NormalizePath(rootDirectoryPath);
            foreach (var file in currentDirectoryInfo.GetFiles())
            {
                var entryName = file.FullName.Substring(rootDirectoryPath.Length);

                using (var reader = file.OpenRead())
                {
                    var hash = AddToArchive(entryName, reader, archive, hashName);
                    hashWriter.WriteLine(HashEntryFormat, hash, entryName);
                    Serilog.Log.Verbose("Added {filePath} to zip archive.  MD5: {md5}", file.FullName, hash);
                }
            }

            // recurse
            foreach (var directory in currentDirectoryInfo.GetDirectories())
            {
                ZipRecursive(rootDirectoryPath, directory, archive, hashWriter, hashName);
            }
        }

        private static string AddToArchive(string entryName, Stream inputStream, ZipArchive zipArchive, string hashName)
        {
            var entry = zipArchive.CreateEntry(entryName);

            HashAlgorithm hashAlgorithm = null;
            BinaryWriter zipEntryWriter = null;
            try
            {
                hashAlgorithm = HashAlgorithm.Create(hashName);
                zipEntryWriter = new BinaryWriter(entry.Open());

                var readBuffer = new byte[StreamReadBufferSize];
                int bytesRead;
                while ((bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    zipEntryWriter.Write(readBuffer, 0, bytesRead);
                    hashAlgorithm.TransformBlock(readBuffer, 0, bytesRead, readBuffer, 0);
                }
                hashAlgorithm.TransformFinalBlock(readBuffer, 0, 0);

                var hashHexStringBuilder = new StringBuilder();
                foreach (byte hashByte in hashAlgorithm.Hash)
                {
                    hashHexStringBuilder.Append(hashByte.ToString("x2"));
                }

                return hashHexStringBuilder.ToString();
            }
            finally
            {
                hashAlgorithm.SafeDispose();
                zipEntryWriter.SafeDispose();
            }
        }

        private static string NormalizePath(string filePath)
        {
            var isEndsWithDirectorySeparator = (filePath.EndsWith(Path.DirectorySeparatorChar.ToString()));
            return isEndsWithDirectorySeparator
                ? filePath
                : filePath + Path.DirectorySeparatorChar;
        }

        public static void Unzip(string targetDirectoryPath, ZipArchive archive)
        {
            foreach (var zipEntry in archive.Entries)
            {
                var outFilePath = Path.Combine(targetDirectoryPath, zipEntry.FullName);
                var outFileInfo = new FileInfo(outFilePath);
                if (!outFileInfo.Directory.Exists)
                {
                    outFileInfo.Directory.Create();
                }

                Stream outFileStream = null;
                Stream inputStream = null;
                try
                {
                    outFileStream = new FileStream(
                        outFilePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None);

                    inputStream = zipEntry.Open();

                    inputStream.CopyTo(outFileStream);
                }
                finally
                {
                    outFileStream.SafeDispose();
                    inputStream.SafeDispose();
                }
            }
        }
    }
}
