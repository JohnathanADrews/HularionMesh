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
using HularionMesh.DomainLink;
using HularionMesh.Injector;
using HularionMesh.MeshType;
using HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using HularionCore.Injector;
using HularionCore.Pattern.Functional;

namespace HularionMesh.Translator.SqlBase
{
    /// <summary>
    /// Implements an injector for an ISqlRepository to replace IMeshKey with string.
    /// </summary>
    public class SqlRepositoryInjector : Injectable<ISqlRepository>, ISqlRepository
    {
        public IParameterizedProvider<DataType, ISqlType> SqlTypeProvider { get; set; }

        public IProvider<IEnumerable<ISqlType>> SqlTypesProvider { get; set; }

        public IParameterizedCreator<MeshDomain, IMeshKey> DomainValueKeyCreator { get; set; }

        public IParameterizedProvider<LinkedDomains, DomainLinkForm> LinkKeyFormProvider { get; set; }

        public IParameterizedCreator<SqlObject, string> ObjectNameCreator { get; set; }

        public ITransform<WhereTransformRequest, WhereTransformResult> WhereTransformer { get; set; }

        private ISqlRepository repository;

        private Type IMeshKeyType = typeof(IMeshKey);


        public SqlRepositoryInjector(ISqlRepository repository)
        {
            this.repository = repository;
            this.SetMembers(InjectorOverwriteMode.AnyOverAny, repository);
        }

        public void CreateTable(CreateTableSpecification table)
        {
            repository.CreateTable(table);
        }

        public void AddTableColumns(string tableName, CreateColumnSpecification[] columns)
        {
            repository.AddTableColumns(tableName, columns);
        }

        public void ExecuteCommand(string command, IEnumerable<SqlMeshParameter> parameters = null)
        {
            AdjustParameters(parameters);
            repository.ExecuteCommand(command, parameters);
        }

        public DataTable ExecuteQuery(string query, IEnumerable<SqlMeshParameter> parameters = null)
        {
            AdjustParameters(parameters);
            return repository.ExecuteQuery(query, parameters);
        }

        private void AdjustParameters(IEnumerable<SqlMeshParameter> parameters)
        {
            if(parameters == null) { return; }
            foreach(var parameter in parameters)
            {
                if(parameter.Value != null && IMeshKeyType.IsAssignableFrom(parameter.Value.GetType())) { parameter.Value = parameter.Value.ToString(); }
            }
        }
    }
}
