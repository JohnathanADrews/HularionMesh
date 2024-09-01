#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Stores the type to table mapping information for the specified type.
    /// </summary>
    public class TypeTableInfo
    {
        /// <summary>
        /// The type that is mapping to a table.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The name of the corresponding table as set by TableAttribute.
        /// </summary>
        public string TableName { get; private set; } = null;

        /// <summary>
        /// The members of this type that map to columns.
        /// </summary>
        public IDictionary<string, TypeMemberInfo> Members { get; private set; } = new Dictionary<string, TypeMemberInfo>();

        /// <summary>
        /// The key members.
        /// </summary>
        public IEnumerable<TypeMemberInfo> KeyMembers { get; private set; } = new TypeMemberInfo[] { };

        /// <summary>
        /// The non-key members.
        /// </summary>
        public IEnumerable<TypeMemberInfo> NonKeyMembers { get; private set; } = new TypeMemberInfo[] { };

        /// <summary>
        /// The repository.
        /// </summary>
        public SqlMeshRepository Repository { get; private set; }

        /// <summary>
        /// The details for creating the table.
        /// </summary>
        public CreateTableSpecification CreateTableSpecification { get; private set; } = new CreateTableSpecification();


        private static Type tableAttribute = typeof(TableAttribute);


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type to map.</param>
        public TypeTableInfo(Type type, SqlMeshRepository repository)
        {
            Repository = repository;
            Type = type;
            Initialize();
        }

        private void Initialize()
        {
            var table = (TableAttribute)Type.GetCustomAttributes(false).Where(x => x.GetType() == tableAttribute).FirstOrDefault();
            if (table == null) { return; }
            TableName = Repository.SqlRepository.ObjectNameCreator.Create(new SqlObject { Name = table.TableName, ObjectType = SqlObjectType.Table });

            Members = Type.GetProperties().Where(x => TypeMemberInfo.PropertyIsAColumn(x)).Select(x => new TypeMemberInfo(x, Repository)).ToDictionary(x=>x.Name, x=>x);

            CreateTableSpecification.Name = TableName;
            foreach(var member in Members.Values)
            {
                CreateTableSpecification.Columns.AddRange(member.CreateColumnSpecifications);
            }

            KeyMembers = Members.Values.Where(x => x.IsPrimaryKey).ToList();
            NonKeyMembers = Members.Values.Where(x => !x.IsPrimaryKey).ToList();
        }

        /// <summary>
        /// Determines whether the specified type is mapped to a table.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true iff the specified type is mapped to a table.</returns>
        public static bool TypeIsATable(Type type)
        {
            return (type.GetCustomAttributes(false).Where(x => x.GetType() == tableAttribute).Count() > 0);
        }

        /// <summary>
        /// Gets the member info given the member's name.
        /// </summary>
        /// <param name="memberName">The name of the member.</param>
        /// <returns>The member's info.</returns>
        public TypeMemberInfo GetMember(string memberName)
        {
            return Members[memberName];
        }

    }
}
