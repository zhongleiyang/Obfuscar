#region Copyright (c) 2007 Ryan Williams <drcforbin@gmail.com>
/// <copyright>
/// Copyright (c) 2007 Ryan Williams <drcforbin@gmail.com>
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Obfuscar
{
	class Program
	{
		static void ShowHelp()
		{
			Console.WriteLine("Usage:  obfuscar [projectfile]");
		}

		static int Main(string[] args)
		{
			if (args.Length < 1)
			{
				ShowHelp();
				return 1;
			}

			int start = Environment.TickCount;

			try
			{
				Console.Write("Loading project...");
				Obfuscator obfuscator = new Obfuscator(args[0]);
				Console.WriteLine("Done.");

				// The SemanticAttributes of MethodDefinitions have to be loaded before any fields,properties or events are removed
				obfuscator.LoadMethodSemantics();

				Console.Write("Renaming:  fields...");
				obfuscator.RenameFields();

				Console.Write("parameters...");
				obfuscator.RenameParams();

				Console.Write("properties...");
				obfuscator.RenameProperties();

				Console.Write("events...");
				obfuscator.RenameEvents();

				Console.Write("methods...");
				obfuscator.RenameMethods();

				Console.Write("types...");
				obfuscator.RenameTypes();

				if (obfuscator.Project.Settings.HideStrings)
				{
					Console.WriteLine("hiding strings...");
					obfuscator.HideStrings();
				}

				Console.WriteLine("Done.");

				Console.Write("Saving assemblies...");
				obfuscator.SaveAssemblies();
				Console.WriteLine("Done.");

				Console.Write("Writing log file...");
				obfuscator.SaveMapping();
				Console.WriteLine("Done.");

				Console.WriteLine("Completed, {0:f2} secs.", (Environment.TickCount - start) / 1000.0);
			}
			catch (ApplicationException e)
			{
				Console.WriteLine();
				Console.Error.WriteLine("An error occurred during processing:");
				Console.Error.WriteLine(e.Message);
				if (e.InnerException != null)
					Console.Error.WriteLine(e.InnerException.Message);
				return 1;
			}

			return 0;
		}
	}
}
