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
using  HularionMesh.Translator.SqlBase.SqlGenerator;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.Mechanic
{
    /// <summary>
    /// The mechanics for interacting with a SQL repository in a unique way.
    /// </summary>
    public interface ISqlRepositoryMechanic
    {
        /// <summary>
        /// Sets the SqlDomainTranslator when the domain is being created.
        /// </summary>
        SqlDomainTranslator SqlDomainTranslator { get; set; }

        /// <summary>
        /// Creates the domain using the given table specification. 
        /// </summary>
        void CreateDomain(CreateTableSpecification table);

        /// <summary>
        /// Prepares the domain to handle the provided generics.
        /// </summary>
        /// <param name="meshRepository">The calling repository.</param>
        /// <param name="sqlRepository">The SQL implementation connector.</param>
        /// <param name="domain">The domain to prepare.</param>
        /// <param name="generics">The generics to prepare.</param>
        void PrepareGenerics(SqlMeshRepository meshRepository, ISqlRepository sqlRepository, MeshDomain domain, MeshGeneric[] generics);

        /// <summary>
        /// Sets up the insert statement.
        /// </summary>
        /// <param name="insert">The insert statement to setup.</param>
        /// <param name="values">The values to insert.S</param>
        void SetupInsert(SqlMeshInsert insert, DomainObject[] values);

        /// <summary>
        /// Sets up the update statement.
        /// </summary>
        /// <param name="update">The update statement.</param>
        /// <param name="updaters">The update values.</param>
        MechanicUpdateOption SetupUpdate(SqlMeshUpdate update, DomainObjectUpdater[] updaters);

        /// <summary>
        /// Further manifests the result of a query.
        /// </summary>
        /// <param name="objects">The objects to further manifest.</param>
        void ManifestQueryResult(IEnumerable<DomainObject> objects);

    }
}
