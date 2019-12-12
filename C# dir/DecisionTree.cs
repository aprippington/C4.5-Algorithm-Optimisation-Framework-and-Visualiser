using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;


//Plaigiarism Check needed (As code similar to that found online @"https://archive.codeplex.com/?p=id3algorithm")

namespace CE301___Attempt_2
{
    class DecisionTree
    {
        private Node Root;

        public Node root
        {
            get { return Root; }
            set { Root = value; }
        }


        public DecisionTree()
        {
            //    this.dt = dt_;
            //    this.attributes = attributes;
            this.root = null;
        }


        int count = 0;  //FOR TESTING
        public Node RunC4_5(DataTable x, List<DataColumn> attributes)    //ID3 / C4.5 ALGORITHM
        {
            //int i = -1;
            //Console.WriteLine("No. of rows in this iteration table=" + x.Rows.Count);  //FOR TESTING how many rows in the current iteration, ie. 4177 for full table

            Console.WriteLine("C4.5 Iteration number: " + count);
            count += 1; //FOR TESTING

            Node n = new Node();
            n.subTable = x;
            n.subTableAttributes = attributes;
            
            //int reference;

            //INSERT DEPTH LIMITING PARAMETER HERE (IE. IF BRANCH LENGTH > 10, RETURN MOST COMMON LABEL)

            //////////////////////////////////////////////// STOPPING PARAMETER//////////////////////////////////////////////////////////////
            if (x.Rows.Count <= 250 || attributes.Count <= 1) //if only the class column is remaining or there are 5 or fewer rows left, find most common label
            {
                n.label = GetMostCommonLabel(x);
                //Console.WriteLine("LABELING WITH MOST COMMON VALUE --> Labeling this leaf:   " + n.label);  //Not being used for abalone dataset!
                return n;
            }
            //swap these two methods around?

            if (AllLabelsTheSame(x))                                                 //checks if all labels are equal and returns the node as a leaf if so 
            {
                n.label = Convert.ToInt32(x.Rows[0][x.Columns.Count - 1]);
                //Console.WriteLine("ALL LABELS THE SAME --> Labeling this leaf:   " + n.label);
                return n;
            }

            DataColumn bestAttribute = ReturnBestAttribute(x, attributes);          //need a "Get rows that meet specific criteria" method           
            n.attribute = bestAttribute;                                             //sets the node attribute to the correct column
                                                                                     //the number of rows in the supplied datatable, immediate children and grandchildren etc.

            //Console.WriteLine("bestAttribute this iteration is:" + bestAttribute.ColumnName);   //FOR TESTING




            if (bestAttribute.DataType == typeof(string))
            {
                DataTable unique_vals = x.DefaultView.ToTable(true, bestAttribute.ColumnName);  //Creates rows with all distinct attvalues (so can iterate through easily)                 
                int indexStr = -1;

                List<DataTable> tables = new List<DataTable>();

                foreach (DataRow row in unique_vals.Rows) //Splitting table into sub-tables for each value of this categorical attribte (ie. separate tables for F,M,I)
                {

                    string thisValue = row[bestAttribute.ColumnName].ToString().ToLower();
                    DataTable subSet = x.Clone();

                    foreach (DataRow r in x.Rows)
                    {
                        if (r[bestAttribute.ColumnName].ToString().ToLower() == thisValue.ToLower())  //Collect matching rows
                        {
                            subSet.ImportRow(r);
                        }
                    }
                    tables.Add(subSet);

                    //DataTable subSet = x.Clone();
                    //DataTable collect = CollectMatchingRowsString(x, bestAttribute, thisValue);                  
                    //foreach (DataRow r in collect.Rows)
                    //{
                    //    subSet.ImportRow(r);
                    //}
                    //tables.Add(collect);
                }

                foreach (DataTable t in tables)
                {
                    indexStr += 1;                     //M --> (n.children[0]), F --> (n.children[1]), I --> (n.children[2]) 
                    if (t.Rows.Count != 0)
                    {
                        attributes.Remove(bestAttribute);
                        //Console.WriteLine("Splitting on String attribute:  " + bestAttribute.ColumnName);
                        //Console.WriteLine("NEW ITERATION");   //TESTING
                        n[indexStr] = RunC4_5(t, attributes);  //n.children.Nodes.Add(RunID3(t, attributes));
                        n[indexStr].category = t.Rows[0][bestAttribute.Ordinal].ToString();

                    }
                    else
                        n.removeChild(n[indexStr]);           //remove node at children index 1 or 2 if no rows match this condition 
                }
            }


            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////

            if (bestAttribute.DataType == typeof(double))
            {

                int indexDoub = -1;

                FindInfoGainPerThisAttribute(x, n.attribute, out double thresh);  //returns the threshold value of best attribute to split on 

                n.threshold = thresh;

                DataTable LowerSet = x.Clone();    //copies the structure of input table
                DataTable rem = x.Clone();
                DataTable matching = CollectMatchingRowsDouble(x, bestAttribute, thresh, out rem);  //collects matching rows

                foreach (DataRow r in matching.Rows)
                {
                    LowerSet.ImportRow(r);
                }


                List<DataTable> tables = new List<DataTable>();
                tables.Add(LowerSet);                                       //Matching rows added first, will have index "1" in children 
                tables.Add(rem);

                foreach (DataTable t in tables)
                {
                    indexDoub += 1;

                    //Console.WriteLine("Number of Rows in this LowerSet: " + LowerSet.Rows.Count);

                    if (t.Rows.Count != 0)            //Rows with values below or equal to threshold are indexed "1" in children, those above are indexed "2"
                    {

                        //Console.WriteLine("Splitting on Double attribute:  " + bestAttribute.ColumnName); //TESTING                        
                        //Console.WriteLine("NEW ITERATION");   //TESTING

                        attributes.Remove(bestAttribute);
                        n[indexDoub] = RunC4_5(t, attributes);
                    }
                    else
                    {
                        n.removeChild(n[indexDoub]);
                        //n.label = Convert.ToInt32(x.Rows[0][x.Columns.Count-1]);    //if there are no more subrows, label the node with it's class value
                        //Console.WriteLine("Labeling this leaf:   " + n.label);
                    }
                }
            }

            else Console.WriteLine("Error: DataColumn not of the type string or double");
            return n;
        }

        /*public DataTable CollectMatchingRowsString(DataTable t, DataColumn col, string attValue)
        {

            //DataTable subTable = t.Clone();
            DataTable subTable = t.Clone();
            int Target = col.Ordinal;
            foreach (DataRow row in t.Rows)
            {
                if (row[Target].ToString() == attValue)           //collects all rows where attribute matches the supplies attValue (ie. "M")
                {
                    subTable.ImportRow(row);
                }
            }


            return subTable;
        }  */ //Unused, task is now completed within RunID3 method

        public DataTable CollectMatchingRowsDouble(DataTable t, DataColumn col, double Threshold, out DataTable remainder)  //WORKING (TESTED)
        {
            DataTable z = t.Clone();
            DataTable subTable = t.Clone();
            string Target = col.ColumnName;
            foreach (DataRow row in t.Rows)
            {
                if (Convert.ToDouble(row[Target]) <= Threshold)           //collects all rows below or equal to threshold 
                {
                    subTable.ImportRow(row);                              //copies matching rows to subTable, keeping format and values          
                }
                else z.ImportRow(row);
            }
            remainder = z;
            return subTable;
        }

        public DataColumn ReturnBestAttribute(DataTable t, List<DataColumn> attributes)
        {
            double highestInfoGain = 0;                          //generic starting point, could be +INFINITY
            DataColumn BestCol = t.Columns[0];                   //initialise as first column, arbitrary 

            for (int i = 0; i < attributes.Count - 1; i++)
            {           //-1 ensures we don't try to calculate entropy for the "Rings" column itself
                if (attributes[i].ColumnName != t.Columns[t.Columns.Count - 1].ColumnName)
                {
                    double disregard = 0;

                    double current = FindInfoGainPerThisAttribute(t, attributes[i], out disregard);
                    if (current > highestInfoGain)
                    {
                        highestInfoGain = current;
                        BestCol = t.Columns[i];
                    }
                }
            }

            //Console.WriteLine("Column with highest Information Gain -->" + BestCol.ColumnName);
            return BestCol;
        }                                       //WORKING

        public double FindInfoGainPerThisAttribute(DataTable t, DataColumn attribute, out double thresholdVal)   //two different methods for string column and for double column
        {                                                                               //works for "Sex" attribute with a 0.05 discrepancy from online calculator???
                                                                                        //Working correctly for selecting best double (C4.5 continuous variable info gain maximisation)
            double infoGainThisAtt = 0;
            string[] cols = new string[] { attribute.ColumnName, "Rings" };
            DataView v = new DataView(t);
            DataTable reduced_table = v.ToTable(false, cols);

            var values_of_class = reduced_table.AsEnumerable()                      //create a paired list <int key(age label), int value(no. of occurances)> for "Rings"
                .GroupBy(r => r.Field<int>("Rings"))                                              //
                .Select(r => new
                {
                    Str = r.Key,
                    Count = r.Count()
                }
               )
                .ToList();

            Dictionary<int, int> dic_classes = new Dictionary<int, int>();            //key = name of that attribute value(string), value = frequency in attribute column
            foreach (var item in values_of_class)
            {
                dic_classes.Add(item.Str, item.Count);
            }
            //////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            if (attribute.DataType == typeof(string))
            {                                           //will need an ELSE to deal with other column data types (ie. double)

                var att_unique_vals = reduced_table.AsEnumerable()                                    //need to fully understand how this lambda/LINQ works
                .GroupBy(r => r.Field<string>(attribute.ColumnName))                                     //change "Sex" to argument 'attribute' after testing
                .Select(r => new
                {
                    Str = r.Key,
                    Count = r.Count()
                }
               )
                .ToList();

                Dictionary<string, int> dic = new Dictionary<string, int>();            //key = name of that attribute value(string), value = frequency in attribute column
                foreach (var item in att_unique_vals)
                {
                    dic.Add(item.Str, item.Count);
                }

                int total_instances = 0;                                                //ie. will equal 4177 for whole set
                foreach (KeyValuePair<string, int> entry in dic)
                {
                    total_instances += entry.Value;
                }



                foreach (KeyValuePair<string, int> item in dic)
                {
                    if (item.Value != 0)                                                            //ensure we are not multiplying by 0..UNNECCESARY ?
                    {
                        var thisAttValue = from r in reduced_table.AsEnumerable()
                                           where r.Field<string>(attribute.ColumnName) == item.Key
                                           select r;

                        //Console.WriteLine(item.Key + "-->" + item.Value);  // = CORRECT
                        //Console.WriteLine(total_instances);  // = CORRECT
                        DataTable miniTable = thisAttValue.CopyToDataTable();
                        double entropyOfMiniTable = FindEntropy(miniTable);

                        infoGainThisAtt += entropyOfMiniTable * (Convert.ToDouble(item.Value) / Convert.ToDouble(total_instances));  //multiples proportion of M/F/I in Sex by their Entropy to get weighted total
                        //Console.WriteLine(infoGainThisAtt);                                      
                    }
                }

                double total = FindEntropy(t) - infoGainThisAtt;
                thresholdVal = 0;
                //Console.WriteLine("Total Information Gain for Column: " + attribute.ColumnName + " is:     " + total); //TESTING
                return total;

            }

            //////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////C4.5 Implementation-->finds best threshold value///////////////////////////////////
            if (attribute.DataType == typeof(double))
            {

                var att_unique_vals = reduced_table.AsEnumerable()
                .GroupBy(r => r.Field<double>(attribute.ColumnName))
                .Select(r => new
                {
                    Doub = r.Key,
                    Count = r.Count()
                }
               )
                .ToList();

                Dictionary<double, int> dicdouble = new Dictionary<double, int>();            //key = name of that attribute value(string), value = frequency in attribute column
                foreach (var item in att_unique_vals)
                {
                    dicdouble.Add(item.Doub, item.Count);
                }

                double ThresholdEntropy = 10;                                                      //generic starting point, could be +INFINITY
                double ThresholdValue = 0;
                //List<double> testThreshVals = new List<double>(); //FOR TESTING
                foreach (KeyValuePair<double, int> item in dicdouble)
                {
                    var thisAttValueHigher = from r in reduced_table.AsEnumerable()
                                             where r.Field<double>(attribute.ColumnName) > item.Key   //gets values greater than this attribute Value
                                             select r;

                    var thisAttValueLower = from r in reduced_table.AsEnumerable()
                                            where r.Field<double>(attribute.ColumnName) <= item.Key   //gets values greater than this attribute Value
                                            select r;


                    if (thisAttValueHigher.Count() != 0 && thisAttValueLower.Count() != 0)
                    {
                        DataTable miniTableHigher = thisAttValueHigher.CopyToDataTable();
                        DataTable miniTableLower = thisAttValueLower.CopyToDataTable();

                        if (miniTableHigher.Rows.Count != 0 && miniTableLower.Rows.Count != 0)
                        {
                            double total_rows = miniTableHigher.Rows.Count + miniTableLower.Rows.Count;
                            double entMTH = (miniTableHigher.Rows.Count / total_rows) * FindEntropy(miniTableHigher);   //mutiply each conditional entropy by its respective weight 
                            double entMTL = (miniTableLower.Rows.Count / total_rows) * FindEntropy(miniTableLower);
                            double totalEntropyOfThreshold = entMTH + entMTL;
                            //testThreshVals.Add(totalEntropyOfThreshold);   //FOR TESTING 
                            if (totalEntropyOfThreshold < ThresholdEntropy)
                            {
                                ThresholdEntropy = totalEntropyOfThreshold;
                                ThresholdValue = item.Key;                                     //finds the best threshold value best on entropy of that split                               
                            }
                        }

                    }
                }

                //File.WriteAllLines(@"C:\Users\Andy Rippington\Documents\CE301 Test Folder\TestDoc.CSV", testThreshVals.Select(x => string.Join(",", x))); //FOR TESTING               
                double total = FindEntropy(t) - ThresholdEntropy;
                thresholdVal = ThresholdValue;
                //Console.WriteLine("Threshold Value is: -->  " + ThresholdValue);
                //Console.WriteLine("Threshold Entropy is: -->  " + ThresholdEntropy);
                //Console.WriteLine("Information Gain for the best split in Column: " + attribute.ColumnName + " is: -->" + total);
                return total;
                //return total +"   " + ThresholdValue
            }
            thresholdVal = 0;
            return 0;  //else return 0 (Will also return 0 in the case of empty sub-sets, thus signifying that a leaf node has been reached?)

        }

        public double FindEntropy(DataTable t)                                          //Entropy value will lie in the range [0-->log2(N)] where N = no. of classes
        {                                                                               //log2(28) = 4.80735492206 = Max Entropy Value (equal occurances of each class)
            DataView v = new DataView(t);
            DataTable label_col = v.ToTable("Rings");                                   //Select the coulmn which refers to the class/labels

            var result = label_col.AsEnumerable()                                       //need to fully understand how this lambda/LINQ works
               .GroupBy(r => r.Field<int>("Rings"))
               .Select(r => new
               {
                   Str = r.Key,
                   Count = r.Count()                                                   //might have to perform calculation inside this loop...

               }
              )
               .ToArray();

            int total_number_of_labels = 0;

            for (int i = 0; i < result.Count(); i++)                                     //retrieve total number of labels in supplied datatable (
            {
                total_number_of_labels += result[i].Count;
                //result[i].Count is the numberof times each label occurs               
            }                                                                           //result[i] is the List of pairs --> (int Str, int Count)

            //Console.WriteLine(total_number_of_labels);

            double Entropy = 0;

            for (int i = 0; i < result.Count(); i++)
            {
                Entropy += X_log2_X(result[i].Count, total_number_of_labels);

            }

            return Entropy;                                                            //Checked with WolframAlpha, WORKING CORRECTLY
        }

        private double X_log2_X(double x, double y)                           //x = numerator, y = denumerator //WORKING CORRECTLY
        {                                                                      //x is x/y form and is first input parameter. y is second input parameter
            double z;
            if (x == 0 || y == 0) { return 0; }                                //return 0 if x or y = 0
            else
            {
                z = (-x / y) * (Math.Log(x / y, 2));                               //Use this like: - X_log2_X(2,3750) - X_log2_X(3,478) - ...
            }                                                                  //REMEMBER THIS ALREADY INCLUDES THE MINUS SIGN
            return z;
        }

        private int GetMostCommonLabel(DataTable t)                             //checked and working correctly
        {
            var result = t.AsEnumerable()
               .GroupBy(r => r.Field<int>("Rings"))
               .Select(r => new
               {
                   Str = r.Key,
                   Count = r.Count()
               }
               );

            int most_frequent = 0;
            int most_common_label = 0;

            foreach (var item in result)
            {
                int l = Convert.ToInt32(item.Count);
                if (l > most_frequent)
                {
                    most_frequent = l;
                    most_common_label = Convert.ToInt32(item.Str);
                }
            }
            return most_common_label;
        }


        private bool AllLabelsTheSame(DataTable t)  //is a problem with the supplied dataTable schema???
        {
            //Console.WriteLine("AllLablesTheSame row count:" + t.Rows.Count); // correct no. or rows
            //Console.WriteLine("AllLablesTheSame column count:" + t.Columns.Count); //correct no. of columns
            //Console.WriteLine("Convert.ToInt32(t.Rows[0][t.Columns.Count - 1]) = " + Convert.ToInt32(t.Rows[0][t.Columns.Count - 1])); //correct cell

            for (int i = 0; i < t.Rows.Count - 1; i++)
            {
                if (Convert.ToInt32(t.Rows[0][t.Columns.Count - 1]) == Convert.ToInt32(t.Rows[i][t.Columns.Count - 1]))  
                {
                    continue;
                }
                else
                {

                    return false;
                }


            }
            return true;
        }


        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//

        static void Main(string[] args)
        {
            //PRINT OUTPUT TO CSV (NOT CONSOLE)
            /*FileStream filestream = new FileStream("CE301_console_output.csv", FileMode.Create);   //Outputs console text to CSV file: "console_data.csv"
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);*/

            //Console.WriteLine("");

            //MAIN ALGORITHM SEGMENT
            DataLoader test = new DataLoader();          // To view the table, set breakpoint here with F9 then F5 to start debugging,
            DataTable x = test.MakeMasterTable();        //then click on the magnifying glass next to "MasterTable (In the "Locals" window at bottom)
            List<DataColumn> all_attributes = new List<DataColumn>();
            foreach (DataColumn c in x.Columns)
            {
                all_attributes.Add(c);
            }

            //PREICTION TESTING
            DecisionTree proto = new DecisionTree();
            proto.root = proto.RunC4_5(x, all_attributes);
            Node root = proto.root;
            Accuracy a = new Accuracy();


            //Try 10 iterations of mutations

            for (int i = 0; i<10; i++){
                Console.WriteLine("Mutation number: " + i);
                a.GetAccuracy(root);
                Node[] nodes = root.flattenTree(root);
                root = root.randomMutateAndRebuild(nodes);
            }
            
            //a.GetAccuracy(root);

            //int n = 0;

            //Node[] nodes = root.flattenTree(root);       //Adds all nodes in the tree to an array

            /*
            for(int i = 0; i< nodes.Length; i++)
            {
                Console.WriteLine("Node: " + n + "   Threshold = " + nodes[i].threshold + "   Label = "+ nodes[i].label + "   Attribute = " + nodes[i].attribute);
                n++;
            }
            */


             
        }

    }
}
