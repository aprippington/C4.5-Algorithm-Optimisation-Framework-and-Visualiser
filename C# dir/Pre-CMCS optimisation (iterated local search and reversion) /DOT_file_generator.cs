using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using DotNetGraph.SubGraph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Decision_Tree_Optimisation_Generalised 
{
    //Once a .DOT file is created, it can be opened using Visual Studio Code. (Right click anywhere on the screen, and click 'Preview to the side')
     
    class DOT_file_generator                             //SOURCE: https://github.com/vfrz/DotNetGraph
    {
        public static DotGraph tree;

        static int count = 0; //1
        public void createDOTfile (Node root)          //Args: root of the tree, string to specify where the generated DOT file will be saved. 
        {
            if(count == 0) { root.unique_id = "-1"; }  //2 //Set the first iteration (root) ID to 0                       

            if (root.isRoot)                            //Create root node on first iteration
            {
                tree = new DotGraph("Tree", true);      //second parameter should always be true as this refers to a directed graph, which a decision tree always is
                string x;
                if(root.attribute.DataType != typeof(string)) { x = "att:   " + root.attribute + "   thresh:   " + root.threshold; }
                else { x = root.attribute.ColumnName + ":  binary split"; }
                
                var root_node = new DotNode(root.unique_id)
                {
                    Shape = DotNodeShape.Ellipse,
                    Label = x,
                    FillColor = Color.Coral,
                    FontColor = Color.Black,
                    Style = DotNodeStyle.Filled,
                    Width = 0.5f,
                    Height = 0.5f
                };
                tree.Elements.Add(root_node);                
            }

            if (root.children != null && root.children.Nodes.Count != 0)
            {
                for (int x = 0; x < root.children.Nodes.Count; x++)
                {
                    root.children.Nodes[x].unique_id = count.ToString(); //3
                    count++; //4

                    if (root.children.Nodes[x].attribute != null)  //if not a leaf node
                    {
                        string c = root.children.Nodes[x].attribute.ColumnName + ":  binary split";

                        if ((root.children.Nodes[x].attribute.DataType != typeof(string)))
                        {
                            string a = root.children.Nodes[x].attribute.ColumnName;
                            string b = root.children.Nodes[x].threshold.ToString();
                            c = "att:  " + a + "   " + "thresh:  " + b;
                        }           
                        
                        var this_node = new DotNode(root.children.Nodes[x].unique_id)
                        {
                            Shape = DotNodeShape.Ellipse,
                            Label = c,
                            FillColor = Color.Coral,
                            FontColor = Color.Black,
                            Style = DotNodeStyle.Bold,
                            Width = 0.5f,
                            Height = 0.5f
                        };

                        tree.Elements.Add(this_node);

                        string parent_id = root.children.Nodes[x].parent.unique_id;

                        var this_edge = new DotEdge(parent_id, root.children.Nodes[x].unique_id); // Create and add an edge with identifiers //Does this create two blank nodes as we are calling by string?
                        tree.Elements.Add(this_edge);

                        createDOTfile(root.children.Nodes[x]);
                    }
                    else  //attribute is null, thus is a leaf node
                    {
                        if (root.children.Nodes[x].label != "x")  //Ensure mutated nodes with no attribute nor label are added to the tree //BUG PATCHING
                        {                            
                            var this_node = new DotNode(root.children.Nodes[x].unique_id)  
                            {
                                Shape = DotNodeShape.Ellipse,
                                Label = root.children.Nodes[x].label,
                                FillColor = Color.LightGray,
                                FontColor = Color.Black,
                                Style = DotNodeStyle.Filled,
                                Width = 0.5f,
                                Height = 0.5f
                            };

                            tree.Elements.Add(this_node);

                            string parent_id = root.children.Nodes[x].parent.unique_id;

                            var this_edge = new DotEdge(parent_id, root.children.Nodes[x].unique_id); // Create and add an edge with identifiers //Does this create two blank nodes as we are calling by string?
                            tree.Elements.Add(this_edge);
                        }

                    }
                }
            }            

            if (root.isRoot)          //Ensure the tree is created only once recursion returns all the way to the root
            {                
                Console.WriteLine("Nodes in DotGraph:  " + tree.Elements.Count); //Testing
                int edges = 0;
                int nodes = 0;
                foreach(var x in tree.Elements)
                {
                    if(x.GetType() == typeof(DotEdge)) { edges += 1; }
                    if(x.GetType() == typeof(DotNode)) { nodes += 1; }                                       
                }

                Console.WriteLine("Number of nodes in tree: " + nodes);
                Console.WriteLine("Number of edges in tree: " + edges);


                var dot = tree.Compile();

                // Save it to a file
                File.WriteAllText("myFile.dot", dot);
            }
            
        }



    }
}
