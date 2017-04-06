// --------------------------------
// <copyright file="ThreadSafeDictionary.cs" company="Thomas Loehlein">
//     WebDavNet - A WebDAV client
//     Copyright (C) 2009 - Thomas Loehlein
//     This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
//     This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License along with this program; if not, see http://www.gnu.org/licenses/.
// </copyright>
// <author>Thomas Loehlein</author>
// <email>thomas.loehlein@gmail.com</email>
// ---------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WebDav
{
    // TODO: TL - missing documentation
    public class ThreadSafeDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        private readonly object _dictLock = new object();

        public virtual bool Remove(TKey key)
        {
            Monitor.Enter(_dictLock);

            try
            {
                _dict.Remove(key);

                return true;
            } 
            finally
            {
                Monitor.Exit(_dictLock);
            }
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            return Remove(pair.Key);
        }

        public virtual bool ContainsKey(TKey key)
        {
            Monitor.Enter(_dictLock);

            try
            {
                return _dict.ContainsKey(key);
            }
            finally
            {
                Monitor.Exit(_dictLock);
            }
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return ContainsKey(pair.Key);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            Monitor.Enter(_dictLock);

            try
            {
                return _dict.TryGetValue(key, out value);
            }
            finally
            {
                Monitor.Exit(_dictLock);
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                Monitor.Enter(_dictLock);

                try
                {
                    return _dict[key];
                }
                finally
                {
                    Monitor.Exit(_dictLock);
                }
            }
            set
            {
                Monitor.Enter(_dictLock);

                try
                {
                    _dict[key] = value;
                }
                finally
                {
                    Monitor.Exit(_dictLock);;
                }
            }
        }

        public virtual ICollection Keys
        {
            get
            {
                Monitor.Enter(_dictLock);

                try
                {
                    return _dict.Keys;
                }
                finally
                {
                    Monitor.Exit(_dictLock);
                }
            }
        }

        public virtual ICollection Values
        {
            get
            {
                Monitor.Enter(_dictLock);

                try
                {
                    return _dict.Values;
                }
                finally
                {
                    Monitor.Exit(_dictLock);
                }
            }
        }

        public virtual void Clear()
        {
            Monitor.Enter(_dictLock);

            try
            {
                _dict.Clear();
            }
            finally
            {
                Monitor.Exit(_dictLock);;
            }
        }

        public virtual int Count
        {
            get
            {
                Monitor.Enter(_dictLock);

                try
                {
                    return _dict.Count;
                }
                finally
                {
                    Monitor.Exit(_dictLock);
                }
            }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }


        public virtual void Add(TKey key, TValue value)
        {
            Monitor.Enter(_dictLock);

            try
            {
                _dict.Add(key, value);
            }
            finally
            {
                Monitor.Exit(_dictLock);;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            throw new NotSupportedException("Cannot enumerate a threadsafe dictionary. Instead, enumerate the keys or values collection");
        }
    }
}
