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
    /// Represents a value attached to a domain.
    /// </summary>
    [Table("DomainValue")]
    public class SqlDomainValue
    {

        /// <summary>
        /// The key of the domain for which this value is set.
        /// </summary>
        [ColumnAttribute("DomainKey")]
        public string DomainKey { get; set; }

        [ColumnAttribute("Key")]
        [PrimaryKeyAttribute]
        public string Key { get { return MeshKey.Parse(DomainKey).SetPart(SqlMeshKeyword.KeyNamePart.Alias, Name).Serialized; } set { return; } }

        /// <summary>
        /// The name of the value.
        /// </summary>
        [ColumnAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// The serialized value.
        /// </summary>
        [ColumnAttribute("Value")]
        public string Value { get; set; }

    }
}
