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
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.DomainValue
{
    /// <summary>
    /// The partial value requested by the domain key
    /// </summary>
    public class DomainObject
    {
        /// <summary>
        /// The key of the domain object.
        /// </summary>
        public IMeshKey Key { get; set; }

        /// <summary>
        /// The named property values.
        /// </summary>
        public IDictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The named meta property values.
        /// </summary>
        public IDictionary<string, object> Meta { get; set; } = new Dictionary<string, object>();


        private static TypeManager TypeManager = new TypeManager();

        /// <summary>
        /// Constructor.
        /// </summary>
        public DomainObject()
        {
        }

        public T Manifest<T>()
            where T : class, new()
        {
            return TypeManager.Manifest<T>(this);
        }

        /// <summary>
        /// Manifests an object of the provided type using information from this domain object.
        /// </summary>
        /// <param name="type">The type to manifest.</param>
        /// <returns>The manifested type value.</returns>
        public object Manifest(Type type)
        {
            return TypeManager.Manifest(type, this);
        }

        /// <summary>
        /// Derives a domain object from the provided value.
        /// </summary>
        /// <param name="value">The value of source object.</param>
        /// <returns>The domain object.</returns>
        public static DomainObject Derive(object value)
        {
            return TypeManager.Derive(value.GetType(), value);
        }

        ///// <summary>
        ///// Derives a domain object from the provided value, which is not necessarily the type T. The result will contain all members with matching names and types.
        ///// </summary>
        ///// <typeparam name="T">The type of the value to derive.</typeparam>
        ///// <param name="value">The value of source object.</param>
        ///// <returns>The domain object.</returns>
        //public static DomainObject Derive<T>(object value)
        //{
        //    return Derive(typeof(T), value);
        //}

        ///// <summary>
        ///// Derives a domain object from the provided value, which is not necessarily the type T. The result will contain all members with matching names and types.
        ///// </summary>
        ///// <param name="type">The type of the value to derive.</param>
        ///// <param name="value">The value of source object.</param>
        ///// <returns>The domain object.</returns>
        //public static DomainObject Derive(Type type, object value)
        //{
        //    var result = TypeManager.Derive(type, value);
        //    return result;
        //}

        /// <summary>
        /// Updates this value using the provided value.
        /// </summary>
        /// <param name="value">The object containing the new values.</param>
        public void Update(object value)
        {
            TypeManager.UpdateTypeObject(this, value);
        }

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        /// <param name="includeMeta">If true (default) the meta will be included.</param>
        /// <param name="includeValues">If true (default) the values will be included.</param>
        /// <returns>A copy of this object.</returns>
        public DomainObject Clone(bool includeMeta = true, bool includeValues = true)
        {
            var result = new DomainObject() { Key = Key };
            if (includeMeta)
            {
                foreach (var meta in Meta) { result.Meta.Add(meta.Key, meta.Value); }
            }
            if (includeValues)
            {
                foreach (var value in Values) { result.Values.Add(value.Key, value.Value); }
            }
            return result;
        }

        /// <summary>
        /// Gets the generics.
        /// </summary>
        /// <returns>The generics for this object.</returns>
        public MeshGeneric[] GetGenerics()
        {
            if (!Meta.ContainsKey(MeshKeyword.Generics.Alias)) { return new MeshGeneric[] { }; }
            if (String.IsNullOrWhiteSpace((string)Meta[MeshKeyword.Generics.Alias])) { return new MeshGeneric[] { }; }
            return MeshGeneric.Deserialize((string)Meta[MeshKeyword.Generics.Alias]);
        }


        public override string ToString()
        {
            return String.Format("Key: {0}", Key);
        }

    }
}
