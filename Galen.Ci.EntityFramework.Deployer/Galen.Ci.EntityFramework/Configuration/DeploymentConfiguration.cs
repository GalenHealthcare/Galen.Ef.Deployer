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
using System.Xml.Serialization;

namespace Galen.Ci.EntityFramework.Configuration
{
    [XmlRoot("DeploymentConfiguration", Namespace = "")]
    public class DeploymentConfiguration
    {
        [XmlElement("MigrationConfiguration")]
        public MigrationConfigurationInfo MigrationConfigurationInfo { get; set; }

        [XmlElement("InitializerConfiguration")]
        public InitializerConfigurationInfo InitializerConfigurationInfo { get; set; }
    }

    public class MigrationConfigurationInfo
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string ContextType { get; set; }
    }

    public class InitializerConfigurationInfo
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public bool DisableForcedSeeding { get; set; }

        [XmlElement("ServiceAccount")]
        public ServiceAccountInfo ServiceAccount { get; set; }
        
    }

    public class ServiceAccountInfo
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Domain { get; set; }

        [XmlAttribute]
        public string DatabaseUser { get; set; }

        [XmlAttribute]
        public string DatabaseUserPassword { get; set; }

        [XmlAttribute]
        public string AccountType { get; set; }
    }
}
