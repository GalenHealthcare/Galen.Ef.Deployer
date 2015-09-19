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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Galen.Ci.EntityFramework.Deployer.Tests
{
	[TestClass]
	public class DatabaseConnectionInfoArgReviverTests
	{
		[TestMethod]
		public void ShouldReviveSingleServerDatabasePair()
		{
			string argument = "server1|database1";
			var expected = new[] { new DatabaseEndpoint() { ServerName="server1", DatabaseName="database1" } };
			var actual = DatabaseEndpointArgReviver.Revive(string.Empty, argument);

			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual.Endpoints);
			Assert.IsTrue(actual.Endpoints.Length==1);
			Assert.AreEqual(expected[0].ServerName, actual.Endpoints[0].ServerName);
			Assert.AreEqual(expected[0].DatabaseName, actual.Endpoints[0].DatabaseName);
		}

		[TestMethod]
		public void ShouldReviveMultipleServerDatabasePairs()
		{
			string argument = "server1|database1,server2|database1,server2|database2";
			var expected = new[]
			{
				new DatabaseEndpoint() { ServerName = "server1", DatabaseName = "database1" },
				new DatabaseEndpoint() { ServerName = "server2", DatabaseName = "database1" },
				new DatabaseEndpoint() { ServerName = "server2", DatabaseName = "database2" },
			};
			var actual = DatabaseEndpointArgReviver.Revive(string.Empty, argument);

			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual.Endpoints);
			Assert.IsTrue(actual.Endpoints.Length==3);

			for (int i = 0; i<expected.Length; i++)
			{
				Assert.AreEqual(expected[i].ServerName, actual.Endpoints[i].ServerName);
				Assert.AreEqual(expected[i].DatabaseName, actual.Endpoints[i].DatabaseName);
			}
		}

		[TestMethod]
		public void ShouldReturnAnEmptyArrayForNullOrEmptyArgumentStrings()
		{
			var actual = DatabaseEndpointArgReviver.Revive(string.Empty, null);
			Assert.IsTrue(actual.Endpoints.Length==0);

			actual=DatabaseEndpointArgReviver.Revive(string.Empty, string.Empty);
			Assert.IsTrue(actual.Endpoints.Length==0);
		}

		[TestMethod]
		public void ShouldReturnAnEmptyArrayForArgumentsWithNoPairSeperator()
		{
			var actual = DatabaseEndpointArgReviver.Revive(string.Empty, "servername,database");
			Assert.IsTrue(actual.Endpoints.Length==0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldThrowArgumentExceptionForMissingPairParts()
		{
			var actual = DatabaseEndpointArgReviver.Revive(string.Empty, "servername|");
		}
	}
}
