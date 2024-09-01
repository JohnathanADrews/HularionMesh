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
using HularionMesh.MeshType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.Domain
{
    /// <summary>
    /// The details for a generic component of a domain.
    /// </summary>
    public class MeshGeneric
    {
        /// <summary>
        /// The key of the domain or primitive type.
        /// </summary>
        public IMeshKey Key { get; set; }

        /// <summary>
        /// The name of the generic.  This is used to match members of the generic type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The mode of the generic. Parameter or Argument.
        /// </summary>
        public TypeGenericMode Mode { get; set; }

        /// <summary>
        /// The generics of the presented type.
        /// </summary>
        public List<MeshGeneric> Generics { get; set; } = new List<MeshGeneric>();

        /// <summary>
        /// Gets the serialized generic string.
        /// </summary>
        public string Serialized { get { return MeshGeneric.SerializeGenerics(this); } }

        public MeshGeneric Clone()
        {
            return MeshGeneric.CloneAll(new MeshGeneric[] { this })[0];
            //return new MeshGeneric() { Key = Key, Name = Name, Mode = Mode };
        }

        public static IDictionary<string, MeshGeneric> ExtractArguments(MeshGeneric[] generics)
        {
            var result = new Dictionary<string, MeshGeneric>();
            var traverser = new TreeTraverser<MeshGeneric>();
            foreach (var generic in generics)
            {
                var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, generic, node => node.Generics.ToArray(), true);
                foreach (var item in plan)
                {
                    if (!String.IsNullOrWhiteSpace(item.Name) && !result.ContainsKey(item.Name)) { result.Add(item.Name, item); }
                }
            }
            return result;
        }

        public static MeshGeneric[] AssignArguments(MeshGeneric[] generics, IDictionary<string, MeshGeneric> arguments)
        {
            var result = new List<MeshGeneric>();
            var traverser = new TreeTraverser<MeshGeneric>();
            var map = new Dictionary<MeshGeneric, MeshGeneric>();
            foreach (var generic in generics)
            {
                var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, generic, node => node.Generics.ToArray(), true);
                foreach (var item in plan) { map.Add(item, item.Clone()); }
                foreach (var item in plan) { map[item].Generics = item.Generics.Select(x => map[x]).ToList(); }
                foreach (var item in plan)
                {
                    var mapped = map[item];
                    if ((mapped.Key == null || mapped.Key.IsNull) && !String.IsNullOrWhiteSpace(mapped.Name) && arguments.ContainsKey(mapped.Name))
                    {
                        mapped.Key = arguments[item.Name].Key;
                        mapped.Generics = arguments[item.Name].Generics;
                    }
                }
                foreach (var item in plan)
                {
                    map[item].Name = null;
                    map[item].Mode = TypeGenericMode.Argument;
                }
                result.Add(map[plan[0]]);
            }
            return result.ToArray();
        }


        public static MeshGeneric[] FromType<T>(IParameterizedProvider<Type, IMeshKey> typeKeyProvider)
        {
            return FromType(typeof(T), typeKeyProvider);
        }

        public static MeshGeneric[] FromType(Type type, IParameterizedProvider<Type, IMeshKey> typeKeyProvider)
        {
            var traverser = new TreeTraverser<Type>();
            var types = new Dictionary<Type, MeshGeneric>();

            types.Add(type, new MeshGeneric() { Mode = TypeGenericMode.Argument });
            traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, type, node =>
            {
                var generic = types[node];
                generic.Key = typeKeyProvider.Provide(node);
                if (generic.Key == null) { generic.Key = MeshKey.NullKey; }
                if (generic.Key.IsNull)
                {
                    generic.Mode = TypeGenericMode.Parameter;
                    generic.Name = node.Name;
                }
                var genericTypes = node.GenericTypeArguments;
                if (genericTypes.Length == 0 && node.IsGenericType) { genericTypes = node.GetTypeInfo().GenericTypeParameters; }
                var next = genericTypes.Where(x => !types.ContainsKey(x)).Distinct().ToArray();
                foreach (var genericType in genericTypes)
                {
                    if (!types.ContainsKey(genericType)) { types.Add(genericType, new MeshGeneric() { Mode = TypeGenericMode.Argument }); }
                    generic.Generics.Add(types[genericType]);
                }
                return next;
            }, true);
            var result = types[type].Clone();
            if (type.IsGenericType)
            {
                var parameters = type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
                for (var i = 0; i < parameters.Length; i++)
                {
                    result.Generics[i].Name = parameters[i].Name;
                }
            }
            return result.Generics.Select(x=>x.Clone()).ToArray();

        }

        /// <summary>
        /// Returns true iff the generics or any of the generics they contain is a parameter.
        /// </summary>
        /// <returns>true iff the generics or any of the generics they contain is a parameter.</returns>
        public static bool ContainsParameter(MeshGeneric[] generics)
        {
            var traverser = new TreeTraverser<MeshGeneric>();
            var root = new MeshGeneric() { Mode = TypeGenericMode.Argument, Generics = generics.ToList() };
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node => node.Generics.ToArray(), true);
            foreach (var node in plan) { if (node.Mode == TypeGenericMode.Parameter) { return true; } }
            return false;
        }

        private static readonly char startArgument = '(';
        private static readonly char endArgument = ')';
        private static readonly char startParameter = '<';
        private static readonly char endParameter = '>';

        /// <summary>
        /// Deserializes the generics string into a generics tree.
        /// </summary>
        /// <param name="serialized">The serialized generics.</param>
        /// <returns>The generics tree.</returns>
        public static MeshGeneric[] Deserialize(string serialized)
        {
            var generics = new List<MeshGeneric>();
            if (String.IsNullOrWhiteSpace(serialized)) { return generics.ToArray(); }
            var stack = new Stack<Tuple<int, MeshGeneric, int[]>>();
            for (var i = 0; i < serialized.Length; i++)
            {
                var value = serialized[i];
                if (value == startArgument || value == startParameter)
                {
                    var generic = new MeshGeneric();
                    if (value == startArgument) { generic.Mode = TypeGenericMode.Argument; }
                    if (value == startParameter) { generic.Mode = TypeGenericMode.Parameter; }
                    if (stack.Count == 0) { generics.Add(generic); }
                    else
                    {
                        var peek = stack.Peek();
                        if (peek.Item2.Generics.Count() == 0) { peek.Item3[0] = i; }
                        peek.Item2.Generics.Add(generic);
                    }
                    stack.Push(new Tuple<int, MeshGeneric, int[]>(i, generic, new int[] { 0 }));
                }
                if (value == endArgument || value == endParameter)
                {
                    var top = stack.Pop();
                    var content = string.Empty;
                    if (top.Item2.Generics.Count() == 0) { content = serialized.Substring(top.Item1 + 1, i - top.Item1 - 1); }
                    else { content = serialized.Substring(top.Item1 + 1, top.Item3[0] - top.Item1 - 1); }

                    if (content.Contains("="))
                    {
                        var colonIndex = content.IndexOf("=");
                        top.Item2.Name = content.Substring(0, colonIndex);
                        top.Item2.Key = MeshKey.Parse(content.Substring(colonIndex + 1, content.Length - colonIndex - 1));
                    }
                    else { top.Item2.Key = MeshKey.Parse(content); }
                }
            }
            return generics.ToArray();
        }

        /// <summary>
        /// Serializes the generics tree into a string using type keys, preserving the tree structure.
        /// </summary>
        /// <param name="generics">The generics to serialize.</param>
        /// <returns>The serialized generics tree.</returns>
        public static string SerializeGenerics(params MeshGeneric[] generics)
        {
            var traverser = new TreeTraverser<MeshGeneric>();
            var root = new MeshGeneric() { Generics = generics.ToList() };
            var result = new StringBuilder();
            traverser.WeaveExecute(TreeWeaveOrder.FromLeft, root, node => node.Generics.ToArray(),
                nodeEntry =>
                {
                    if (nodeEntry.Subject != root)
                    {
                        if (nodeEntry.Subject.Mode == TypeGenericMode.Parameter)
                        {
                            result.Append(startParameter);
                            result.Append(nodeEntry.Subject.Name);
                        }
                        else
                        {
                            result.Append(startArgument);
                            if (!String.IsNullOrWhiteSpace(nodeEntry.Subject.Name))
                            {
                                result.Append(nodeEntry.Subject.Name);
                                result.Append("=");
                            }
                            result.Append(nodeEntry.Subject.Key.Serialized);
                        }
                    }
                },
                nodeExit =>
                {
                    if (nodeExit.Subject != root)
                    {
                        if (nodeExit.Subject.Mode == TypeGenericMode.Parameter) { result.Append(endParameter); }
                        else { result.Append(endArgument); }
                    }
                },
                nodeLast =>
                {
                },
                nodeUp =>
                {
                });
            return result.ToString();
        }

        public static MeshGeneric CloneTop(MeshGeneric generic)
        {
            return new MeshGeneric() { Key = generic.Key, Name = generic.Name, Mode = generic.Mode };
        }

        public static MeshGeneric[] CloneAll(MeshGeneric[] generics)
        {
            var traverser = new TreeTraverser<GenericContainer>();
            //var root = new GenericContainer() { Next = generics.Select(x=>new GenericContainer() { Generic = x }).ToList() };
            var roots = generics.Select(x => new GenericContainer() { Generic = x }).ToList();
            foreach (var root in roots)
            {
                var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
                {
                    node.Clone = CloneTop(node.Generic);
                    node.Next = node.Generic.Generics.Select(x => new GenericContainer() { Generic = x }).ToList();
                    return node.Next.ToArray();
                }, true);
                foreach (var item in plan)
                {
                    item.Clone.Generics = item.Next.Select(x => x.Clone).ToList();
                }
            }

            return roots.Select(x=>x.Clone).ToArray();
        }


        private class GenericContainer
        {
            public MeshGeneric Generic { get; set; }

            public MeshGeneric Clone { get; set; }

            public List<GenericContainer> Next { get; set; } = new List<GenericContainer>();
        }
    }

    /// <summary>
    /// The possible modes for a generic.
    /// </summary>
    public enum TypeGenericMode
    {
        /// <summary>
        /// The generic is a variable, which can be set when a generic domain is assigned generic values.
        /// </summary>
        Parameter,
        /// <summary>
        /// The generic is an realized domain, having generics which are realized domains or data types.
        /// </summary>
        Argument
    }
}
