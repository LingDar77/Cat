namespace TUI.Utillities
{
    using System.Collections;
    using System.Collections.Generic;
    public class LinkedHashSet<Type> : ICollection<Type> where Type : class, new()
    {
        private readonly HashSet<Type> set = new();
        private readonly LinkedList<Type> list = new();
        public int Count => set.Count;
        public bool IsReadOnly => false;

        public void Add(Type item)
        {
            if (!set.Contains(item))
            {
                set.Add(item);
                list.Add(item);
            }
        }
        public bool Remove(Type item)
        {
            if (set.Remove(item))
            {
                list.Remove(item);
                return true;
            }
            return false;
        }
        public Type GetOldestItem()
        {
            return list.First.Value;
        }

        public void Clear()
        {
            list.Clear();
            set.Clear();
        }

        public bool Contains(Type item)
        {
            return set.Contains(item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        public HashSet<Type>.Enumerator GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }
    }

}