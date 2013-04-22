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
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Obfuscar
{
	class AssemblyInfo
	{
		private readonly Project project;

		private readonly PredicateCollection<string> skipNamespaces = new PredicateCollection<string>();
		private readonly PredicateCollection<TypeKey> skipTypes = new PredicateCollection<TypeKey>();
		private readonly PredicateCollection<MethodKey> skipMethods = new PredicateCollection<MethodKey>();
		private readonly PredicateCollection<FieldKey> skipFields = new PredicateCollection<FieldKey>();
		private readonly PredicateCollection<PropertyKey> skipProperties = new PredicateCollection<PropertyKey>();
		private readonly PredicateCollection<EventKey> skipEvents = new PredicateCollection<EventKey>();
		private readonly PredicateCollection<MethodKey> skipStringHiding = new PredicateCollection<MethodKey>();

		private readonly List<AssemblyInfo> references = new List<AssemblyInfo>();
		private readonly List<AssemblyInfo> referencedBy = new List<AssemblyInfo>();

		private List<TypeReference> unrenamedTypeReferences;
		private List<MemberReference> unrenamedReferences;

		private string filename;

		private AssemblyDefinition definition;
		private string name;

		bool initialized = false;

		// to create, use FromXml
		private AssemblyInfo(Project project)
		{
			this.project = project;
		}

		private static bool AssemblyIsSigned(AssemblyDefinition def)
		{
			if (def.Name.PublicKeyToken != null && def.Name.Hash != null)
				return Array.Exists(def.Name.Hash, delegate(byte b) { return b != 0; });
			else
				return false;
		}

		public static AssemblyInfo FromXml(Project project, XmlReader reader, Variables vars)
		{
			Debug.Assert(reader.NodeType == XmlNodeType.Element && reader.Name == "Module");

			AssemblyInfo info = new AssemblyInfo(project);

			// pull out the file attribute, but don't process anything empty
			string val = Helper.GetAttribute(reader, "file", vars);
			if (val.Length > 0)
			{
				info.LoadAssembly(val);

				if (AssemblyIsSigned(info.Definition) && project.Settings.KeyFile == null)
					throw new ApplicationException("Obfuscating a signed assembly would result in an invalid assembly:  " + info.Name + "; use the KeyFile property to set a key to use");
			}
			else
				throw new InvalidOperationException("Need valid file attribute.");

			if (!reader.IsEmptyElement)
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "SkipNamespace":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									if (val.Length > 0)
										info.skipNamespaces.Add(new NamespaceTester(val));
								}
								break;
							case "SkipType":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									if (val.Length > 0)
									{
										string typeName = val;

										TypeSkipFlags skipFlags = TypeSkipFlags.SkipNone;

										val = Helper.GetAttribute(reader, "skipMethods", vars);
										if (val.Length > 0 && XmlConvert.ToBoolean(val))
											skipFlags |= TypeSkipFlags.SkipMethod;

										val = Helper.GetAttribute(reader, "skipStringHiding", vars);
										if (val.Length > 0 && XmlConvert.ToBoolean(val))
											skipFlags |= TypeSkipFlags.SkipStringHiding;

										val = Helper.GetAttribute(reader, "skipFields", vars);
										if (val.Length > 0 && XmlConvert.ToBoolean(val))
											skipFlags |= TypeSkipFlags.SkipField;

										val = Helper.GetAttribute(reader, "skipProperties", vars);
										if (val.Length > 0 && XmlConvert.ToBoolean(val))
											skipFlags |= TypeSkipFlags.SkipProperty;

										val = Helper.GetAttribute(reader, "skipEvents", vars);
										if (val.Length > 0 && XmlConvert.ToBoolean(val))
											skipFlags |= TypeSkipFlags.SkipEvent;

										info.skipTypes.Add(new TypeTester(typeName, skipFlags, attrib));
									}
								}
								break;
							case "SkipMethod":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string type = Helper.GetAttribute(reader, "type", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									string typeattrib = Helper.GetAttribute(reader, "typeattrib", vars);

									if (val.Length > 0)
										info.skipMethods.Add(new MethodTester(val, type, attrib, typeattrib));
									else
									{
										val = Helper.GetAttribute(reader, "rx");
										if (val.Length > 0)
											info.skipMethods.Add(new MethodTester(new Regex(val), type, attrib, typeattrib));
									}
								}
								break;
							case "SkipStringHiding":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string type = Helper.GetAttribute(reader, "type", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									string typeattrib = Helper.GetAttribute(reader, "typeattrib", vars);


									if (val.Length > 0)
										info.skipStringHiding.Add(new MethodTester(val, type, attrib, typeattrib));
									else
									{
										val = Helper.GetAttribute(reader, "rx");
										if (val.Length > 0)
											info.skipStringHiding.Add(new MethodTester(new Regex(val), type, attrib, typeattrib));
									}
								}
								break;
							case "SkipField":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string type = Helper.GetAttribute(reader, "type", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									string typeattrib = Helper.GetAttribute(reader, "typeattrib", vars);

									if (val.Length > 0)
										info.skipFields.Add(new FieldTester(val, type, attrib, typeattrib));
									else
									{
										val = Helper.GetAttribute(reader, "rx");
										if (val.Length > 0)
											info.skipFields.Add(new FieldTester(new Regex(val), type, attrib, typeattrib));
									}
								}
								break;
							case "SkipProperty":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string type = Helper.GetAttribute(reader, "type", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									string typeattrib = Helper.GetAttribute(reader, "typeattrib", vars);

									if (val.Length > 0)
										info.skipProperties.Add(new PropertyTester(val, type, attrib, typeattrib));
									else
									{
										val = Helper.GetAttribute(reader, "rx");
										if (val.Length > 0)
											info.skipProperties.Add(new PropertyTester(new Regex(val), type, attrib, typeattrib));
									}
								}
								break;
							case "SkipEvent":
								{
									val = Helper.GetAttribute(reader, "name", vars);
									string type = Helper.GetAttribute(reader, "type", vars);
									string attrib = Helper.GetAttribute(reader, "attrib", vars);
									string typeattrib = Helper.GetAttribute(reader, "typeattrib", vars);

									if (val.Length > 0)
										info.skipEvents.Add(new EventTester(val, type, attrib, typeattrib));
									else
									{
										val = Helper.GetAttribute(reader, "rx");
										if (val.Length > 0)
											info.skipEvents.Add(new EventTester(new Regex(val), type, attrib, typeattrib));
									}
								}
								break;
						}
					}
					else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Module")
					{
						// hit end of module element...stop reading
						break;
					}
				}
			}

			return info;
		}

		/// <summary>
		/// Called by project to finish initializing the assembly.
		/// </summary>
		internal void Init()
		{
			unrenamedReferences = new List<MemberReference>();
			foreach (MemberReference member in getMemberReferences())
			{
				MethodReference mr = member as MethodReference;
				FieldReference fr = member as FieldReference;
				if (project.Contains(member.DeclaringType))
					unrenamedReferences.Add(member);
			}

			C5.HashSet<TypeReference> typerefs = new C5.HashSet<TypeReference>();
			foreach (TypeReference type in definition.MainModule.GetTypeReferences())
			{
				if (type.FullName == "<Module>")
					continue;

				if (project.Contains(type))
					typerefs.Add(type);
			}

			// Type references in CustomAttributes
			List<CustomAttribute> customattributes = new List<CustomAttribute>();
			customattributes.AddRange(this.Definition.CustomAttributes);
			foreach (TypeDefinition type in GetAllTypeDefinitions())
			{

				customattributes.AddRange(type.CustomAttributes);
				foreach (MethodDefinition methoddef in type.Methods)
					customattributes.AddRange(methoddef.CustomAttributes);
				foreach (FieldDefinition fielddef in type.Fields)
					customattributes.AddRange(fielddef.CustomAttributes);
				foreach (EventDefinition eventdef in type.Events)
					customattributes.AddRange(eventdef.CustomAttributes);
				foreach (PropertyDefinition propertydef in type.Properties)
					customattributes.AddRange(propertydef.CustomAttributes);

				foreach (CustomAttribute customattribute in customattributes)
				{
					// Check Constructor and named parameter for argument of type "System.Type". i.e. typeof()
					List<CustomAttributeArgument> customattributearguments = new List<CustomAttributeArgument>();
					customattributearguments.AddRange(customattribute.ConstructorArguments);
					foreach (CustomAttributeNamedArgument namedargument in customattribute.Properties)
						customattributearguments.Add(namedargument.Argument);

					foreach (CustomAttributeArgument ca in customattributearguments)
					{
						if (ca.Type.FullName == "System.Type")
							typerefs.Add((TypeReference)ca.Value);
					}
				}
				customattributes.Clear();
			}

			unrenamedTypeReferences = new List<TypeReference>(typerefs);

			initialized = true;
		}

		public IEnumerable<TypeDefinition> GetAllTypeDefinitions()
		{
			foreach (TypeDefinition typedef in definition.MainModule.Types)
			{
				yield return typedef;
				foreach (TypeDefinition nestedtypedef in typedef.NestedTypes)
					yield return nestedtypedef;
			}
		}

		IEnumerable<MemberReference> getMemberReferences()
		{
			C5.HashSet<MemberReference> memberreferences = new C5.HashSet<MemberReference>();
			foreach (TypeDefinition type in this.GetAllTypeDefinitions())
			{
				foreach (MethodDefinition method in type.Methods)
				{
					if (method.Body != null)
					{
						foreach (Instruction inst in method.Body.Instructions)
						{
							MemberReference memberref = inst.Operand as MemberReference;
							if (memberref != null)
							{
								if (memberref is MethodReference && !(memberref is MethodDefinition || memberref is MethodSpecification || memberref is CallSite)
									|| memberref is FieldReference && !(memberref is FieldDefinition))
								{
									int c = memberreferences.Count;
									memberreferences.Add(memberref);
								}
							}
						}
					}
				}
			}
			return memberreferences;
		}

		IEnumerable<TypeReference> getTypeReferences()
		{
			List<TypeReference> typereferences = new List<TypeReference>();
			foreach (TypeDefinition type in this.GetAllTypeDefinitions())
			{
				foreach (MethodDefinition method in type.Methods)
				{
					if (method.Body != null)
					{
						foreach (Instruction inst in method.Body.Instructions)
						{
							TypeReference typeref = inst.Operand as TypeReference;
							if (typeref != null)
							{
								if (!(typeref is TypeDefinition) && !(typeref is TypeSpecification))
									typereferences.Add(typeref);
							}
						}
					}
				}
			}
			return typereferences;
		}

		private void LoadAssembly(string filename)
		{
			this.filename = filename;

			try
			{
				definition = AssemblyDefinition.ReadAssembly(filename);
				name = definition.Name.Name;
			}
			catch (System.IO.FileNotFoundException e)
			{
				throw new ApplicationException("Unable to find assembly:  " + filename, e);
			}
		}

		public string Filename
		{
			get
			{
				CheckLoaded();
				return filename;
			}
		}

		public AssemblyDefinition Definition
		{
			get
			{
				CheckLoaded();
				return definition;
			}
		}

		public string Name
		{
			get
			{
				CheckLoaded();
				return name;
			}
		}

		public List<MemberReference> UnrenamedReferences
		{
			get
			{
				CheckInitialized();
				return unrenamedReferences;
			}
		}

		public List<TypeReference> UnrenamedTypeReferences
		{
			get
			{
				CheckInitialized();
				return unrenamedTypeReferences;
			}
		}

		public List<AssemblyInfo> References
		{
			get { return references; }
		}

		public List<AssemblyInfo> ReferencedBy
		{
			get { return referencedBy; }
		}

		public void ForceSkip(MethodKey method)
		{
			skipMethods.Add(new MethodTester(method));
		}

		public bool ShouldSkip(string ns)
		{
			return skipNamespaces.IsMatch(ns);
		}

		public bool ShouldSkip(TypeKey type, TypeSkipFlags flag)
		{
			if (ShouldSkip(type.Namespace))
				return true;

			foreach (TypeTester typeTester in skipTypes)
			{
				if ((typeTester.SkipFlags & flag) > 0 && typeTester.Test(type))
					return true;
			}

			return false;
		}

		public bool ShouldSkip(TypeKey type)
		{
			if (ShouldSkip(type.Namespace))
				return true;

			return skipTypes.IsMatch(type);
		}

		public bool ShouldSkip(MethodKey method)
		{
			if (ShouldSkip(method.TypeKey, TypeSkipFlags.SkipMethod))
				return true;

			return skipMethods.IsMatch(method);
		}

		public bool ShouldSkipStringHiding(MethodKey method)
		{
			if (ShouldSkip(method.TypeKey, TypeSkipFlags.SkipStringHiding))
				return true;

			return skipStringHiding.IsMatch(method);
		}

		public bool ShouldSkip(FieldKey field)
		{
			if (ShouldSkip(field.TypeKey, TypeSkipFlags.SkipField))
				return true;

			return skipFields.IsMatch(field);
		}

		public bool ShouldSkip(PropertyKey prop)
		{
			if (ShouldSkip(prop.TypeKey, TypeSkipFlags.SkipProperty))
				return true;

			return skipProperties.IsMatch(prop);
		}

		public bool ShouldSkip(EventKey evt)
		{
			if (ShouldSkip(evt.TypeKey, TypeSkipFlags.SkipEvent))
				return true;

			return skipEvents.IsMatch(evt);
		}

		/// <summary>
		/// Makes sure that the assembly definition has been loaded (by <see cref="LoadAssembly"/>).
		/// </summary>
		private void CheckLoaded()
		{
			if (definition == null)
				throw new InvalidOperationException("Expected that AssemblyInfo.LoadAssembly would be called before use.");
		}

		/// <summary>
		/// Makes sure that the assembly has been initialized (by <see cref="Init"/>).
		/// </summary>
		private void CheckInitialized()
		{
			if (!initialized)
				throw new InvalidOperationException("Expected that AssemblyInfo.Init would be called before use.");
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
