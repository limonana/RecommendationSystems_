using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityForest
{
    class Program
    {
        static void Main(string[] args)
        {
            DB db = new DB();
            db.LoadPart("AllCommands.txt",1000);

            Forest f = new Forest(0.05, db);
            Console.WriteLine(f.Predict("336262", "org.eclipse.ui.views.showView",0.8));
            Console.WriteLine(f.Predict("336553", "org.eclipse.ui.edit.delete",0.8));
            
                
            Console.ReadLine();
        }
    }
}
