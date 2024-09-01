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
using HularionMesh.DomainAggregate;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using HularionMesh.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.SystemDomain
{
    /// <summary>
    /// A system domain representing key/value pair, which is primarily used by the Map domain.
    /// </summary>
    /// <typeparam name="KeyType">The type of the key.</typeparam>
    /// <typeparam name="ValueType">The type of the value.</typeparam>
    [MeshSystemDomainAttribute(KeyedValue.KeyedValue_KeyPartial, KeyedValue.KeyedValue_Name)]
    public class KeyedValue<KeyType, ValueType>
    {
        /// <summary>
        /// The key of the obejct.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Key)]
        public IMeshKey MeshKey { get; set; }

        /// <summary>
        /// The creator of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Creator)]
        public string Creator { get; set; }

        /// <summary>
        /// The creation time of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.CreationTime)]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// The last updater of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.Updater)]
        public string Updater { get; set; }

        /// <summary>
        /// The time the object was last updated.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.UpdateTime)]
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// The serialized generics of the object.
        /// </summary>
        [DomainPropertyAttribute(DomainObjectPropertySelector.SerializedGeneric)]
        public string SerializedGeneric { get; set; }

        /// <summary>
        /// The type of the key.
        /// </summary>
        public KeyType Key { get; set; }

        /// <summary>
        /// The type of the value.
        /// </summary>
        public ValueType Value { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public KeyedValue()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public KeyedValue(KeyType key, ValueType value)
        {
            Key = key;
            Value = value;
        }

    }

    public static class KeyedValue
    {
        /// <summary>
        /// The unique name for a Set.
        /// </summary>
        public const string KeyedValue_KeyPartial = "System_KeyedValue";
        public const string KeyedValue_Name = "Set";

        public static Type KeyedValueType = typeof(KeyedValue<,>);

        public static Type[] DomainTypes = new Type[] { KeyedValueType };

    }

}
