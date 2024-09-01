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
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionMesh.Domain
{
    /// <summary>
    /// Contains a mesh domain and any additional modifiers
    /// </summary>
    public class RealizedMeshDomain
    {
        /// <summary>
        /// The key for this realized domain.
        /// </summary>
        public IMeshKey Key { get { return MeshKey.Parse(String.Format("{0}-{1}", Domain.Key, SerializedGenerics)); } }

        /// <summary>
        /// The mesh domain.
        /// </summary>
        public MeshDomain Domain { get; set; }

        /// <summary>
        /// The generic-realized properties.
        /// </summary>
        public List<ValueProperty> Properties { get; set; } = new List<ValueProperty>();

        /// <summary>
        /// The generics assigned to a particular mesh domain type.
        /// </summary>
        public IDictionary<string, MeshGeneric> GenericsArguments { get; set; } = new Dictionary<string, MeshGeneric>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain">The domain from which the realized domain is derived.</param>
        /// <param name="genericsArguments">The generics to apply to the domain.</param>
        /// <param name="domainProvider">Provides a domain given the domain's key.</param>
        public RealizedMeshDomain(MeshDomain domain, MeshGeneric[] genericsArguments, IParameterizedProvider<IMeshKey, MeshDomain> domainProvider)
        {
            Domain = domain;
            GenericsArguments = genericsArguments.ToDictionary(x => x.Name, x => x);
            foreach (var property in domain.Properties)
            {
                var assignedProperty = property.Clone();
                assignedProperty.Generics = MeshGeneric.AssignArguments(assignedProperty.Generics.ToArray(), GenericsArguments).ToList();
                if (property.IsGenericParameter) //The property has a type of a generic parameter.
                {
                    var generics = genericsArguments.Where(x => x.Name == property.Type).First();
                    assignedProperty.Type = generics.Key.ToString();
                    assignedProperty.Generics = MeshGeneric.CloneAll(generics.Generics.ToArray()).ToList();
                    var genericDomain = domainProvider.Provide(MeshKey.Parse(assignedProperty.Type));
                    if (genericDomain != null)
                    {
                        for (var i = 0; i < genericDomain.GenericsParameters.Count(); i++) { assignedProperty.Generics[i].Name = genericDomain.GenericsParameters[i].Name; }
                    }
                }
                if (property.HasGenerics && assignedProperty.Domain != null) //The property has a type that is generic. Feed the generic arguments to the property.
                {
                    var generics = new List<MeshGeneric>();
                    for (var i = 0; i < assignedProperty.Generics.Count(); i++)
                    {
                        if (assignedProperty.Generics[i].Mode == TypeGenericMode.Parameter)
                        {
                            generics.Add(genericsArguments.Where(x => x.Name == assignedProperty.Generics[i].Name).First().Clone());
                        }
                        if (assignedProperty.Generics[i].Mode == TypeGenericMode.Argument)
                        {
                            generics.Add(assignedProperty.Generics[i].Clone());
                        }
                    }
                    for (var i = 0; i < assignedProperty.Domain.GenericsParameters.Count(); i++)
                    {
                        generics[i].Name = assignedProperty.Domain.GenericsParameters[i].Name;
                    }
                    assignedProperty.Generics = generics;
                }
                Properties.Add(assignedProperty);
            }
        }
        /// <summary>
        /// Removes null and proxy members from the domain object.
        /// </summary>
        /// <param name="domainObject">The domain object.</param>
        public void Prune(DomainObject domainObject)
        {
            Domain.Prune(domainObject, GenericsArguments);
        }

        /// <summary>
        /// The serialized generics using the selected types.
        /// </summary>
        public string SerializedGenerics { get { return MeshGeneric.SerializeGenerics(Domain.GenericsParameters.Select(x => GenericsArguments[x.Name]).ToArray()); } }

        public override string ToString()
        {
            return String.Format("RealizedMeshDomain - {0}{1}", Domain.Key, SerializedGenerics);
        }
    }
}
