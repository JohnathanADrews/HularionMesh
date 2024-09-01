#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.DomainValue;
using System;

namespace HularionMesh.DomainLink
{
    /// <summary>
    /// Represents the linking between two domain objects.
    /// </summary>
    public class DomainLinker
    {
        /// <summary>
        /// The key of the S-type domain.
        /// </summary>
        public IMeshKey SKey { get; set; }
        /// <summary>
        /// The key of the T-type domain.
        /// </summary>
        public IMeshKey TKey { get; set; }
        /// <summary>
        /// The link domain key.
        /// </summary>
        public IMeshKey DomainKey { get; set; }

        /// <summary>
        /// The name of the S-type member that references the T-type member.
        /// </summary>
        public string SMember { get; set; }
        /// <summary>
        /// The name of the T-type member that references the S-type member.
        /// </summary>
        public string TMember { get; set; }

        /// <summary>
        /// The time the link was created.
        /// </summary>
        public DateTime Creation { get; set; }
        /// <summary>
        /// The creator of the link.
        /// </summary>
        public IMeshKey Creator { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainLinker()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainObject">This linker represented as a domain object.</param>
        public DomainLinker(DomainObject domainObject)
        {
            DomainKey = domainObject.Key;
            Func<string, bool> valueIsNotNull = new Func<string, bool>(key =>
            {
                if (!domainObject.Values.ContainsKey(key) && !domainObject.Meta.ContainsKey(key)) { return false; }
                if (domainObject.Values.ContainsKey(key) && domainObject.Values[key] == null) { return false; }
                if (domainObject.Meta.ContainsKey(key) && domainObject.Meta[key] == null) { return false; }
                return true;
            });
            SKey = MeshKey.Parse(valueIsNotNull(MeshKeyword.SKey.Name) ? domainObject.Values[MeshKeyword.SKey.Name].ToString() : null);
            TKey = MeshKey.Parse(valueIsNotNull(MeshKeyword.TKey.Name) ? domainObject.Values[MeshKeyword.TKey.Name].ToString() : null);
            SMember = valueIsNotNull(MeshKeyword.SMember.Name) ? domainObject.Values[MeshKeyword.SMember.Name].ToString() : null;
            TMember = valueIsNotNull(MeshKeyword.TMember.Name) ? domainObject.Values[MeshKeyword.TMember.Name].ToString() : null;
            Creator = MeshKey.Parse(valueIsNotNull(MeshKeyword.ValueCreator.Name) ? domainObject.Meta[MeshKeyword.ValueCreator.Name].ToString() : null);
            Creation = valueIsNotNull(MeshKeyword.ValueCreationTime.Name) ? DateTime.Parse(domainObject.Meta[MeshKeyword.ValueCreationTime.Name].ToString()) : default(DateTime);
            
        }

        /// <summary>
        /// Creates the domain object version of this linker.
        /// </summary>
        /// <returns>The domain object version of this linker.</returns>
        public DomainObject ToDomainObject()
        {
            var result = new DomainObject() { Key = DomainKey };
            result.Values.Add(MeshKeyword.SKey.Name, SKey);
            result.Values.Add(MeshKeyword.TKey.Name, TKey);
            result.Values.Add(MeshKeyword.SMember.Name, SMember);
            result.Values.Add(MeshKeyword.TMember.Name, TMember);
            result.Meta.Add(MeshKeyword.ValueCreator.Name, Creator);
            result.Meta.Add(MeshKeyword.ValueCreationTime.Name, Creation);
            return result;
        }

        public override string ToString()
        {
            return String.Format("DomainLinker - ({0}:{1}), ({2}:{3}), Key: {4}", SMember, SKey, TMember, TKey, DomainKey);
        }

        /// <summary>
        /// Derives a linker from the provided domain object.
        /// </summary>
        /// <param name="domainObject">The domain object version of this linker.</param>
        /// <returns>A linker from the provided domain object.</returns>
        public static DomainLinker FromDomainObject(DomainObject domainObject)
        {
            return new DomainLinker(domainObject);
        }

    }
}
