using System;
using System.Collections.Generic;
using System.Text;

namespace CE301___Attempt_2
{
    class Branch
    {

        public List<Node> Nodes;

        public Branch()
        {
            this.Nodes = new List<Node>();
        }

        public void removeChild(Node n)
        {
            Nodes.Remove(n);
        }

        public Branch(int length) : this()
        {
            for (int i = 0; i < length; i++)
            {
                this.Nodes.Add(new Node());
            }
        }

        public override string ToString()        //Is this useful? Not used anywhere currently...
        {
            foreach (Node n in Nodes)
                Console.WriteLine("Attribute of this Node: " + n.attribute  + "------------" + "Children: " + n.children);               
            return "Done";
        }

    }
}
