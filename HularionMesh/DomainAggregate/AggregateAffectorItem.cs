#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh
{
    /// <summary>
    /// A single affector in an aggregate affector request.
    /// </summary>
    public class AggregateAffectorItem
    {
        /// <summary>
        /// The domin of the item.
        /// </summary>
        public MeshDomain Domain { get; set; }

        /// <summary>
        /// The Create affector if this represents a create operation.
        /// </summary>
        public DomainValueAffectCreate Creator { get; set; }

        /// <summary>
        /// The Inserter affector if this represents an insert operation.
        /// </summary>
        public DomainValueAffectInsert Inserter { get; set; }

        /// <summary>
        /// The Update affector if this represents an update operation.
        /// </summary>
        public DomainValueAffectUpdate Updater { get; set; }

        /// <summary>
        /// The Delete affector if this represents a delete operation.
        /// </summary>
        public DomainValueAffectDelete Deleter { get; set; }

        /// <summary>
        /// The Link affector if this represents a link operation.
        /// </summary>
        public DomainLinkAffectRequest Link { get; set; }

        /// <summary>
        /// If Updater is set but the key does not exist, and this property is true, the object will be inserted instead.
        /// </summary>
        public bool InsertUpdateIfNotPresent { get; set; } = false;

    }
}
