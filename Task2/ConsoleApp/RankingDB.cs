using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystem
{
    public class RankingDB<TRanker,TItem>
    {
        public class RankData : IEquatable<RankData>
        {
            public RankData(TRanker ranker, TItem item, double rank)
            {
                this.ranker = ranker;
                this.item = item;
                this.rank = rank;                
            }
            public TRanker ranker;
            public TItem item;
            public double rank;           

            public bool Equals(RankData other)
            {                
                return ranker.Equals(other.ranker) &&
                    item.Equals(other.item) && rank.Equals(other.rank);
            }
        }

        Dictionary<TRanker, Dictionary<TItem, double>> _data 
            = new Dictionary<TRanker, Dictionary<TItem, double>>();

        int _numOfRanks = 0;
        public void Add(TRanker ranker, TItem item, double rank)
        {
            if (!_data.ContainsKey(ranker))
            {
                _data[ranker] = new Dictionary<TItem, double>();
            }
            _data[ranker][item] = rank;
            //_allRankes = null;
            ++_numOfRanks;
        }
        
        public double? GetRank(TRanker ranker, TItem item)
        {
            if (!_data.ContainsKey(ranker))
                return null;
            
            if (!_data[ranker].ContainsKey(item))
                return null;

            return _data[ranker][item];
        }

        public IEnumerable<TRanker> GetRankers() { return _data.Keys; } 

        public IEnumerable<double> GetRanks(TRanker ranker)
        {
            if (_data.ContainsKey(ranker))
                return _data[ranker].Select(x => x.Value);
            else
                return new List<double>();
        }

        public IEnumerable<TItem> getItems(TRanker usr)
        {
            if (_data.ContainsKey(usr))
            {
                //suppose a user have a rank for item only once
                return _data[usr].Select(x => x.Key);
            }
            else
            {
                return new List<TItem>();
            }
        }

        //List<RankData> _allRankes;
        public IEnumerable<RankData> GetAllData()
        {
            /*
            //for effiecny
            if (_allRankes == null)
            {
                _allRankes = new List<RankData>();
                foreach (var ranker in GetRankers())
                {
                    _allRankes.AddRange(GetRanksAndItems(ranker));
                }                
            }
            return _allRankes;*/
            return GetRankers().SelectMany(ranker => GetRanksAndItems(ranker));
        } 


        public int NumOfRanks()
        {
            return _numOfRanks;
        }

        public int NumOfRanks(TRanker ranker)
        {            
            return _data[ranker].Count;            
        }

        public IEnumerable<RankData> GetRanksAndItems(TRanker ranker)
        {            
            return getItems(ranker).Select<TItem,RankData>(item=>
                new RankData(ranker, item, GetRank(ranker, item).Value));            
        }

        public void AddUser(TRanker ranker, Dictionary<TItem, double> data)
        {
            _data.Add(ranker, data);
            _numOfRanks += data.Count;
        }

        public void Add(IEnumerable<RankData> data)
        {
            foreach (var a in data)
            {
                Add(a.ranker, a.item, a.rank);
            }
        }

        public IEnumerable<TRanker> GetRankersOfItem(TItem item)
        {
            return GetRankers().Where(ranker=>_data[ranker].ContainsKey(item));
        }

        public IEnumerable<TItem> GetAllItems()
        {
            List<TItem> res = new List<TItem>();
            var users = GetRankers();
            return users.SelectMany(usr => getItems(usr)).Distinct();
            /*foreach (var usr in users)
            {
                res.AddRange(getItems(usr));
            }
            return res.Distinct();*/
        }

        public double Average()
        {
            double sum = SumRanks();
            return sum / NumOfRanks();
        }

        public double SumRanks()
        {
            double sum = 0;
            foreach (var ranker in _data.Keys)
            {
                foreach (var rank in _data[ranker].Values)
                {
                    sum += rank;
                }
            }
            return sum;
        }

        public Dictionary<TItem,double> GetUserData(TRanker usr)
        {
            return _data[usr];
        }
    }
}
