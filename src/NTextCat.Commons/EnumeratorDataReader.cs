using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NTextCat.Commons
{
    public class EnumeratorDataReader<T> : IDataReader
    {
        private readonly PropertyInfo[] _properties;
        private readonly Func<T, object>[] _getters;
        private IEnumerator<T> _enumerator;

        public EnumeratorDataReader(IEnumerator<T> enumerator, IEnumerable<PropertyInfo> properties, IEnumerable<Func<T, object>> getters)
        {
            _enumerator = enumerator;
            _properties = properties.ToArray();
            _getters = getters.ToArray();
        }

        public EnumeratorDataReader(IEnumerator<T> enumerator, IEnumerable<PropertyInfo> properties)
            : this(enumerator, properties, GenerateGetters(properties))
        {
        }
        
        
        public EnumeratorDataReader(IEnumerator<T> enumerator, IEnumerable<string> propertyNames)
            : this(enumerator, propertyNames.Select(pn => typeof(T).GetProperty(pn)))
        {
        }

        public EnumeratorDataReader(IEnumerator<T> enumerator)
            : this(enumerator, typeof(T).GetProperties())
        {
        }

        private static IEnumerable<Func<T, object>> GenerateGetters(IEnumerable<PropertyInfo> propertyNames)
        {
            return propertyNames.Select(GenerateGetter);
        }

        private static Func<T, object> GenerateGetter(PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(T), "obj");
            var getter = Expression.MakeMemberAccess(parameter, property);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(getter, typeof(object)), parameter).Compile();
        }

        public void Close()
        {
            if (_enumerator != null)
            {
                _enumerator.Dispose();
                _enumerator = null;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public string GetName(int i)
        {
            return _properties[i].Name;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return _properties[i].PropertyType;
        }

        public object GetValue(int i)
        {
            return _getters[i](_enumerator.Current);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            var indexOf = Array.IndexOf(_properties, name);
            if (indexOf < 0)
                throw new KeyNotFoundException();
            return indexOf;
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public int FieldCount
        {
            get { return _properties.Length; }
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return _enumerator.MoveNext();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }
    }
}
