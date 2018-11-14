using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystem
{
    class Vector:List<double>
    {
        public Vector(int size)
            : base(size)
        {           
            for (int i = 0; i < size; i++)
            {
                this.Add(0);
            }
        }

        /// <summary>
        /// scalar multiple
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double operator *(Vector v1,Vector v2)    
        {
            if (v1.Count!= v2.Count)
                throw new Exception("vector sized must be equal");
            double sum = 0;
            for (int i = 0; i < v1.Count; i++)
            {
                sum += v1[i] * v2[i];
            }
            return sum;
        }

        public static Vector operator *(Vector v1,double scalar)    
        {
            Vector res = new Vector(v1.Count);
            for (int i = 0; i < v1.Count; i++)
            {
                res[i] = scalar * v1[i];
            }
            return res;
        }

        public static Vector operator +(Vector v1,Vector v2)
        {
            if (v1.Count != v2.Count)
                throw new Exception("vector sized must be equal");

            Vector res = new Vector(v1.Count);
            for (int i = 0; i < v1.Count; i++)
            {
                res[i] = v1[i] + v2[i];
            }
            return res;
        }

        public static Vector operator -(Vector v1,Vector v2)
        {
            if (v1.Count != v2.Count)
                throw new Exception("vector sized must be equal");

            Vector res = new Vector(v1.Count);
            for (int i = 0; i < v1.Count; i++)
            {
                res[i] = v1[i] - v2[i];
            }
            return res;
        }

        public static Vector operator *(double scalar,Vector v1)
        {
            return v1 * scalar;
        }
    }
}
