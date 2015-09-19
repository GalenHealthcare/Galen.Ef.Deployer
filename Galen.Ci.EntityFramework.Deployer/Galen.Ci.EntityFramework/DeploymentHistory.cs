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
using System.Data;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Text;
using Galen.Ci.EntityFramework.Utilities;

namespace Galen.Ci.EntityFramework
{
    public static class DeploymentHistory
    {
        private static readonly string m_HashName = ZipUtility.DefaultHashName;

        public static void Setup(DbConnection connection, string schemaName)
        {
            const string sql =
                "IF (NOT EXISTS ( " +
                    "SELECT 1 " +
                    "FROM INFORMATION_SCHEMA.TABLES " +
                    "WHERE TABLE_SCHEMA = '{0}' " +
                    "AND TABLE_NAME = '__DeploymentHistory')) " +
                "BEGIN " +
                    "CREATE TABLE [{0}].[__DeploymentHistory]( " +
                        "[DeploymentId][NVARCHAR](150) NOT NULL, " +
                        "[ContextKey] [NVARCHAR](300) NOT NULL, " +
                        "[AssemblyFileName] [NVARCHAR](255) NOT NULL, " +
                        "[Binaries] [VARBINARY](MAX) NOT NULL, " +
                        "[Hashes] [VARBINARY](MAX) NOT NULL, " +
                        "[DeployerVersion] [NVARCHAR](32) NOT NULL, " +
                        "CONSTRAINT[PK___DeploymentHistory] PRIMARY KEY CLUSTERED " +
                        "(" +
                            "[DeploymentId] ASC " +
                        ")) " +
                "END";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(sql, schemaName);
                command.ExecuteNonQuery();
            }
        }

        public static void Create(
            string contextKey,
            string deployerVersion,
            string targetAssemblyPath,
            DbConnection connection,
            string schemaName)
        {
            var targetAssemblyFileInfo = new FileInfo(targetAssemblyPath);

            Stream binariesStream = null;
            Stream hashesStream = null;
            byte[] binaries;
            byte[] hashes;
            try
            {
                GetDeploymentBinaries(targetAssemblyFileInfo.Directory, out binariesStream, out hashesStream);

                binaries = new byte[binariesStream.Length];
                binariesStream.Read(binaries, 0, (int)binariesStream.Length);

                hashes = new byte[hashesStream.Length];
                hashesStream.Read(hashes, 0, (int)hashesStream.Length);
            }
            finally
            {
                binariesStream.SafeDispose();
                hashesStream.SafeDispose();
            }

                var assemblyFileName = targetAssemblyFileInfo.Name;
                Save(contextKey, assemblyFileName, binaries, hashes, deployerVersion, connection, schemaName);
            }

        private static void Save(
            string contextKey,
            string assemblyFileName,
            byte[] binaries,
            byte[] hashes,
            string deployerVersion,
            DbConnection connection,
            string schemaName)
        {
            const string sql =
                "INSERT INTO [{0}].[__DeploymentHistory] " +
                    "([DeploymentId] " +
                    ",[ContextKey] " +
                    ",[AssemblyFileName] " +
                    ",[Binaries] " +
                    ",[Hashes] " +
                    ",[DeployerVersion]) " +
                "VALUES " +
                    "(@DeploymentId " +
                    ",@ContextKey " +
                    ",@AssemblyFileName " +
                    ",@Binaries " +
                    ",@Hashes " +
                    ",@DeployerVersion) ";

            var deploymentId = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(sql, schemaName);

                var deploymentIdParam = command.CreateParameter();
                deploymentIdParam.DbType = DbType.String;
                deploymentIdParam.Direction = ParameterDirection.Input;
                deploymentIdParam.ParameterName = "@DeploymentId";
                deploymentIdParam.Value = deploymentId;
                command.Parameters.Add(deploymentIdParam);

                var contextKeyParam = command.CreateParameter();
                contextKeyParam.DbType = DbType.String;
                contextKeyParam.Direction = ParameterDirection.Input;
                contextKeyParam.ParameterName = "@ContextKey";
                contextKeyParam.Value = contextKey;
                command.Parameters.Add(contextKeyParam);

                var assemblyFileNameParam = command.CreateParameter();
                assemblyFileNameParam.DbType = DbType.String;
                assemblyFileNameParam.Direction = ParameterDirection.Input;
                assemblyFileNameParam.ParameterName = "@AssemblyFileName";
                assemblyFileNameParam.Value = assemblyFileName;
                command.Parameters.Add(assemblyFileNameParam);

                var binariesParam = command.CreateParameter();
                binariesParam.DbType = DbType.Binary;
                binariesParam.Direction = ParameterDirection.Input;
                binariesParam.ParameterName = "@Binaries";
                binariesParam.Value = binaries;
                command.Parameters.Add(binariesParam);

                var hashesParam = command.CreateParameter();
                hashesParam.DbType = DbType.Binary;
                hashesParam.Direction = ParameterDirection.Input;
                hashesParam.ParameterName = "@Hashes";
                hashesParam.Value = hashes;
                command.Parameters.Add(hashesParam);

                var deployerVersionParam = command.CreateParameter();
                deployerVersionParam.DbType = DbType.String;
                deployerVersionParam.Direction = ParameterDirection.Input;
                deployerVersionParam.ParameterName = "@DeployerVersion";
                deployerVersionParam.Value = deployerVersion;
                command.Parameters.Add(deployerVersionParam);

                var rows = command.ExecuteNonQuery();
                if (rows != 1)
                {
                    throw new Exception($"Failed to INSERT __DeploymentHistory, result: {rows}");
                }
            }
        }

        private static void GetDeploymentBinaries(
            DirectoryInfo targetAssemblyDirectory,
            out Stream binariesStream,
            out Stream hashesStream)
        {
            ZipArchive zipArchive = null;
            StreamWriter hashesWriter = null;
            try
            {
                binariesStream = new MemoryStream();
                zipArchive = new ZipArchive(binariesStream, ZipArchiveMode.Create, leaveOpen: true);

                hashesStream = new MemoryStream();
                hashesWriter = new StreamWriter(hashesStream, Encoding.UTF8, 1024, leaveOpen: true);

                ZipUtility.ZipRecursive(
                    targetAssemblyDirectory.FullName,
                    targetAssemblyDirectory,
                    zipArchive,
                    hashesWriter,
                    m_HashName);

                hashesWriter.Flush();
                hashesStream.Seek(0, SeekOrigin.Begin);     // rewind the stream for the caller
            }
            finally
            {
                zipArchive.SafeDispose();
                hashesWriter.SafeDispose();
            }

            // for some reason zipArchive isn't fully written to the stream until it is disposed
            // (tried calling Flush on binariesStream and that didn't work)
            // rewind the stream for the caller after the Dispose
            binariesStream.Seek(0, SeekOrigin.Begin);
        }

        private static bool GetIsDeploymentHistoryTableExists(DbConnection connection, string schemaName)
        {
            const string sql =
                "SELECT 1 " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_SCHEMA = @SchemaName " +
                "AND TABLE_NAME = '__DeploymentHistory' ";

            DbCommand command = null;
            try
            {
                command = connection.CreateCommand();
                command.CommandText = sql;

                var schemaNameParam = command.CreateParameter();
                schemaNameParam.DbType = DbType.String;
                schemaNameParam.Direction = ParameterDirection.Input;
                schemaNameParam.ParameterName = "@SchemaName";
                schemaNameParam.Value = schemaName;
                command.Parameters.Add(schemaNameParam);

                var result = (int?)command.ExecuteScalar();

                return (result == 1);
            }
            finally
            {
                command.SafeDispose();
            }
        }

        public static string Extract(
            string deploymentId,
            DbConnection connection,
            string schemaName,
            string extractDirectoryPath,
            bool disableVerification = false)
        {
            const string sql =
                "SELECT " +
                    "   AssemblyFileName " +
                    " , Binaries " +
                    " , Hashes " +
                "FROM [{0}].[__DeploymentHistory] " +
                "WHERE DeploymentId = @DeploymentId ";

            var hasDeploymentHistoryTable = GetIsDeploymentHistoryTableExists(connection, schemaName);
            if (!hasDeploymentHistoryTable)
            {
                return null;
            }

            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                command = connection.CreateCommand();
                command.CommandText = string.Format(sql, schemaName);

                var deploymentIdParam = command.CreateParameter();
                deploymentIdParam.DbType = DbType.String;
                deploymentIdParam.Direction = ParameterDirection.Input;
                deploymentIdParam.ParameterName = "@DeploymentId";
                deploymentIdParam.Value = deploymentId;
                command.Parameters.Add(deploymentIdParam);

                reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return null;
                }

                if (!reader.Read())
                {
                    throw new Exception($"Couldn't read {schemaName} deployment history; deploymentId: {deploymentId}");
                }

                using (var zipArchive = new ZipArchive(reader.GetStream(1), ZipArchiveMode.Read))
                {
                    ZipUtility.Unzip(extractDirectoryPath, zipArchive);
                }

                if (!disableVerification)
                {                    
                    using (var hashesReader = new StreamReader(reader.GetStream(2)))
                    {
                        HashUtility.Verify(extractDirectoryPath, hashesReader, m_HashName);
                    }
                }

                var assemblyFilePath = Path.Combine(extractDirectoryPath, reader.GetString(0));

                reader.Close();

                return assemblyFilePath;
            }
            finally
            {
                reader.SafeDispose();
                command.SafeDispose();
            }
        }

        public static string ExtractCurrent(
            string contextKey, 
            DbConnection connection, 
            string schemaName, 
            string extractDirectoryPath)
        {
            const string sql =
                "SELECT TOP 1 " +
                    "   DeploymentId " +
                    " , ContextKey " +
                    " , AssemblyFileName " +
                    " , Binaries " +
                    " , Hashes " +
                    " , DeployerVersion " +
                "FROM [{0}].[__DeploymentHistory] " +
                "WHERE ContextKey = @ContextKey " +
                "ORDER BY DeploymentId DESC ";

            var hasDeploymentHistoryTable = GetIsDeploymentHistoryTableExists(connection, schemaName);
            if (!hasDeploymentHistoryTable)
            {
                return null;
            }

            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                command = connection.CreateCommand();
                command.CommandText = string.Format(sql, schemaName);

                var contextKeyParam = command.CreateParameter();
                contextKeyParam.DbType = DbType.String;
                contextKeyParam.Direction = ParameterDirection.Input;
                contextKeyParam.ParameterName = "@ContextKey";
                contextKeyParam.Value = contextKey;
                command.Parameters.Add(contextKeyParam);

                reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    return null;
                }

                if (!reader.Read())
                {
                    throw new Exception($"Couldn't read {schemaName} deployment history; contextKey: {contextKey}");
                }

                // extract to a working sub-directory using the deploymentId
                var deploymentId = reader.GetString(0);
                var targetDirectoryPath = Path.Combine(extractDirectoryPath, deploymentId);

                using (var zipArchive = new ZipArchive(reader.GetStream(3), ZipArchiveMode.Read))
                {
                    ZipUtility.Unzip(targetDirectoryPath, zipArchive);
                }

                using (var hashesReader = new StreamReader(reader.GetStream(4)))
                {
                    HashUtility.Verify(targetDirectoryPath, hashesReader, m_HashName);
                }

                var assemblyFilePath = Path.Combine(targetDirectoryPath, reader.GetString(2));

                reader.Close();

                return assemblyFilePath;
            }
            finally
            {
                reader.SafeDispose();
                command.SafeDispose();
            }
        }
    }
}
