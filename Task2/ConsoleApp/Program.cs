using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*using RecommenderSystem.MovieLens;
using RecommenderSystem.Infrastructure;
using RecommenderSystem.Algorithms;
using RecommenderSystem.EvaluationMetrics;
using RecommenderSystem.Gimp;*/

namespace RecommenderSystem
{
    class Program
    {
        

        static void Assignment2()
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("Dating/ratings.dat", 0.95);
            rs.TrainBaseModel(10);
            Console.WriteLine("Real rating of user 1 to item 133 using SVD is " + Math.Round(rs.GetRating("1", "133"), 4));
            Console.WriteLine("Predicted rating of user 1 to item 133 using SVD is " + Math.Round(rs.PredictRating(RecommenderSystem.PredictionMethod.SVD, "1", "133"), 4));
            List<RecommenderSystem.PredictionMethod> lMethods = new List<RecommenderSystem.PredictionMethod>();
            lMethods.Add(RecommenderSystem.PredictionMethod.SVD);
            lMethods.Add(RecommenderSystem.PredictionMethod.Pearson);
            lMethods.Add(RecommenderSystem.PredictionMethod.Cosine);
            lMethods.Add(RecommenderSystem.PredictionMethod.Random);
            DateTime dtStart = DateTime.Now;
            Dictionary<RecommenderSystem.PredictionMethod, Dictionary<RecommenderSystem.PredictionMethod, double>> dConfidence = null;
            Dictionary<RecommenderSystem.PredictionMethod, double> dResults = rs.ComputeRMSE(lMethods, out dConfidence);
            Console.WriteLine("Hit ratio scores for Pearson, Cosine, SVD, and Random are:");
            foreach (KeyValuePair<RecommenderSystem.PredictionMethod, double> p in dResults)
                Console.Write(p.Key + "=" + Math.Round(p.Value, 4) + ", ");
            Console.WriteLine("Confidence P-values are:");
            foreach (RecommenderSystem.PredictionMethod sFirst in dConfidence.Keys)
                foreach (RecommenderSystem.PredictionMethod sSecond in dConfidence[sFirst].Keys)
                    Console.WriteLine("p(" + sFirst + "=" + sSecond + ")=" + dConfidence[sFirst][sSecond].ToString("F3"));
                    
            Console.WriteLine();
            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));
            Console.ReadLine();
        }

        static void Main(string[] args)
        {

            Assignment2();
        }
    }
}
