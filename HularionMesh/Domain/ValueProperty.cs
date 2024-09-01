#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.General;
using HularionCore.Pattern;
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Domain
{
    /// <summary>
    /// Describes a property of a value.
    /// </summary>
    public class ValueProperty 
    {
        /// <summary>
        /// The property's key.
        /// </summary>
        //public IMeshKey Key { get; set; }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the property.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The domain of the property or null if it is not a domain type.
        /// </summary>
        public MeshDomain Domain { get; set; }

        /// <summary>
        /// The default Value of the property.
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Describes whether this value is a proxy for the Domain's Key or a Meta value.
        /// </summary>
        public ValuePropertyProxy Proxy { get; set; } = ValuePropertyProxy.None;

        /// <summary>
        /// Contains the generic types.
        /// </summary>
        public List<MeshGeneric> Generics { get; set; } = new List<MeshGeneric>();

        /// <summary>
        /// True iff this property is a generic type.
        /// </summary>
        public bool HasGenerics { get; set; } = false;

        /// <summary>
        /// True iff this property is a generic parameter.
        /// </summary>
        public bool IsGenericParameter { get; set; } = false;

        /// <summary>
        /// The property's generics seralized to a string.
        /// </summary>
        public string SerializedGenerics { get { return MeshGeneric.SerializeGenerics(Generics.ToArray()); } }


        private static MemberMapper mapper = new MemberMapper();

        static ValueProperty()
        {
            mapper.CreateMap<ValueProperty, ValueProperty>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValueProperty()
        {
            //Key = MeshKey.CreateUniqueTagKey();
        }

        public ValueProperty(string name,  DataType dataType)
        {
            //Key = MeshKey.CreateUniqueTagKey();
            this.Name = name;

        }

        /// <summary>
        /// Creates a copy of this property.
        /// </summary>
        /// <returns>A copy of this property.</returns>
        public ValueProperty Clone()
        {
            //var result = new ValueProperty() 
            //{ 
            //    //Key = Key, 
            //    Name = Name, Type = Type, Domain = Domain, Generics = Generics, HasGenerics = HasGenerics, IsGenericParameter = IsGenericParameter, Default = Default, Proxy = Proxy };
            ValueProperty result = new ValueProperty();
            mapper.Map(this, result);
            return result;
        }

        /// <summary>
        /// Returns the data type if the property holds one. Returns null otherwise (e.g. a domain type).
        /// </summary>
        /// <returns>The data type.</returns>
        public DataType GetDataType()
        {
            return DataType.FromKey(Type);
        }


        public override string ToString()
        {
            return String.Format("{0}-{1}-{2}", Name, Type, MeshGeneric.SerializeGenerics(Generics.ToArray()));
        }
    }

}
