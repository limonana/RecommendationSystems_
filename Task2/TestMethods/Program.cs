using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Expirement
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now);
            RecommenderSystem.RecommenderSystem testRS = new RecommenderSystem.RecommenderSystem();
            testRS.Load("Dating/ratings.dat",1000000);
            TestMethods test = new TestMethods();
            //assuming there are about 168000 / 17 users in dataSetThat contains 1000000 instead of 17,000,000
            var res = test.Run(testRS, 70, 95, (168000 / 17) / 4, (168000 / 17)/2);
            foreach (var p in res)
                Console.WriteLine("{0}: grade = {1},method ={2},paramter value = {3}", p.Key, p.Value.grade, p.Value.method, p.Value.value);
            
            Console.WriteLine(DateTime.Now);
            Console.ReadLine();
        }
    }
}
