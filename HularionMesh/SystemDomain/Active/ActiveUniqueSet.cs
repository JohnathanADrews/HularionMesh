#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Domain;
using HularionMesh.DomainValue;
using HularionMesh.MeshType;
using HularionMesh.Repository;
using HularionMesh.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionMesh.SystemDomain.Active
{
    /// <summary>
    /// Updates the repository as items are added and deleted.
    /// </summary>
    /// <typeparam name="ItemType">The type of items in the set.</typeparam>
    public class ActiveUniqueSet<ItemType>
    {
        /// <summary>
        /// The key of the set.
        /// </summary>
        public IMeshKey Key { get; private set; }

        /// <summary>
        /// The repository containing the set.
        /// </summary>
        public MeshRepository Repository { get; private set; }

        /// <summary>
        /// The profile of the user modifying the set.
        /// </summary>
        public UserProfile UserProfile { get; private set; }

        /// <summary>
        /// The kind of item type in the mesh context.
        /// </summary>
        public ItemTypeMeshMode MeshMode { get; private set; } = ItemTypeMeshMode.Unknown;

        /// <summary>
        /// The domain of the item type. Set only if the item type is a domain type.
        /// </summary>
        public MeshDomain ItemDomain { get; set; } = null;

        /// <summary>
        /// The data type of the item type. Set only if the item type is a domain type.
        /// </summary>
        public DataType ItemDataType { get; set; } = null;

        /// <summary>
        /// Constructor, using an existing set.
        /// </summary>
        /// <param name="key">The key of the set.</param>
        /// <param name="repository">The repository containing the set.</param>
        /// <param name="userProfile">The profile of the user modifying the set.</param>
        public ActiveUniqueSet(IMeshKey key, MeshRepository repository, UserProfile userProfile)
        {
            this.Key = key;
            this.Repository = repository;
            this.UserProfile = userProfile;

            Initialize();
        }

        /// <summary>
        /// Constructor, creating a new set.
        /// </summary>
        /// <param name="repository">The repository containing the set.</param>
        /// <param name="userProfile">The profile of the user modifying the set.</param>
        public ActiveUniqueSet(MeshRepository repository, UserProfile userProfile)
        {
            this.Repository = repository;
            this.UserProfile = userProfile;
            var set = new UniqueSet<ItemType>();
            repository.Save(UserProfile, set);
            this.Key = set.Key;
            Initialize();
        }

        private void Initialize()
        {
            var itemType = typeof(ItemType);

            ItemDomain = Repository.GetDomainFromType(itemType);
            if (ItemDomain != null)
            {
                MeshMode = ItemTypeMeshMode.DomainType;
                return;
            }

            if (DataType.TypeIsKnown(itemType))
            {
                ItemDataType = DataType.FromCSharpType<ItemType>();
                MeshMode = ItemTypeMeshMode.DataType;
                return;
            }
        }

        /// <summary>
        /// Adds the items to the set.
        /// </summary>
        /// <param name="items">The items to add to the set.</param>
        public void Add(IEnumerable<ItemType> items)
        {
            if(MeshMode == ItemTypeMeshMode.DataType)
            {
                var set = Repository.QueryTree<UniqueSet<ItemType>>(this.Key).First;
                if(set!= null)
                {
                    set.AddMany(items.ToArray());
                    Repository.Save(UserProfile, set);
                }
            }
            if(MeshMode == ItemTypeMeshMode.DomainType)
            {
                var set = Repository.QueryTree<UniqueSet<ItemType>>(this.Key).First;
                items = items.Distinct().ToList();

                var map = items.ToDictionary(x=>x, x=> DomainObject.Derive(x));
                var objects = items.Select(x => DomainObject.Derive(x));
                var newObjects = map.Where(x => MeshKey.KeyIsNull(x.Value.Key)).Select(x=>x.Key).ToList();
                Repository.Save(UserProfile, newObjects.ToArray());

                var keys = items.Select(x => DomainObject.Derive(x)).Select(x => x.Key).ToArray();

                UniqueSet.Link(Repository, Key, keys);
            }
        }

        /// <summary>
        /// Adds the items to the set.
        /// </summary>
        /// <param name="items">The items to add to the set.</param>
        public void Add(params ItemType[] items)
        {
            Add(items.ToList());
        }

        /// <summary>
        /// Adds the items with the provided keys to the set.
        /// </summary>
        /// <param name="itemKeys">The keys of the items to add to the set.</param>
        public void Add(IEnumerable<IMeshKey> itemKeys)
        {
            if (MeshMode == ItemTypeMeshMode.DomainType)
            {
                UniqueSet.Link(Repository, Key, itemKeys.ToArray());
            }
        }

        /// <summary>
        /// Adds the items with the provided keys to the set.
        /// </summary>
        /// <param name="itemKeys">The keys of the items to add to the set.</param>
        public void Add(params IMeshKey[] itemKeys)
        {
            Add(itemKeys.ToList());
        }

        /// <summary>
        /// Removes the items from the set.
        /// </summary>
        /// <param name="items">The items to remove from the set.</param>
        public void Remove(IEnumerable<ItemType> items)
        {
            if (MeshMode == ItemTypeMeshMode.DataType)
            {
                var set = Repository.QueryTree<UniqueSet<ItemType>>(this.Key).First;
                if (set != null)
                {
                    foreach(var item in items)
                    {
                        set.Remove(item);
                    }
                    Repository.Save(UserProfile, set);
                }
            }
            if (MeshMode == ItemTypeMeshMode.DomainType)
            {
                items = items.Distinct().ToList();
                var keys = items.Select(x => DomainObject.Derive(x)).Select(x => x.Key).ToArray();
                UniqueSet.Unlink(Repository, Key, keys);
            }
        }

        /// <summary>
        /// Removes the items from the set.
        /// </summary>
        /// <param name="items">The items to remove from the set.</param>
        public void Remove(params ItemType[] items)
        {
            Remove(items.ToList());
        }

        /// <summary>
        /// Removes the items with the provided keys from the set.
        /// </summary>
        /// <param name="itemKeys">The keys of the items to remove from the set.</param>
        public void Remove(IEnumerable<IMeshKey> itemKeys)
        {
            if (MeshMode == ItemTypeMeshMode.DomainType)
            {
                UniqueSet.Unlink(Repository, Key, itemKeys.ToArray());
            }
        }

        /// <summary>
        /// Removes the items with the provided keys from the set.
        /// </summary>
        /// <param name="itemKeys">The keys of the items to remove from the set.</param>
        public void Remove(params IMeshKey[] itemKeys)
        {
            Remove(itemKeys.ToList());
        }

        /// <summary>
        /// The kind of data contained in the UniqueSet
        /// </summary>
        public enum ItemTypeMeshMode
        {
            Unknown,
            DomainType,
            DataType
        }
    }
}
