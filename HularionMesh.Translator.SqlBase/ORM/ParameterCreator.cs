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
using System.Text;

namespace HularionMesh.Translator.SqlBase.ORM
{
    /// <summary>
    /// Creates parameters with names unique to the creator instance.
    /// </summary>
    public class ParameterCreator
    {
        /// <summary>
        /// The index used to create a parameter.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The parameters that were created;
        /// </summary>
        public List<SqlMeshParameter> Parameters { get; private set; } = new List<SqlMeshParameter>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParameterCreator()
        {

        }

        /// <summary>
        /// Creates a parameter with a name unique to this creator.
        /// </summary>
        /// <returns>The created parameter.</returns>
        public SqlMeshParameter Create()
        {
            var parameter = new SqlMeshParameter() { Name = String.Format("@p{0}", Index++) };
            Parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// Creates a parameter with a name unique to this creator.
        /// </summary>
        /// <param name="value">The value to assign to the parameter.</param>
        /// <returns>The created parameter.</returns>
        public SqlMeshParameter Create(object value)
        {
            var parameter = new SqlMeshParameter() { Name = String.Format("@p{0}", Index++), Value = value };
            Parameters.Add(parameter);
            return parameter;
        }
    }
}
