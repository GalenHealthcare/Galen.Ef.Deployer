﻿#region License
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
using System.Linq;
using System.Reflection;
using System.Text;

namespace Galen.Ci.EntityFramework.Utilities
{
	public class ParsedAssemblyQualifiedName
	{
		public Lazy<AssemblyName> AssemblyNameDescriptor;
		public Lazy<Type> FoundType;
		public readonly string AssemblyDescriptionString;
		public readonly string TypeName;
		public readonly string ShortAssemblyName;
		public readonly string Version;
		public readonly string Culture;
		public readonly string PublicKeyToken;
		public readonly List<ParsedAssemblyQualifiedName> GenericParameters = new List<ParsedAssemblyQualifiedName>();
		public readonly Lazy<string> CSharpStyleName;
		public readonly Lazy<string> VBNetStyleName;

		public ParsedAssemblyQualifiedName(string AssemblyQualifiedName)
		{
			int index = -1;
			block rootBlock = new block();
			{
				int bcount = 0;
				block currentBlock = rootBlock;
				for (int i = 0; i < AssemblyQualifiedName.Length; ++i)
				{
					char c = AssemblyQualifiedName[i];
					if (c == '[')
					{
						++bcount;
						var b = new block() { iStart = i + 1, level = bcount, parentBlock = currentBlock };
						currentBlock.innerBlocks.Add(b);
						currentBlock = b;
					}
					else if (c == ']')
					{
						currentBlock.iEnd = i - 1;
						if (AssemblyQualifiedName[currentBlock.iStart] != '[')
						{
							currentBlock.parsedAssemblyQualifiedName = new ParsedAssemblyQualifiedName(AssemblyQualifiedName.Substring(currentBlock.iStart, i - currentBlock.iStart));
							if (bcount == 2)
								this.GenericParameters.Add(currentBlock.parsedAssemblyQualifiedName);
						}
						currentBlock = currentBlock.parentBlock;
						--bcount;
					}
					else if (bcount == 0 && c == ',')
					{
						index = i;
						break;
					}
				}
			}
		
			this.TypeName = AssemblyQualifiedName.Substring(0, index);

			this.CSharpStyleName = new Lazy<string>(
				() =>
				{
					return this.LanguageStyle("<", ">");
				});

			this.VBNetStyleName = new Lazy<string>(
				() =>
				{
					return this.LanguageStyle("(Of ", ")");
				});

			this.AssemblyDescriptionString = AssemblyQualifiedName.Substring(index + 2);

			{
				List<string> parts = AssemblyDescriptionString.Split(',')
																 .Select(x => x.Trim())
																 .ToList();
				this.Version = LookForPairThenRemove(parts, "Version");
				this.Culture = LookForPairThenRemove(parts, "Culture");
				this.PublicKeyToken = LookForPairThenRemove(parts, "PublicKeyToken");
				if (parts.Count > 0)
					this.ShortAssemblyName = parts[0];
			}

			this.AssemblyNameDescriptor = new Lazy<AssemblyName>(
				() => new System.Reflection.AssemblyName(this.AssemblyDescriptionString));

			this.FoundType = new Lazy<Type>(
				() =>
				{
					var searchedType = Type.GetType(AssemblyQualifiedName);
					if (searchedType != null)
						return searchedType;
					foreach (var assem in Assemblies.Value)
					{
						searchedType =
							assem.GetType(AssemblyQualifiedName);
						if (searchedType != null)
							return searchedType;
					}
					return null; // Not found.
				});
		}

		internal string LanguageStyle(string prefix, string suffix)
		{
			if (this.GenericParameters.Count > 0)
			{
				StringBuilder sb = new StringBuilder(this.TypeName.Substring(0, this.TypeName.IndexOf('`')));
				sb.Append(prefix);
				bool pendingElement = false;
				foreach (var param in this.GenericParameters)
				{
					if (pendingElement)
						sb.Append(", ");
					sb.Append(param.LanguageStyle(prefix,suffix));
					pendingElement = true;
				}
				sb.Append(suffix);
				return sb.ToString();
			}
			else
				return this.TypeName;
		}
		class block
		{
			internal int iStart;
			internal int iEnd;
			internal int level;
			internal block parentBlock;
			internal List<block> innerBlocks = new List<block>();
			internal ParsedAssemblyQualifiedName parsedAssemblyQualifiedName;
		}

		static string LookForPairThenRemove(List<string> strings, string Name)
		{
			for (int istr = 0; istr < strings.Count; istr++)
			{
				string s = strings[istr];
				int i = s.IndexOf(Name);
				if (i == 0)
				{
					int i2 = s.IndexOf('=');
					if (i2 > 0)
					{
						string ret = s.Substring(i2 + 1);
						strings.RemoveAt(istr);
						return ret;
					}
				}
			}
			return null;
		}

		static readonly Lazy<Assembly[]> Assemblies =
			new Lazy<Assembly[]>(() =>
			AppDomain.CurrentDomain.GetAssemblies());

#if DEBUG
		// Makes debugging easier.
		public override string ToString()
		{
			return this.CSharpStyleName.ToString();
		}
#endif
	}
}
