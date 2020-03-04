using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;


//Plaigiarism Check needed (As some code is similar to that found online @"https://archive.codeplex.com/?p=id3algorithm")

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


        int count = 0;  //FOR TESTING
        public Node RunC4_5(DataTable x, List<DataColumn> attributes, DataColumn forceBestAttribute = null)    //C4.5 ALGORITHM
        {
            Node n = new Node();
            n.unique_id = count.ToString();
            count += 1;
            n.subTable = x;
            n.subTableAttributes = attributes;

            //INSERT DEPTH LIMITING PARAMETER HERE (IE. IF BRANCH LENGTH > 10, RETURN MOST COMMON LABEL)

            //////////////////////////////////////////////// STOPPING PARAMETERS//////////////////////////////////////////////////////////////
            if (x.Rows.Count <= 15)     //if only the class column is remaining or there are n or fewer rows left, find most common label
            {
                //Console.WriteLine("Row limiter reached. Labeling with most common target.");
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }

            if (attributes.Count < 1)
            {
                //Console.WriteLine("attributes.Count < 1");
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }

            if (AllLabelsTheSame(x))                                                 //checks if all labels are equal and returns the node as a leaf if so 
            {
                n.label = x.Rows[0][x.Columns.Count - 1].ToString();
                n.isLeaf = true;
                //Console.WriteLine("ALL LABELS THE SAME TRUE--> Labeling this leaf:   " + n.label);
                return n;
            }


            DataColumn bestAttribute = ReturnBestAttribute(x, attributes);           //need a "Get rows that meet specific criteria" method           
            n.attribute = bestAttribute;                                             //sets the node attribute to the correct column
                                                                                     //the number of rows in the supplied datatable, immediate children and grandchildren etc.

            if (forceBestAttribute != null) { bestAttribute = n.attribute = forceBestAttribute; }  //Used to force splitting on a pre-defined attribute (from mutation)

            if (bestAttribute.ColumnName == "Empty")
            {
                //Console.WriteLine("bestAttribute.ColumnName == Empty");
                n.label = GetMostCommonLabel(x);
                n.isLeaf = true;
                return n;
            }


            //////////////////////////////////////////////////////STRING BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////STRING BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////STRING BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////STRING BELOW//////////////////////////////////////////////////////////////////////////////


            if (bestAttribute.DataType == typeof(String))
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
                        n[indexStr].parent = n;
                        n[indexStr].parentRef = indexStr;
                    }
                    else
                        //attributes.Remove(bestAttribute);
                        //return n = RunC4_5(x, attributes);           //remove node at children index 1 or 2 if no rows match this condition 
                        //n[indexStr].removeChild();
                        n.removeChild(n[indexStr]);
                }
            }


            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////DOUBLE BELOW//////////////////////////////////////////////////////////////////////////////

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

                    //Console.WriteLine("Number of Rows in this LowerSet: " + LowerSet.Rows.Count);

                    if (t.Rows.Count != 0)            //Rows with values below or equal to threshold are indexed "1" in children, those above are indexed "2"
                    {

                        //Console.WriteLine("Splitting on Double attribute:  " + bestAttribute.ColumnName); //TESTING                        
                        //Console.WriteLine("NEW ITERATION");   //TESTING

                        attributes.Remove(bestAttribute);
                        n[indexDoub] = RunC4_5(t, attributes);
                        n[indexDoub].category = t.Rows[0][bestAttribute.Ordinal].ToString();
                        n[indexDoub].parent = n;
                        n[indexDoub].parentRef = indexDoub;
                    }
                    else
                    {
                        n.removeChild(n[indexDoub]);
                        //attributes.Remove(bestAttribute);
                        //return n = RunC4_5(x, attributes);
                        //n.label = Convert.ToInt32(x.Rows[0][x.Columns.Count-1]);    //if there are no more subrows, label the node with it's class value
                        //Console.WriteLine("Labeling this leaf:   " + n.label);
                    }
                }
            }

            //////////////////////////////////////////////////////INTEGER BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////INTEGER BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////INTEGER BELOW//////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////INTEGER BELOW//////////////////////////////////////////////////////////////////////////////

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

                //Console.WriteLine("Number of Rows in LowerSet: " + LowerSet.Rows.Count);
                //Console.WriteLine("Number of Rows in rem: " + rem.Rows.Count);

                foreach (DataTable t in tables)
                {
                    indexInt += 1;
                    //Console.WriteLine("Number of Rows in this LowerSet: " + LowerSet.Rows.Count);

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
                        //Console.WriteLine("Labeling this leaf:   " + n.label);
                        //n.label = GetMostCommonLabel(t);   //IS THIS NECESSARY???                        
                        //n[indexInt] = new Node();
                        //attributes.Remove(bestAttribute);
                        //return n = RunC4_5(x, attributes);
                        n.removeChild(n[indexInt]);
                        //return n;                                              
                    }
                }
            }
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
            /*
            if (z.Rows.Count == 0 || subTable.Rows.Count ==0)
            {
                Console.WriteLine("\n\n");
                Console.WriteLine("SUBTABLE");
                Console.WriteLine("Column:  " + col.ColumnName + "   Threshold:  " + Threshold);
                for(int i = 0; i < subTable.Rows.Count; i++) { Console.WriteLine(subTable.Rows[i][col].ToString()); }
                Console.WriteLine("\n\n");
                Console.WriteLine("REM");
                Console.WriteLine("Column:  " + col.ColumnName + "   Threshold:  " + Threshold);
                for (int i = 0; i < z.Rows.Count; i++) { Console.WriteLine(z.Rows[i][col].ToString()); }
                Console.WriteLine("\n\n");

            }*/
            return subTable;
        }

        public DataColumn ReturnBestAttribute(DataTable t, List<DataColumn> attributes)
        {

            double highestInfoGain = -1;                          //generic starting point, could be -INFINITY
            DataColumn BestCol = t.Columns[0];                   //initialise as first column, arbitrary 

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
            //Console.WriteLine("Column with highest Information Gain -->" + BestCol.ColumnName);            
            return BestCol;
        }                                       //WORKING

        public double FindInfoGainPerThisAttribute(DataTable t, DataColumn attribute, out double thresholdVal)   //two different methods for string column and for double column
        {                                                                               //works for "Sex" attribute with a 0.05 discrepancy from online calculator???
                                                                                        //Working correctly for selecting best double (C4.5 continuous variable info gain maximisation)
            double infoGainThisAtt = 0;
            string[] cols = new string[] { attribute.ColumnName, t.Columns[t.Columns.Count - 1].ColumnName };  // "t.Columns[t.Columns.Count-1].ColumnName" = name of target column
            DataView v = new DataView(t);
            DataTable reduced_table = v.ToTable(false, cols);
            Type column_type = t.Columns[t.Columns.Count - 1].DataType;

            /*
            if (column_type == typeof(String))
            {
                var values_of_class = reduced_table.AsEnumerable()                         //create a paired list <int key(age label), int value(no. of occurances)> for Target Column
                    .GroupBy(r => r.Field<string>(t.Columns[t.Columns.Count - 1].ColumnName))
                    .Select(r => new
                    {
                        Str = r.Key,
                        Count = r.Count()
                    }
                   )
                    .ToList();

                Dictionary<string, int> dic_classes = new Dictionary<string, int>();            //key = name of that attribute value(string), value = frequency in attribute column
                foreach (var item in values_of_class)
                {
                    dic_classes.Add(item.Str, item.Count);
                }
            }*/

            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////STRING COLUMN TYPE//////////////////////////////////////////////////////////////////
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
                //Console.WriteLine("Identifying as string");
                //Console.WriteLine("Total Information Gain for Column: " + attribute.ColumnName + " is:     " + total); //TESTING
                //Console.WriteLine("Selected Split Information Gain =  " + total + "  Column Name =  " + attribute.ColumnName);
                return total;

            }

            //////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////DOUBLE COLUMN TYPE//////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////C4.5 Implementation-->finds best threshold value///////////////////////////////////
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

                double ThresholdEntropy = 1000;                                                      //generic starting point, could be +INFINITY
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
                //Console.WriteLine("Identifying as double");
                //Console.WriteLine("Threshold Value is: -->  " + ThresholdValue);
                //Console.WriteLine("Threshold Entropy is: -->  " + ThresholdEntropy);
                //Console.WriteLine("Information Gain for the best split in Column: " + attribute.ColumnName + " is: -->" + total);
                //Console.WriteLine("Selected Split Information Gain =  " + total + "  Column Name =  " + attribute.ColumnName);
                return total;
                //return total +"   " + ThresholdValue
            }

            //////////////////////////////////////////////////INT COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////INT COLUMN TYPE//////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////INT COLUMN TYPE//////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////C4.5 Implementation-->finds best threshold value///////////////////////////////////
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

                Dictionary<int, int> dicInt = new Dictionary<int, int>();            //key = name of that attribute value, value = frequency in attribute column
                foreach (var item in att_unique_vals)
                {
                    dicInt.Add(item.IntVal, item.Count);
                }

                double ThresholdEntropy = 1000;                                                      //generic starting point, could be +INFINITY
                double ThresholdValue = 0;
                //List<double> testThreshVals = new List<double>(); //FOR TESTING
                foreach (KeyValuePair<int, int> item in dicInt)
                {
                    var thisAttValueHigher = from r in reduced_table.AsEnumerable()
                                             where r.Field<int>(attribute.ColumnName) > item.Key   //gets values greater than this attribute Value
                                             select r;

                    var thisAttValueLower = from r in reduced_table.AsEnumerable()
                                            where r.Field<int>(attribute.ColumnName) <= item.Key   //gets values less than or equal to this attribute Value
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
                                ThresholdValue = item.Key;                                     //finds the best threshold value based on entropy of that split                               
                            }
                        }
                    }
                }

                thresholdVal = ThresholdValue;

                if (Convert.ToInt32(ThresholdEntropy) != 1000)   //Ensure that ThresholdValue has changed from the default 
                {
                    double total = FindEntropy(t) - ThresholdEntropy;
                    //Console.WriteLine("Selected Split Information Gain =  " + total + "  Column Name =  " + attribute.ColumnName);
                    return total;
                }
                else
                    return 0;
                //File.WriteAllLines(@"C:\Users\Andy Rippington\Documents\CE301 Test Folder\TestDoc.CSV", testThreshVals.Select(x => string.Join(",", x))); //FOR TESTING               
                //Console.WriteLine("Threshold Value is: -->  " + ThresholdValue);
                //Console.WriteLine("Threshold Entropy is: -->  " + ThresholdEntropy);
                //Console.WriteLine("Information Gain for the best split in Column: " + attribute.ColumnName + " is: -->" + total);
                //return total +"   " + ThresholdValue
            }

            else
            {
                Console.WriteLine("Error: Column not identified as double/int/string");
                thresholdVal = 0;
                return 0;
            }//else return 0 (Will also return 0 in the case of empty sub-sets, thus signifying that a leaf node has been reached?)          
        }

        public double FindEntropy(DataTable t)                                          //Entropy value will lie in the range [0-->log2(N)] where N = no. of classes
        {                                                                               //i.e. log2(28) = 4.80735492206 = Max Entropy Value (equal occurances of each class)
            DataView v = new DataView(t);
            DataTable label_col = v.ToTable(t.Columns[t.Columns.Count - 1].ColumnName);       //Select the coulmn which refers to the class/labels
            Type target_type = t.Columns[t.Columns.Count - 1].DataType;
            int total_number_of_labels = 0;
            double Entropy = 0;

            if (target_type == typeof(String))
            {
                var result = label_col.AsEnumerable()                                       //need to fully understand how this lambda/LINQ works
                   .GroupBy(r => r.Field<string>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()                                                   //might have to perform calculation inside this loop...

                   }
                  )
                   .ToArray();

                for (int i = 0; i < result.Count(); i++)                                     //retrieve total number of labels in supplied datatable (
                {
                    total_number_of_labels += result[i].Count;
                    //result[i].Count is the numberof times each label occurs               
                }

                for (int i = 0; i < result.Count(); i++)
                {
                    Entropy += _log_(result[i].Count, total_number_of_labels);

                }
                //Console.WriteLine("Entropy =  " + Entropy);
                return Entropy;

            }

            //Same logic but with taget data type as int (allows for RMSE calculation)
            else if (target_type == typeof(Int32))
            {
                var result = label_col.AsEnumerable()                                       //need to fully understand how this lambda/LINQ works
                   .GroupBy(r => r.Field<int>(t.Columns[t.Columns.Count - 1].ColumnName))
                   .Select(r => new
                   {
                       Str = r.Key,
                       Count = r.Count()                                                   //might have to perform calculation inside this loop...

                   }
                  )
                   .ToArray();

                for (int i = 0; i < result.Count(); i++)                                     //retrieve total number of labels in supplied datatable (
                {
                    total_number_of_labels += result[i].Count;
                    //result[i].Count is the numberof times each label occurs               
                }

                for (int i = 0; i < result.Count(); i++)
                {
                    Entropy += _log_(result[i].Count, total_number_of_labels);

                }
                //Console.WriteLine("Entropy =  " + Entropy);
                return Entropy;
            }

            else Console.WriteLine("Error in FindEntropy. Target Column not recognised as int or string");
            return 0;
        }

        private double _log_(double a, double b)                               //Function: x log2 x
        {                                                                      //x is x/y form and is first input parameter. y is second input parameter
            double c;
            if (a == 0 || b == 0) { return 0; }                                //return 0 if x or y = 0
            else
            {
                c = (-a / b) * (Math.Log(a / b, 2));                           //Use this like: - X_log2_X(2,3750) - X_log2_X(3,478) - ...
            }                                                                  //REMEMBER THIS ALREADY INCLUDES THE MINUS SIGN
            return c;
        }

        private string GetMostCommonLabel(DataTable t)                             //checked and working correctly
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
            //Console.WriteLine("AllLablesTheSame row count:" + t.Rows.Count); // correct no. or rows
            //Console.WriteLine("AllLablesTheSame column count:" + t.Columns.Count); //correct no. of columns
            //Console.WriteLine("Convert.ToInt32(t.Rows[0][t.Columns.Count - 1]) = " + Convert.ToInt32(t.Rows[0][t.Columns.Count - 1])); //correct cell

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


        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//
        // ----------------TESTING BELOW----------------------------TESTING BELOW----------------TESTING BELOW------------------------//

        static void Main(string[] args)
        {
            /*
            //PRINT OUTPUT TO CSV
            FileStream filestream = new FileStream("Abalone_Dynamic_test_log.csv", FileMode.Create);   //Outputs console text to CSV file: "console_data.csv"
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);*/



            //DataLoader d = new DataLoader();
           // d.get_K_Partitions("C:\\Users\\Andy Rippington\\Documents\\BSc Data Science and Analytics\\Year 3\\CE301 - Capstone Project\\Source Datasets\\Heart_Disease\\heart_disease_training_data_with_titles.csv", 5);

            
            //MAIN ALGORITHM SEGMENT
            DataLoader test = new DataLoader();
            DecisionTree proto = new DecisionTree();
            Accuracy a = new Accuracy();
            //test.get_K_Partitions();

            /////////////////////////////////////////////COMMENT/UNCOMMENT TO SELECT DATASET/////////////////////////////////////////
            //Abalone Dataset
            //DataTable x = test.MakeMasterTable("C:\\Users\\Andy Rippington\\Documents\\BSc Data Science and Analytics\\Year 3\\CE301 - Capstone Project\\Source Datasets\\Abalone\\abalone_training_dataset_with_column_names.csv");

            //Heart Dataset
            DataTable x = test.MakeMasterTable("C:\\Users\\Andy Rippington\\Documents\\BSc Data Science and Analytics\\Year 3\\CE301 - Capstone Project\\Source Datasets\\Heart_Disease\\heart_disease_training_data_with_titles.csv");
                       
            List<DataColumn> all_attributes = test.getAllAttributes(x);                          
            Node root = proto.root = proto.RunC4_5(x, all_attributes);
            root.isRoot = true;  //Set identifier of the root                      

            Node[] nodes = root.flattenTree(root);
            Console.WriteLine("Total nodes in this tree:  " + nodes.Length);           

            root.setAllDepths(nodes);
            foreach (Node n in nodes) { Console.WriteLine("Node depth:  " + n.depth); }

            Console.WriteLine("Levels in tree:   " + root.depthOfTree(nodes));
            Console.WriteLine("root.depth = " + root.depth);

            List<int> k = root.sizeOfSubTrees(nodes);
            foreach (int i in k) { Console.WriteLine("Subtree size:  " + i); }
            

            //PARAMETERS BELOW: No. of optimisation iterations, Objective Function

            //Set 'i < xxx' to adjust how many optimisation iterations are performed. 
            for (int i = 0; i < 1000; i++)
            {                                
                Console.WriteLine("Mutation number: " + i);                
                Console.WriteLine("Starting Accuracy: " + a.GetAccuracy(root));

                /////////////////////////////////////////////////SELECT OBJECTIVE FUNCTION///////////////////////////////////////////////////////////
                
                //root = root.randomMutateAndRebuild_Accuracy(root);                   //Objective Function: Maximise Accuracy
                //root = root.randomMutateAndRebuild_RMSE(root);                       //Objective Function: Minimise RMSE (For regression trees)

                //PARETO FRONT
                //The below objective function is a pareto front. It minimises the size of the tree while also increasing accuracy (if either remain stable, the change is accepted)
                root = root.randomMutateAndRebuild_Size(root);                         //Objective Function: Minimise size of the tree (number of nodes)                                                              
            }                            

            DOT_file_generator d = new DOT_file_generator();                         //Create visualisation of the tree
            d.createDOTfile(root);

        }



    } 
}
