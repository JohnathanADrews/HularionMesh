#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.TypeGraph;
using HularionMesh.MeshType;
using HularionMesh.SystemDomain;
using HularionText.Language.Json;
using HularionText.Language.Json.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HularionMesh.Serializer.Json
{
    /// <summary>
    /// A JSON serializer for Mesh UniqueSet objects.
    /// </summary>
    public class UniqueSetSerializer
    {
        private TypeSerializer uniqueSetSerializer = new TypeSerializer();
        private Type uniqueSetType = typeof(UniqueSet<>);
        private Type listType = typeof(List<>);

        public UniqueSetSerializer()
        {
        }

        public void SetSerializerTypes(JsonSerializer serializer)
        {
            uniqueSetSerializer.Serialize = detail => new JsonObject();

            uniqueSetSerializer.Deserialize = detail =>
            {
                var elements = ((JsonObject)detail.Element).Values;
                var element = elements.Where(x => x.Name == UniqueSet.UniqueSetItemsDomainValue).First();
                elements.Remove(element);
                object uniqueSet = Activator.CreateInstance(uniqueSetType.MakeGenericType(detail.TypedValue.Type.GetGenericArguments().First()), new object[] { detail.ValueMap[element] });
                serializer.DeserializeJsonObjectMembers(detail, uniqueSet);
                return uniqueSet;
            };

            uniqueSetSerializer.GetNextDeserializationRequests = request =>
            {
                var result = new List<DeserializationRequest>();
                var nodes = request.Element.GetNextNodes();
                foreach(var node in nodes)
                {
                    if(node.Name == UniqueSet.UniqueSetItemsDomainValue)
                    {
                        result.Add(new DeserializationRequest()
                        {
                            Element = node,
                            Type = request.TypeNodeProvider.Provide(listType.MakeGenericType(request.Type.Generics.First().Type))
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

            uniqueSetSerializer.GetNextValues = new Func<TypedValue, IEnumerable<TypedValue>>(value =>
            {
                var result = serializer.CreateStandardGetNextValues()(value).ToList();
                if (value.Value == null)
                {
                    return result;
                }
                var genericType = value.Type.GetGenericArguments().FirstOrDefault();
                if (genericType == null)
                {
                    return result;
                }
                result.Add(new TypedValue()
                {
                    Value = UniqueSet.MakeGenericList(genericType, UniqueSet.GetUniqueSetValues(value.Value)),
                    Name = serializer.MemberNameCaseModifier.CaseTransform.Transform(UniqueSet.UniqueSetItemsDomainValue),
                    Type = listType.MakeGenericType(new Type[] { uniqueSetType })
                });
                return result;
            });


            serializer.SetTypeSerializer(typeof(UniqueSet<>), uniqueSetSerializer);

        }
    }
}
