#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Identifier;
using HularionCore.Pattern.Set;
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HularionMesh
{
    /// <summary>
    /// Represents a key that can contain many component parts.
    /// </summary>
    public class MeshKey : IMeshKey
    {

        private static ObjectKey staticKey { get; set; } = new ObjectKey();

        /// <summary>
        /// A null-value key
        /// </summary>
        public static IMeshKey NullKey = MeshKey.Parse("NullKey");

        /// <summary>
        /// The type key for a MeshKey.
        /// </summary>
        //public static IMeshKey TypeKey = MeshKey.Parse(String.Format("HularionMeshKey{0}Key", staticKey.PartialSeparator));
        public static IMeshKey TypeKey { get { return DataType.MeshKey.Key; } }


        private ObjectKey key { get; set; } = new ObjectKey();

        private static Type meshKeyType = typeof(MeshKey);


        /// <summary>
        /// The serialized version of the key.
        /// </summary>
        public string Serialized { get { return key.Serialized; } set { key.Serialized = value; } }

        /// <summary>
        /// MUST return true iff this key's Serialized value equals the provided keys Serialized value.
        /// </summary>
        /// <param name="key">The key to compare to.</param>
        /// <returns>true iff this key's Serialized value equals the provided keys Serialized value.</returns>
        public bool EqualsKey(IMeshKey key)
        {
            if(key == null) { return false; }
            return (key.Serialized == this.Serialized);
        }

        public bool IsNull { get { return NullKey.EqualsKey(this); } }


        /// <summary>
        /// Creates a unique string tag.
        /// </summary>
        /// <returns>A unique string tag.</returns>
        public static string CreateUniqueTag()
        {
            return ObjectKey.CreateUniqueTag();
        }

        /// <summary>
        /// Creates a unique string tag and places it into a key.
        /// </summary>
        /// <returns>A key with a unique string tag.</returns>
        public static MeshKey CreateUniqueTagKey()
        {
            return MeshKey.Parse(ObjectKey.CreateUniqueTag());
        }

        /// <summary>
        /// Sets the part of the key indicated by partKey.
        /// </summary>
        /// <param name="partKey">The part of the key to set.</param>
        /// <param name="partValue">The value to associate with the part.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetPart(MeshKeyPart partKey, string partValue)
        {
            key.UpdateParts();
            key.SetPart(partKey.Name, partValue);
            return this;
        }

        /// <summary>
        /// Sets the s-type key part for this key.
        /// </summary>
        /// <param name="partValue">The value of the s-type key.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetPart(MeshKeyPart partKey, IMeshKey partValue)
        {
            key.UpdateParts();
            if (meshKeyType.IsAssignableFrom(partValue.GetType())) { key.SetPart(partKey.Name, ((MeshKey)partValue).key); }
            else
            {
                var newKey = ObjectKey.Parse(partValue.Serialized);
                key.SetPart(partKey.Name, newKey);
            }
            return this;
        }

        public IMeshKey SetPart(string partKey, string partValue)
        {
            key.UpdateParts();
            key.SetPart(partKey, partValue);
            return this;
        }

        /// <summary>
        /// Sets the s-type key part for this key.
        /// </summary>
        /// <param name="partValue">The value of the s-type key.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetSKeyPart(IMeshKey partValue)
        {
            return SetPart(MeshKeyPart.SKey, partValue);
        }

        /// <summary>
        /// Sets the t-type key part for this key.
        /// </summary>
        /// <param name="partValue">The value of the t-type key.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetTKeyPart(IMeshKey partValue)
        {
            return SetPart(MeshKeyPart.TKey, partValue);
        }

        /// <summary>
        /// Sets the s-type member part for this key.
        /// </summary>
        /// <param name="partValue">The name of the s-type member.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetSMemberPart(string partValue)
        {
            return SetPart(MeshKeyPart.SMember, partValue);
        }

        /// <summary>
        /// Sets the t-type member part for this key.
        /// </summary>
        /// <param name="partValue">The name of the t-type member.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetTMemberPart(string partValue)
        {
            return SetPart(MeshKeyPart.TMember, partValue);
        }

        /// <summary>
        /// Sets the unique part of the key to the given unique string value.
        /// </summary>
        /// <param name="partValue">The unique string value.</param>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetUniquePart(string partValue)
        {
            return SetPart(MeshKeyPart.Unique, partValue);
        }

        /// <summary>
        /// Sets the unique part of the key to a newly generated unique string value.
        /// </summary>
        /// <returns>this IMeshKey</returns>
        public IMeshKey SetUniquePart()
        {
            return SetPart(MeshKeyPart.Unique, ObjectKey.CreateUniqueTag());
        }

        /// <summary>
        /// Clones this, returning the copy.
        /// </summary>
        /// <returns>The cloned key.</returns>
        public IMeshKey Clone()
        {
            var newKey = new MeshKey() { Serialized = Serialized };
            newKey.key.UpdateParts();
            return newKey;
        }

        /// <summary>
        /// Gets the partial corresponding to the provided part.
        /// </summary>
        /// <param name="part">The part of the key to get.</param>
        /// <returns>The partial of the key.</returns>
        public IMeshKey GetKeyPart(MeshKeyPart part)
        {
            var keyPart = key.GetKeyPart(part.Name);
            if(keyPart == null) { return NullKey; }
            var newKey = keyPart.ToKey();
            return Parse(newKey.Serialized);
        }


        /// <summary>
        /// Gets the part of the key that indicates the domain and creates a new key from it.
        /// </summary>
        /// <returns>The domain key part of this key.</returns>
        public IMeshKey GetDomainKeyPart()
        {
            return GetKeyPart(MeshKeyPart.Domain);
        }

        /// <summary>
        /// Returns true iff the key is null or the key is the null key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>true iff the key is null or the key is the null key.</returns>
        public static bool KeyIsNull(IMeshKey key)
        {
            return (key == null || NullKey.EqualsKey(key));
        }

        /// <summary>
        /// Parses the given string, creating a new key.
        /// </summary>
        /// <param name="serialized">The string to parse.</param>
        /// <returns>The parsed key.</returns>
        public static MeshKey Parse(string serialized)
        {
            if (String.IsNullOrWhiteSpace(serialized)) { return (MeshKey)NullKey; }
            return new MeshKey() { Serialized = serialized.Trim(new char[] { '\"' }) };
        }

        /// <summary>
        /// Parses the given object, creating a new key.
        /// </summary>
        /// <param name="value">The object to parse.</param>
        /// <returns>The parsed object.</returns>
        public static MeshKey Parse(object value)
        {
            if(value == null) { return null; }
            if (value.GetType() == typeof(MeshKey)) { return (MeshKey)value; }
            if (value.GetType() == typeof(string)) { return new MeshKey() { Serialized = String.Format("{0}", value).Trim(new char[] { '\"' }) }; }
            return new MeshKey() { Serialized = String.Format("{0}", value.ToString()).Trim(new char[] { '\"' }) };
        }



        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (obj.GetType() != typeof(IMeshKey) && obj.GetType() != typeof(MeshKey)) { return false; }
            return this.Serialized == ((IMeshKey)obj).Serialized;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            var serialized = Serialized;
            return serialized.GetHashCode();
            //var h = serialized.GetHashCode();
            //lock (serialized)
            //{
            //    for (int i = 0; i < serialized.Length; i++)
            //    {
            //        hash += (byte)serialized[i];
            //    }
            //}
            //return hash;
        }

        public static bool operator ==(MeshKey key1, MeshKey key2)
        {
            if (Object.ReferenceEquals(key1, null))
            {
                if (Object.ReferenceEquals(key2, null)) { return true; }
                return false;
            }
            if (Object.ReferenceEquals(key2, null)) { return false; }
            return key1.Equals(key2);
        }

        public static bool operator !=(MeshKey key1, MeshKey key2)
        {
            return !(key1 == key2);
        }

        public override string ToString()
        {
            return Serialized;
        }

    }

}
