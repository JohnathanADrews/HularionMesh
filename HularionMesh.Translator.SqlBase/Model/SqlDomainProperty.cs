#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.Model
{
    /// <summary>
    /// Domain property represented in a SQL-compatible form.
    /// </summary>
    [Table("DomainProperty")]
    public class SqlDomainProperty
    {
        /// <summary>
        /// The key of the domain for which this value is set.
        /// </summary>
        [ColumnAttribute("DomainKey")]
        public string DomainKey { get; set; }

        /// <summary>
        /// The key of the property.
        /// </summary>
        [ColumnAttribute("Key")]
        [PrimaryKeyAttribute]
        //public string Key { get { return MeshKey.Parse(DomainKey).Append(MeshKey.Parse(Name)).Serialized; } set { return; } }
        public string Key { get { return MeshKey.Parse(DomainKey).SetPart(SqlMeshKeyword.KeyNamePart.Alias, Name).SetPart(SqlMeshKeyword.PropertyTypeOrder.Alias, String.Format("{0}", MultiTypeOrder)).Serialized; } set { return; } }

        /// <summary>
        /// The name of the property.
        /// </summary>
        [ColumnAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the property.
        /// </summary>
        [ColumnAttribute("Type")]
        public string Type { get; set; }

        /// <summary>
        /// The order number of this property if it is a multi-type property.
        /// </summary>
        [ColumnAttribute("MultiTypeOrder")]
        public int MultiTypeOrder { get; set; }

        /// <summary>
        /// The SQL type of the property.
        /// </summary>
        [ColumnAttribute("SqlType")]
        public string SqlType { get; set; }

        /// <summary>
        /// The default Value of the property serialized to a string.
        /// </summary>
        [ColumnAttribute("Default")]
        public string Default { get; set; }

        /// <summary>
        /// Describes whether this value is a proxy for the Domain's Key or a Meta value.
        /// </summary>
        [ColumnAttribute("ValuePropertyProxy")]
        public string ValuePropertyProxy { get; set; }

        /// <summary>
        /// Contains the generic types serialized to a string.
        /// </summary>
        [ColumnAttribute("Generics")]
        public string Generics { get; set; }

        /// <summary>
        /// True iff the type of this property is set by a generic argument of the domain.
        /// </summary>
        [ColumnAttribute("IsGenericParameter")]
        public bool IsGenericParameter { get; set; }


    }
}
