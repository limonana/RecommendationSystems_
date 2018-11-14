using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    using RankingDB = RankingDB<string, string>;
    public class SVD//<TRanker, TItem, TRank>
    {
        Dictionary<string, double> bu;
        Dictionary<string, double> bi;
        Dictionary<string, Vector> pu;
        Dictionary<string, Vector> qi;
        double avg;

        private double gama;
        private double lambda;

        Random _rand = new Random();
        private int vectorSize;
        private double _MaxValue = 1000000;
        
        public SVD(double avgAllData, int latentFeatures)
        {            
            bu = new Dictionary<string, double>();
            pu = new Dictionary<string, Vector>();            
            bi = new Dictionary<string, double>();
            qi = new Dictionary<string, Vector>();

            vectorSize = latentFeatures;
            avg = avgAllData;

            //TODO:change them?
            gama = 0.005;
            lambda = 0.005;
        }

        /*private Vector GetRandomVector(int size)
        {
            Vector v = new Vector(size);
            for (int i = 0; i < size; i++)
            {                         
                v[i] = GetRandomValue();               
            }
            return v;
        }*/

        private Vector GetSmallVector(int size)
        {
            Vector v = new Vector(size);
            for (int i = 0; i < size; i++)
            {
                v[i] = GetSmallValue()/size;
            }
            return v;
        }

        /*private double GetRandomValue()
        {
            double factor = 1;
            return (_rand.NextDouble() - 0.5) * factor;
        }*/
        
        private double GetSmallValue()
        {
            return 0.01;
        }

        /*private double GetRandomValueForVector(int size)
        {
            return GetRandomValue() / (double)size;
        }*/

        public double Predict(string user, string item)
        {
            InitIfNeeded(user, item);
            double res= avg + bu[user] + bi[item] + (pu[user] * qi[item]);            
            return  res;
        }

        private void InitIfNeeded(string user, string item)
        {
            if (!bi.ContainsKey(item))
                bi[item] = GetSmallValue();
            if (!bu.ContainsKey(user))
                bu[user] = GetSmallValue();
            if (!pu.ContainsKey(user))
                pu[user] = GetSmallVector(vectorSize);
            if (!qi.ContainsKey(item))
                qi[item] = GetSmallVector(vectorSize);

        }

        public void Train(IEnumerable<RankingDB.RankData> ranks)
        {
            foreach (var rankData in ranks)
            {
                string usr = rankData.ranker;
                string item = rankData.item;                            

                double error = rankData.rank - Predict(usr,item);
                if (error.Equals(double.NaN))
                    Console.WriteLine("problem in svd. one of the parameters is infinity");                   

                //update params
                double BuChange = gama * (error - lambda * bu[usr]);
                if (CanUpdate(bu[usr],BuChange))
                    bu[usr] += BuChange;
                double BiChange = gama * (error - lambda * bi[item]);
                if (CanUpdate(bi[item] , BuChange))
                    bi[item] += BiChange;
                Vector qiChange = gama * (error * pu[usr] - lambda * qi[item]);
                if (CanUpdate(qi[item], qiChange))
                    qi[item] += qiChange;
                Vector puChange = gama * (error * qi[item] - lambda * pu[usr]);
                if (CanUpdate(pu[usr],puChange))
                    pu[usr] += gama * (error * qi[item] - lambda * pu[usr]);
            }
        }

        private bool CanUpdate(double x, double change)
        {
            if (x > _MaxValue && change > 0)
                return false;
            if (x < -_MaxValue && change < 0)
                return false;
            if (Math.Abs(change) > _MaxValue)
                return false;
            return true;
        }

        private bool CanUpdate(Vector vector, Vector change)
        {
            for (int i = 0; i < vector.Count; i++)
            {
                if (!CanUpdate(vector[i], change[i]))
                    return false;
            }
            return true;
        }
    }
}
