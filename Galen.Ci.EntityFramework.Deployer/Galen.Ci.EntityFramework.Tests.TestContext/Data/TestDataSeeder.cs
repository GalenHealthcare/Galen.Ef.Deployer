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
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Galen.Ci.EntityFramework.Initialization;
using Galen.Ci.EntityFramework.Tests.TestContext.Domain;

namespace Galen.Ci.EntityFramework.Tests.TestContext.Data
{
	public class TestDataSeeder : ISeedData<TestDbContext>
	{
		public void Seed(TestDbContext context)
		{
			context.BasicEntities.AddOrUpdate(e => e.Id,
				new BasicEntity
				{
					Id = 1,
					Name = "Number One",
					Created = DateTime.UtcNow,
					Updated = DateTime.UtcNow
				},
				new BasicEntity
				{
					Id = 2,
					Name = "Number Two",
					Created = DateTime.UtcNow,
					Updated = DateTime.UtcNow
				},
				new BasicEntity
				{
					Id = 3,
					Name = "Number Three",
					Created = DateTime.UtcNow,
					Updated = DateTime.UtcNow
				});
			context.SaveChanges();
		}
	}
}
