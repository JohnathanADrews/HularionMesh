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
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.SqlGenerator
{
    /// <summary>
    /// A SQL select clause derived from a mesh request.
    /// </summary>
    public class SqlMeshSelect
    {
        /// <summary>
        /// The string containing the select logic.
        /// </summary>
        public string Select { get; private set; }

        /// <summary>
        /// if true, "select" appears at the beginning of the Select string.
        /// </summary>
        public bool SelectKeyWordIncluded { get; private set; }

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// The domain translator.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }

        /// <summary>
        /// The items to read.
        /// </summary>
        public DomainReadRequest Reads { get; private set; }

        /// <summary>
        /// The property translators.
        /// </summary>
        public IEnumerable<SqlDomainPropertyTranslator> DomainProperties { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reads">The items to read.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="sqlDomain">The domain translator.</param>
        /// <param name="domainProperties">The property translators.</param>
        /// <param name="includeSelectKeyword">iff true, the Select stirng will start with "select ".</param>
        public SqlMeshSelect(DomainReadRequest reads, SqlMeshRepository repository, SqlDomainTranslator sqlDomain, IEnumerable<SqlDomainPropertyTranslator> domainProperties = null, bool includeSelectKeyword = false)
        {
            Reads = reads;
            Repository = repository;
            SqlDomain = sqlDomain;
            DomainProperties = domainProperties;
            SelectKeyWordIncluded = includeSelectKeyword;
            SetFromDomainReadRequest();
        }

        private void SetFromDomainReadRequest()
        {
            var prefix = SelectKeyWordIncluded ? "select " : string.Empty;
            var keySelect = String.Format("{0}{1}", prefix, SqlDomain.KeyProperty.GetSelectString());
            switch (Reads.Mode)
            {
                case DomainReadRequestMode.All:
                    Select = String.Format("{0}*", prefix);
                    break;
                case DomainReadRequestMode.None:
                    Select = String.Format("{0}null", prefix);
                    break;
                case DomainReadRequestMode.JustKeys:
                    Select = keySelect;
                    break;
                case DomainReadRequestMode.JustMeta:
                    {
                        var selectClause = new StringBuilder();
                        selectClause.Append(keySelect);
                        foreach (var meta in SqlDomain.MetaProperties)
                        {
                            selectClause.Append(String.Format(", {0}", meta.Value.GetSelectString()));
                        }
                        Select = selectClause.ToString();
                    }
                    break;
                case DomainReadRequestMode.JustValues:
                    {
                        var selectClause = new StringBuilder();
                        selectClause.Append(keySelect);
                        foreach (var property in SqlDomain.NonGenericValueProperties)
                        {
                            selectClause.Append(String.Format(", {0}", property.GetSelectString()));
                        }
                        Select = selectClause.ToString();
                    }
                    break;
                //case DomainReadRequestMode.Include:
                //    {
                //        if (domainProperties == null) { domainProperties = new List<ValueProperty>(); }
                //        var selectClause = new StringBuilder();
                //        selectClause.Append(String.Format("{0}{1}", prefix, SqlRepository.CreateColumnName(MeshKeyword.Key.Alias)));
                //        var properties = domainProperties.Where(x => reads.Values.Contains(x.Name)).ToList();
                //        foreach (var domainProperty in properties)
                //        {
                //            var property = new SqlDomainPropertyTranslator(SqlPropertyCategory.Property, domainProperty, SqlRepository.SqlTypeProvider.Provide(domainProperty.GetDataType()));
                //            selectClause.Append(String.Format(", {0}", SqlRepository.CreateColumnName(property.ColumnName)));
                //        }
                //        properties = MeshDomain.MetaProperties.Where(x => reads.Meta.Contains(x.Name)).ToList();
                //        foreach (var meta in properties)
                //        {
                //            var property = new SqlDomainPropertyTranslator(SqlPropertyCategory.Meta, meta, SqlRepository.SqlTypeProvider.Provide(meta.GetDataType()));
                //            selectClause.Append(String.Format(", {0}", SqlRepository.CreateColumnName(property.ColumnName)));
                //        }
                //        Select = selectClause.ToString();
                //    }
                //    break;
                //case DomainReadRequestMode.Exclude:
                //    {
                //        if (domainProperties == null) { domainProperties = new List<ValueProperty>(); }
                //        var selectClause = new StringBuilder();
                //        selectClause.Append(String.Format("{0}{1}", prefix, SqlRepository.CreateColumnName(MeshKeyword.Key.Alias)));
                //        var properties = domainProperties.Where(x => !reads.Values.Contains(x.Name)).ToList();
                //        foreach (var domainProperty in properties)
                //        {
                //            var property = new SqlDomainPropertyTranslator(SqlPropertyCategory.Property, domainProperty, SqlRepository.SqlTypeProvider.Provide(domainProperty.GetDataType()));
                //            selectClause.Append(String.Format(", {0}", SqlRepository.CreateColumnName(property.ColumnName)));
                //        }
                //        properties = MeshDomain.MetaProperties.Where(x => !reads.Meta.Contains(x.Name)).ToList();
                //        foreach (var meta in properties)
                //        {
                //            var property = new SqlDomainPropertyTranslator(SqlPropertyCategory.Meta, meta, SqlRepository.SqlTypeProvider.Provide(meta.GetDataType()));
                //            selectClause.Append(String.Format(", {0}", SqlRepository.CreateColumnName(property.ColumnName)));
                //        }
                //        Select = selectClause.ToString();
                //    }
                //    break;
            }

        }
    }
}
