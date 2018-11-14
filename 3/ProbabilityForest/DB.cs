using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityForest
{
    class DB
    {
        public void LoadPart(string fileName, int lineCount)
        {
            var reader = new StreamReader(fileName);            
            for (int i = 0; i < lineCount; i++)
            {
                addLine(reader.ReadLine());
            }
            reader.Close();
            
        }

        public void LoadAll(string fileName)
        {
            var lines = File.ReadLines(fileName);            
            foreach (var line in lines)
            {
                addLine(line);
            }
        }

        Dictionary<string, HashSet<string>> usersTOItems = new Dictionary<string, HashSet<string>>();               

        public void Add(string user, string item)
        {
            if (!usersTOItems.ContainsKey(user))
                usersTOItems.Add(user, new HashSet<string>());
            usersTOItems[user].Add(item);                
        }        


        public IEnumerable<string> GetAllUsers()
        {
            return usersTOItems.Keys;
        }

        public IEnumerable<string> GetAllItems()
        {
            return usersTOItems.SelectMany(x => x.Value).Distinct();
        }

        public bool isUsing(string user, string item)
        {
            return usersTOItems[user].Contains(item);
        }

        public int NumOfUsers()
        {
            return usersTOItems.Keys.Count;
        }
        public IEnumerable<string> ItemsOf(string user)
        {
            return usersTOItems[user];
        }

        public IEnumerable<string> ItemsOf(IEnumerable<string> users)
        {
            return users.SelectMany(usr => ItemsOf(usr));
        }

        public IEnumerable<string> UsersOf(string item)
        {
            return usersTOItems.Keys.Where(usr => isUsing(usr, item));
        }

        private void addLine(String line)
        {
            var parts = line.Split('\t');
            for (int i = 1; i < parts.Length; i+=2)
            {
                Add(parts[0], parts[i].Split(',')[0]);
            }
        }

    }
}
