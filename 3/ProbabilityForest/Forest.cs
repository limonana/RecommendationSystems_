using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityForest
{
    class Forest
    {
        Dictionary<string, Node> itemsTrees = new Dictionary<string, Node>();
        double treshold;
        private DB db;
        HashSet<string> chosenItems;

        public Forest(double threshold, DB db)
        {
            this.db = db;
            this.treshold = threshold;
            Build();
        }

        private void Build()
        {
            foreach (var item in db.GetAllItems())
            {
                itemsTrees.Add(item, BuildTree(item));
            }
        }

        private Node BuildTree(String item)
        {
            chosenItems = new HashSet<string>();
            return BuildTreeRec(item, db.GetAllUsers().ToList());            
        }

        private Node BuildTreeRec(String item, List<string> users)
        {
            if (users.Count <=Math.Ceiling(db.NumOfUsers() * treshold))
            {
                return CreateProbalityNode(item, users);
            }

            List<string> use;
            List<string> NotUse;
            string chosenItem;
            findBestDivide(item, users, out use, out NotUse,out chosenItem);
            
            if (use == null || NotUse == null || chosenItem == null)
                //can't divide anymore
                return CreateProbalityNode(item, users);

            chosenItems.Add(chosenItem);
            //for memory
            users.Clear();    
        
            Node node = new Node(chosenItem,BuildTreeRec(item, use),BuildTreeRec(item, NotUse));            
            return node;
        }

        private Node CreateProbalityNode(string item, List<string> users)
        {
            double p = (double)users.Count(usr => db.isUsing(usr, item)) / users.Count;
            return new Node(p);
        }

        private void findBestDivide(string item, List<string> users, out List<string> use, out List<string> NotUse, out string chosenItem)
        {
            double maxDiff = double.MinValue;
            double divideItemDiff = double.MaxValue;
            use = null;
            chosenItem = null;
            NotUse = null;            
            //Dont know if suppose to be db.ItemsOf(db.UsersOf(item))
            //because all users belong to item
            //can only hurt run time
            foreach (var divideItem in db.GetAllItems())
            {
                if (divideItem.Equals(item)) continue;
                if (chosenItems.Contains(divideItem)) continue;

                List<string> divideItemUse;
                List<string> divideItemNotUse;
                Divide(divideItem, users, out divideItemUse, out divideItemNotUse);
                
                if (divideItemUse.Count == 0 || divideItemNotUse.Count == 0)
                    continue;
                
                //O(users)
                IEnumerable<string> ItemUse1 = divideItemUse.Where(usr => db.isUsing(usr, item));
                IEnumerable<string> ItemUse2 = divideItemNotUse.Where(usr => db.isUsing(usr, item));

                double distForItem = difference(users, ItemUse1) + difference(users, ItemUse2);
                if (distForItem > maxDiff)
                {
                    chosenItem = divideItem;
                    maxDiff = distForItem;
                    use = divideItemUse;
                    NotUse = divideItemNotUse;
                    divideItemDiff = difference(users,use);
                }
                else
                {                
                    //for performance - not sure if have better way
                    //not must
                    if (distForItem == maxDiff)
                    {
                        if (difference(use, users) < divideItemDiff)
                        {
                            chosenItem = divideItem;
                            maxDiff = distForItem;
                            use = divideItemUse;
                            NotUse = divideItemNotUse;
                            divideItemDiff = difference(users, use);
                        }
                    }
                }
            }            
        }

        private static double difference(List<string> users, IEnumerable<string> ItemUse1)
        {
            return Math.Abs((double)ItemUse1.Count() / users.Count - 0.5);
        }

        private void Divide(string item,List<string> users, out List<string> use, out List<string> NotUse)
        {
            use = new List<string>();
            NotUse = new List<string>();
            foreach (var usr in users)
            {
                if (db.isUsing(usr, item))
                    use.Add(usr);
                else
                    NotUse.Add(usr);
            }
        }

        public double GetProbability(string usr, string item)
        {
            Node pNode = Travel(usr, itemsTrees[item]);
            return pNode.data.probability.Value;
        }

        private Node Travel(string usr, Node node)
        {
            if (node.isLeaf())
                return node;

            if (db.isUsing(usr, node.data.item))
                return Travel(usr, node.Use);
            else
                return Travel(usr, node.NotUse);
        }

        public bool Predict(string user, string item, double confidance)
        {
            return (GetProbability(user, item) >= confidance);
        }

    }
}
