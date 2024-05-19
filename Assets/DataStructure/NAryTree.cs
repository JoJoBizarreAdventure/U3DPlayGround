namespace DataStructure
{
    public class NAryTreeNode<T> where T : NAryTreeNode<T>
    {
        protected readonly T[] Children;
        public int N;
        public T Parent;

        protected NAryTreeNode(int n)
        {
            N = n;
            Parent = null;
            Children = new T[n];
        }
    }
}