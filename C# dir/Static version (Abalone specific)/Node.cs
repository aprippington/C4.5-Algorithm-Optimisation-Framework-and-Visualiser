using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Version_1___Abalone_Specific_Code
{
    class Node
    {
        public DataTable subTable;

        public List<DataColumn> subTableAttributes;

        public DataColumn attribute;

        public Branch children = null;

        public int depth;                //Stores the depth of a particular node

        public int label;

        public double threshold;
        public string category;

        public void setDepth(Node n, int currentDepth)
        {
            n.depth = currentDepth;
            if (n.children != null)
            {
                for (int i = 0; i < n.children.Nodes.Count; i++)
                {
                    currentDepth += 1;
                    setDepth(n.children.Nodes[i], currentDepth - i);            // "-i" Ensures that nodes on the same level retain the same depth value
                }
            }
        }

        public void visualiseTree(Node[] t)
        {
            foreach (Node n in t)
            {
                Console.WriteLine("Node: " + n.depth);
            }
        }

        public void removeChild(Node n)
        {
            children.Nodes.Remove(n);
        }

        public void removeAllChildren(Node node)      //UNTESTED
        {
            int i = -1;
            foreach (Node n in node.children.Nodes)
            {
                i++;
                if (n.children != null)
                {
                    removeAllChildren(n);
                }
            }
            node.children.Nodes.Remove(node.children.Nodes[i]);
        }

        public Node this[int index]
        {
            get
            {
                if (children == null || children.Nodes.Count + 1 <= index)
                {
                    return null;
                }
                else
                {
                    return children.Nodes[index];
                }

            }

            // The setter for a node should replace the subnode at a specific index, e.g. nodes[index] = value;
            set
            {
                if (children == null)
                {

                    children = new Branch(index); //Creates a new branch 
                    children.Nodes.Add(value);
                }
                else
                {
                    children.Nodes.Add(value);


                }
            }
        }




        public int Predict(string[] data)
        {
            Node node = this;
            if (node.label != 0)
            {
                //Console.WriteLine("Predicted Label =" + node.label);  //Print label to console
                return node.label;
            }

            if (node.children.Nodes.Count <= 1)
            {
                return node.children.Nodes[0].label;
            }

            int columnIndex = node.attribute.Ordinal;  //retrieves the index of this column in the MasterTable
            //Console.WriteLine("node.attribute.ordinal returns:" + node.attribute.Ordinal);  //Testing
            string value = data[columnIndex];
            if (node.threshold != 0)   //should be (node.attribute.GetType() == typeof(double))  -------->add this:  && !Double.IsNaN(node.threshold) <-------------
            {
                //Console.WriteLine("Identifed as double");  //Testing
                double d = double.Parse(value);
                return d < node.threshold ? children.Nodes[0].Predict(data) : children.Nodes[1].Predict(data);
            }
            else
            {
                //Console.WriteLine("Identified as not double");  //Testing
                //Console.WriteLine("value =:" + value); //Testing

                for (int i = 0; i < children.Nodes.Count; i++)
                {
                    if (children.Nodes[i].category == value)
                        return children.Nodes[i].Predict(data);
                }
            }
            Console.WriteLine("Error");  //Testing           
            return node.label;

        }


        //stores every node in the tree in an array of Nodes.
        //start at root and run through the tree, adding each Node as we go...
        //we can then pick a node at random and "mutate" on it, ie. rebuild the tree below it again using C4.5

        static List<Node> nodeList = new List<Node>();
        public Node[] flattenTree(Node root)   //Builds but does not add the root to the array
        {
            if (root.children != null)
            {
                for (int i = 0; i < root.children.Nodes.Count; i++)
                {
                    nodeList.Add(root.children.Nodes[i]);
                    flattenTree(root.children.Nodes[i]);
                }
            }
            return nodeList.ToArray();
        }


        public Node changeThreshold(Node[] nodes, out int index)             //Mutator
        {

            int i = new Random().Next(0, nodes.Length);
            index = i;
            //Node selected = nodes[i];                                      //Select a random node from the flattened tree                 

            if (nodes[i].children != null && nodes[i].children.Nodes.Count > 1)     //Ensure we do not select a leaf node
            {
                if (nodes[i].threshold != 0)
                {                                                                          //Ensure we do not pick a Categorical node
                    double min = double.NegativeInfinity;
                    double max = double.PositiveInfinity;
                    foreach (Node n in nodes)                                     //Get the range of values for this attribute
                    {
                        if (n.attribute != null && nodes[i].attribute != null)
                        {
                            if (n.attribute.ColumnName == nodes[i].attribute.ColumnName)
                            {
                                if (n.threshold > max) { max = n.threshold; }
                                if (n.threshold < min) { min = n.threshold; }
                            }
                        }
                    }
                    nodes[i].threshold = GetRandomNumberDouble(min, max);        //Set threshold to a random number between local min and max                        
                    return nodes[i];
                }
                else return changeThreshold(nodes, out index);    //Try again if a categorical attribute is selected
            }
            else return changeThreshold(nodes, out index);        //Try again if a leaf node is selected
        }



        public Node changeAttribute(Node[] nodes, out int index)                                //Mutator
        {

            int i = new Random().Next(0, nodes.Length);
            index = i;
            Node selected = nodes[i];     //Select a random node from the flattened tree
            selected.attribute.ColumnName = nodes[i].attribute.ColumnName;
            List<DataColumn> attributes = new List<DataColumn>();
            if (selected.children.Nodes.Count > 1)               //Ensure we do not select a leaf node 
            {
                //if (selected.threshold == 0)       
                //{
                foreach (Node n in nodes)
                {
                    if (!attributes.Contains(n.attribute))
                    {
                        attributes.Add(n.attribute);
                    }
                }
                Random rnd = new Random();
                int count = attributes.ToArray().Length;
                selected.attribute = attributes[rnd.Next(0, count)];    //Change nodes attribute to a random DataColumn from the dataset
                return selected;
                //}
                //else return changeAttribute(nodes, out index);    //Try again if a categorical attribute is selected
            }
            else return changeAttribute(nodes, out index);        //Try again if a leaf node is selected
        }



        public static Random rnd = new Random();

        public double GetRandomNumberDouble(double minimum, double maximum)
        {

            double x = rnd.NextDouble() * (maximum - minimum) + minimum;
            return x;
        }


        public Node randomMutateAndRebuild(Node[] nodes)    //Takes a flattened tree and randomly breaks and rebuilds a branch
        {
            Accuracy a = new Accuracy();
            double initialAcc = a.GetAccuracy(nodes[0]);    //Store accuracy of tree before mutation
            int selectedNodeIndex = 0;

            Node pivot = new Node();
            int mutatorID = rnd.Next(0, 1);                  //Always returning 0???
            //Console.WriteLine("MutatorID =" + mutatorID);
            if (mutatorID == 0) { pivot = changeThreshold(nodes, out int location); selectedNodeIndex = location; }       //DOES THIS COPY CLASS VALUES TOO? (IE. THRESHOLD/ATTRIBUTE ETC.?)
            if (mutatorID == 1) { pivot = changeAttribute(nodes, out int location); selectedNodeIndex = location; }

            Node old_branch = nodes[selectedNodeIndex];

            int colID = pivot.attribute.Ordinal;

            if (mutatorID == 0)     //Remove and rebuild sub-tree of pivot if threshold was mutated
            {
                DataTable lowerOrEqualTable = pivot.subTable.Clone();
                DataTable higherTable = pivot.subTable.Clone();

                foreach (DataRow r in pivot.subTable.Rows)
                {
                    string value = r[colID].ToString();
                    double d = double.Parse(value);
                    if (d <= pivot.threshold)
                    {
                        lowerOrEqualTable.ImportRow(r);
                    }
                    else higherTable.ImportRow(r);
                }

                DecisionTree dt = new DecisionTree();
                pivot.children.Nodes[0] = dt.RunC4_5(lowerOrEqualTable, pivot.subTableAttributes);  //Rebuild left sub-tree
                pivot.children.Nodes[1] = dt.RunC4_5(higherTable, pivot.subTableAttributes);        //Rebuild right sub-tree              

                //Remove old branch
                removeAllChildren(nodes[selectedNodeIndex]);

                //Insert new branch
                nodes[selectedNodeIndex] = pivot;
            }

            double newAcc = a.GetAccuracy(nodes[0]);
            if (newAcc < initialAcc)
            {                                                     //If the accuracy has worsened, roll back the change
                Console.WriteLine("Accuracy has decreased, rolling back mutation.");
                nodes[selectedNodeIndex] = old_branch;
                return randomMutateAndRebuild(nodes);
            }

            else return nodes[0];



        }






    }
}


