#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.SystemDomain;
using HularionText.Language.Json;
using HularionText.Language.Json.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionMesh.Serializer.Json
{

    /// <summary>
    /// A JSON serializer for Mesh Map objects.
    /// </summary>
    public class MapSerializer
    {
        private TypeSerializer mapSerializer = new TypeSerializer();
        private Type mapType = typeof(Map<,>);
        private Type dictionaryType = typeof(Dictionary<,>);

        public MapSerializer()
        {
        }

        public void SetSerializerTypes(JsonSerializer serializer)
        {
            mapSerializer.Serialize = detail => new JsonObject();

            mapSerializer.Deserialize = detail =>
            {
                var elements = ((JsonObject)detail.Element).Values;
                var element = elements.Where(x => x.Name == Map.MappingDomainValue).First();
                elements.Remove(element);
                object map = Activator.CreateInstance(mapType.MakeGenericType(detail.TypedValue.Type.GetGenericArguments()), new object[] { detail.ValueMap[element] });
                serializer.DeserializeJsonObjectMembers(detail, map);
                return map;
            };

            mapSerializer.GetNextDeserializationRequests = request =>
            {
                var result = new List<DeserializationRequest>();
                var nodes = request.Element.GetNextNodes();
                foreach (var node in nodes)
                {
                    if (node.Name == Map.MappingDomainValue)
                    {
                        result.Add(new DeserializationRequest()
                        {
                            Element = node,
                            Type = request.TypeNodeProvider.Provide(dictionaryType.MakeGenericType(request.Type.Generics[0].Type, request.Type.Generics[1].Type))
                        });
                    }
                    else
                    {
                        result.Add(new DeserializationRequest()
                        {
                            Element = node,
                            Type = request.TypeNodeProvider.Provide(request.Type.Members[node.Name].Type)
                        });
                    }
                }

                return result;
            };

            mapSerializer.GetNextValues = new Func<TypedValue, IEnumerable<TypedValue>>(value =>
            {
                var result = serializer.CreateStandardGetNextValues()(value).ToList();
                if (value.Value == null)
                {
                    return result;
                }
                var generics = value.Type.GetGenericArguments();
                if (generics.Length < 2 || generics[0] == null || generics[1] == null)
                {
                    return result;
                }
                result.Add(new TypedValue()
                {
                    Value = Map.ToDictionary(value.Value),
                    Name = Map.MappingDomainValue,
                    Type = dictionaryType.MakeGenericType(generics)
                });
                return result;
            });


            serializer.SetTypeSerializer(typeof(Map<,>), mapSerializer);

        }
    }
}
