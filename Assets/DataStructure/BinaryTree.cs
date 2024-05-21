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

        public static void Swap(T node1, T node2)
        {
            if (node1 == null || node2 == null)
                return;

            if (node1.Parent == node2) (node1, node2) = (node2, node1);

            if (node2.Parent == node1)
            {
                if (node1.Parent != null)
                {
                    if (node1.Parent.Left == node1)
                        node1.Parent.SetLeft(node2);
                    else
                        node1.Parent.SetRight(node2);
                }
                else
                {
                    node2.Parent = null;
                }

                T left2 = node2.Left, right2 = node2.Right;
                if (node1.Left == node2)
                {
                    node2.SetLeft(node1);
                    node2.SetRight(node1.Right);
                }
                else
                {
                    node2.SetLeft(node1.Left);
                    node2.SetRight(node1);
                }

                node1.SetLeft(left2);
                node1.SetRight(right2);
            }
            else
            {
                T left1 = node1.Left, right1 = node1.Right;

                node1.SetLeft(node2.Left);
                node1.SetRight(node2.Right);

                node2.SetLeft(left1);
                node2.SetRight(right1);

                if (node1.Parent == null)
                {
                    if (node2.Parent.Left == node2)
                        node2.Parent.SetLeft(node1);
                    else
                        node2.Parent.SetRight(node1);

                    node2.Parent = null;
                }
                else if (node2.Parent == null)
                {
                    if (node1.Parent.Left == node1)
                        node1.Parent.SetLeft(node2);
                    else
                        node1.Parent.SetRight(node2);

                    node1.Parent = null;
                }
                else
                {
                    var parent1 = node1.Parent;

                    if (node2.Parent.Left == node2)
                        node2.Parent.SetLeft(node1);
                    else
                        node2.Parent.SetRight(node1);

                    if (parent1.Left == node1)
                        parent1.SetLeft(node2);
                    else
                        parent1.SetRight(node2);
                }
            }
        }
    }
}