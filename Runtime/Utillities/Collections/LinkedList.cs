namespace TUI.Utillities
{
    using TUI.PoolingSystem;

    public class LinkedListNode<Type> : IPooledObject<LinkedListNode<Type>>
    {
        public IPoolingSystem<LinkedListNode<Type>> Pool { get; set; }
        public LinkedListNode<Type> Prev;
        public LinkedListNode<Type> Next;
        public Type Value;

        public LinkedListNode<Type> Init(Type value, LinkedListNode<Type> prev = null, LinkedListNode<Type> next = null)
        {
            Value = value;
            Prev = prev;
            Next = next;
            return this;
        }
        public void Dispose()
        {
            Pool?.Enpool(this);
        }
    }
    public class LinkedList<Type> where Type : class, new()
    {
        private static readonly BuiltinPoolingSystem<LinkedListNode<Type>> pool = new();
        public LinkedListNode<Type> Last { get; private set; }
        public LinkedListNode<Type> First { get; private set; }

        public void Add(Type item)
        {
            if (Last == null)
            {
                Last = First = pool.Depool().Init(item);
            }
            else
            {
                Last.Next = pool.Depool().Init(item, Last);
                Last = Last.Next;
            }

        }
        public bool Remove(Type item)
        {
            if (Last == null) return false;
            var current = First;
            while (current != null)
            {
                if (current.Value == item)
                {
                    var next = current.Next;
                    if (current.Prev != null)
                        current.Prev.Next = next;
                    if (next != null)
                        next.Prev = current.Prev;

                    if (current == First)
                    {
                        First = next;
                    }

                    if (current == Last)
                    {
                        Last = current.Prev;
                    }

                    current.Dispose();
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        public void Clear()
        {
            var current = First;
            while (current != null)
            {
                current.Dispose();
                current = current.Next;
            }
            First = Last = null;
        }

    }
}