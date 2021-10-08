using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// Internal class used to count referencs to an object.
    /// </summary>
    public class ReferenceCounter : IReferenceCounter
    {
        private readonly object referenceLock = new object();
        ConcurrentDictionary<object,int> dictionary = new ConcurrentDictionary<object, int>();

        public void AddReference(object src)
        {
            lock (referenceLock)
            {
                dictionary.AddOrUpdate(src, 1, (key, oldValue) => oldValue + 1);
            }
        }

        public void RemoveReference(object src)
        {
            lock (referenceLock)
            {
                if (dictionary.TryGetValue(src, out int value))
                {
                    if (value > 1)
                    {
                        dictionary.AddOrUpdate(src, 0, (key, oldValue) => oldValue - 1);
                    }
                    else
                    {   // remove the ref from the collection
                        dictionary.TryRemove(src, out value);
                    }
                }
            }
        }

        public bool hasReference(object src)
        {
            // if its in the dictionary, we have a reference.
            return dictionary.ContainsKey(src);
        }

        public int getReferenceCount(object src)
        {
            int value;
            dictionary.TryGetValue(src, out value);
            return value;                
        }
    }
}
