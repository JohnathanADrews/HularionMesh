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
using System.Linq;
using System.Text;

namespace HularionMesh.Repository
{
    /// <summary>
    /// Represents a type that should be used as a repository domain.
    /// </summary>
    internal class MeshRepositoryDomainAttribute : Attribute
    {
        /// <summary>
        /// The unique name part of the domain key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// A name that could be displayed to users.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Additional descriptive information.
        /// </summary>
        public string Description { get; private set; }


        private Dictionary<string, object> values = new Dictionary<string, object>();

        /// <summary>
        /// Gets the values associated with the domian.
        /// </summary>
        public IDictionary<string, object> Values { get { return values.ToDictionary(x => x.Key, x => x.Value); } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The unique domain key.</param>
        public MeshRepositoryDomainAttribute(string key)
        {
            Key = key;
            SetupValues();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The unique domain key.</param>
        /// <param name="name">A name that could be displayed to users</param>
        public MeshRepositoryDomainAttribute(string key, string name)
        {
            Key = key;
            Name = name;
            SetupValues();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The unique domain key.</param>
        /// <param name="name">A name that could be displayed to users.</param>
        /// <param name="description">Additional descriptive information.</param>
        public MeshRepositoryDomainAttribute(string key, string name, string description)
        {
            Key = key;
            Name = name;
            Description = description;
            SetupValues();
        }

        private void SetupValues()
        {
            if (Key != null) { values.Add("Key", Key); }
            if (Name != null) { values.Add("Name", Name); }
            if (Description != null) { values.Add("Description", Description); }
        }
    }
}
