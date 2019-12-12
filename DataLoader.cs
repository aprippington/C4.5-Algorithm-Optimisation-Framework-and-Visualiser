using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace CE301___Attempt_2
{
    class DataLoader
    {       
        //column.Ordinal --> returns the 0 based index of the column in the DataColumnCollection

        DataColumn column;
        DataRow row;
        List<string> label = new List<string>();                          //create list of STRINGS to represent the labels (ie. values in col.8 "Rings")

        public DataTable MakeMasterTable()                                //CORRESPONDS TO "x" in pseudo
        {
            DataTable table = new DataTable("MasterTable");
                   
            //Column 0                                                    //Create a foreach loop here to create all columns and detect their types (ie. int/string/double)
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Sex";
            column.ReadOnly = true;
            column.Unique = false;                 
            table.Columns.Add(column);

            //Column 1
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "Length";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 2
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "Diameter";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 3
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "Height";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 4
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "WholeWeight";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 5
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "ShuckedWeight";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 6
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "VisceraWeight";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 7
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "ShellWeight";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            //Column 8 - To be predicted
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Rings";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);



            string[] data = System.IO.File.ReadAllLines(@"C:\Users\Andy Rippington\Documents\BSc Data Science and Analytics\Year 3\CE301 - Capstone Project\Source Datasets\abalone_training.csv");
          
            for (int i = 0; i < data.Length; i++)
            {
                String[] elements = data[i].Split(',');
              
                    row = table.NewRow();

                    row["Sex"] = elements[0];
                    row["Length"] = elements[1];
                    row["Diameter"] = elements[2];
                    row["Height"] = elements[3];
                    row["WholeWeight"] = elements[4];
                    row["ShuckedWeight"] = elements[5];
                    row["VisceraWeight"] = elements[6];
                    row["ShellWeight"] = elements[7];
                    row["Rings"] = elements[8];
                    label.Add(elements[8]);                                      //add int value as a label (stored as strings)



                table.Rows.Add(row);
            }

            string[] labels = label.ToArray();                                  //convert labels to string aray, CORRESPONDS TO "LABELS" in pseudo (might have to delete)

            List<String> attribute = new List<string>();         
            if (table.Rows.Count > 0)
            {
                int count = table.Rows[1].Table.Columns.Count;
                for (int i = 0; i < count; i++)
                {
                    attribute.Add(Convert.ToString(table.Rows[0][i]));
                    
                }
            }

            string[] attributes = attribute.ToArray();                         //convert column names to string array, CORRESPONDS TO "y" in pseudo (might have to delete)

            Console.WriteLine(attributes);

            return table;
                                       
        }


        public List<string[]> parseTestRows()   
        {
            string[] data = System.IO.File.ReadAllLines(@"C:\Users\Andy Rippington\Documents\BSc Data Science and Analytics\Year 3\CE301 - Capstone Project\Source Datasets\abalone_testing.csv");
            List<string[]> testRows = new List<string[]>();
            for (int i = 0; i < data.Length; i++)
            {
                String[] elements = data[i].Split(',');
                testRows.Add(elements);
                for (int j = 0; j < elements.Length; j++) {
                    //Console.WriteLine(elements[j]);
                        }
                
            }                    
            return testRows;
        }
      
    }
}
