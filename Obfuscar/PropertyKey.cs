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

using Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Obfuscar
{
	class PropertyKey
	{
		readonly TypeKey typeKey;
		readonly string type;
		readonly string name;
		readonly PropertyDefinition propertyDefinition;

		public PropertyKey(TypeKey typeKey, PropertyDefinition prop)
		{
			this.typeKey = typeKey;
			this.type = prop.PropertyType.FullName;
			this.name = prop.Name;
			this.propertyDefinition = prop;
		}

		public TypeKey TypeKey
		{
			get { return typeKey; }
		}

		public string Type
		{
			get { return type; }
		}

		public string Name
		{
			get { return name; }
		}

		public MethodAttributes GetterMethodAttributes
		{
			get { return propertyDefinition.GetMethod != null ? propertyDefinition.GetMethod.Attributes : 0; }
		}

		public TypeDefinition DeclaringType
		{
			get { return (TypeDefinition)propertyDefinition.DeclaringType; }
		}

		public virtual bool Matches(MemberReference member)
		{
			PropertyReference propRef = member as PropertyReference;
			if (propRef != null)
			{
				if (typeKey.Matches(propRef.DeclaringType))
					return type == propRef.PropertyType.FullName && name == propRef.Name;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			PropertyKey key = obj as PropertyKey;
			if (key == null)
				return false;

			return this == key;
		}

		public static bool operator ==(PropertyKey a, PropertyKey b)
		{
			if ((object)a == null)
				return (object)b == null;
			else if ((object)b == null)
				return false;
			else
				return a.typeKey == b.typeKey && a.type == b.type && a.name == b.name;
		}

		public static bool operator !=(PropertyKey a, PropertyKey b)
		{
			if ((object)a == null)
				return (object)b != null;
			else if ((object)b == null)
				return true;
			else
				return a.typeKey != b.typeKey || a.type != b.type || a.name != b.name;
		}

		public override int GetHashCode()
		{
			return typeKey.GetHashCode() ^ type.GetHashCode() ^ name.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("[{0}]{1} {2}::{3}", typeKey.Scope, type, typeKey.Fullname, name);
		}
	}
}
