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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace HularionMesh.MeshType
{
    /// <summary>
    /// A mesh data type.
    /// </summary>
    public class DataType 
    {
        /// <summary>
        /// The key of the data type.
        /// </summary>
        public IMeshKey Key { get; private set; }

        /// <summary>
        /// The name of the data type.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type to which the data type maps.
        /// </summary>
        public Type CSharpType { get; set; }

        /// <summary>
        /// True iff the type is known.
        /// </summary>
        public bool IsKnown { get; set; } = true;

        /// <summary>
        /// True iff the type can parse a string.
        /// </summary>
        public bool CanParse { get { return Parser != null; } }

        /// <summary>
        /// True iff the type can make a string equivalent of the object.
        /// </summary>
        public bool CanStringify { get { return Stringify != null; } }

        private Func<string, object> Parser { get; set; } = null;

        private Func<object, string> Stringify { get; set; } = value => value == null ? null : value.ToString();

        /// <summary>
        /// Parses the string to the value type if it can be parsed.
        /// </summary>
        /// <param name="serialized">The serialized value.</param>
        /// <returns></returns>
        public object Parse(string serialized)
        {
            if (!CanParse)
            {
                throw new NotImplementedException(String.Format("Parse not defined for DataType {0}", Name));
            }
            return Parser(serialized);
        }

        /// <summary>
        /// Creates a string version of the object.
        /// </summary>
        /// <param name="value">The value to stringify.</param>
        /// <returns>The string version of the object.</returns>
        public string ObjectString(object value)
        {
            return Stringify(value);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DataType(string name)
        {
            Name = name;
            Key = (new MeshKey()).SetPart("DataType", name);
        }


        public static DataType UnknownCSType { get; private set; } = new DataType("UnknownCSType") { IsKnown = false };

        private static Dictionary<Type, DataType> csTypes = new Dictionary<Type, DataType>();


        public static DataType MeshKey { get; private set; } = new DataType("MeshKey") { CSharpType = typeof(IMeshKey), Parser = s => HularionMesh.MeshKey.Parse(s), Stringify = value => value == null ? HularionMesh.MeshKey.NullKey.Serialized : ((IMeshKey)value).Serialized };
        public static DataType Byte { get; private set; } = new DataType("Byte") { CSharpType = typeof(byte), Parser = s => byte.Parse(s) };
        public static DataType SignedInteger16 { get; private set; } = new DataType("SignedInteger16") { CSharpType = typeof(short), Parser = s=>short.Parse(s)};
        public static DataType SignedInteger32 { get; private set; } = new DataType("SignedInteger32") { CSharpType = typeof(int), Parser = s => int.Parse(s)};
        public static DataType SignedInteger64 { get; private set; } = new DataType("SignedInteger64") { CSharpType = typeof(long), Parser = s => long.Parse(s)};
        public static DataType UnsignedInteger16 { get; private set; } = new DataType("UnsignedInteger16") { CSharpType = typeof(ushort), Parser = s => ushort.Parse(s)};
        public static DataType UnsignedInteger32 { get; private set; } = new DataType("UnsignedInteger32") { CSharpType = typeof(uint), Parser = s => uint.Parse(s)};
        public static DataType UnsignedInteger64 { get; private set; } = new DataType("UnsignedInteger64") { CSharpType = typeof(ulong), Parser = s => ulong.Parse(s)};
        public static DataType Float32 { get; private set; } = new DataType("Float32") { CSharpType = typeof(float), Parser = s => float.Parse(s)};
        public static DataType Float64 { get; private set; } = new DataType("Float64") { CSharpType = typeof(double), Parser = s => double.Parse(s)};
        public static DataType Decimal { get; private set; } = new DataType("Decimal") { CSharpType = typeof(decimal), Parser = s => decimal.Parse(s) };
        public static DataType Text8 { get; private set; } = new DataType("Text8") { CSharpType = typeof(string), Parser = s => s };
        public static DataType Truth { get; private set; } = new DataType("Truth") { CSharpType = typeof(bool), Parser = s => (String.Format("{0}", s).ToLower() == "true") };
        //public static DataType Bit { get; private set; } = new DataType("Bit") { CSharpType = null, Parser = s => 
        //{
        //    //Get as byte since there is no bit.
        //    var result = new byte();
        //    byte.TryParse(s, out result);
        //    return result;
        //} };
        public static DataType NByte { get; private set; } = new DataType("NByte") { CSharpType = typeof(byte[]), Stringify = value => (value == null || value.GetType() != typeof(byte[])) ? null : Convert.ToBase64String((byte[])value), Parser = s => String.IsNullOrWhiteSpace(s) ? null : Convert.FromBase64String(s) };
        public static DataType DateTime { get; private set; } = new DataType("DateTime") { CSharpType = typeof(DateTime), Stringify = value => value == null ? null : ((System.DateTime)value).ToString("yyyy-MM-dd hh:mm:ss tt"), Parser = s => System.DateTime.ParseExact(s, "yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture) };

        private static Dictionary<IMeshKey, DataType> dataTypes { get; set; }


        static DataType()
        {
            var types = GetDataTypes();
            foreach (var dataType in types)
            {
                if (dataType.CSharpType != null && !csTypes.ContainsKey(dataType.CSharpType)) { csTypes.Add(dataType.CSharpType, dataType); }
            }
            csTypes.Add(typeof(MeshKey), DataType.MeshKey);
        }

        static bool initialized = false;
        static Mutex initializeMutex = new Mutex();

        static void Initialize()
        {
            if (initialized) { return; }
            initializeMutex.WaitOne();
            if (initialized)
            {
                initializeMutex.ReleaseMutex();
                return;
            }
            dataTypes = typeof(DataType).GetProperties().Where(x => x.PropertyType == typeof(DataType)).Select(x => (DataType)x.GetValue(x)).ToList()
                .Where(x => x.IsKnown).ToDictionary(x => x.Key, x => x);
            initialized = true;
            initializeMutex.ReleaseMutex();
        }

        /// <summary>
        /// Gets all the data types.
        /// </summary>
        /// <returns>All the data types.</returns>
        public static IEnumerable<DataType> GetDataTypes()
        {
            Initialize();
            return dataTypes.Values;
        }


        /// <summary>
        /// Gets the data type given the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The data type.</returns>
        public static DataType FromCSharpType(Type type)
        {
            if (csTypes.ContainsKey(type)) { return csTypes[type]; }
            return UnknownCSType;
        }

        /// <summary>
        /// Gets the data type given the provided type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The data type.</returns>
        public static DataType FromCSharpType<T>()
        {
            return FromCSharpType(typeof(T));
        }


        /// <summary>
        /// Gets the data type having the provided key.
        /// </summary>
        /// <param name="key">The key of the data type.</param>
        /// <returns>The data type with the provided key or an unknown data type if not found.</returns>
        public static DataType FromKey(IMeshKey key)
        {
            Initialize();
            if(key == null) { return null; }
            if(key.Serialized == HularionMesh.MeshKey.TypeKey.Serialized) { return MeshKey; }
            if (!dataTypes.ContainsKey(key)) { return null; }
            return dataTypes[key];
        }

        /// <summary>
        /// Gets the data type given the provided type key.
        /// </summary>
        /// <param name="key">The key of the type.</param>
        /// <returns>The data type having the provided key.</returns>
        public static DataType FromKey(string key)
        {
            return FromKey(HularionMesh.MeshKey.Parse(key));
        }

        /// <summary>
        /// Determines whether the provided key corresponds to a known data type/
        /// </summary>
        /// <param name="key">The type key.</param>
        /// <returns>True iff the provided type is known.</returns>
        public static bool TypeIsKnown(IMeshKey key)
        {
            Initialize();
            return dataTypes.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the provided key corresponds to a known data type.
        /// </summary>
        /// <param name="key">The type key.</param>
        /// <returns>True iff the provided type is known.</returns>
        public static bool TypeIsKnown(string key)
        {
            Initialize();
            return dataTypes.ContainsKey(HularionMesh.MeshKey.Parse(key));
        }

        /// <summary>
        /// Determines whether the provided type corresponds to a known data type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>True iff the provided type is known.</returns>
        public static bool TypeIsKnown(Type type)
        {
            Initialize();
            return (FromCSharpType(type) != DataType.UnknownCSType);
        }

        public override string ToString()
        {
            return Name;
        }


    }

}
