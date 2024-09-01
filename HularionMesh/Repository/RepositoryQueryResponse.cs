#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// A response for a repository query request.
    /// </summary>
    public class RepositoryQueryResponse
    {
        /// <summary>
        /// The type of objects being queried.
        /// </summary>
        public Type QueryType { get; set; }

        /// <summary>
        /// The resulting query objects.
        /// </summary>
        public object[] Result { get { return Items.ToArray(); } }

        /// <summary>
        /// The first object or null if there are no objects.
        /// </summary>
        public object First { get { return (Result == null || Result.Length == 0 ) ? null : Result[0]; } }

        /// <summary>
        /// The number of objects returned.
        /// </summary>
        public int Count { get { return Result == null ? 0 : Result.Length; } }

        /// <summary>
        /// The messages from the repository and the underlying storage services.
        /// </summary>
        public List<ServiceResponseMessage> Messages { get; private set; } = new List<ServiceResponseMessage>();

        private List<object> Items { get; set; } = new List<object>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="queryType">The type of object queried.</param>
        public RepositoryQueryResponse(Type queryType)
        {
            QueryType = queryType;
        }

        /// <summary>
        /// Adds an item to the query result.
        /// </summary>
        /// <param name="item">The result item to add.</param>
        public void AddResultItem(object item)
        {
            Items.Add(item);
        }

    }
    /// <summary>
    /// A response for a repository query request.
    /// </summary>
    /// <typeparam name="T">The type of object being queried.</typeparam>
    public class RepositoryQueryResponse<T> : RepositoryQueryResponse
        where T : class
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public RepositoryQueryResponse() : base(typeof(T))
        {
        }

        /// <summary>
        /// The resulting query objects.
        /// </summary>
        public new T[] Result { get { return (base.Result == null) ? new T[] { } : base.Result.Select(x => (T)x).ToArray(); } }

        /// <summary>
        /// The first object or null if there are no objects.
        /// </summary>
        public new T First { get { return base.First == null ? null : (T)base.First; } }


    }
}
