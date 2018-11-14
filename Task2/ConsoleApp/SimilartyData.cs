using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystem
{
    class SimilartyData<T> : SimilartyData<T, T>
    {
        public SimilartyData(Func<T, T, double> similartyFunc) :base(similartyFunc)
        {
        }        
        public override double GetSimilarity(T arg1, T arg2)
        {
            var sim = get(arg2,arg1);            
            if (sim.HasValue)
                return sim.Value;
            else
                return base.GetSimilarity(arg1, arg2);
        }
    }
    class SimilartyData<T1,T2>
    {
        Func<T1, T2, double> _similartyFunc;
        Dictionary<T1, Dictionary<T2, double>> _data = new Dictionary<T1, Dictionary<T2, double>>();

        public SimilartyData(Func<T1, T2, double> similartyFunc)
        {
            _similartyFunc = similartyFunc;
        }

        
        public void Add(T1 arg1, T2 arg2, double similarity)
        {
            if (!_data.ContainsKey(arg1))
                _data[arg1] = new Dictionary<T2, double>();
            _data[arg1][arg2] = similarity;
        }

        protected double? get(T1 arg1, T2 arg2)
        {
            if (_data.ContainsKey(arg1) && _data[arg1].ContainsKey(arg2))
                return _data[arg1][arg2];
            else
                return null;
        }

        /// <summary>
        /// get the similarirty if stored or if not calculate it
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="similartyFunc"></param>
        /// <returns>the similarity</returns>
        public virtual double GetSimilarity(T1 arg1, T2 arg2)
        {
            /*var sim = get(arg1, arg2);
            if (!sim.HasValue)
                Add(arg1,arg2,_similartyFunc(arg1,arg2));

            return get(arg1, arg2).Value;*/
            return _similartyFunc(arg1, arg2);
        }

    }
}
