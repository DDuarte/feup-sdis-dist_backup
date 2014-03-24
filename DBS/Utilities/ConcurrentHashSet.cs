using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

// ConcurrentHashSet<T> impl that uses a HashSet<T> and a ReaderWriterLockSlim under the hood
// Original code by Ben Mosher (http://stackoverflow.com/questions/4306936/how-to-implement-concurrenthashset-in-net)
// Modified to fully implement ISet<T>

namespace DBS.Utilities
{
    public class ConcurrentHashSet<T> : ISet<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> _hashSet;

        public ConcurrentHashSet()
        {
            _hashSet = new HashSet<T>();
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            _hashSet = new HashSet<T>(comparer);
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            _hashSet = new HashSet<T>(collection);
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _hashSet = new HashSet<T>(collection, comparer);
        }

        public bool Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return _hashSet.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.UnionWith(other);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterWriteLock();
                _lock.EnterUpgradeableReadLock();
                _hashSet.IntersectWith(other);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.ExceptWith(other);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.SymmetricExceptWith(other);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.IsSubsetOf(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.IsSupersetOf(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.IsProperSupersetOf(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.IsProperSubsetOf(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.Overlaps(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.SetEquals(other);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        void ICollection<T>.Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                _lock.EnterWriteLock();
                _hashSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return _hashSet.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _hashSet.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return ((ICollection<T>)_hashSet).IsReadOnly;
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                _lock.EnterReadLock();
                return _hashSet.GetEnumerator();
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            try
            {
                _lock.EnterReadLock();
                return ((IEnumerable)_hashSet).GetEnumerator();
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        ~ConcurrentHashSet()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_lock != null)
                    _lock.Dispose();
        }
    }
}
