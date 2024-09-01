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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// A representation of a ValueProperty for file storage.
    /// </summary>
    public class FileValueProperty
    {

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the property.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The serialized generics.
        /// </summary>
        public string Generics { get; set; }

        /// <summary>
        /// True iff this property is a generic type.
        /// </summary>
        public bool HasGenerics { get; set; } = false;

        /// <summary>
        /// True iff this property is a generic parameter.
        /// </summary>
        public bool IsGenericParameter { get; set; } = false;

        /// <summary>
        /// Describes whether this value is a proxy for the Domain's Key or a Meta value.
        /// </summary>
        public ValuePropertyProxy Proxy { get; set; } = ValuePropertyProxy.None;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileValueProperty()
        {

        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="property">The property this object represents.</param>
        public FileValueProperty(ValueProperty property)
        {
            //Key = property.Key;
            Name = property.Name;
            Type = property.Type;
            Proxy = property.Proxy;
            Generics = property.SerializedGenerics;
            IsGenericParameter = property.IsGenericParameter;
            HasGenerics = property.HasGenerics;
        }

        public ValueProperty GetProperty()
        {
            var property = new ValueProperty();
            //property.Key = Key;
            property.Name = Name;
            property.Type = Type;
            property.Proxy = Proxy;
            property.Generics = MeshGeneric.Deserialize(Generics).ToList();
            property.IsGenericParameter = IsGenericParameter;
            property.HasGenerics = HasGenerics;

            return property;
        }

    }
}
