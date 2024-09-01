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
using HularionCore.Pattern.Identifier;
using HularionMesh.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainValue
{
    /// <summary>
    /// A response to a DomainAffectRequest.
    /// </summary>
    public class DomainValueAffectResponse : IKeyedValue<object>
    {
        /// <summary>
        /// The key of the request.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// The values that were read during the update process.
        /// </summary>
        public IEnumerable<DomainObject> Reads
        {
            get
            {
                var reads = new List<DomainObject>();
                reads.AddRange(CreateReads);
                reads.AddRange(UpdateReads);
                reads.AddRange(DeleteReads);
                return reads;
            }
        }

        /// <summary>
        /// A map of created objects to the DomainObjects.
        /// </summary>
        //public IDictionary<object, DomainObject> CreateReads { get; set; }
        public IList<DomainObject> CreateReads { get; set; }

        /// <summary>
        /// The updated objects to the DomainObjects.
        /// </summary>
        public IEnumerable<DomainObject> UpdateReads { get; set; }

        /// <summary>
        /// The deleted DomainObjects.
        /// </summary>
        public IEnumerable<DomainObject> DeleteReads { get; set; }

        /// <summary>
        /// The messages related to processing the request.
        /// </summary>
        public List<ServiceResponseMessage> Messages { get; private set; } = new List<ServiceResponseMessage>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainValueAffectResponse()
        {
            DeleteReads = new List<DomainObject>();
            CreateReads = new List<DomainObject>();
            UpdateReads = new List<DomainObject>();
        }

        /// <summary>
        /// Adds update reads to the response.
        /// </summary>
        /// <param name="updates">The updates to add to the response.</param>
        public void AddUpdates(params DomainObject[] updates)
        {
            ((List<DomainObject>)this.UpdateReads).AddRange(updates);
        }

        /// <summary>
        /// Adds delete reads to the response.
        /// </summary>
        /// <param name="deletes">The deletes to add to the response.</param>
        public void AddDeletes(params DomainObject[] deletes)
        {
            ((List<DomainObject>)this.DeleteReads).AddRange(deletes);
        }
    }
}
