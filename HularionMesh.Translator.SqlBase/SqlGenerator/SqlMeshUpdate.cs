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
using HularionMesh.Translator.SqlBase.Mechanic;
using  HularionMesh.Translator.SqlBase.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.SqlGenerator
{
    /// <summary>
    /// A SQL update statement derived from a mesh request.
    /// </summary>
    public class SqlMeshUpdate
    {
        /// <summary>
        /// The key of the user making the update.
        /// </summary>
        public IMeshKey UserKey { get; private set; }

        /// <summary>
        /// The domain translator.
        /// </summary>
        public SqlDomainTranslator SqlDomain { get; private set; }

        /// <summary>
        /// The generated update statement.
        /// </summary>
        public string Update { get; set; }

        /// <summary>
        /// Creates and maintains parameters.
        /// </summary>
        public ParameterCreator ParameterCreator { get; private set; } = new ParameterCreator();

        /// <summary>
        /// The SqlMeshRepository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userKey">The key of the user making the update.</param>
        /// <param name="sqlDomain">The domain translator.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="updater">Contains the update information.</param>
        public SqlMeshUpdate(IMeshKey userKey, SqlDomainTranslator sqlDomain, SqlMeshRepository repository, DomainObjectUpdater updater)
        {
            UserKey = userKey;
            SqlDomain = sqlDomain;
            Repository = repository;
            var mechanic = repository.MechanicProvider.Provide(sqlDomain.Domain.Key);
            if (mechanic == null)
            {
                var updateCommand = new StringBuilder();
                AddUpdater(updater, updateCommand);
                Update = updateCommand.ToString();
            }
            else
            {
                var option = mechanic.SetupUpdate(this, new DomainObjectUpdater[] { updater });
                if (option != MechanicUpdateOption.UpdateNone)
                {
                    var updateCommand = new StringBuilder();
                    AddUpdater(updater, updateCommand, option);
                    Update = (updateCommand.ToString() + Update);
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userKey">The key of the user making the update.</param>
        /// <param name="sqlDomain">The domain translator.</param>
        /// <param name="repository">The SqlMeshRepository.</param>
        /// <param name="updaters">Contains the update information.</param>
        public SqlMeshUpdate(IMeshKey userKey, SqlDomainTranslator sqlDomain, SqlMeshRepository repository, params DomainObjectUpdater[] updaters)
        {
            UserKey = userKey;
            SqlDomain = sqlDomain;
            Repository = repository;
            var mechanic = repository.MechanicProvider.Provide(sqlDomain.Domain.Key);
            if (mechanic == null)
            {
                var updateCommand = new StringBuilder();
                foreach (var updater in updaters)
                {
                    AddUpdater(updater, updateCommand);
                }
                Update = updateCommand.ToString();
            }
            else
            {
                var option = mechanic.SetupUpdate(this, updaters);

                if(option != MechanicUpdateOption.UpdateNone)
                {
                    var updateCommand = new StringBuilder();
                    foreach (var updater in updaters)
                    {
                        AddUpdater(updater, updateCommand, option);
                    }
                    Update = (updateCommand.ToString() + Update);
                }
            }
        }

        private void AddUpdater(DomainObjectUpdater updater, StringBuilder updateCommand, MechanicUpdateOption mechanicOption = MechanicUpdateOption.UpdateAll)
        {
            var where = new SqlMeshWhere(SqlDomain, updater.Where, Repository, parameterCreator: ParameterCreator);
            ParameterCreator = where.ParameterCreator;

            updateCommand.Append(String.Format("update {0} set ", SqlDomain.TableName));

            var first = true;
            var assignCreator = new Action<SqlDomainPropertyTranslator, object>((property, value) =>
            {
                var sqlValues = property.SqlType.ToSqlTransform.Transform(value);
                for(var i=0;i< property.ColumnNames.Length; i++)
                {
                    if(!first || i > 0) { updateCommand.Append(", "); }
                    first = false;
                    updateCommand.Append(String.Format("{0} = {1}", property.ColumnNames[i], ParameterCreator.Create(sqlValues[i]).Name));
                }
            });

            if (mechanicOption == MechanicUpdateOption.UpdateAll || mechanicOption == MechanicUpdateOption.UpdateMeta)
            {
                assignCreator(SqlDomain.GetProperty(SqlPropertyCategory.Meta, MeshDomain.UpdateTime.Name), DateTime.UtcNow);
                assignCreator(SqlDomain.GetProperty(SqlPropertyCategory.Meta, MeshDomain.UpdateUser.Name), UserKey);
            }

            if (mechanicOption == MechanicUpdateOption.UpdateAll)
            {
                foreach (var update in updater.Values)
                {
                    var property = SqlDomain.GetProperty(SqlPropertyCategory.Value, update.Key);
                    assignCreator(property, update.Value);
                }
            }

            updateCommand.Append(String.Format(" where {0};\n", where.Where));
        }

    }
}
