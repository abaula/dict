
using System;

namespace ConsoleApp
{
    struct Node
    {
        public char Value;
        public bool LastCharInWord;
        public bool IsNull;
        public Node[] Children;

        public Node(char value)
        {
            Value = value;
            Children = Array.Empty<Node>();
            LastCharInWord = false;
            IsNull = false;
        }

        public Node(bool isNull) : this()
        {
            IsNull = isNull;
        }

        public static Node NullNode()
        {
            return new Node(true);
        }

        public Node AddChild(char value)
        {
            var (found, i) = GetChildIndex(value);

            if (found)
                return Children[i];

            var node = new Node(value);
            Array.Resize(ref Children, Children.Length + 1);
            Children[i] = node;

            return node;
        }

        public Node GetChild(char value)
        {
            var (found, i) = GetChildIndex(value);

            if (!found)
                return NullNode();

            return Children[i];
        }

        private (bool, int) GetChildIndex(char value)
        {
            var end = Children.Length - 1;

            if (end < 0)
                return (false, 0);

            var start = 0;
            var insertIndex = 0;

            while (end >= start)
            {
                var i = start + ((end - start) / 2);
                var v = Children[i].Value;

                if (v == value)
                    return (true, i);

                if (value < v)
                {
                    end = i - 1;
                    insertIndex = i;
                }
                else
                {
                    start = i + 1;
                    insertIndex = i + 1;
                }
            }

            return (false, insertIndex);
        }
    }
}
