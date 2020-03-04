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

        public bool isLeaf;

        public string unique_id;

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
            try { children.Nodes.Remove(n); }
            catch { Console.WriteLine("ERROR: No child to remove"); }
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
                if (children == null || children.Nodes.Count <= index) //Why was this originally children.Nodes.Count + 1 <= index
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

        public Node[] flattenTree(Node root)                              //Does not add leaf nodes
        {           
            if (root.children != null && root.children.Nodes.Count != 0)
            {
                for (int i = 0; i < root.children.Nodes.Count; i++)
                {
                    nodeList.Add(root.children.Nodes[i]);
                    flattenTree(root.children.Nodes[i]);
                }
            }           
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
                        
                        Console.WriteLine("OLD THRESHOLD =   " + nodes[i].threshold + "  max: " + max + "  min: " + min);   //TESTING
                        initialThresh = nodes[i].threshold;
                        nodes[i].threshold = GetRandomNumberInt(min, max);           //Set threshold to a random number between local min and max  
                        Console.WriteLine("NEW THRESHOLD =   " + nodes[i].threshold);   
                        code = 0;   //Suggests mutation has worked
                        return nodes[i];
                    }
                    else    
                    { 
                        initialThresh = -999;
                        code = 1;         //Informs outer function to try again                   
                        return nodes[i];
                    }
                } 
                else    //Column is categorical, thus no threshold exists
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



        public Node changeAttribute(Node[] nodes, int index, out int code)
        {           
            if (nodes[index].attribute == null) { code = 1; return nodes[index]; }
            int i = index;
            int colID = nodes[i].attribute.Ordinal;

            //Ensure we do not select a leaf node // Dont select root -> && nodes[i].parent != null
            if (nodes[i].children != null && nodes[i].children.Nodes.Count > 1 && nodes[i].parent != null && nodes[i].parent.children.Nodes.Count != 0 
                && nodes[i].subTableAttributes.Count >= 2)
            {
                //Randomly select another attribute from the subTableAttributes to split on                         
                int k = GetRandomNumberInt(0, nodes[i].subTableAttributes.Count-1);                
                DataColumn selectedAttribute = nodes[i].subTableAttributes[k];
                nodes[i].subTableAttributes.Add(nodes[i].attribute);              //Add old attribute back into the list so it can be used to split again later
                nodes[i].attribute = selectedAttribute;
                code = 0;
                return nodes[i];
            }
            else 
                code = 1;
            return nodes[i];
        }

        public static Random rnd = new Random();

        public double GetRandomNumberDouble(double minimum, double maximum)
        {
            double x = rnd.NextDouble() * (maximum - minimum) + minimum;
            return x;
        }

        public int GetRandomNumberInt(int minimum, int maximum)
        {
            int x = rnd.Next(minimum, maximum);
            return x;
        }       
      
        public static int zero_count = 0;
        public static int one_count = 0;

        //Working but extremely inefficient (running algorithm twice rather than deep copying) 
        public Node randomMutateAndRebuild_Accuracy(Node root)  //Takes a flattened tree and randomly breaks then rebuilds a branch (Stop this from selecting leaf nodes to improve performance?)
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

            int mutatorID = rnd.Next(0, 2);                  //2 set as (exclusive) upper bound
            //int mutatorID = 1;
            if (mutatorID == 0) { zero_count += 1; }
            if (mutatorID == 1) { one_count += 1; }
            Console.WriteLine("zero_count = " + zero_count);
            Console.WriteLine("one_count = " + one_count);


            if (mutatorID == 0) { pivot = changeThreshold(nodes, selectedNodeIndex, out double initialT, out int c, root); initialThresh = initialT; code = c; }
            if (mutatorID == 1) { pivot = changeAttribute(nodes, selectedNodeIndex, out int c); code = c; } //Need to be changed to match above function
            if (code == 1) { return root; } //If mutator picked an invalid node, return the root.

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~OLDACC =" + initialAcc);

            if (mutatorID == 0)     //Remove and rebuild sub-tree of pivot if threshold was mutated
            {
                buildBranch_Thresh(root, pivot, nodes);
            }

            if (mutatorID == 1)
            {
                buildBranch_Attribute(root, pivot, nodes);
            }
         
            newAcc = a.GetAccuracy(root);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEWACC =" + newAcc);

            if (newAcc >= initialAcc)
            {
                root.pruneTree(root);                                                 //Prune tree after successful mutation
                return root;
            }

            if (newAcc < initialAcc)  
            {
                //If the accuracy has worsened, roll back the change
                Console.WriteLine("//////////////////////////Accuracy has decreased, rolling back mutation.//////////////////////////////");                               
                pivot.threshold = initialThresh;                
                root = copy;
                double newAcc2 = a.GetAccuracy(root);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEWACC after reverting to old branch =" + newAcc2); //SHOULD BE THE SAME AS 'OLDACC'!?

                //CATCH EXCEPTION (TESTING)                   
                if (newAcc2 < initialAcc)
                {
                    
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
                    
                    foreach (DataRow r in initialTable.Rows)
                    {
                        Console.WriteLine(r[colID].ToString());
                    }
                    throw new Exception();
                }               
            }
            return root;
        }

        public static double initialRMSE;

        public static double newRMSE;

        public Node randomMutateAndRebuild_RMSE(Node root)  //Takes a flattened tree and randomly breaks then rebuilds a branch (Stop this from selecting leaf nodes to improve performance?)
        {
            Node copy = root.getDeepCopy(root);  //COPY IS STAYING BLANK AFTER THIS COPY ATTEMPT!!!  
            copy.isRoot = true;

            nodeList.Clear();
            Node[] nodes = root.flattenTree(root);
            nodeList.Add(root);
            Accuracy a = new Accuracy();
            initialRMSE = a.getRMSE(root);     //Store accuracy of tree before mutation            
            double initialThresh = 9.999;
            int code = 0;
            Node pivot = new Node();

            int selectedNodeIndex = new Random().Next(0, nodes.Length);
            DataTable initialTable = nodes[selectedNodeIndex].subTable.Copy();

            int mutatorID = rnd.Next(0, 2);                  //2 set as (exclusive) upper bound
            //int mutatorID = 1;
            if (mutatorID == 0) { zero_count += 1; }
            if (mutatorID == 1) { one_count += 1; }
            Console.WriteLine("zero_count = " + zero_count);
            Console.WriteLine("one_count = " + one_count);


            if (mutatorID == 0) { pivot = changeThreshold(nodes, selectedNodeIndex, out double initialT, out int c, root); initialThresh = initialT; code = c; }
            if (mutatorID == 1) { pivot = changeAttribute(nodes, selectedNodeIndex, out int c); code = c; } //Need to be changed to match above function
            if (code == 1) { return root; } //If mutator picked an invalid node, return the root.

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~OLD_RMSE =" + initialRMSE);

            if (mutatorID == 0)     //Remove and rebuild sub-tree of pivot if threshold was mutated
            {
                buildBranch_Thresh(root, pivot, nodes);
            }

            if (mutatorID == 1)
            {
                buildBranch_Attribute(root, pivot, nodes);
            }

            newRMSE = a.getRMSE(root);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEW_RMSE =" + newRMSE);

            if (newRMSE <= initialRMSE && newRMSE != 0)
            {
                root.pruneTree(root);                                               //Prune tree after successful mutation
                return root;
            }

            if (newRMSE > initialRMSE || newRMSE == 0)  
            {
                //If the accuracy has worsened, roll back the change
                Console.WriteLine("//////////////////////////RMSE has increased, rolling back mutation.//////////////////////////////");
                pivot.threshold = initialThresh;
                root = copy;
                double newRMSE2 = a.getRMSE(root);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NEW_RMSE after reverting to old branch =" + newRMSE2); //SHOULD BE THE SAME AS 'OLDACC'!?

                //CATCH EXCEPTION (TESTING)                   
                if (newRMSE2 > initialRMSE)
                {

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

                    foreach (DataRow r in initialTable.Rows)
                    {
                        Console.WriteLine(r[colID].ToString());
                    }
                    throw new Exception();
                }
            }
            return root;
        }

        public Node randomMutateAndRebuild_Size(Node root) 
        {
            Node copy = root.getDeepCopy(root);
            copy.isRoot = true;

            nodeList.Clear();
            nodeList.Add(root);
            Node[] nodes = root.flattenTree(root);
            
            Accuracy a = new Accuracy();
            int initialSize = nodes.Length;       //Store size of tree before mutation     
            initialAcc = a.GetAccuracy(root);     //Store accuracy of tree before mutation            
            double initialThresh = 9.999;
            int code = 0;
            Node pivot = new Node();

            int selectedNodeIndex = new Random().Next(0, nodes.Length);
            DataTable initialTable = nodes[selectedNodeIndex].subTable.Copy();

            int mutatorID = rnd.Next(0, 2);                  //2 set as (exclusive) upper bound                      
            if (mutatorID == 0) { pivot = changeThreshold(nodes, selectedNodeIndex, out double initialT, out int c, root); initialThresh = initialT; code = c; }
            if (mutatorID == 1) { pivot = changeAttribute(nodes, selectedNodeIndex, out int c); code = c; } 
            if (code == 1) { return root; } //If mutator picked an invalid node, return the root.

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~OLD accuracy =" + initialAcc);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~OLD size =" + initialSize);

            if (mutatorID == 0)     //Remove and rebuild sub-tree of pivot if threshold was mutated
            {
                buildBranch_Thresh(root, pivot, nodes);
            }

            if (mutatorID == 1)    //Remove and rebuild sub-tree of pivot if attribute was mutated
            {
                buildBranch_Attribute(root, pivot, nodes);
            }

            newAcc = a.GetAccuracy(root);
            root.pruneTree(root);                                   //Prune tree after each mutation 
            nodeList.Clear();
            nodeList.Add(root);
            Node[] nodes_2 = root.flattenTree(root);
            
            int newSize = nodes_2.Length;
            
            if (newAcc >= initialAcc && newSize <= initialSize)     //If accuracy has improved or remained stable and tree size has decreased or remained stable, keep the change
            {
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~New accuracy =" + newAcc);
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~New size =" + newSize);               
                return root;
            }

            if (newAcc < initialAcc || newSize > initialSize)
            {
                //If the accuracy has worsened, roll back the change
                Console.WriteLine("//////////////////////////Accuracy has decreased, rolling back mutation.//////////////////////////////");
                pivot.threshold = initialThresh;
                root = copy;
                double newAcc2 = a.GetAccuracy(root);               

                //CATCH EXCEPTION            
                if (newAcc2 < initialAcc)
                {

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

        public Node buildBranch_Attribute(Node root, Node pivot, Node[] nodes) 
        {
            DecisionTree dt = new DecisionTree();
            int p = pivot.parentRef;
            pivot = dt.RunC4_5(pivot.subTable, pivot.subTableAttributes, pivot.attribute); //parent and parent ref will remain the same
            pivot.parentRef = p;
            nodeList.Clear();
            nodes = root.flattenTree(root);
            nodeList.Add(root);

            return root; 
        }


        public void pruneTree (Node root)    //currently getting stuck whereby a node has a parent but that parent has no children...SHOULD NOT BE POSSIBLE!
        {
            //Function to prune the tree (ie. if a split gives us no gain.) 
            //Remember: to remove a node, we must remove it from its parent's children (ie.remove the reference to it) 
            //This function could then be used every time the tree has been altered (ie. after each mutation)  
            nodeList.Clear();
            nodeList.Add(root);
            Node[] tree = flattenTree(root);
                for(int i = 0; i < tree.Length; i++)
                {
                    if (tree[i].parent != null && /*tree[i].parent.children != null &&*/ tree[i].label != null)  //if this node is a leaf and this node is not the root
                    {
                        string lab = tree[i].label;
                        int counter = 0;
                        foreach (Node n in tree[i].parent.children.Nodes)
                        {
                            if (Equals(n.label,  lab)) { counter += 1; }
                            if (counter > 1)
                            {                                                    //if all leaf nodes have the same label
                                tree[i].parent.isLeaf = true;
                                tree[i].parent.label = lab;                      //add this label to the parent and remove the children
                                Node temp = tree[i].parent;
                                foreach (Node m in tree[i].parent.children.Nodes) { m.parent = null; }
                                temp.children = null;
                                temp.attribute = null;                          //re-set the attribute to 'null' so DOT-file_generator identifies this as a leaf                                                                                                                                            
                                pruneTree(root);
                                break;
                            }                       
                        }                    
                    }                    
                }            
        }

        /*
        public void pruneTree(Node root)
        {
            //Function to prune the tree (ie. if a split gives us no gain.) 
            //Remember: to remove a node, we must remove it from its parent's children (ie.remove the reference to it) 
            //This function could then be used every time the tree has been altered (ie. after each successful mutation) 
            if (root.children != null)
            {
                foreach (Node child in root.children.Nodes)
                {
                    if (child.isLeaf)
                    {
                        string lab = child.label;
                        for (int i = 0; i < child.parent.children.Nodes.Count; i++)
                        {
                            if (child.parent.children.Nodes[i].label == lab)
                            {
                                child.parent.isLeaf = true;
                                child.parent.label = lab;
                                child.parent.children = null;
                                child.parent.attribute = null;
                                pruneTree(root);
                                break;
                            }
                        }
                    }
                    else pruneTree(child);
                }
            }
        }
        */




        ////////////////////////////////////////////////////////////////////Deep Copying///////////////////////////////////////////////////////////////////////////
        
        public Node getDeepCopy(Node root)
        {                        
            Node copy = root.DeepCopy();                       
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
            return new Node(parentRef, subTable, subTableAttributes, attribute, depth, label, threshold, category);          
        }
                   
        public Node(int a, DataTable c, List<DataColumn> d, DataColumn e, int g, string h, double i, string j)  
        {
            this.parentRef = a;
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
            if (e != null) { this.attribute = this.subTable.Columns[e.Ordinal]; }  //N.B. Leaf nodes do not contain an attribute  
            this.depth = g;
            this.label = h;
            this.threshold = i;
            this.category = j;           
        }

            public Node() { }  //Empty constructor must be defined as we have explicity defined another constructor (thus, an empty constructor will no longer be created by default)

    }
}


