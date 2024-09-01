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

namespace HularionMesh.DomainValue
{
    /// <summary>
    /// Indicates the properties to read during a service operation.
    /// </summary>
    public class DomainReadRequest
    {
        /// <summary>
        /// The value properties to include or exclude.
        /// </summary>
        public IList<string> Values { get; set; }

        /// <summary>
        /// The meta properties to include or exclude.
        /// </summary>
        public IList<string> Meta { get; set; }

        /// <summary>
        /// The mode indicating how to use the provided property names.  Defaults to Include.
        /// </summary>
        public DomainReadRequestMode Mode { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainReadRequest()
        {
            Meta = new List<string>();
            Values = new List<string>();
            Mode = DomainReadRequestMode.Include;
        }

        /// <summary>
        /// A read request that reads no values.
        /// </summary>
        public static DomainReadRequest ReadNone { get { return new DomainReadRequest() { Mode = DomainReadRequestMode.None }; } }

        /// <summary>
        /// A read request that reads all values.
        /// </summary>
        public static DomainReadRequest ReadAll { get { return new DomainReadRequest() { Mode = DomainReadRequestMode.All }; } }

        /// <summary>
        /// A read request that reads just the keys.
        /// </summary>
        public static DomainReadRequest ReadKeys { get { return new DomainReadRequest() { Mode = DomainReadRequestMode.JustKeys }; }}
    }

    /// <summary>
    /// The modes a DomainReadRequest can take.
    /// </summary>
    public enum DomainReadRequestMode
    {
        /// <summary>
        /// Includes all of the indicated properties.
        /// </summary>
        Include,
        /// <summary>
        /// Excludes all of the indicated properties.
        /// </summary>
        Exclude,
        /// <summary>
        /// Includes all Properties.
        /// </summary>
        All,
        /// <summary>
        /// Excludes all Properties.
        /// </summary>
        None,
        /// <summary>
        /// Reads just the domain keys.
        /// </summary>
        JustKeys,
        /// <summary>
        /// Reads just the domain values and keys.
        /// </summary>
        JustValues,
        /// <summary>
        /// Reads just the domain meta and keys.
        /// </summary>
        JustMeta,
        /// <summary>
        /// Read the total number of records found.
        /// </summary>
        Count
    }

}
