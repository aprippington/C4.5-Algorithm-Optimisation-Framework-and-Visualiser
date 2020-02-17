using System;
using System.Collections.Generic;
using System.Text;

namespace Decision_Tree_Optimisation_Generalised
{
    class Accuracy
    {
        public double GetAccuracy(Node root)
        {
            DataLoader test = new DataLoader();
            List<string[]> L = test.parseTestRows();

            int sum_of_differences = 0;
            double sum_of_differences_squared = 0;
            int correct = 0;
            int incorrect = 0;
            int j = 0;
            foreach (string[] s in L)
            {
                j += 1;
                //Console.WriteLine("Prediction for row " + j + " =");                              
                bool compbool = Int32.TryParse(root.Predict(s), out int p);
                if (!compbool) { return 0.0; }                               //INCOMPATIBLE IF RETURNS 0.0
                int comp = Int32.Parse(root.Predict(s));
                if (comp == Int32.Parse(s[s.Length - 1]))
                {
                    correct += 1;
                }
                else incorrect += 1;
                int difference = Math.Abs(comp - Int32.Parse(s[s.Length - 1]));
                sum_of_differences += difference;
                sum_of_differences_squared += Math.Pow((comp - Int32.Parse(s[s.Length - 1])), 2);
            }


            double total = correct + incorrect;
            double Accuracy = Math.Round((((double)correct / total) * 100), 2);
            double MSE = ((1 / (double)L.Count)) * sum_of_differences_squared;

            double RMSE = Math.Sqrt(MSE);
            Console.WriteLine("Accuracy = " + Accuracy + "%");
            Console.WriteLine("Average difference(MAE) = " + ((double)sum_of_differences / (double)L.Count)); //Mean Absolute Error           
            Console.WriteLine("RMSE =" + RMSE); //Root Mean Squared Error

            return Accuracy;
        }


        //Implement k-fold cross validation for accuracy checking (https://machinelearningmastery.com/k-fold-cross-validation/)















    }
}
