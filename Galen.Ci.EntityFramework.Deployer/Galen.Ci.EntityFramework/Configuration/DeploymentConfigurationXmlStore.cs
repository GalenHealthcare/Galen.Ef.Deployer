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
using Serilog;

namespace Galen.Ci.EntityFramework.Configuration
{
    public class DeploymentConfigurationXmlStore : IDeploymentConfigurationStore
    {
        private readonly string m_FilePath;
        private readonly Stream m_Stream;

        public DeploymentConfigurationXmlStore(string filePath)
        {
            m_FilePath = filePath;
        }

        public DeploymentConfigurationXmlStore(Stream stream)
        {
            m_Stream = stream;
        }

        public DeploymentConfiguration Load()
        {
            Stream stream = null;

            try
            {
                stream = m_Stream ?? new FileStream(m_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof (DeploymentConfiguration));
                return (DeploymentConfiguration) serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to load deployment configuration XML from path {filePath}", m_FilePath);
            }
            finally
            {
                if(m_Stream == null && stream != null) //We own the stream
                    stream.Dispose();
            }
            return null;
        }

        public void Save(DeploymentConfiguration configuration)
        {
            Stream stream = null;

            try
            {
                stream = m_Stream ?? new FileStream(m_FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof (DeploymentConfiguration));
                serializer.Serialize(stream, configuration);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to save deployment configuration XML to path {filePath}", m_FilePath);
                throw;
            }
            finally
            {
                if (m_Stream == null && stream != null) //We own the stream
                    stream.Dispose();
            }
        }
    }
}