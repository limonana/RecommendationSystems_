using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbabilityForest
{
    class Node
    {
        public Node Use;
        public Node NotUse;
        public DataNode data;                
        public Node(double p)
        {
            this.data = new DataNode(p);
        }

        public Node(string chosenItem, Node use, Node notUse)
        {            
            this.data = new DataNode(chosenItem);
            this.Use = use;
            this.NotUse = notUse;
        }

        public bool isLeaf()
        {
            return data.probability.HasValue;
        }
    }

    class DataNode
    {
        public DataNode(string item)
        {
            this.item = item;
        }
        
        public DataNode(double p)
        {
            this.probability = p;
        }

        public string item;
        public double? probability;    
    }
}
