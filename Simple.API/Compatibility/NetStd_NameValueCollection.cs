#if NETSTANDARD1_1
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Specialized
{
    /// <summary>
    /// A simplified version of System.Collections.Specialized.NameValueCollection for .NET STANDARD
    /// </summary>
    public class NameValueCollection : ICollection, IEnumerable, IEnumerable<KeyValuePair<string,string>>
    {
        private Dictionary<string, string> valuePairs;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public NameValueCollection()
        {
            valuePairs = new Dictionary<string, string>();
        }

        /// <summary>
        ///  Gets or sets the entry with the specified key
        /// </summary>
        public string this[string name]
        {
            get { return valuePairs[name]; }
            set { valuePairs[name] = value; }
        }
        /// <summary>
        /// [AVOID USING] Gets the entry at the specified index
        /// </summary>
        public string this[int index]
        {
            get { return valuePairs[GetKey(index)]; }
        }
        /// <summary>
        /// Gets all keys
        /// </summary>
        public string[] AllKeys
        {
            get { return valuePairs.Keys.ToArray(); }
        }

        /// <summary>
        /// Adds an entire collection to the collection
        /// </summary>
        public void Add(NameValueCollection c)
        {
            foreach (KeyValuePair<string, string> pair in c) Add(pair.Key, pair.Value);
        }
        /// <summary>
        /// Adds a pair to the collection
        /// </summary>
        public void Add(string name, string value) => valuePairs[name] = value;
        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear() => valuePairs.Clear();
        /// <summary>
        /// Gets a value from the collection
        /// </summary>
        public string Get(string name) => valuePairs[name];
        /// <summary>
        /// Gets a value from the collection
        /// </summary>
        public string Get(int index) => this[index];
        /// <summary>
        /// Gets a key from the collection
        /// </summary>
        public string GetKey(int index) => AllKeys[index];
        /// <summary>
        /// Gets an array with a single value
        /// </summary>
        public string[] GetValues(string name) => new string[] { Get(name) };
        /// <summary>
        /// Gets an array with a single value
        /// </summary>
        public string[] GetValues(int index) => new string[] { Get(index) };
        /// <summary>
        /// Gets a value indicating whether the collection has keys
        /// </summary>
        /// <returns></returns>
        public bool HasKeys() => valuePairs.Count > 0;
        /// <summary>
        /// Removes a value of the collection
        /// </summary>
        public void Remove(string name) => valuePairs.Remove(name);
        /// <summary>
        /// Sets a value for the collection
        /// </summary>
        public void Set(string name, string value) => valuePairs[name] = value;

        /// <summary>
        /// The number of pairs in the collection
        /// </summary>
        public int Count => valuePairs.Count;
        /// <summary>
        /// Not implemented
        /// </summary>
        public bool IsSynchronized => false;
        /// <summary>
        /// Not implemented
        /// </summary>
        public object SyncRoot => null;
        /// <summary>
        /// Not implemented
        /// </summary>
        public void CopyTo(Array array, int index) => throw new NotImplementedException();
        /// <summary>
        /// Get enumerator
        /// </summary>
        public IEnumerator GetEnumerator() => valuePairs.GetEnumerator();
        /// <summary>
        /// Get enumerator
        /// </summary>
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            foreach(var pair in valuePairs) yield return pair;
        }
    }
}
#endif
