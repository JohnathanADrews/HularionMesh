#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern;
using HularionCore.Pattern.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.DomainValue
{
    /// <summary>
    /// Contains information required to make a read request to a domain value service.
    /// </summary>
    public class DomainValueQueryRequest
    {
        /// <summary>
        /// The key of the request.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The key of the user making the request.
        /// </summary>
        public IMeshKey UserKey { get; set; }

        /// <summary>
        /// The root node in the where expression.
        /// </summary>
        public WhereExpressionNode Where { get; set; }

        /// <summary>
        /// The properies of the object to read.
        /// </summary>
        public DomainReadRequest Reads { get; set; }

        /// <summary>
        /// Creates a new DomainValueQueryResponse with a matching key.
        /// </summary>
        /// <returns></returns>
        public DomainValueQueryResponse CreateResponse()
        {
            return new DomainValueQueryResponse() { Key = Key };
        }


        private static int requestId = 0;
        /// <summary>
        /// Sets a provider for request keys, allowing the short create methods to be used.
        /// </summary>
        public static IProvider<object> RequestKeyProvider { get; set; } = new ProviderFunction<object>(() => requestId++);


        /// <summary>
        /// (Requires static members RequestKeyProvider be set.) Creates a query request that will retrieve the indicated properties.
        /// </summary>
        /// <param name="where">The root expression node.</param>
        /// <param name="reads">The properties of the domain value to get.</param>
        /// <returns>A new query request with the indicated values.</returns>
        public static DomainValueQueryRequest CreateQueryRequest(WhereExpressionNode where, DomainReadRequest reads)
        {
            if (RequestKeyProvider == null)
            {
                throw new InvalidOperationException(String.Format("This method requires that RequestKeyProvider be set. [ZRZfW90pgUGaSsyoGC29sg]"));
            }
            var request = new DomainValueQueryRequest()
            {
                Key = RequestKeyProvider.Provide(),
                Where = where,
                Reads = reads
            };
            return request;
        }
    }
}
