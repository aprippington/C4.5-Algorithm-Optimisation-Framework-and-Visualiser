using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;


namespace Decision_Tree_Optimisation_Generalised
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
            this.root = null;
        }

        int count = 0;  
        public Node RunC4_5(DataTable x, List<DataColumn> attributes, DataColumn forceBestAttribute = null)    
        {
            Node n = new Node();
            n.unique_id = count.ToString();
            count += 1;
            n.subTable = x;
            n.subTableAttributes = attributes;           

            //////////////////////////////////////////////// STOPPING PARAMETERS//////////////////////////////////////////////////////////////
            if (x.Rows.Count <= 15)     //if only the class column is remaining or there are n or fewer rows left, find most common label
            {               
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }
            if (attributes.Count < 1)  
            {               
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }

            if (AllLabelsTheSame(x))      //checks if all labels are equal and returns the node as a leaf if so 
            {
                n.label = x.Rows[0][x.Columns.Count - 1].ToString();
                n.isLeaf = true;                
                return n;
            }

            DataColumn bestAttribute = ReturnBestAttribute(x, attributes);           
            n.attribute = bestAttribute;                                                                                                                                

            if (forceBestAttribute != null) { bestAttribute = n.attribute = forceBestAttribute; }  //Used to force splitting on a pre-defined attribute (from mutation)

            if (bestAttribute.ColumnName == "Empty")
            {                
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }

            //////////////////////////////////////////////////////STRING VARIABLLES//////////////////////////////////////////////////////////////////////////////

            if (bestAttribute.DataType == typeof(String))
            {
                DataTable unique_vals = x.DefaultView.ToTable(true, bestAttribute.ColumnName);  //Creates rows with all distinct attvalues (so can iterate through easily)                 
                int indexStr = -1;

                List<DataTable> tables = new List<DataTable>();

                foreach (DataRow row in unique_vals.Rows) //Splitting table into sub-tables for each value of this categorical attribte 
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
                }

                foreach (DataTable t in tables)
                {
                    indexStr += 1;                     
                    if (t.Rows.Count != 0)
                    {
                        attributes.Remove(bestAttribute);                      
                        n[indexStr] = RunC4_5(t, attributes);                        
                        n[indexStr].category = t.Rows[0][bestAttribute.Ordinal].ToString();
                        n[indexStr].parent = n;
                        n[indexStr].parentRef = indexStr;
                    }
                    else                       
                        n.removeChild(n[indexStr]);
                }
            }

            //////////////////////////////////////////////////////DOUBLE VARIABLE/////////////////////////////////////////////////////////////////////////////

            if (bestAttribute.DataType == typeof(Double))
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

                    if (t.Rows.Count != 0)            //Rows with values below or equal to threshold are indexed "1" in children, those above are indexed "2"
                    {                       
                        attributes.Remove(bestAttribute);
                        n[indexDoub] = RunC4_5(t, attributes);
                        n[indexDoub].category = t.Rows[0][bestAttribute.Ordinal].ToString();
                        n[indexDoub].parent = n;
                        n[indexDoub].parentRef = indexDoub;
                    }
                    else
                    {
                        n.removeChild(n[indexDoub]);                        
                    }
                }
            }

            //////////////////////////////////////////////////////INTEGER VARIABLE/////////////////////////////////////////////////////////////////////

            if (bestAttribute.DataType == typeof(Int32))
            {

                int indexInt = -1;

                FindInfoGainPerThisAttribute(x, n.attribute, out double thresh);  //returns the threshold value of best attribute to split on                

                n.threshold = thresh;

                DataTable LowerSet = x.Clone();    //copies the structure of input table
                DataTable rem = x.Clone();
                int threshInt = Convert.ToInt32(thresh);
                DataTable matching = CollectMatchingRowsInt(x, bestAttribute, threshInt, out rem);  //collects matching rows for each DataTable

                foreach (DataRow r in matching.Rows)
                {
                    LowerSet.ImportRow(r);
                }

                List<DataTable> tables = new List<DataTable>();

                tables.Add(LowerSet);                                       //Matching rows added first, will have index "1" in children 
                tables.Add(rem);

                foreach (DataTable t in tables)
                {
                    indexInt += 1;                   
                    if (t.Rows.Count != 0)            //Rows with values below or equal to threshold are indexed "1" in children, those above are indexed "2"
                    {
                        attributes.Remove(bestAttribute);
                        n[indexInt] = RunC4_5(t, attributes);
                        n[indexInt].category = t.Rows[0][bestAttribute.Ordinal].ToString();
                        n[indexInt].parent = n;
                        n[indexInt].parentRef = indexInt;
                    }
                    else
                    {                       
                        n.removeChild(n[indexInt]);                                                                    
                    }
                }
            }
            return n;
        }

        public DataTable CollectMatchingRowsDouble(DataTable t, DataColumn col, double Threshold, out DataTable remainder) 
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

        public DataTable CollectMatchingRowsInt(DataTable t, DataColumn col, int Threshold, out DataTable remainder)
        {
            DataTable z = t.Clone();
            DataTable subTable = t.Clone();
            string Target = col.ColumnName;
            foreach (DataRow row in t.Rows)
            {
                if (Convert.ToInt32(row[Target]) <= Threshold)
                {
                    subTable.ImportRow(row);
                }
                else z.ImportRow(row);
            }
            remainder = z;           
            return subTable;
        }

        public DataColumn ReturnBestAttribute(DataTable t, List<DataColumn> attributes)
        {
            double highestInfoGain = -1;                          
            DataColumn BestCol = t.Columns[0];                   
            for (int i = 0; i < attributes.Count; i++)
            {
                double disregard = 0;
                double current = FindInfoGainPerThisAttribute(t, t.Columns[i], out disregard);
                if (current > highestInfoGain)
                {
                    highestInfoGain = current;
                    BestCol = t.Columns[i];
                }
            }                 
            return BestCol;
        }                                       

        public double FindInfoGainPerThisAttribute(DataTable t, DataColumn attribute, out double thresholdVal) 
        {                                                                                                                                                                      
            double infoGainThisAtt = 0;
            string[] cols = new string[] { attribute.ColumnName, t.Columns[t.Columns.Count - 1].ColumnName };  
            DataView v = new DataView(t);
            DataTable reduced_table = v.ToTable(false, cols);
            Type column_type = t.Columns[t.Columns.Count - 1].DataType;
            
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            
            if (attribute.DataType == typeof(string))
            {
                var att_unique_vals = reduced_table.AsEnumerable()
                .GroupBy(r => r.Field<string>(attribute.ColumnName))
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

                int total_instances = 0;                                              
                foreach (KeyValuePair<string, int> entry in dic)
                {
                    total_instances += entry.Value;
                }

                foreach (KeyValuePair<string, int> item in dic)
                {
                    if (item.Value != 0)                                                            
                    {
                        var thisAttValue = from r in reduced_table.AsEnumerable()
                                           where r.Field<string>(attribute.ColumnName) == item.Key
                                           select r;                       
                        DataTable miniTable = thisAttValue.CopyToDataTable();
                        double entropyOfMiniTable = FindEntropy(miniTable);

                        infoGainThisAtt += entropyOfMiniTable * (Convert.ToDouble(item.Value) / Convert.ToDouble(total_instances));                                      
                    }
                }

                double total = FindEntropy(t) - infoGainThisAtt;
                thresholdVal = 0;                
                return total;

            }

            //////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            
            else if (attribute.DataType == typeof(double))
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
                double ThresholdEntropy = 1000;                                                     
                double ThresholdValue = 0;                
                foreach (KeyValuePair<double, int> item in dicdouble)
                {
                    var thisAttValueHigher = from r in reduced_table.AsEnumerable()
                                             where r.Field<double>(attribute.ColumnName) > item.Key   //gets values greater than this attribute Value
                                             select r;

                    var thisAttValueLower = from r in reduced_table.AsEnumerable()
                                            where r.Field<double>(attribute.ColumnName) <= item.Key   //gets values less than or equal to this attribute Value
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
                            if (totalEntropyOfThreshold < ThresholdEntropy)
                            {
                                ThresholdEntropy = totalEntropyOfThreshold;
                                ThresholdValue = item.Key;                                     //finds the best threshold value based on information gain                           
                            }
                        }

                    }
                }                           
                double total = FindEntropy(t) - ThresholdEntropy;
                thresholdVal = ThresholdValue;
                return total;               
            }

            //////////////////////////////////////////////////INT COLUMN TYPE//////////////////////////////////////////////////////////////////
            
            else if (attribute.DataType == typeof(Int32))
            {
                var att_unique_vals = reduced_table.AsEnumerable()
                .GroupBy(r => r.Field<int>(attribute.ColumnName))
                .Select(r => new
                {
                    IntVal = r.Key,
                    Count = r.Count()
                }
               )
                .ToList();

                Dictionary<int, int> dicInt = new Dictionary<int, int>();            
                foreach (var item in att_unique_vals)
                {
                    dicInt.Add(item.IntVal, item.Count);
                }

                double ThresholdEntropy = 1000;                                                    
                double ThresholdValue = 0;                
                foreach (KeyValuePair<int, int> item in dicInt)
                {
                    var thisAttValueHigher = from r in reduced_table.AsEnumerable()
                                             where r.Field<int>(attribute.ColumnName) > item.Key   
                                             select r;

                    var thisAttValueLower = from r in reduced_table.AsEnumerable()
                                            where r.Field<int>(attribute.ColumnName) <= item.Key   
                                            select r;


                    if (thisAttValueHigher.Count() != 0 && thisAttValueLower.Count() != 0)
                    {
                        DataTable miniTableHigher = thisAttValueHigher.CopyToDataTable();
                        DataTable miniTableLower = thisAttValueLower.CopyToDataTable();

                        if (miniTableHigher.Rows.Count != 0 && miniTableLower.Rows.Count != 0)
                        {
                            double total_rows = miniTableHigher.Rows.Count + miniTableLower.Rows.Count;
                            double entMTH = (miniTableHigher.Rows.Count / total_rows) * FindEntropy(miniTableHigher);   
                            double entMTL = (miniTableLower.Rows.Count / total_rows) * FindEntropy(miniTableLower);
                            double totalEntropyOfThreshold = entMTH + entMTL;                        
                            if (totalEntropyOfThreshold < ThresholdEntropy)
                            {
                                ThresholdEntropy = totalEntropyOfThreshold;
                                ThresholdValue = item.Key;                                                           
                            }
                        }
                    }
                }

                thresholdVal = ThresholdValue;
                if (Convert.ToInt32(ThresholdEntropy) != 1000)  
                {
                    double total = FindEntropy(t) - ThresholdEntropy;                    
                    return total;
                }
                else
                    return 0;               
            }
            else
            {
                Console.WriteLine("Error: Column not identified as double/int/string");
                thresholdVal = 0;
                return 0;
            }         
        }

        public double FindEntropy(DataTable t)                                             //Entropy value will lie in the range [0-->log2(N)] where N = no. of classes
        {                                                                                  //i.e. log2(28) = 4.80735492206 = Max Entropy Value (equal occurances of each class)
            DataView v = new DataView(t);
            DataTable label_col = v.ToTable(t.Columns[t.Columns.Count - 1].ColumnName);       
            Type target_type = t.Columns[t.Columns.Count - 1].DataType;
            int total_number_of_labels = 0;
            double Entropy = 0;

            if (target_type == typeof(String))
            {
                var result = label_col.AsEnumerable()                                     
                   .GroupBy(r => r.Field<string>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()                                                   

                   }
                  )
                   .ToArray();

                for (int i = 0; i < result.Count(); i++)                                     //retrieve total number of labels in supplied datatable 
                {
                    total_number_of_labels += result[i].Count;                               //result[i].Count is the number of times each label occurs 
                    Entropy += _log_(result[i].Count, total_number_of_labels);
                }                           
                return Entropy;
            }

            //Same logic but with taget data type as int (allows for RMSE calculation)
            else if (target_type == typeof(Int32))
            {
                var result = label_col.AsEnumerable()                                       
                   .GroupBy(r => r.Field<int>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()                                                   
                   }
                  )
                   .ToArray();

                for (int i = 0; i < result.Count(); i++)                                     //retrieve total number of labels in supplied datatable 
                {
                    total_number_of_labels += result[i].Count;                                 
                }
                for (int i = 0; i < result.Count(); i++)
                {
                    Entropy += _log_(result[i].Count, total_number_of_labels);
                }                
                return Entropy;
            }

            else Console.WriteLine("Error in FindEntropy. Target Column not recognised as int or string");
            return 0;
        }

        private double _log_(double a, double b)                               //Function: x log2 x
        {                                                                      //x is x/y form and is first input parameter. y is second input parameter
            double c;
            if (a == 0 || b == 0) { return 0; }                                
            else
            {
                c = (-a / b) * (Math.Log(a / b, 2));                           
            }                                                                 
            return c;
        }

        private string GetMostCommonLabel(DataTable t)                            
        {
            Type target_type = t.Columns[t.Columns.Count - 1].DataType;

            if (target_type == typeof(String))
            {
                var result = t.AsEnumerable()
                   .GroupBy(r => r.Field<string>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()
                   }
                   );

                int most_frequent = 0;
                string most_common_label = ("x");

                foreach (var item in result)
                {
                    int l = Convert.ToInt32(item.Count);
                    if (l > most_frequent)
                    {
                        most_frequent = l;
                        most_common_label = item.Str;
                    }
                }
                return most_common_label;
            }

            else
            {
                var result = t.AsEnumerable()
                   .GroupBy(r => r.Field<int>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()
                   }
                   );

                int most_frequent = 0;
                string most_common_label = ("x");

                foreach (var item in result)
                {
                    int l = Convert.ToInt32(item.Count);
                    if (l > most_frequent)
                    {
                        most_frequent = l;
                        most_common_label = item.Str.ToString();
                    }
                }
                return most_common_label;

            }
        }

        private bool AllValuesTheSame(DataTable t, int index)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if (t.Rows[0][index].ToString() == t.Rows[i][index].ToString())
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

        private bool AllLabelsTheSame(DataTable t)
        {           
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if (t.Rows[0][t.Columns.Count - 1].ToString() == t.Rows[i][t.Columns.Count - 1].ToString())
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

        static List<double> Accuracies_training = new List<double>();
        static List<double> Accuracies_validation = new List<double>();
        static List<double> Accuracies_testing= new List<double>();
        static List<Node> trees = new List<Node>();
        static List<double> tree_sizes = new List<double>();
        static List<double> rmse_list = new List<double>();

        static List<double> final_training_accuracies = new List<double>();
        static List<double> final_validation_accuracies = new List<double>();
        static List<double> final_testing_accuracies = new List<double>();
        static List<double> final_avg_tree_size = new List<double>();

        public void run_KMeans(string file_path, int k, out double train, out double valid, out double test, out double size, out double rmse)  
        {
            Accuracies_training.Clear();
            Accuracies_validation.Clear();
            Accuracies_testing.Clear();
            trees.Clear();
            tree_sizes.Clear();
            rmse_list.Clear();

            DataLoader d = new DataLoader();
            DecisionTree tree = new DecisionTree();
            Accuracy a = new Accuracy();

            d.get_K_Partitions(file_path, k);       //fills d.partitions with k even partitions of the dataset (each contains a header row)    
            
            for(int i = 0; i<k; i++){               //for each partition configuration

                Console.WriteLine("Partition  " + i + " / " + k + "   ---------------------------------------------------------------");
               
                List<string> training_data = new List<string>(); 
                List<string> testing_data = new List<string>();
                List<string> validation_data = new List<string>();

                training_data.Add(d.title_row);                        //Add title row to top of training set

                for (int j = 0; j < k; j++)
                {
                    if (j != i)                                        //Iteratively keep one partition to be used as the test set
                    {
                        for (int z = 0; z < d.partitions[j].Length; z++)
                        {
                            training_data.Add(d.partitions[j][z]);
                        }
                    }
                    else
                    {
                        for (int z = 0; z < d.partitions[j].Length; z++)
                        {
                            testing_data.Add(d.partitions[j][z]);
                        }
                    }
                }

                    //Reserve 50% of the training data to be the validation set (move the rows to validation_data)
                    int s = training_data.Count / 2;                    
                    validation_data = training_data.GetRange(training_data.Count-s, s);
                    training_data.RemoveRange(training_data.Count - s, s);
               
                    DataTable x = d.CreateTable(training_data.ToArray());     //input: string[] output: DataTable               
                    List<DataColumn> all_attributes = d.getAllAttributes(x);
                    Node root = tree.root = tree.RunC4_5(x, all_attributes);
                    root.isRoot = true;                                       //Set identifier of the root
                    root.pruneTree(root);
                    trees.Add(root);

                    training_data.RemoveAt(0);

                List<string> validation_subset = getValidationSubset(validation_data);

                //Optimise with respect to the validation set
                for (int it = 0; it < 10000; it++)
                {
                    /////////////////////////////////////////////////SELECT OBJECTIVE FUNCTION///////////////////////////////////////////////////////////                
                    //root = root.randomMutateAndRebuild_Accuracy(root);                   //Objective Function: Maximise Accuracy (regardless of size)
                    //root = root.randomMutateAndRebuild_RMSE(root);                       //Objective Function: Minimise RMSE (For regression trees, regardless of size)

                    //PARETO FRONT
                    //The below objective function is a pareto front. It minimises the size of the tree while also increasing accuracy (if either remain stable, the change is accepted)                    
                    if ((it % 100) == 0) { validation_subset = getValidationSubset(validation_data); }                  //Randomise validation subset every x iterations
                    root = root.randomMutateAndRebuild_Size(root, validation_subset.ToArray());                         //Objective Function: Minimise size of the tree (number of nodes)                           
                    //force a mutation here if the accuracy has not improved in the last 100 iterations, for instance...
                }

                //Save the accuracies of each partition
                Accuracies_training.Add(a.GetAccuracy(root, training_data.ToArray()));
                Accuracies_validation.Add(a.GetAccuracy(root, validation_data.ToArray()));
                Accuracies_testing.Add(a.GetAccuracy(root, testing_data.ToArray()));
                tree_sizes.Add(Convert.ToDouble(root.flattenTree(root).Length));
                rmse_list.Add(a.getRMSE(root, testing_data.ToArray()));

                    x.Clear();  //Clear DataTable so that we can begin the next C4.5 run - on the next partition                                                                                                         
            }

            Console.WriteLine("\n\n");
            Console.WriteLine("Final report: ");

            double training_total = 0;
            foreach (double q in Accuracies_training.Reverse<double>())
            {
                if (q != 0) { training_total += q; }
                else Accuracies_training.Remove(q);
                              
            }
            double average_training_accuracy = training_total / Accuracies_training.Count;
            
            double validation_total = 0;
            foreach (double q in Accuracies_validation.Reverse<double>())
            {
                if (q != 0)
                {
                    validation_total += q;
                }
                else Accuracies_validation.Remove(q);
            }
            double average_validation_accuracy = validation_total / Accuracies_validation.Count;

            double testing_total = 0;
            double highest_acc = double.NegativeInfinity;
            int highest_acc_index = 0;           

            for (int t = 0; t < Accuracies_testing.Count; t++)
            {
                if (Accuracies_testing[t] != 0) { 
                    testing_total += Accuracies_testing[t];
                    if (Accuracies_testing[t] > highest_acc) { highest_acc = Accuracies_testing[t]; highest_acc_index = t; }
                }
                else Accuracies_testing.RemoveAt(t);              
            }

            double average_testing_accuracy = testing_total / Accuracies_testing.Count;

            double tot = 0;
            foreach(double i in tree_sizes)
            {               
                tot += i/2;
            }

            double average_size = tot / tree_sizes.Count;

            double tot_rmse = 0;
            foreach(double r in rmse_list.Reverse<double>())
            {
                if (r != 0) { tot_rmse += r; }
                else rmse_list.Remove(r);
            }

            double average_rmse = tot_rmse / rmse_list.Count;

            //Set 'out' variables for collection
            train = average_training_accuracy;
            valid = average_validation_accuracy;
            test = average_testing_accuracy;
            size = average_size;
            rmse = average_rmse;

            
            Console.WriteLine("Training accuracies:");
            foreach(double p in Accuracies_training) { Console.WriteLine(p); }
            Console.WriteLine("Validation accuracies:");
            foreach (double p in Accuracies_validation) { Console.WriteLine(p); }
            Console.WriteLine("Testing accuracies:");
            foreach (double p in Accuracies_testing) { Console.WriteLine(p); }
            

            Console.WriteLine("Average training accuracy: " + average_training_accuracy);
            Console.WriteLine("Average validation accuracy: " + average_validation_accuracy);
            Console.WriteLine("Average testing accuracy: " + average_testing_accuracy);
            Console.WriteLine("Average tree size: " + average_size);

            Console.WriteLine("Printed tree (highest test accuracy) :  " + Accuracies_testing[highest_acc_index]);   
            
            //Visualise the tree with the highest test accuracy 
            DOT_file_generator df = new DOT_file_generator();                          
            df.createDOTfile(trees[highest_acc_index]);          
        }

        public List<string> getValidationSubset(List<string> full_set)
        {
            //Randomise validation data
            List<string> subset = new List<string>();
            Random rnd = new Random();
            full_set = full_set.OrderBy(i => rnd.Next(0, full_set.Count - 1)).ToList();
            int s = full_set.Count / 2;  //50% of validation set to be used in each iteration 
            for(int i = s; i<full_set.Count; i++)
            {
                subset.Add(full_set[i]);
            }
            return subset;
        }        

        public void runProgram()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();           

            /*
            //PRINT CONSOLE OUTPUT TO CSV
            FileStream filestream = new FileStream("console_data.csv", FileMode.Create);   //Outputs console text to CSV file: "console_data.csv"
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);*/

            List<double> trains = new List<double>();
            List<double> valids = new List<double>();
            List<double> tests = new List<double>();
            List<double> sizes = new List<double>();
            List<double> rmses = new List<double>();

            DecisionTree tree = new DecisionTree();

       
         
            for (int i = 0; i < 10; i++)
            {
                tree.run_KMeans("PATH_TO_TARGET_DATASET", 10, out double train, out double valid, out double test, out double size, out double rmse);                
                trains.Add(train);
                valids.Add(valid);
                tests.Add(test);
                sizes.Add(size);
                rmses.Add(rmse);
                Console.WriteLine("---------------------------------- ITERATION: " + i + "  -----------------------------------");
            }

            for( int i = 0; i<10; i++)
            {
                Console.WriteLine("\n");
                Console.WriteLine("Iteration: " + i + "\n" + "Train:   " + trains[i] + "\n" + "Valid:   " + valids[i] + "\n" + "Test:   " + tests[i]+ "\n" + "Size:   " + sizes[i] + "\n" + "RMSE:   " + rmses[i]);
            }

            //Create new Excel file containing test results
            XLWorkbook workbook = new XLWorkbook();                 
            DataTable table = new DataTable("table");

            
            DataColumn column1 = new DataColumn();
            column1.DataType = typeof(double);
            column1.ColumnName = "Training Accuracy";            
            column1.Unique = false;            
            column1.AllowDBNull = false;

            DataColumn column2 = new DataColumn();
            column2.DataType = typeof(double);
            column2.ColumnName = "Validation Accuracy";
            column2.Unique = false;
            column2.AllowDBNull = false;

            DataColumn column3 = new DataColumn();
            column3.DataType = typeof(double);
            column3.ColumnName = "Test Accuracy";
            column3.Unique = false;
            column3.AllowDBNull = false;

            DataColumn column4 = new DataColumn();
            column4.DataType = typeof(double);
            column4.ColumnName = "Average Size";
            column4.Unique = false;
            column4.AllowDBNull = false;

            
            DataColumn column5 = new DataColumn(); 
            column5.DataType = typeof(double);
            column5.ColumnName = "RMSE";
            column5.Unique = false;
            column5.AllowDBNull = false;
       
            table.Columns.Add(column1);
            table.Columns.Add(column2);
            table.Columns.Add(column3);
            table.Columns.Add(column4);
            table.Columns.Add(column5);  

            for (int i = 0; i < trains.Count; i++)
            {
                DataRow row = table.NewRow();
                row[0] = trains[i];
                row[1] = valids[i];
                row[2] = tests[i];
                row[3] = sizes[i];
                row[4] = rmses[i];
                table.Rows.Add(row);
            }
            
            workbook.Worksheets.Add(table);
            workbook.SaveAs("Example_statistics_output");

            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
            ts.Hours, ts.Minutes, ts.Seconds);
            Console.WriteLine("RunTime " + elapsedTime);

            TimeSpan tt = watch.Elapsed / 100;
            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            tt.Hours, tt.Minutes, tt.Seconds, tt.Milliseconds / 10);
            Console.WriteLine("Avg. training time (seconds) :  " + elapsedTime2);

        }

    } 
}
