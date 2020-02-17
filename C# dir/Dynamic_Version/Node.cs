using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Decision_Tree_Optimisation_Generalised
{
    //DataColumn is not serializable, thus deep copying must be done via reflection
    class Node
    {
        public bool isRoot = false;

        public int parentRef = -1;      //Arbitrary starting number

        public Node parent = null;

        public DataTable subTable;  //Consider!

        public List<DataColumn> subTableAttributes;

        public DataColumn attribute;

        public Branch children = null;

        public int depth;                //Stores the depth of a particular node

        public string label;

        public double threshold;

        public string category;

        public static double initialAcc;

        public static double newAcc;

        public override string ToString()
        {
            GCHandle objHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            long address = GCHandle.ToIntPtr(objHandle).ToInt64();
            string s = "No Children / Leaf";
            if (attribute != null && children != null)
            {
                s = ($"isRoot: {isRoot}  depth:  {depth}  attribute:  {attribute.ColumnName}  category:  {category}   no. of children:  {children.Nodes.Count}  label:  {label}  memory address: {address}");
            }
            return s; 
        }

        //Prints whole tree
        public void printTree(Node root)
        {
            root.ToString();
            if (root.children != null && root.children.Nodes.Count != 0)  
            {
                for (int x = 0; x < root.children.Nodes.Count; x++)
                {
                    printTree(root.children.Nodes[x]);
                }
            }
        }


        public void setDepthOfNode(Node n, int depthTemp)  //Sets the depth of a given node
        {
            if (n.parent != null)   //Currently returning depths inversed? (ie. root.depth = 5 = maxDepth) ie. root has highest score...HOW???????
            {
                depthTemp += 1;
                setDepthOfNode(n.parent, depthTemp);
            }
            n.depth = depthTemp;
        }


        public void setAllDepths(Node[] tree)  //Sets depths of all nodes given a flattened tree
        {
            foreach (Node n in tree) { setDepthOfNode(n, -1); }

            /* Why is this not working either????
            //Invert the indexing (ie. [0,1,2,3,4], if initially sets to lvl '0', then it should be lvl '4'
            List<int> l = new List<int>();
            foreach(Node n in tree) 
            { 
                l.Add(n.depth);
            }

            int[] lOrdered = l.Distinct().ToArray();
            Array.Sort(lOrdered);           
            foreach(Node n in tree)
            {
                for(int i = 0; i < lOrdered.Length; i++)
                {
                    if (n.depth == lOrdered[i]) { n.depth = lOrdered[lOrdered.Length - 1 - i]; }                    
                }
            }*/
        }

        public List<int> sizeOfSubTrees(Node[] tree)  //Returns the sizes of the left and right subtrees (from the root)
        {
            int s = depthOfTree(tree);
            int rootIndex = 0;

            for (int i = 0; i < tree.Length; i++)
            {
                if (tree[i].depth == s - 1) { rootIndex = i; }
            }

            int left = tree.Length - (tree.Length - rootIndex);
            int right = tree.Length - 1 - rootIndex;
            List<int> sizes = new List<int>();
            sizes.Add(left);
            sizes.Add(right);
            return sizes;
        }

        public int depthOfTree(Node[] tree) //Finds the maximum depth of the tree
        {
            int maxDepth = 0;
            foreach (Node n in tree)
            {
                if (n.depth > maxDepth) { maxDepth = n.depth; }
            }
            return maxDepth + 1;  // +1 as we are not 0-indexing here, it will be an absolute count of tree levels
        }

        ///////////////////////////////////////////////////////////////////////Visualisation////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////Visualisation////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////Visualisation////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////Visualisation////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////Visualisation////////////////////////////////////////////////////////////////

        public void visualiseTree(Node[] flatTree)
        {
            foreach (Node n in flatTree)
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

        public string Predict(string[] data)
        {           
            Node node = this;
            if (node.label != null)
            {
                //Console.WriteLine("Predicted Label =" + node.label);  //Print label to console
                return node.label;
            }

            /*
            if (node.children == null)
            {
                return node.label;
            }*/

            if (node.children.Nodes.Count <= 1)
            {
                return node.children.Nodes[0].label;
            }

            int columnIndex = node.attribute.Ordinal;  //retrieves the index of this column in the MasterTable
            //Console.WriteLine("node.attribute.ordinal returns:" + node.attribute.Ordinal);  //Testing
            string value = data[columnIndex];

            //Need to allow below code to handle int and string too
            if (node.attribute.DataType == typeof(Double))   //should be (node.attribute.GetType() == typeof(double))  -------->add this:  && !Double.IsNaN(node.threshold) <-------------
            {
                //Console.WriteLine("Identifed as double");  //Testing
                double d = Double.Parse(value);
                return d < node.threshold ? children.Nodes[0].Predict(data) : children.Nodes[1].Predict(data);
            }
            else if (node.attribute.DataType == typeof(Int32))
            {
                int i = Int32.Parse(value);
                return i < node.threshold ? children.Nodes[0].Predict(data) : children.Nodes[1].Predict(data);
            }

            else    //else column is categorical
            {
                //Console.WriteLine("Identified as not double");  //Testing
                //Console.WriteLine("value =:" + value); //Testing

                for (int i = 0; i < children.Nodes.Count; i++)
                {
                    if (children.Nodes[i].category == value)
                        return children.Nodes[i].Predict(data);
                }
            }
            //Console.WriteLine("Error");  //Testing           
            return node.label;
        }


        //stores every node in the tree in an array of Nodes.
        //start at root and run through the tree, adding each Node as we go...
        //we can then pick a node at random and "mutate" on it, ie. rebuild the tree below it again using C4.5


        public static List<Node> nodeList = new List<Node>();

        public Node[] flattenTree(Node root)
        {
            if (root.children != null && root.children.Nodes.Count != 0)
            {
                for (int i = 0; i < root.children.Nodes.Count; i++)
                {
                    nodeList.Add(root.children.Nodes[i]);
                    flattenTree(root.children.Nodes[i]);
                }
            }
            //Console.WriteLine(nodeList.Count);
            return nodeList.ToArray();
        }

        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////OPTIMISATION///////////////////////////////////////////////////////////////////////////

        public Node changeThreshold(Node[] nodes, int index, out double initialThresh, out int code, Node root)             //Mutator
        {

            if (nodes[index].attribute == null) { code = 1; initialThresh = -999; return nodes[index]; }  //Try again is a leaf node is selected

            int i = index;
            int colID = nodes[i].attribute.Ordinal;

            if (nodes[i].children != null && nodes[i].children.Nodes.Count > 1 && nodes[i].parent != null && nodes[i].parent.children.Nodes.Count != 0)  //Ensure we do not select a leaf node // Dont select root -> && nodes[i].parent != null
            {
                if (nodes[i].attribute.DataType != typeof(String))   //Ensure we do not pick a Categorical node
                {


                    if (nodes[i].attribute.DataType == typeof(Double))
                    {
                        double min = double.PositiveInfinity;
                        double max = double.NegativeInfinity;

                        //Could also use nodes[i].subTable.Rows here if we want to get the min and max values of rows that have reached that root, rather than global min and max
                        for (int k = 0; k < root.subTable.Rows.Count; k++)
                        {
                            if (Convert.ToDouble(root.subTable.Rows[k][colID]) > max) { max = Convert.ToDouble(root.subTable.Rows[k][colID]); }
                            if (Convert.ToDouble(root.subTable.Rows[k][colID]) < min) { min = Convert.ToDouble(root.subTable.Rows[k][colID]); }
                        }


                        /*
                        if (n.attribute != null && nodes[i].attribute != null)
                        {
                            if (Equals(n.attribute.ColumnName, nodes[i].attribute.ColumnName))  //check string equality BUG HERE!
                            {
                                if (n.threshold > max) { max = n.threshold; }
                                if (n.threshold < min) { min = n.threshold; }
                            }
                        }*/

                        Console.WriteLine("OLD THRESHOLD =   " + nodes[i].threshold + " max" + max + " min" + min);   //TESTING
                        initialThresh = nodes[i].threshold;
                        nodes[i].threshold = GetRandomNumberDouble(min, max);           //Set threshold to a random number between local min and max  
                        Console.WriteLine("NEW THRESHOLD =   " + nodes[i].threshold);   //TESTING
                        code = 0;                                                       //Informs outer function that mutation has been successful 
                        return nodes[i];
                    }

                    else if (nodes[i].attribute.DataType == typeof(int))
                    {
                        int min = int.MaxValue;
                        int max = int.MinValue;

                        //Could also use nodes[i].subTable.Rows here if we want to get the min and max values of rows that have reached that root, rather than global min and max
                        for (int k = 0; k < root.subTable.Rows.Count; k++)
                        {
                            if (Convert.ToInt32(root.subTable.Rows[k][colID]) > max) { max = Convert.ToInt32(root.subTable.Rows[k][colID]); }
                            if (Convert.ToInt32(root.subTable.Rows[k][colID]) < min) { min = Convert.ToInt32(root.subTable.Rows[k][colID]); }
                        }

                        /*
                        if (n.attribute != null && nodes[i].attribute != null)
                        {
                            if (Equals(n.attribute.ColumnName, nodes[i].attribute.ColumnName)) 
                            {
                                if (n.threshold > max) { max = n.threshold; }
                                if (n.threshold < min) { min = n.threshold; }
                            }
                        }*/

                        Console.WriteLine("OLD THRESHOLD =   " + nodes[i].threshold + "  max: " + max + "  min: " + min);   //TESTING
                        initialThresh = nodes[i].threshold;
                        nodes[i].threshold = GetRandomNumberInt(min, max);        //Set threshold to a random number between local min and max  
                        Console.WriteLine("NEW THRESHOLD =   " + nodes[i].threshold);   //TESTING
                        code = 0;   //Suggests mutation has worked
                        return nodes[i];
                    }
                    else
                    { // must be string
                        initialThresh = -999;
                        code = 1;     //Informs outer function to try again                   
                        return nodes[i];
                    }
                }
                else
                {
                    initialThresh = -999;
                    code = 1;     //Informs outer function to try again                   
                    return nodes[i];
                }
                //changeThreshold(nodes, out index, out initialThresh);    //Try again if a categorical attribute is selected
            }
            else
            {
                initialThresh = -999;
                code = 1;        //Informs outer function to try again
                return nodes[i];
            } //changeThreshold(nodes, out index, out initialThresh);        //Try again if a leaf node is selected
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

        public double GetRandomNumberInt(double minimum, double maximum)
        {
            double x = rnd.NextDouble() * (maximum - minimum) + minimum;
            return Convert.ToInt32(x);
        }       
      
        

        //Working but extremely inefficient (running algorithm twice rather than deep copying) 
        public Node randomMutateAndRebuild(Node root)  //Takes a flattened tree and randomly breaks then rebuilds a branch (Stop this from selecting leaf nodes to improve performance?)
        {
            Node copy = root.getDeepCopy(root);  //COPY IS STAYING BLANK AFTER THIS COPY ATTEMPT!!!  
            copy.isRoot = true;
            
            nodeList.Clear();
            Node[] nodes = root.flattenTree(root);
            nodeList.Add(root);            
            Accuracy a = new Accuracy();
            initialAcc = a.GetAccuracy(root);     //Store accuracy of tree before mutation            
            double initialThresh = 9.999;
            int code = 0;
            Node pivot = new Node();
            
            int selectedNodeIndex = new Random().Next(0, nodes.Length);
            DataTable initialTable = nodes[selectedNodeIndex].subTable.Copy();

            //int mutatorID = rnd.Next(0, 1);                  //Decomment this line to allow for attribute mutation also 
            int mutatorID = 0;
           
            if (mutatorID == 0) { pivot = changeThreshold(nodes, selectedNodeIndex, out double initialT, out int c, root); initialThresh = initialT; code = c; }
            if (mutatorID == 1) { pivot = changeAttribute(nodes, out int location); selectedNodeIndex = location; } //Need to be changed to match above function
            if (code == 1) { return root; } //If mutator picked an invalid node, return the root.

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~OLDACC =" + initialAcc);

            if (mutatorID == 0)     //Remove and rebuild sub-tree of pivot if threshold was mutated
            {
                buildBranch_Thresh(root, pivot, nodes);
            }
         
            newAcc = a.GetAccuracy(root);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEWACC =" + newAcc);

            if (newAcc >= initialAcc)
            {
                return root;
            }

            if (newAcc < initialAcc)  //This below code will only work for ChangeThreshold (NOT for ChangeAttribute)
            {
                //If the accuracy has worsened, roll back the change
                Console.WriteLine("//////////////////////////Accuracy has decreased, rolling back mutation.//////////////////////////////");                               
                pivot.threshold = initialThresh;
                //rebuildBranchThresh(root, pivot, nodes);

                //TESTING//
                Node[] root_nodes = root.flattenTree(root);
                Node[] copy_nodes = copy.flattenTree(copy);
                //Console.WriteLine("copy == root?" + (copy == root));
                Console.WriteLine("root.toString: " + root.ToString());
                Console.WriteLine("copy.toString: " + copy.ToString());
                Console.WriteLine("root_nodes_length =  " + root_nodes.Length + "  copy_nodes_length =  " + copy_nodes.Length);
                //Console.WriteLine("root_nodes[0] = copy_nodes[0]?" + (root_nodes[0] == copy_nodes[0]) + "        SHOULD BE FALSE");
                //Console.WriteLine("root_nodes[1] = copy_nodes[1]?" + (root_nodes[1] == copy_nodes[1]) + "        SHOULD BE FALSE");
                Console.WriteLine("root.children.Nodes.Count: " + root.children.Nodes.Count);
                Console.WriteLine("copy.children.Nodes.Count: " + copy.children.Nodes.Count);
                Console.WriteLine("root.subTable == copy.subTable: " + (root.subTable == copy.subTable));
                //TESTING//

                root = copy;
                double newAcc2 = a.GetAccuracy(root);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEWACC after reverting to old branch =" + newAcc2); //SHOULD BE THE SAME AS 'OLDACC'!?

                //CATCH EXCEPTION (TESTING)                   
                if (newAcc2 < initialAcc)
                {
                    Console.WriteLine("Initial Threshold = " + initialThresh);
                    Console.WriteLine("Final Threshold = " + pivot.threshold);
                    Console.WriteLine("Initial SubTable cols:   " + initialTable.Columns.Count);
                    Console.WriteLine("Initial SubTable rows:   " + initialTable.Rows.Count);
                    Console.WriteLine("Final SubTable cols:   " + pivot.subTable.Columns.Count);
                    Console.WriteLine("Final SubTable rows:   " + pivot.subTable.Rows.Count);
                    Console.WriteLine("Print statistics about initial tree and re-built tree here...?");
                    int higher = 0;
                    int lower = 0;
                    int colID = pivot.attribute.Ordinal;
                    foreach (DataRow r in initialTable.Rows)
                    {
                        string value = r[colID].ToString();
                        double d = double.Parse(value);
                        if (d <= pivot.threshold) { lower += 1; }
                        else higher += 1;
                    }
                    Console.WriteLine("Higher =   " + higher);
                    Console.WriteLine("Lower/Equal =   " + lower);
                    Console.WriteLine("root.isRoot:  " + root.isRoot);
                    foreach (DataRow r in initialTable.Rows)
                    {
                        Console.WriteLine(r[colID].ToString());
                    }
                    throw new Exception();
                }               
            }
            return root;
        }

        
        public Node buildBranch_Thresh(Node root, Node pivot, Node[] nodes)
        {
            int colID = pivot.attribute.Ordinal;
            Console.WriteLine(pivot.attribute);
            Console.WriteLine(pivot.subTable.Columns[colID].ColumnName);

            DataTable lowerOrEqualTable2 = pivot.subTable.Clone();
            DataTable higherTable2 = pivot.subTable.Clone();

            foreach (DataRow r in pivot.subTable.Rows)
            {
                string value = r[colID].ToString();
                double d = double.Parse(value);
                if (d <= pivot.threshold)
                {
                    lowerOrEqualTable2.ImportRow(r);
                }
                else higherTable2.ImportRow(r);
            }

            Console.WriteLine("Mutated Higher_Table contains: " + higherTable2.Rows.Count);
            Console.WriteLine("Mutated Lower_Equal_Table contains: " + lowerOrEqualTable2.Rows.Count);

            //Build new branch using new threshold
            DecisionTree dt = new DecisionTree();

            pivot.children.Nodes[0] = dt.RunC4_5(lowerOrEqualTable2, pivot.subTableAttributes);  //Rebuild left sub-tree
            pivot.children.Nodes[0].parent = pivot;
            pivot.children.Nodes[0].parentRef = 0;
            pivot.children.Nodes[1] = dt.RunC4_5(higherTable2, pivot.subTableAttributes);        //Rebuild right sub-tree   
            pivot.children.Nodes[1].parent = pivot;
            pivot.children.Nodes[1].parentRef = 1;

            nodeList.Clear();
            nodes = root.flattenTree(root);
            nodeList.Add(root);

            //Replace old branch with new branch 
            if (pivot.parent != null && pivot.children.Nodes.Count != 0)
            {  //if selected node is not root or a leaf                   
                pivot.parent.children.Nodes[pivot.parentRef] = pivot;
            }
            else
            {
                Console.WriteLine("pivot.parent == null" + pivot.parent == null);
                return root;
            }
            return root;
        }

        public Node rebuildBranchAttribute(Node root) { return root; }








        ////////////////////////////////////////////////////////////////////Deep Copying///////////////////////////////////////////////////////////////////////////

        public static int copy_counter = 0; 
        public Node getDeepCopy(Node root)
        {
            copy_counter += 1;
            Console.WriteLine("Deep copying " + copy_counter);
            Node copy = root.DeepCopy();
            copy.attribute = root.attribute;
            //copy.subTable = root.subTable;
            //copy.subTableAttributes = root.subTableAttributes;
            //copy.label = root.label;

            if (root.children != null && root.children.Nodes.Count != 0)  //Do not try to copy children of leaf nodes
            {
                for (int x = 0; x < root.children.Nodes.Count; x++)
                {
                    copy[x] = getDeepCopy(root.children.Nodes[x]);   //Adding child and parent references here, so these do not need to be copied explicitly
                    copy[x].parent = copy;
                    copy[x].parentRef = x;
                }
            }
            return copy;
        }

        public Node DeepCopy() 
        {
            return new Node(parentRef, parent, subTable, subTableAttributes, attribute, children, depth, label, threshold, category);          
        }

        //10-arg Constructor (recursive)       
        public Node(int a, Node b, DataTable c, List<DataColumn> d, DataColumn e, Branch f, int g, string h, double i, string j)
        {           
            new Node(c, d, e, h, i, j); 
        }
            
        public Node(DataTable c, List<DataColumn> d, DataColumn e, string h, double i, string j)  
        {                                              
            this.subTable = c.Copy();   //Copy subtable
           
            List<DataColumn> l = new List<DataColumn>();  //Create new list of attributes from the new DataTable
            foreach(DataColumn x in d) { 
                foreach(DataColumn y in this.subTable.Columns)
                {
                    if(x.ColumnName == y.ColumnName)
                    {
                        l.Add(y);
                    }
                }
            }           

            this.subTableAttributes = l;
            if (e != null) { this.attribute = this.subTable.Columns[e.Ordinal]; }

            this.label = h;
            this.threshold = i;
            this.category = j;           
        }

            public Node() { }  //Empty constructor must be defined as we have explicity defined another constructor (thus, an empty constructor will no longer be created by default)

    }
}


