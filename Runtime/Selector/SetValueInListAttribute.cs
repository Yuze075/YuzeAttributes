using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SetValueInListAttribute : PropertyAttribute
    {
        public string ValuesName { get; }

        public SetValueInListAttribute(string valuesName)
        {
            ValuesName = valuesName;
        }
    }
    
    public interface IListValue : IEnumerable<KeyValuePair<string, object>>
    {
    }

    public class ListValue<T> : IListValue
    {
        private readonly List<KeyValuePair<string, object>> _values = new();

        public void Add(string displayName, T value)
        {
            _values.Add(new KeyValuePair<string, object>(displayName, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static explicit operator ListValue<object>(ListValue<T> target)
        {
            var result = new ListValue<object>();
            foreach (var kv in target)
            {
                result.Add(kv.Key, kv.Value);
            }

            return result;
        }
    }
}