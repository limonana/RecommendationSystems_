using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expirement
{
    using RecommenderSystem = RecommenderSystem.RecommenderSystem;
    public class TestMethods
    {        
        public struct TestData
        {
            public string method;
            public double value;
            public double grade;
        }

        Dictionary<string, TestData> _res = new Dictionary<string, TestData>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="min_minSimilarity">in percents</param>
        /// <param name="max_minSimilarity">in percents</param>
        /// <param name="minMaxSimilarUsers"></param>
        /// <param name="maxMaxSimilarUsers"></param>
        public Dictionary<string,TestData> Run(RecommenderSystem rs, int min_minSimilarity, int max_minSimilarity, int minMaxSimilarUsers, int maxMaxSimilarUsers)
        {            
            _res.Clear();
            List<RecommenderSystem.PredictionMethod> lMethods = new List<RecommenderSystem.PredictionMethod>();
            lMethods.Add(RecommenderSystem.PredictionMethod.Pearson);
            lMethods.Add(RecommenderSystem.PredictionMethod.Cosine);

            checkSimilarity(rs, min_minSimilarity, max_minSimilarity, lMethods);
            checkMaxUsers(rs, minMaxSimilarUsers, maxMaxSimilarUsers, lMethods);
            
            return _res;
        }

        private void checkMaxUsers(RecommenderSystem rs, int min, int max, List<RecommenderSystem.PredictionMethod> lMethods)
        {            
            for (int i = min; i <= max; i++)
            {
                rs.SetMaxUsers(i);
                var tmp = rs.ComputeHitRatio(lMethods, 0.95);
                foreach (var item in tmp)
                {
                    update(item.Key, item.Value, i, "Max Users");
                }

            }            
        }

        private void checkSimilarity(RecommenderSystem rs, int min_minSimilarity, int max_minSimilarity, List<RecommenderSystem.PredictionMethod> lMethods)
        {            
            for (int i = min_minSimilarity; i <= max_minSimilarity; i++)
            {
                double value = (double)i / 100.0;
                rs.SetSimilaritySaf(value);
                var tmp = rs.ComputeHitRatio(lMethods, 0.95);
                foreach (var item in tmp)
                {
                    update(item.Key, item.Value, value, "similarity");
                }
            }            
        }

        private void update(string method,double grade,double parameterValue,string mode)
        {
            if (!_res.ContainsKey(method))
            {
                _res.Add(method, new TestData(){grade = grade,value = parameterValue,method = mode});
            }
            else
            {
                if (grade > _res[method].grade || (mode =="similarity" && grade > _res[method].grade ))
                {
                    _res[method] = new TestData() { grade = grade, value = parameterValue, method = mode };                    
                }
            }            
        }
    }
}
