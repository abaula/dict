
namespace ConsoleApp
{
    class NodeIndex
    {
        private readonly Node _roots;

        public NodeIndex()
        {
            _roots = new Node(char.MinValue);
        }

        public void Add(string word)
        {
            var current = _roots;

            for (var i = 0; i < word.Length; i++)
            {
                var ch = word[i];
                var child = current.GetChild(ch);

                if (!child.IsNull)
                {
                    current = child;
                }
                else
                {
                    var next = current.AddChild(ch);
                    next.LastCharInWord = i == word.Length - 1;
                    current = next;
                }
            }
        }

        public int GetNodesCount()
        {
            int Count(Node parent)
            {
                var cnt = 0;

                foreach (var child in parent.Children)
                    cnt += 1 + Count(child);

                return cnt;
            }

            return 1 + Count(_roots);
        }
    }
}
