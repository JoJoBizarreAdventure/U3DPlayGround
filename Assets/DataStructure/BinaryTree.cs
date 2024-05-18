namespace DataStructure
{
    public class BinaryTreeNode<T> : NAryTreeNode<T> where T : BinaryTreeNode<T>
    {
        protected BinaryTreeNode() : base(2)
        {
        }

        public T Left
        {
            get => Children[0];
            set => Children[0] = value;
        }

        public T Right
        {
            get => Children[1];
            set => Children[1] = value;
        }

        public void SetLeft(T node)
        {
            Left = node;
            if (node != null)
                node.Parent = (T)this;
        }

        public void SetRight(T node)
        {
            Right = node;
            if (node != null)
                node.Parent = (T)this;
        }
    }
}