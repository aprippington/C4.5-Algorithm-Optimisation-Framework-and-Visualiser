using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Decision_Tree_Optimisation_Generalised
{
    class DataLoader
    {
        List<DataColumn> columns = new List<DataColumn>();
        List<string> label = new List<string>();                          //create list of STRINGS to represent the labels (ie. values in col.8 "Rings")

        //OPTION ONE, for single split
        public DataTable MakeMasterTable(string fileName)
        {            
            DataTable x = CreateTable(System.IO.File.ReadAllLines(@fileName));          
            return x;
        }

        public List<DataColumn> getAllAttributes(DataTable x)
        {
            List<DataColumn> all_attributes = new List<DataColumn>();
            for (int i = 0; i < x.Columns.Count - 1; i++) // -1 so we don't add the target column to the attribute list 
            {
                all_attributes.Add(x.Columns[i]);
            }
            return all_attributes;
        }



        //OPTION TWO, for k-validation
        //public static List<string[]> training_sets;
        //public static List<string[]> testing_sets;

        public static List<string[]> partitions = new List<string[]>();  // a list of k partitions, each with title row

        public void get_K_Partitions(String fileName, int k)
        {            
            string[] data = System.IO.File.ReadAllLines(fileName);
            
            string columnNames = data[0];                      //Store title row, must be added to the top of each training set
            List<string> data_list = new List<string>(data);
            data_list.RemoveAt(0);                             //Remove title row 

            //Randomly shuffle dataset
            Random rnd = new Random();                 
            data_list = data_list.OrderBy(i => rnd.Next(0, data_list.Count-1)).ToList();

            data = data_list.ToArray();

            int r = (data_list.Count) / k;                      //Number of rows within in each partition
            int remainder = (data_list.Count) % k;              //No. of "Extra" rows that do not fit into k - iterate through these and add one to each of the existing partitions           
            int r_original = r;                                   
            
            for(int i = 0; i<k; i++)
            {                
                for (int j = 0; j < data_list.Count; j += r_original) {                        
                    if(r > data_list.Count) { break; }
                    partitions.Add(data[j..r]);                    
                    r += r_original;                       
                }                
            }

            for(int i = 0; i<remainder; i++)                          //Add "Extra" rows that do not fit into k evenly
            {
                List<string> list = new List<string>(partitions[i]);
                list.Add(data[data.Length - 1 -i]);
                partitions[i] = list.ToArray();
            }

            for (int i = 0; i<k; i++)                                 //Insert column row to the top of each partition (must be removed when the partition is used for testing?) 
            {
                List<string> list = new List<string>(partitions[i]);
                list.Insert(0, columnNames);
                partitions[i] = list.ToArray();
            }
            Console.WriteLine("breakpoint");        
        }
       
        public DataTable CreateTable(string[] data)
        {
            DataTable table = new DataTable("MasterTable");
           
            string[] columnNames = data[0].Split(',');
            string[] firstRowOfData = data[1].Split(',');
            string[] columnDataTypes = new string[firstRowOfData.Length];

            for (int i = 0; i < columnNames.Length; i++)
            {
                if (Int32.TryParse(firstRowOfData[i], out int numberx)) { columnDataTypes[i] = "System.Int32"; }
                else if (Double.TryParse(firstRowOfData[i], out double number)) { columnDataTypes[i] = "System.Double"; }
                else { columnDataTypes[i] = "System.String"; }
                CreateColumn(columnNames[i], columnDataTypes[i]);
            }

            foreach (DataColumn col in columns)
            {
                table.Columns.Add(col);
            }

            int noOfColumns = data[0].Split(',').Length;

            for (int i = 1; i < data.Length; i++)        //Start index row at 1 to skip the title row
            {
                String[] elements = data[i].Split(',');
                row = table.NewRow();

                for (int j = 0; j < noOfColumns; j++)
                {
                    if (columns[j].DataType == typeof(Double))
                    {
                        if (Double.TryParse(elements[j], out double number) == true)
                        {
                            row[j] = Double.Parse(elements[j]);
                        }
                    }
                    else if (columns[j].DataType == typeof(Int32))
                    {
                        if (Int32.TryParse(elements[j], out int number1) == true)
                        {
                            row[j] = Int32.Parse(elements[j]);
                        }
                    }
                    else row[j] = elements[j];
                }

                table.Rows.Add(row);
                label.Add(elements[noOfColumns - 1]);
            }

            //Checks if column is binary and reassign to type "String" if so (APART FROM TARGET COLUMN)
            for (int x = 0; x < table.Columns.Count; x++)
            {
                //Console.WriteLine("Column name:  " + table.Columns[x] + "   " + isColumnBinary(table.Columns[x]));  //TESTING
                if (isColumnBinary(table.Columns[x]))
                {
                    //DataTable temptable = new DataTable();
                    column = new DataColumn();
                    column.DataType = System.Type.GetType("System.String");
                    column.ColumnName = "temp";
                    column.ReadOnly = false;
                    column.Unique = false;
                    //temptable.Columns.Add(column);

                    List<string> values = new List<string>();  //Store values to be inserted into column when it reenters master table

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        //DataRow r = temptable.NewRow();
                        //r[0] = table.Columns[x].Table.Rows[i][table.Columns[x].Ordinal].ToString();
                        values.Add(table.Columns[x].Table.Rows[i][table.Columns[x].Ordinal].ToString());
                    }
                    int o = table.Columns[x].Ordinal;
                    string n = table.Columns[x].ColumnName;

                    table.Columns.Remove(n);
                    //temptable.Columns.Remove(column);
                    table.Columns.Add(column);
                    column.ColumnName = n;
                    table.Columns[n].SetOrdinal(o);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        table.Rows[i][o] = values[i];
                    }

                }
            }

            string[] labels = label.ToArray();              //convert labels to string aray, CORRESPONDS TO "LABELS" in pseudo (might have to delete)

            List<String> attribute = new List<string>();
            if (table.Rows.Count > 0)
            {
                int count = table.Rows[1].Table.Columns.Count;
                for (int i = 0; i < count; i++)
                {
                    attribute.Add(table.Columns[i].ColumnName);
                }
            }

            string[] attributes = attribute.ToArray();

            return table;
        }

        DataColumn column;
        DataRow row;
        public void CreateColumn(string name, string type)
        {
            column = new DataColumn();
            column.DataType = System.Type.GetType(type);
            column.ColumnName = name;
            column.ReadOnly = true;
            column.Unique = false;
            columns.Add(column);
        }

        //column.Ordinal --> returns the 0 based index of the column in the DataColumnCollection

        public List<string[]> parseTestRows()  //NEEDS TO ALSO BE MADE DYNAMIC
        {

            //string[] data = System.IO.File.ReadAllLines(@"C:\Users\Andy Rippington\Documents\BSc Data Science and Analytics\Year 3\CE301 - Capstone Project\Source Datasets\Abalone\abalone_testing_dataset_no_column_names.csv");
            string[] data = System.IO.File.ReadAllLines(@"C:\Users\Andy Rippington\Documents\BSc Data Science and Analytics\Year 3\CE301 - Capstone Project\Source Datasets\Heart_Disease\heart_disease_test_data_no_column_titles.csv");
            //string[] data = System.IO.File.ReadAllLines(@"C:\Users\Andy Rippington\Documents\BSc Data Science and Analytics\Year 3\CE301 - Capstone Project\Source Datasets\Malware\pe_section_headers_shortened.csv"); 
            List<string[]> testRows = new List<string[]>();
            for (int i = 0; i < data.Length; i++)
            {
                String[] elements = data[i].Split(',');
                testRows.Add(elements);
            }
            return testRows;
        }

        public bool isColumnBinary(DataColumn c)             //WORKING CORRECTLY
        {
            string[] dic = new string[c.Table.Rows.Count];

            for (int i = 0; i < dic.Length; i++)
            {
                dic[i] = c.Table.Rows[i][c.Ordinal].ToString();
            }

            string a = dic[0];
            string b = dic[1];

            for (int j = 1; j < dic.Length; j++)
            {
                if (dic[j] == a) { continue; }
                else b = dic[j]; break;
            }

            for (int i = 2; i < dic.Length; i++)
            {
                if (dic[i] == a || dic[i] == b) { continue; }
                else return false;
            }
            return true;
        }

    }
}
