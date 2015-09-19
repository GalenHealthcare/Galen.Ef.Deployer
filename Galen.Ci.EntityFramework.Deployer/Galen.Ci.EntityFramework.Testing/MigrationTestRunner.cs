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
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;

namespace Galen.Ci.EntityFramework.Testing
{
    public class MigrationTestRunner : IMigrationTestRunner
    {
        private readonly Lazy<Assembly> m_AssemblyUnderTest;

        internal MigrationTestRunner(string assemblyPath)
        {
            m_AssemblyUnderTest = new Lazy<Assembly>(() => Assembly.LoadFrom(assemblyPath));
        }

        private Assembly AssemblyUnderTest => (m_AssemblyUnderTest.Value);

        private DbMigrator GetMigrator(string configurationType, DbConnectionInfo targetDatabase)
        {
            var configuration = (DbMigrationsConfiguration)AssemblyUnderTest.CreateInstance(configurationType);
            if (configuration == null)
            {
                throw new ArgumentException(
                    $"Failed to create an instance of {configurationType} using assembly {AssemblyUnderTest}.",
                    nameof(configurationType));
            }

            configuration.TargetDatabase = targetDatabase;
            return new DbMigrator(configuration);
        }

        private static void ApplyMigration(DbMigrator migrator, string migrationId)
        {
            try
            {
                migrator.Update(migrationId);
            }
            catch (Exception ex)
            {
                var exceptionToThrow = new MigrationAssertFailedExecption(
                    migrationId,
                    $"Failed to migrate to {migrationId}; see inner exception for details.",
                    ex);
                throw exceptionToThrow;
            }
        }

        private static void AssertDatabaseMigrationsAreEqual(
            string appliedMigrationId, 
            IEnumerable<string> expected, 
            IEnumerable<string> actual)
        {
            if (expected.SequenceEqual(actual))
            {
                return;
            }

            throw new MigrationAssertFailedExecption(
                appliedMigrationId,
                $"Expected migrations did not match the actual migrations after migrating to {appliedMigrationId}.");
        }

        public void AssertEachPendingUpwardMigration(string configurationType, DbConnectionInfo targetDatabase)
        {
            var migrator = GetMigrator(configurationType, targetDatabase);
            var migrationsToApply = migrator
                .GetPendingMigrations()
                .OrderBy(migrationId => migrationId);

            var expectedMigrations = new List<string>(migrator
                .GetDatabaseMigrations()
                .OrderBy(migrationId => migrationId));

            foreach (var migrationId in migrationsToApply)
            {
                ApplyMigration(migrator, migrationId);

                expectedMigrations.Add(migrationId);
                var actualMigrations = migrator.GetDatabaseMigrations().OrderBy(actualId => actualId);

                AssertDatabaseMigrationsAreEqual(migrationId, expectedMigrations, actualMigrations);
            }
        }

        public void AssertEachPossibleDownwardMigration(string configurationType, DbConnectionInfo targetDatabase)
        {
            var migrator = GetMigrator(configurationType, targetDatabase);
            var currentMigrationId = migrator
                .GetDatabaseMigrations()
                .OrderBy(migrationId => migrationId)
                .LastOrDefault();

            var migrationsToApply = migrator
                .GetLocalMigrations()
                .OrderByDescending(migrationId => migrationId)
                .SkipWhile(migrationId =>
                    string.Compare(migrationId, currentMigrationId, StringComparison.OrdinalIgnoreCase) >= 0
                )
                .ToList();

            var expectedMigrations = new List<string>(migrationsToApply);

            // this will test migrating to before InitialCreate
            migrationsToApply.Add("0");

            foreach (var migrationId in migrationsToApply)
            {
                ApplyMigration(migrator, migrationId);

                var actualMigrations = migrator.GetDatabaseMigrations().OrderByDescending(actualId => actualId);

                AssertDatabaseMigrationsAreEqual(migrationId, expectedMigrations, actualMigrations);

                expectedMigrations.Remove(migrationId);
            }
        }

        public void AssertMigration(string targetMigrationId, string configurationType, DbConnectionInfo targetDatabase)
        {
            var migrator = GetMigrator(configurationType, targetDatabase);
            ApplyMigration(migrator, targetMigrationId);

            var expectedMigrations = migrator
                .GetLocalMigrations()
                .OrderBy(migrationId => migrationId)
                .TakeWhile(migrationId =>
                    string.Compare(migrationId, targetMigrationId, StringComparison.OrdinalIgnoreCase) < 1
                );

            var actualMigrations = migrator
                .GetDatabaseMigrations()
                .OrderBy(migrationId => migrationId);

            AssertDatabaseMigrationsAreEqual(targetMigrationId, expectedMigrations, actualMigrations);
        }
    }
}
