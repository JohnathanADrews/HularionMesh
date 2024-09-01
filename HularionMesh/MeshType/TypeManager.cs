#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Topology;
using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionMesh.MeshType
{
    /// <summary>
    /// Registers type manifestation instructions and applies them to manifest types.
    /// </summary>
    public class TypeManager
    {

        private Dictionary<Type, TypeManifest> types { get; set; } = new Dictionary<Type, TypeManifest>();
        private List<IDomainRepositoryMechanic> domainMechanics { get; set; }

        private static Type meshKeyType = typeof(IMeshKey);
        private static Type stringType = typeof(string);
        private static Type genericArrayType = typeof(MeshGeneric[]);
        private static Type dateTimeType = typeof(DateTime);

        /// <summary>
        /// Constructor.
        /// </summary>
        public TypeManager()
        {
            var mechanicType = typeof(IDomainRepositoryMechanic);
            var mechanics = Assembly.GetExecutingAssembly().GetTypes().Where(x=> x != mechanicType && mechanicType.IsAssignableFrom(x)).ToList();
            domainMechanics = mechanics.Select(x => (IDomainRepositoryMechanic)Activator.CreateInstance(x)).ToList();
            foreach(var mechanic in domainMechanics)
            {
                foreach (var type in mechanic.MappedTypes)
                {
                    AddManifestType(type);
                }
            }
        }

        /// <summary>
        /// Manifests a value having the provided type given the object.
        /// </summary>
        /// <typeparam name="T">The type to manifest.</typeparam>
        /// <param name="domainObject">The source domain object.</param>
        /// <returns>The manifested type value.</returns>
        public T Manifest<T>(DomainObject domainObject)
            where T : class
        {
            return (T)Manifest(typeof(T), domainObject);
        }

        /// <summary>
        /// Manifests a value having the provided type given the object.
        /// </summary>
        /// <param name="type">The type to manifest.</param>
        /// <param name="domainObject">The source domain object.</param>
        /// <returns>The manifested type value.</returns>
        public object Manifest(Type type, DomainObject domainObject)
        {
            AddManifestType(type);
            return types[type].ToTypeObject(domainObject);
        }

        /// <summary>
        /// Derives a domain object from the provided typed object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typedObject"></param>
        /// <returns>The domain object derived from the type object.</returns>
        public DomainObject Derive(Type type, object typedObject)
        {
            AddManifestType(type);
            return types[type].ToDomainObject(typedObject);
        }

        /// <summary>
        /// Updates the domain object usng the provided typed object.
        /// </summary>
        /// <param name="domainObject">The domain object.</param>
        /// <param name="typeObject">The typed object.</param>
        public void UpdateTypeObject(DomainObject domainObject, object typeObject)
        {
            if (typeObject == null) { return; }
            var type = typeObject.GetType();
            AddManifestType(type);
            if (!type.IsClass) { return; }
            types[type].UpdateTypeObject(domainObject, typeObject);
        }

        /// <summary>
        /// Adds the type manifest functionality if the type has not already been registered.
        /// </summary>
        /// <param name="type">The type to register.</param>
        private void AddManifestType(Type type)
        {
            //Create an interface that attributes can implement.  They will contain custom conversions.
            if (types.ContainsKey(type)) { return; }
            TypeManifest manifest = null;
            foreach(var mechanic in domainMechanics)
            {
                if (mechanic.CanCreateTypeManifest(type))
                {
                    manifest = mechanic.CreateTypeManifest(type);
                    break;
                }
            }
            lock (types)
            {
                if (types.ContainsKey(type)) { return; }
                if(manifest != null)
                {
                    types.Add(type, manifest);
                    return;
                }

                var memberAssigners = GetMemberAssigners(type);
                var reverseAssigners = GetReverseMemberAssigners(type);

                var properties = new List<PropertyInfo>();
                var propertyDomainObjectSkip = new HashSet<PropertyInfo>();

                var propertyMap = new Dictionary<PropertyInfo, Action<object, DomainObject>>();

                foreach (var property in type.GetProperties())
                {
                    properties.Add(property);
                    if (property.GetCustomAttribute<DomainPropertyAttribute>() == null){ continue; }
                    propertyDomainObjectSkip.Add(property);
                }

                var instanceCreator = new Func<DomainObject, object>(o => Activator.CreateInstance(type));

                var manifestor = new TypeManifest();

                manifestor.ToTypeObject = new Func<DomainObject, object>(domainObject =>
                {
                    var result = instanceCreator(domainObject);

                    foreach(var selector in memberAssigners) { selector(result, domainObject); }

                    foreach (var property in properties)
                    {
                        if (propertyMap.ContainsKey(property))
                        {
                            propertyMap[property](result, domainObject);
                            continue;
                        }
                        if (domainObject.Values.ContainsKey(property.Name)) { property.SetValue(result, domainObject.Values[property.Name]); }
                    }
                    return result;
                });

                manifestor.ToDomainObject = new Func<object, DomainObject>(value =>
                {
                    var result = new DomainObject();
                    foreach (var property in properties)
                    {
                        if (propertyDomainObjectSkip.Contains(property)) { continue; }
                        result.Values.Add(property.Name, property.GetValue(value));
                    }
                    foreach(var assigner in reverseAssigners) { assigner(value, result); }
                    return result;
                });

                manifestor.UpdateTypeObject = new Action<DomainObject, object>((domainObject, typeObject)=>
                {
                    foreach (var selector in memberAssigners) { selector(typeObject, domainObject); }
                    foreach (var property in properties)
                    {
                        if (propertyMap.ContainsKey(property) || !domainObject.Values.ContainsKey(property.Name)) { continue; }
                        property.SetValue(typeObject, domainObject.Values[property.Name]);
                    }
                });


                types.Add(type, manifestor);
            }
        }

        internal static bool IsType(Type subjectType, Type baseType)
        {
            if (subjectType == baseType) { return true; }
            if(subjectType.Assembly == baseType.Assembly && subjectType.Name == baseType.Name && subjectType.Namespace == baseType.Namespace){ return true; }
            return false;
        }

        internal static bool IsOrDerivedFrom(Type subjectType, Type baseType)
        {
            if(subjectType == baseType) { return true;  }
            var traverser = new TreeTraverser<Type>();
            var added = new HashSet<Type>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, subjectType,  node => {
                var types = new List<Type>();
                types.AddRange(node.GetInterfaces());
                if (node.BaseType != null) { types.Add(node.BaseType); }
                var next = types.Where(x => !added.Contains(x)).ToList();
                foreach(var type in next)
                {
                    added.Add(type);
                }
                return next.ToArray();
            }, true);
            foreach(var type in plan)
            {
                if(type == baseType) { return true; }
            }
            return false;
        }

        internal static List<Action<object, DomainObject>> GetMemberAssigners(Type type)
        {
            var memberAssigners = new List<Action<object, DomainObject>>();

            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute<DomainPropertyAttribute>();
                if (attribute == null) { continue; }
                var selector = attribute.Selector;
                switch (attribute.Selector)
                {
                    case DomainObjectPropertySelector.Key:
                        if (IsType(property.PropertyType, stringType)) { memberAssigners.Add((typeObject, domainObject) => { property.SetValue(typeObject, domainObject.Key.Serialized); }); }
                        if (IsOrDerivedFrom(property.PropertyType, meshKeyType)) { memberAssigners.Add((typeObject, domainObject) => { property.SetValue(typeObject, domainObject.Key); }); }                    
                        break;
                    case DomainObjectPropertySelector.Creator:
                        if (IsType(property.PropertyType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreator.Alias) || domainObject.Meta[MeshKeyword.ValueCreator.Alias] == null) { return; }
                                property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueCreator.Alias]).Serialized);
                            });
                        }
                        if (IsOrDerivedFrom(property.PropertyType, meshKeyType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreator.Alias)) { return; }
                                if (IsType(property.PropertyType, stringType)) { property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueCreator.Alias]).Serialized); }
                                if (IsOrDerivedFrom(property.PropertyType, meshKeyType)) { property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueCreator.Alias])); }
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.Updater:
                        if (IsType(property.PropertyType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias) || domainObject.Meta[MeshKeyword.ValueUpdater.Alias] == null) { return; }
                                property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueUpdater.Alias]).Serialized);
                            });
                        }
                        if (IsOrDerivedFrom(property.PropertyType, meshKeyType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias)) { return; }
                                if (IsType(property.PropertyType, stringType)) { property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueUpdater.Alias]).Serialized); }
                                if (IsOrDerivedFrom(property.PropertyType, meshKeyType)) { property.SetValue(typeObject, MeshKey.Parse(domainObject.Meta[MeshKeyword.ValueUpdater.Alias])); }
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.CreationTime:
                        if (IsType(property.PropertyType, dateTimeType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreationTime.Alias)) { return; }
                                property.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueCreationTime.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.UpdateTime:
                        if (IsType(property.PropertyType, dateTimeType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Alias)) { return; }
                                property.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueUpdateTime.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.SerializedGeneric:
                        if (IsType(property.PropertyType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { return; }
                                property.SetValue(typeObject, domainObject.Meta[MeshKeyword.Generics.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.Generic:
                        if (IsType(property.PropertyType, genericArrayType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { return; }
                                property.SetValue(typeObject, MeshGeneric.Deserialize(String.Format("{0}", domainObject.Meta[MeshKeyword.Generics.Alias])));
                            });
                        }
                        break;
                }
            }

            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DomainPropertyAttribute>();
                if (attribute == null) { continue; }
                var selector = attribute.Selector;
                switch (attribute.Selector)
                {
                    case DomainObjectPropertySelector.Key:
                        if (IsType(field.FieldType, stringType)) { memberAssigners.Add((typeObject, domainObject) => { field.SetValue(typeObject, domainObject.Key.Serialized); }); }
                        if (IsOrDerivedFrom(field.FieldType, meshKeyType)) { memberAssigners.Add((typeObject, domainObject) => { field.SetValue(typeObject, domainObject.Key); }); }
                        break;
                    case DomainObjectPropertySelector.Creator:
                        if (IsType(field.FieldType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreator.Alias) || domainObject.Meta[MeshKeyword.ValueCreator.Alias] == null) { return; }
                                field.SetValue(typeObject, ((IMeshKey)domainObject.Meta[MeshKeyword.ValueCreator.Alias]).Serialized);
                            });
                        }
                        if (IsOrDerivedFrom(field.FieldType, meshKeyType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreator.Alias)) { return; }
                                field.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueCreator.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.Updater:
                        if (IsType(field.FieldType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias) || domainObject.Meta[MeshKeyword.ValueUpdater.Alias] == null) { return; }
                                field.SetValue(typeObject, ((IMeshKey)domainObject.Meta[MeshKeyword.ValueUpdater.Alias]).Serialized);
                            });
                        }
                        if (IsOrDerivedFrom(field.FieldType, meshKeyType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdater.Alias)) { return; }
                                field.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueUpdater.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.CreationTime:
                        if (IsType(field.FieldType, dateTimeType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueCreationTime.Alias)) { return; }
                                field.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueCreationTime.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.UpdateTime:
                        if (IsType(field.FieldType, dateTimeType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.ValueUpdateTime.Alias)) { return; }
                                field.SetValue(typeObject, domainObject.Meta[MeshKeyword.ValueUpdateTime.Alias]);
                            });
                        }
                        break;
                    case DomainObjectPropertySelector.SerializedGeneric:
                        if (IsType(field.FieldType, stringType))
                        {
                            memberAssigners.Add((typeObject, domainObject) =>
                            {
                                if (!domainObject.Meta.ContainsKey(MeshKeyword.Generics.Alias)) { return; }
                                field.SetValue(typeObject, domainObject.Meta[MeshKeyword.Generics.Alias]);
                            });
                        }
                        break;
                }
            }
            return memberAssigners;
        }

        internal static List<Action<object, DomainObject>> GetReverseMemberAssigners(Type type)
        {
            var memberAssigners = new List<Action<object, DomainObject>>();

            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute<DomainPropertyAttribute>();
                if (attribute == null) { continue; }
                var selector = attribute.Selector;
                switch (attribute.Selector)
                {
                    case DomainObjectPropertySelector.Key:
                        memberAssigners.Add((typeObject, domainObject) =>
                        {
                            if (domainObject == null) { return; }
                            domainObject.Key = MeshKey.Parse(property.GetValue(typeObject));
                        });
                        break;
                }
            }

            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<DomainPropertyAttribute>();
                if (attribute == null) { continue; }
                var selector = attribute.Selector;
                switch (attribute.Selector)
                {
                    case DomainObjectPropertySelector.Key:
                        memberAssigners.Add((typeObject, domainObject) =>
                        {
                            if (domainObject == null) { return; }
                            domainObject.Key = MeshKey.Parse(field.GetValue(typeObject));
                        });
                        break;
                }
            }

            return memberAssigners;
        } 
    }

}
