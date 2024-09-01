#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Topology;
using HularionCore.TypeGraph;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.Memory;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using HularionMesh.SystemDomain;
using HularionMesh.User;
using HularionText.Language.Json;
using HularionText.Language.Json.Elements;
using HularionText.StringCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Prepares JSON serialization for mesh objects.
    /// </summary>
    public class MeshJsonSerializer
    {


        private const string nullString = "null";

        private MeshRepository repository { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshJsonSerializer()
        {
            repository = MemoryMeshRepositoryBuilder.CreateRepository(UserProfile.DefaultUser, null);
        }

        private JsonSerializer MakeSerializer()
        {
            var serializer = new JsonSerializer(new StringCaseModifier(StringCaseDefinition.Original));

            var meshKeySerializer = new TypeSerializer();
            meshKeySerializer.Serialize = detail => new JsonString() { Value = detail.TypedValue.Value == null ? MeshKey.NullKey.Serialized : ((IMeshKey)detail.TypedValue.Value).Serialized };
            serializer.SetTypeSerializer<IMeshKey>(meshKeySerializer);
            serializer.SetTypeSerializer<MeshKey>(meshKeySerializer);

            var dateTimeSerializer = new TypeSerializer();
            dateTimeSerializer.Serialize = detail => new JsonString() { Value = detail.TypedValue.Value == null ? null : DataType.DateTime.ObjectString(detail.TypedValue.Value) };
            serializer.SetTypeSerializer<DateTime>(dateTimeSerializer);


            return serializer;
        }

        private JsonSerializer MakeDeserializer(List<JsonObject> objects)
        {
            var serializer = new JsonSerializer(new StringCaseModifier(StringCaseDefinition.Original));

            var meshKeySerializer = new TypeSerializer();
            meshKeySerializer.Deserialize = detail =>
            {
                return MeshKey.Parse(detail.Element.GetStringValue());
            };
            serializer.SetTypeSerializer<IMeshKey>(meshKeySerializer);
            serializer.SetTypeSerializer<MeshKey>(meshKeySerializer);


            var domainObjectSerializer = new TypeSerializer();
            domainObjectSerializer.Deserialize = detail =>
            {
                objects.Add((JsonObject)detail.Element);
                return null;
            };
            serializer.SetTypeSerializer<DomainObject>(domainObjectSerializer);


            return serializer;
        }

        public string Serialize(MeshServicesFile file, JsonSerializationSpacing spacing)
        {
            var serializer = MakeSerializer();
            var text = serializer.Serialize(file, spacing);
            return text;
        }

        /// <summary>
        /// Deserialized the file content to a mesh-readable format.
        /// </summary>
        /// <param name="content">The content to deserialize.</param>
        /// <returns>The mesh-readable content.</returns>
        public MeshServicesFile Deserialize(string content)
        {
            var objects = new List<JsonObject>();
            var serializer = MakeDeserializer(objects);
            var file = serializer.Deserialize<MeshServicesFile>(content);
            file.Objects.Clear();
            var domains = file.Domains.ToDictionary(x => (IMeshKey)x.Key, x => x.GetDomain());
            var domainProvider = domains.MakeParameterizedProvider();
            var realizedDomains = domains.ToDictionary(x => x.Key, x=> new Dictionary<string, RealizedMeshDomain>());
            var realizedDomainPropertyTypes = new Dictionary<RealizedMeshDomain, Dictionary<string, DataType>>();
            var metas = MeshDomain.MetaProperties.ToDictionary(x => x.Name, x => DataType.FromKey(x.Type));
            var properties = domains.ToDictionary(x => x.Value, x => x.Value.Properties.ToDictionary(y => y.Name, y => DataType.FromKey(y.Type)));
            foreach (var jo in objects)
            {
                var domainObject = new DomainObject();
                domainObject.Key = MeshKey.Parse(jo.Values.Where(x => x.Name == MeshKeyword.Key.Alias).First().GetStringValue());
                var domainKey = domainObject.Key.GetKeyPart(MeshKeyPart.Domain);
                var domain = domains[domainKey];

                foreach (var meta in ((JsonObject)jo.Values.Where(x => x.Name == MeshKeyword.Meta.Alias).FirstOrDefault()).Values)
                {
                    var dataType = metas[meta.Name];
                    if (dataType == null || !dataType.CanParse) { continue; }
                    var valueString = meta.GetStringValue(false);
                    if (String.Format("{0}", valueString).Trim().ToLower() == nullString) { domainObject.Meta.Add(meta.Name, null); }
                    else
                    {
                        if (dataType == DataType.Text8)
                        {
                            if (valueString == null) { domainObject.Meta.Add(meta.Name, null); }
                            else { domainObject.Meta.Add(meta.Name, valueString.Trim(new char[] { '\"' })); }
                        }
                        else { domainObject.Meta.Add(meta.Name, dataType.Parse(valueString)); }
                    }
                }

                var processProperties = true;
                if (domainKey.EqualsKey(repository.SetDomain.Key))
                {
                    if (!domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { continue; }
                    var generics = MeshGeneric.Deserialize((string)domainObject.Meta[MeshKeyword.Generics.Alias]);
                    if (generics.Length == 0) { continue; }
                    var dataType = DataType.FromKey(generics[0].Key);
                    var valuesObject = (JsonObject)(jo.Values.Where(x => x.Name == "Values").First());
                    var valuesArray = (JsonArray)valuesObject.Values.Where(x => x.Name == "Items").FirstOrDefault();
                    if (valuesArray != null)
                    {
                        domainObject.Values.Add(MeshKeyword.UniqueSetDomainItems.Alias, UniqueSet.MakeGenericList(dataType.CSharpType, valuesArray.Values.Select(x => dataType.Parse(x.GetStringValue()))));
                    }
                    processProperties = false;
                }
                if (domainKey.EqualsKey(repository.MapDomain.Key))
                {

                    processProperties = false;
                }

                if (processProperties)
                {
                    string generics = string.Empty;
                    if (domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias))
                    {
                        generics = (string)domainObject.Meta[MeshKeyword.Generics.Alias];
                        if (String.IsNullOrWhiteSpace(generics)) { generics = string.Empty; }
                        if (!realizedDomains[domain.Key].ContainsKey(generics))
                        {
                            var realized = new RealizedMeshDomain(domain, MeshGeneric.Deserialize(generics), domainProvider);
                            realizedDomains[domain.Key][generics] = realized;
                            realizedDomainPropertyTypes[realized] = realized.Properties.ToDictionary(x => x.Name, x => DataType.FromKey(x.Type));
                        }
                    }
                    var realizedDomain = realizedDomains[domain.Key][generics];
                    var realizedProperties = realizedDomainPropertyTypes[realizedDomain];

                    foreach (var property in ((JsonObject)jo.Values.Where(x => x.Name == MeshKeyword.Values.Alias).FirstOrDefault()).Values)
                    {
                        var dataType = realizedProperties[property.Name];
                        if (dataType == null || !dataType.CanParse) { continue; }
                        var valueString = property.GetStringValue();
                        if (String.Format("{0}", valueString).Trim().ToLower() == nullString) { domainObject.Values.Add(property.Name, null); }
                        else
                        {
                            if (dataType == DataType.Text8)
                            {
                                if (valueString == null) { domainObject.Values.Add(property.Name, null); }
                                else { domainObject.Values.Add(property.Name, valueString.Trim(new char[] { '\"' })); }
                            }
                            else { domainObject.Values.Add(property.Name, dataType.Parse(valueString)); }
                        }
                    }
                }
                file.Objects.Add(domainObject);
            }
            return file;
        }

    }
}
