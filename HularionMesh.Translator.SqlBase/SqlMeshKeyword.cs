#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh;
using System;
using System.Collections.Generic;
using System.Text;

namespace  HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// Keywords used in SqlBase
    /// </summary>
    public class SqlMeshKeyword
    {
        public string Name { get; set; }
        public string Alias { get; set; }

        public static SqlMeshKeyword MeshDomainTable = new SqlMeshKeyword() { Name = "MeshDomainTable", Alias = "MeshDomain" };
        public static SqlMeshKeyword DomainKey = new SqlMeshKeyword() { Name = "DomainKey", Alias = "DomainKey" };
        public static SqlMeshKeyword MeshDomainColumnGenerics = new SqlMeshKeyword() { Name = "MeshDomainColumnGenerics", Alias = "Generics" };
        public static SqlMeshKeyword DomainPropertyTable = new SqlMeshKeyword() { Name = "DomainPropertyTable", Alias = "DomainProperty" };
        public static SqlMeshKeyword DomainPropertyTableName = new SqlMeshKeyword() { Name = "DomainPropertyTableName", Alias = "Name" };
        public static SqlMeshKeyword DomainValueTable = new SqlMeshKeyword() { Name = "DomainValueTable", Alias = "DomainValue" };
        public static SqlMeshKeyword IsLinkDomain = new SqlMeshKeyword() { Name = "IsLinkDomain", Alias = "IsLinkDomain" };
        public static SqlMeshKeyword MeshGenericTable = new SqlMeshKeyword() { Name = "MeshGenericTable", Alias = "MeshGeneric" };
        public static SqlMeshKeyword GenericArguments = new SqlMeshKeyword() { Name = "GenericArguments", Alias = "GenericArguments" };
        public static SqlMeshKeyword GenericArgumentTable = new SqlMeshKeyword() { Name = "GenericArgumentTable", Alias = "GenericTable" };

        public static SqlMeshKeyword ValuePrefix = new SqlMeshKeyword() { Name = "ValuePrefix", Alias = "p_" };
        public static SqlMeshKeyword MetaPrefix = new SqlMeshKeyword() { Name = "MetaPrefix", Alias = "m_" };
        public static SqlMeshKeyword GenericPrefix = new SqlMeshKeyword() { Name = "GenericPrefix", Alias = "g_" };
        public static SqlMeshKeyword WhereParameterPrefix = new SqlMeshKeyword() { Name = "WhereParameterPrefix", Alias = "@w" };
        public static SqlMeshKeyword UpdateParameterPrefix = new SqlMeshKeyword() { Name = "UpdateParameterPrefix", Alias = "@u" };

        public static SqlMeshKeyword CreationTimeColumnName = new SqlMeshKeyword() { Name = "CreationTimeColumnName", Alias = String.Format("{0}{1}", MetaPrefix.Alias, MeshKeyword.ValueCreationTime.Alias) };
        public static SqlMeshKeyword CreationUserColumnName = new SqlMeshKeyword() { Name = "CreationUserColumnName", Alias = String.Format("{0}{1}", MetaPrefix.Alias, MeshKeyword.ValueCreator.Alias) };
        public static SqlMeshKeyword UpdateTimeColumnName = new SqlMeshKeyword() { Name = "UpdateTimeColumnName", Alias = String.Format("{0}{1}", MetaPrefix.Alias, MeshKeyword.ValueUpdateTime.Alias) };
        public static SqlMeshKeyword UpdateUserColumnName = new SqlMeshKeyword() { Name = "UpdateUserColumnName", Alias = String.Format("{0}{1}", MetaPrefix.Alias, MeshKeyword.ValueUpdater.Alias) };
        public static SqlMeshKeyword GenericsColumnName = new SqlMeshKeyword() { Name = "GenericsColumnName", Alias = String.Format("{0}{1}", MetaPrefix.Alias, MeshKeyword.Generics.Alias) };


        public static SqlMeshKeyword SetItemKey = new SqlMeshKeyword() { Name = "SetItemKey", Alias = "ItemKey" };
        public static SqlMeshKeyword SetContainerKey = new SqlMeshKeyword() { Name = "SetContainerKey", Alias = "Container" };
        public static SqlMeshKeyword SetValueColumn = new SqlMeshKeyword() { Name = "SetValueColumn", Alias = "Value" };
        public static SqlMeshKeyword SetDomainReferenceTable = new SqlMeshKeyword() { Name = "SetDomainReferenceTable", Alias = "Domain" };


        public static SqlMeshKeyword KeyNamePart = new SqlMeshKeyword() { Name = "KeyNamePart", Alias = "Name" };
        public static SqlMeshKeyword PropertyTypeOrder = new SqlMeshKeyword() { Name = "PropertyTypeOrder", Alias = "Order" };

        /// <summary>
        /// The reserved name of a table column for databases that require a column when creating a table.
        /// </summary>
        public static SqlMeshKeyword DefaultTableKey = new SqlMeshKeyword() { Name = "DefaultTableKey", Alias = "DefaultTableKey" };


        public static SqlMeshKeyword ISqlTypeKeyMeshType = new SqlMeshKeyword() { Name = "TypeKeyMeshType", Alias = "MeshType" };
        public static SqlMeshKeyword ISqlTypeKeySqlType = new SqlMeshKeyword() { Name = "ISqlTypeKeySqlType", Alias = "SqlType" };
    }
}
