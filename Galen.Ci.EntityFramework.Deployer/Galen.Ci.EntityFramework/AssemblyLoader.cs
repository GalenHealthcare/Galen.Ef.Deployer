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
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace Galen.Ci.EntityFramework
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private readonly ConcurrentDictionary<string, MigrationsSource> m_Sources = new ConcurrentDictionary<string, MigrationsSource>();
        private string m_DeployedPath;
        private string m_TargetPath;

        public AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDependentAssembly;
        }

        ~AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveDependentAssembly;
        }

        private Assembly ResolveDependentAssembly(object sender, ResolveEventArgs args)
        {
            MigrationsSource source;

            if (!args.Name.Contains(".resource") && args.RequestingAssembly == null)
            {
                //Ignore resource file requests
                //If we get here, an assembly that we resolved manually is now requesting another assembly
                //In some caes, the RequestingAssembly will be null - but we *should* already have this assembly in memory
                //So just try and retun an existing one.
                return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == args.Name);
            }

            if (m_Sources.TryGetValue(BuildAssemblyId(args.RequestingAssembly), out source))
            {
                var assemblyName = new AssemblyName(args.Name);

                string targetPath = Path.Combine(
                    source == MigrationsSource.Deployed ? m_DeployedPath : m_TargetPath, string.Format("{0}.dll", assemblyName.Name));

                assemblyName.CodeBase = targetPath;

                Log.Debug(
                    "Assembly loader {loaderHashCode} searching for dependent assembly {assemblyName} requested by migration sourced from {MigrationsSource} at {targetPath}. Request made by assembly {requestingAssembly}.",
                    this.GetHashCode(), args.Name, source, targetPath, args.RequestingAssembly.CodeBase);

                //We have to use LoadFile here, otherwise we won't load a differing
                //version, regardless of the codebase because only LoadFile
                //will actually load a *new* assembly if it's at a different path
                //See: http://msdn.microsoft.com/en-us/library/b61s44e8(v=vs.110).aspx
                var dependentAssembly = Assembly.LoadFile(assemblyName.CodeBase);
                m_Sources.TryAdd(BuildAssemblyId(dependentAssembly), source);

                Log.Debug(
                    "Assembly loader {loaderHashCode} found ependent assembly {assemblyName} requested by migration sourced from {MigrationsSource} at {targetPath}. Request made by assembly {requestingAssembly}.",
                    this.GetHashCode(), args.Name, source, targetPath, args.RequestingAssembly.CodeBase);

                return dependentAssembly;
            }

            return null;
        }

        public Assembly Load(MigrationsSource source, string filePath)
        {
            try
            {
                var assembly = Assembly.LoadFile(filePath);
                var rootPath = Path.GetDirectoryName(filePath);

                if (source == MigrationsSource.Target)
                {
                    m_TargetPath = rootPath;
                }
                else
                {
                    m_DeployedPath = rootPath;
                }
                    
                m_Sources.TryAdd(BuildAssemblyId(assembly), source);
                return assembly;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unablet to load assembly from path {filePath}", filePath);
            }

            return null;
        }

        private string BuildAssemblyId(Assembly assembly)
        {
            return
                Convert.ToBase64String(
                    HashAlgorithm.Create("SHA1")
                        .ComputeHash(UTF8Encoding.UTF8.GetBytes(
                            string.Format("{0}|{1}", assembly.FullName, assembly.Location))));
        }
    }
}