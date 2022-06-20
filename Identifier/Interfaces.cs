using System;
using System.Collections.Generic;

namespace Chowbi_Blazor.Identifier
{
    public interface IIdentifier
    {
        public string Identifier { get; }
    }

    public class IdentifierList<T> : ICollection<T> where T : IIdentifier
    {
        readonly Dictionary<string, T> _Dic;
        public IdentifierList(StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            _Dic = new Dictionary<string, T>(StringComparer.FromComparison(stringComparison));
        }

        #region interface
        public int Count => _Dic.Count;
        public bool IsReadOnly => ((ICollection<T>)_Dic).IsReadOnly;
        public void Add(T item) => _Dic.TryAdd(item.Identifier, item);
        public bool TryAdd(T item) => _Dic.TryAdd(item.Identifier, item);
        public void Clear() => _Dic.Clear();
        public bool Contains(T item) => _Dic.ContainsKey(item.Identifier);
        public void CopyTo(T[] array, int arrayIndex) => _Dic.Values.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _Dic.Values.GetEnumerator();
        public bool Remove(T item) => _Dic.Remove(item.Identifier);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
