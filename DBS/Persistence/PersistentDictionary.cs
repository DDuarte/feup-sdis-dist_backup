﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Community.CsharpSqlite.SQLiteClient;

namespace DBS.Persistence
{
    [DebuggerDisplay("Count = {Count}, DB = {_conn.Database}")]
    public class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
        where TValue : new()
    {
        private readonly string _tableName;
        private readonly Dictionary<TKey, TValue> _dict;
        private readonly SqliteConnection _conn;
// ReSharper disable once StaticFieldInGenericType
        private static readonly object _syncRoot = new object();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public PersistentDictionary(string db, string table)
        {
            if (string.IsNullOrWhiteSpace(db))
                throw new ArgumentNullException("db");
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentNullException("table");

            _tableName = table;
            _dict = new Dictionary<TKey, TValue>();

            var cs = "Data Source=file:" + db + ".sqlite";

            try
            {
                _conn = new SqliteConnection(cs);
                _conn.Open();

                var stm = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='" + _tableName + "'";
                string o;
                if (!ExecuteScalar(stm, out o))
                    throw new Exception("Could not execute query: " + stm);
                if (o.Length == 0) // new db
                {
                    stm = string.Format("CREATE TABLE {0} ({1}, {2}, PRIMARY KEY ({3}))",
                        _tableName,
                        DBFields<TKey>.FieldsWithType,
                        DBFields<TValue>.FieldsWithType,
                        DBFields<TKey>.Fields);
                    if (!ExecuteNonQuery(stm))
                        throw new Exception("Could not execute query: " + stm);
                }
                else
                {
                    stm = string.Format("SELECT {0}, {1} FROM {2}", DBFields<TKey>.Fields,
                        DBFields<TValue>.Fields, _tableName);
                    using (var cmd = new SqliteCommand(stm, _conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var keyType = typeof (TKey);
                                TKey newKey;
                                if (keyType.IsPrimitive || keyType == typeof (string))
                                {
                                    newKey = (TKey) reader[DBFields<TKey>.Fields];
                                }
                                else
                                {
                                    newKey = Activator.CreateInstance<TKey>();
                                    var props = newKey.GetType().GetProperties();
                                    foreach (var prop in props)
                                    {
                                        var obj = reader[prop.Name];
                                        prop.SetValue(newKey, Convert.ChangeType(obj, prop.PropertyType));
                                    }
                                }

                                TValue newValue;
                                var valueType = typeof(TValue);
                                if (valueType.IsPrimitive || valueType == typeof(string))
                                {
                                    newValue = (TValue) reader[DBFields<TValue>.Fields];
                                }
                                else
                                {
                                    newValue = Activator.CreateInstance<TValue>();
                                    var props = newValue.GetType().GetProperties();
                                    foreach (var prop in props)
                                    {
                                        var obj = reader[prop.Name];
                                        prop.SetValue(newValue, Convert.ChangeType(obj, prop.PropertyType));
                                    }
                                }

                                _dict.Add(newKey, newValue);
                            }
                        }
                    }
                }
            }
            catch (SqliteException ex)
            {
                Core.Instance.Log.Error("PersistentDictionary()", ex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            var stm = string.Format("DELETE FROM {0}", _tableName);
            if (ExecuteNonQuery(stm))
                _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>) _dict).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>) _dict).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>) _dict).IsReadOnly; }
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            var stm = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ({3}, {4})",
                _tableName,
                DBFields<TKey>.Fields, DBFields<TValue>.Fields,
                DBFields<TKey>.Values(key), DBFields<TValue>.Values(value));

            if (ExecuteNonQuery(stm))
            {
                try
                {
                    _dict.Add(key, value);
                }
                catch (ArgumentException) // dup keys, np
                {
                }
            }
        }

        public bool Remove(TKey key)
        {
            var stm = string.Format("DELETE FROM {0} WHERE {1}", _tableName, DBFields<TKey>.FieldsAndValues(key, " AND "));
            return ExecuteNonQuery(stm) && _dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _dict[key]; }
            set
            {
                if (!_dict.ContainsKey(key))
                    Add(key, value);
                else
                {
                    var stm = string.Format("UPDATE {0} SET {1} WHERE {2}",
                        _tableName,
                        DBFields<TValue>.FieldsAndValues(value, " , "),
                        DBFields<TKey>.FieldsAndValues(key, " AND "));
                    if (ExecuteNonQuery(stm))
                        _dict[key] = value;
                }
            } 
        }

        public ICollection<TKey> Keys
        {
            get { return _dict.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _dict.Values; }
        }

        protected virtual void Dispose(bool d)
        {
            if (!d) return;
            if (_conn == null) return;
            try
            {
                _conn.Close();
                _conn.Dispose();
            }
            catch (SqliteException) { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private bool ExecuteScalar(string stm, out string ret)
        {
            lock (_syncRoot)
            {
                try
                {
                    using (var cmd = new SqliteCommand(stm, _conn))
                        ret = Convert.ToString(cmd.ExecuteScalar());
                    return true;
                }
                catch (SqliteException ex)
                {
                    Core.Instance.Log.Error("ExecuteScalar()", ex);
                    ret = null;
                    //throw;
                    return false;
                }
            }
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private bool ExecuteNonQuery(string stm)
        {
            lock (_syncRoot)
            {
                try
                {
                    using (var cmd = new SqliteCommand(stm, _conn))
                        cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqliteException ex)
                {
                    Core.Instance.Log.Error("ExecuteNonQuery()", ex);
                    //throw;
                    return false;
                }
            }
        }
    }
}
