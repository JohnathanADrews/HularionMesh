#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HularionMesh
{
    /// <summary>
    /// Keywords in Mesh.
    /// </summary>
    public class MeshKeyword
    {

        public string Name { get; set; }
        public string Alias { get; set; }


        public static MeshKeyword Key = new MeshKeyword() { Name = "Key", Alias = "Key" };
        public static MeshKeyword LinkPrefix = new MeshKeyword() { Name = "LinkPrefix", Alias = ">-<" };
        public static MeshKeyword Values = new MeshKeyword() { Name = "Values", Alias = "Values" };
        public static MeshKeyword Meta = new MeshKeyword() { Name = "Meta", Alias = "Meta" };
        public static MeshKeyword Link = new MeshKeyword() { Name = "Link", Alias = "Link" };
        public static MeshKeyword LinkAMember = new MeshKeyword() { Name = "AMember", Alias = "AMember" };
        public static MeshKeyword LinkBMember = new MeshKeyword() { Name = "BMember", Alias = "BMember" };
        public static MeshKeyword ValueCreationTime = new MeshKeyword() { Name = "CreationTime", Alias = "CreationTime" };
        public static MeshKeyword ValueCreator = new MeshKeyword() { Name = "Creator", Alias = "Creator" };
        public static MeshKeyword ValueUpdateTime = new MeshKeyword() { Name = "UpdateTime", Alias = "UpdateTime" };
        public static MeshKeyword ValueUpdater = new MeshKeyword() { Name = "Updater", Alias = "Updater" };
        public static MeshKeyword ObjectPrefix = new MeshKeyword() { Name = "Object", Alias = "Object" };
        public static MeshKeyword Generics = new MeshKeyword() { Name = "Generics", Alias = "Generics" };

        //Domain Link keywords.
        public static MeshKeyword SMember = new MeshKeyword() { Name = "SMember", Alias = "SMember" };
        public static MeshKeyword TMember = new MeshKeyword() { Name = "TMember", Alias = "TMember" };
        public static MeshKeyword SKey = new MeshKeyword() { Name = "SKey", Alias = "SKey" };
        public static MeshKeyword TKey = new MeshKeyword() { Name = "TKey", Alias = "TKey" };
        public static MeshKeyword LinkFromMember = new MeshKeyword() { Name = "LinkFromMember", Alias = "LinkFrom" };
        public static MeshKeyword LinkToMember = new MeshKeyword() { Name = "LinkToMember", Alias = "LinkTo" };

        //the keyword for items in a Set domain.
        public static MeshKeyword UniqueSetDomainItems = new MeshKeyword() { Name = "Items", Alias = "Items" };
        //the keyword for items in a KeyValuePair domain.
        public static MeshKeyword KeyValuePairDomainKey = new MeshKeyword() { Name = "KeyValuePairDomainKey", Alias = "Key" };
        public static MeshKeyword KeyValuePairDomainValue = new MeshKeyword() { Name = "KeyValuePairDomainValue", Alias = "Value" };


        public static PropertyInfo ArrayPropertyLength = typeof(Array).GetProperty("Length");
        public static MeshKeyword ListMethodAddRange = new MeshKeyword() { Name = "AddRange", Alias = "AddRange" };
        public static MeshKeyword ListMethodAdd = new MeshKeyword() { Name = "Add", Alias = "Add" };
        public static MeshKeyword ListMethodToArray = new MeshKeyword() { Name = "ListMethodToArray", Alias = "ToArray" };

        public static MeshKeyword DomainFriendlyName = new MeshKeyword() { Name = "DomainFriendlyName", Alias = "FriendlyName" };

        public static MeshKeyword SystemRealmKeyword = new MeshKeyword() { Name = "SystemRealmKeyword", Alias = "System" };


        public static MeshKeyword TypeNameGenericDelimiter = new MeshKeyword() { Name = "TypeNameGenericDelimiter", Alias = "`" };

        public static IEnumerable<MeshKeyword> Enumeration = new MeshKeyword[] { Key };




        public override string ToString()
        {
            return Alias;
        }
    }
}
