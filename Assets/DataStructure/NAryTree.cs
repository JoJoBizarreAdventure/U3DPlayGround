namespace DataStructure
{
    public class NAryTreeNode<T> where T : NAryTreeNode<T>
    {
        public int N;
        public T Parent;
        protected readonly T[] Children;

        protected NAryTreeNode(int n)
        {
            N = n;
            Parent = null;
            Children = new T[n];
        }
    }
}