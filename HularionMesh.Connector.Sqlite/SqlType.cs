#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionCore.Pattern.Functional;
using HularionMesh.Translator.SqlBase;

namespace HularionMesh.Connector.Sqlite
{ 

    /// <summary>
    /// Contains the details for translating sql types to and from mesh data types.
    /// </summary>
    public class SqlType : ISqlType
    {
        /// <summary>
        /// The type of the Mesh DataType to which this is related.
        /// </summary>
        public DataType MeshDataType { get; private set; }

        /// <summary>
        /// The names of the SQL types.
        /// </summary>
        public string[] SqlTypeNames { get; set; }

        /// <summary>
        /// The names of the SQL types if it is a primary key.
        /// </summary>
        public string[] SqlPrimaryKeyTypeNames { get; set; }

        /// <summary>
        /// Converts the SQL values to a C# typed value.
        /// </summary>
        public ITransform<object[], object> ToCSTransform { get; set; }

        /// <summary>
        /// Converts a C# typed value to the SQL values.
        /// </summary>
        public ITransform<object, object[]> ToSqlTransform { get; set; }

        /// <summary>
        /// The number of SQL types to which the mesh type maps.
        /// </summary>
        public int SqlTypeCount { get; private set; }


        private static Dictionary<DataType, SqlType> dataTypeMap = new Dictionary<DataType, SqlType>();

        private static ulong lower63 = (ulong.MaxValue >> 1);

        /// <summary>
        /// Provides the ISqlType that corresponds to the provided mesh data type.
        /// </summary>
        public static IParameterizedProvider<DataType, ISqlType> SqlTypeProvider { get; set; } = ParameterizedProvider.FromSingle<DataType, ISqlType>(dataType =>
        {
            if (dataType == null) { return null; }
            if (!dataTypeMap.ContainsKey(dataType)) { return null; }
            return dataTypeMap[dataType];
        });


        /// <summary>
        /// Constructor for direct property to column mapping.
        /// </summary>
        /// <param name="dataType">The mesh data type.</param>
        /// <param name="sqlTypeName">The name of the sql type.</param>
        /// <param name="toCSTransform">Converts a sql value to a c# typed value.</param>
        public SqlType(DataType dataType, string sqlTypeName, Func<object, object> toCSTransform, Func<object, object> toSqlTransform)
        {
            MeshDataType = dataType;
            SqlTypeNames = new string[] { sqlTypeName };
            SqlPrimaryKeyTypeNames = SqlTypeNames;
            SqlTypeCount = 1;
            ToCSTransform = Transform.Create<object[], object>(values =>
            {
                if (values == null || values.Length != SqlTypeCount) { return null; }
                return toCSTransform(values[0]);
            });
            ToSqlTransform = Transform.Create<object, object[]>(o =>
            {
                if (o == null) { return new object[] { DBNull.Value }; }
                return new object[] { toSqlTransform(o) };
            });
        }

        /// <summary>
        /// Constructor for property to many column mapping.
        /// </summary>
        /// <param name="dataType">The mesh data type.</param>
        /// <param name="sqlTypeNames">The name of the sql type.</param>
        /// <param name="toCSTransform">Converts a sql value to a c# typed value.</param>
        public SqlType(DataType dataType, string[] sqlTypeNames, Func<object[], object> toCSTransform, Func<object, object[]> toSqlTransform)
        {
            MeshDataType = dataType;
            SqlTypeNames = sqlTypeNames;
            SqlPrimaryKeyTypeNames = sqlTypeNames;
            SqlTypeCount = sqlTypeNames.Length;
            ToCSTransform = Transform.Create<object[], object>(toCSTransform);
            ToSqlTransform = Transform.Create<object, object[]>(o =>
            {
                if (o == null) { return new object[] { DBNull.Value }; }
                return toSqlTransform(o);
            });
        }

        //SqlTypeMap
        public static SqlType MeshKeyType = new SqlType(DataType.MeshKey, "TEXT", o => MeshKey.Parse(o), o => MeshKey.Parse(o).Serialized) { SqlPrimaryKeyTypeNames = new string[] { "TEXT" } };
        public static SqlType UnsignedInteger16 = new SqlType(DataType.UnsignedInteger16, "INTEGER", o => Convert.ToUInt16(Convert.ToInt64(o) & ushort.MaxValue), o => Convert.ToInt32(o));
        public static SqlType UnsignedInteger32 = new SqlType(DataType.UnsignedInteger32, "INTEGER", o => Convert.ToUInt32(Convert.ToInt64(o) & uint.MaxValue), o => Convert.ToInt64(o));
        public static SqlType UnsignedInteger64 = new SqlType(DataType.UnsignedInteger64, new string[] { "INTEGER", "INTEGER" },
            new Func<object[], object>(values => (Convert.ToUInt64(values[0]) << 63) | Convert.ToUInt64(values[1])),
            new Func<object, object[]>(value => new object[] { (int)((Convert.ToUInt64(value)) >> 63), Convert.ToInt64(Convert.ToUInt64(value) & lower63) }));        
        public static SqlType SignedInteger16 = new SqlType(DataType.SignedInteger16, "INTEGER", o => Convert.ToInt16(Convert.ToInt64(o) & short.MaxValue), o => o);
        public static SqlType SignedInteger32 = new SqlType(DataType.SignedInteger32, "INTEGER", o => Convert.ToInt32(Convert.ToInt64(o) & int.MaxValue), o => o);
        public static SqlType SignedInteger64 = new SqlType(DataType.SignedInteger64, "INTEGER", o => Convert.ToInt64(o), o => o);
        public static SqlType Byte = new SqlType(DataType.Byte, "INTEGER", o => Convert.ToByte(Convert.ToInt16(o) & byte.MaxValue), o => o);

        public static SqlType Float32 = new SqlType(DataType.Float32, "REAL", o => float.Parse(String.Format("{0}", o)), o => o);
        public static SqlType Float64 = new SqlType(DataType.Float64, "REAL", o => (double)o, o => o);
        public static SqlType Decimal = new SqlType(DataType.Decimal, "TEXT", o => decimal.Parse((string)o), o => String.Format("{0}", o));

        public static SqlType String8 = new SqlType(DataType.Text8, "TEXT", o => (string)o, o => o) { SqlPrimaryKeyTypeNames = new string[] { "TEXT" } };
        public static SqlType NByte = new SqlType(DataType.NByte, "BLOB", o => (byte[])o, o => o);

        public static SqlType Truth = new SqlType(DataType.Truth, "INTEGER", o => ((Convert.ToUInt64(o) & 1) == 1) ? true : false, o => (bool)o ? (byte)1 : (byte)0);
        public static SqlType DateTime = new SqlType(DataType.DateTime, "TEXT", o => System.DateTime.Parse(String.Format("{0}", o)), o => o);


        static SqlType()
        {
            dataTypeMap.Add(MeshKeyType.MeshDataType, MeshKeyType);
            dataTypeMap.Add(UnsignedInteger16.MeshDataType, UnsignedInteger16);
            dataTypeMap.Add(UnsignedInteger32.MeshDataType, UnsignedInteger32);
            dataTypeMap.Add(UnsignedInteger64.MeshDataType, UnsignedInteger64);
            dataTypeMap.Add(SignedInteger16.MeshDataType, SignedInteger16);
            dataTypeMap.Add(SignedInteger32.MeshDataType, SignedInteger32);
            dataTypeMap.Add(SignedInteger64.MeshDataType, SignedInteger64);
            dataTypeMap.Add(Byte.MeshDataType, Byte);
            dataTypeMap.Add(Truth.MeshDataType, Truth);
            dataTypeMap.Add(Float32.MeshDataType, Float32);
            dataTypeMap.Add(Float64.MeshDataType, Float64);
            dataTypeMap.Add(String8.MeshDataType, String8);
            dataTypeMap.Add(NByte.MeshDataType, NByte);
            dataTypeMap.Add(DateTime.MeshDataType, DateTime);
            dataTypeMap.Add(Decimal.MeshDataType, Decimal);

            //dataTypeMap.Add(Bit.MeshDataType, Bit);
        }


        /// <summary>
        /// Gets all the SQL types.
        /// </summary>
        /// <returns>All the SQL types.</returns>
        public static IEnumerable<SqlType> GetTypes()
        {
            return new HashSet<SqlType>(dataTypeMap.Values);
        }


    }
}
