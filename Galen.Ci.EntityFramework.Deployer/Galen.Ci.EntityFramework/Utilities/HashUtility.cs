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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Galen.Ci.EntityFramework.Utilities
{
    public static class HashUtility
    {
        private const int StreamReadBufferSize = 4096;
        private static readonly char[] m_WhitespaceChars = {' ', '\t'};

        public static void Verify(string targetDirectoryPath, StreamReader hashesReader, string hashName)
        {
            while (!hashesReader.EndOfStream)
            {
                string entry = hashesReader.ReadLine();

                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                if (IsComment(entry))
                {
                    continue;
                }

                byte[] expectedHash;
                string filePath;
                ParseEntry(entry, out expectedHash, out filePath);

                var fullFilePath = Path.Combine(targetDirectoryPath, filePath);
                Verify(fullFilePath, expectedHash, hashName);
            }
        }

        private static bool IsComment(string entry)
        {
            return (entry.StartsWith("#"));
        }

        private static void ParseEntry(string entry, out byte[] hash, out string filePath)
        {
            var endHashIndex = entry.IndexOfAny(m_WhitespaceChars);
            var beginFileNameIndex = entry.IndexOf('*') + 1;

            var hashString = entry.Substring(0, endHashIndex);
            hash = GetByteArrayFromHexString(hashString);
            filePath = entry.Substring(beginFileNameIndex);
        }

        private static byte[] GetByteArrayFromHexString(string hex)
        {
            // http://stackoverflow.com/a/321404/3541813
            return Enumerable
                .Range(0, hex.Length)
                .Where(i => i % 2 == 0)
                .Select(i => Convert.ToByte(hex.Substring(i, 2), 16))
                .ToArray();
        }

        private static string GetHexString(byte[] hash)
        {
            var hexStringBuilder = new StringBuilder();
            foreach (byte hashByte in hash)
            {
                hexStringBuilder.Append(hashByte.ToString("x2"));
            }
            return hexStringBuilder.ToString();
        }

        private static void Verify(string filePath, byte[] expectedHash, string hashName)
        {
            HashAlgorithm hashAlgorithm = null;
            Stream inputStream = null;
            try
            {
                hashAlgorithm = HashAlgorithm.Create(hashName);
                inputStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);

                var readBuffer = new byte[StreamReadBufferSize];
                int bytesRead;
                while ((bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    hashAlgorithm.TransformBlock(readBuffer, 0, bytesRead, readBuffer, 0);
                }
                hashAlgorithm.TransformFinalBlock(readBuffer, 0, 0);

                var isHashMatches = hashAlgorithm.Hash.SequenceEqual(expectedHash);
                if (!isHashMatches)
                {
                    var expectedHashString = GetHexString(expectedHash);
                    var actualHashString = GetHexString(hashAlgorithm.Hash);
                    throw new Exception(
                        $"Checksum for {filePath} does not match.  Expected: {expectedHashString}, Actual: {actualHashString}");
                }
            }
            finally
            {
                inputStream.SafeDispose();
                hashAlgorithm.SafeDispose();
            }
        }
    }
}
