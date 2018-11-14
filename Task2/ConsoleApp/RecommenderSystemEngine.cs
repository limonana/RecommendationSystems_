using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommenderSystem
{    
    using RankingDB = RankingDB<string, string>;
    using PredictionMethod = RecommenderSystem.PredictionMethod;
    using SimilartyData = SimilartyData<string>;
    public class RecommenderSystemEngine
    {
      
        Dictionary<PredictionMethod, SimilartyData<string>> usr2usr =
            new Dictionary<PredictionMethod, SimilartyData>();
        RankingDB _db;        

        public FilterMode Mode { get; set; }        
        
        private double _minSimilarity;
        public double MinSimilarity
        {
            get { return _minSimilarity; }
            set {
                Mode = FilterMode.Similarity;
                _minSimilarity = value; 
            }
        }
        
        
        private int _maxUser;
        private SVD _svd;
        public int MaxUsers
        {
            get { return _maxUser; }
            set 
            {
                Mode = FilterMode.MaxUsers;
                _maxUser = value; 
            }
        }
        

        public RecommenderSystemEngine(RankingDB db)
        {
            usr2usr.Add(PredictionMethod.Pearson, new SimilartyData(Pearson));
            usr2usr.Add(PredictionMethod.Cosine, new SimilartyData(Cosine));
            _db = db;
            MinSimilarity =0.7;

        }        

        public double PredictRating(PredictionMethod m, string sUID, string sIID)
        {
            SimilartyData similarityData = null;
            switch (m)
            {
                case PredictionMethod.Pearson:
                    similarityData = usr2usr[m];
                    break;
                case PredictionMethod.Cosine:
                    similarityData = usr2usr[m];
                    break;
                case PredictionMethod.Random:
                    return GetRandomRank(sUID);                    
                case PredictionMethod.SVD:
                    return _svd.Predict(sUID, sIID);                    
                case PredictionMethod.Stereotypes:
                    break;
                default:
                    break;
            }
            if (similarityData == null)
                throw new NotImplementedException();
            IEnumerable<string> users = getUsers(sUID,sIID,similarityData).ToList();            
            double mone = 0;
            foreach (var usr in users)
            {
                if (usr == sUID) continue;                
                double avg = _db.GetRanks(usr).Average();
                var sim = similarityData.GetSimilarity(sUID, usr);
                if (sim > 0)
                    mone +=  sim * (_db.GetRank(usr, sIID).Value - avg);
            }

            double avgCurrent = 0;
            var ranks = _db.GetRanks(sUID).ToList();
            if (ranks.Count >0)
                avgCurrent = ranks.Average();
          
            if (mone == 0)
                return avgCurrent;

            double mechane = 0;
            foreach (var usr in users)
            {
                if (usr == sUID) continue;                
                var sim = similarityData.GetSimilarity(sUID, usr);
                if (sim >0)
                    mechane += sim;
            }
            //mechne will be zero only if mone will be zero, and if move is zero
            //we already exit
            return avgCurrent + mone / mechane;
        }

        private double GetRandomRank(string sUID)
        {
            var ranks = _db.GetRanks(sUID);
            Random rand = new Random();
            int index = rand.Next(ranks.Count());
            return ranks.ElementAt(index);
        }

        private double Cosine(string a, string u)
        {
            var items = _db.getItems(a).Intersect(_db.getItems(u)).ToList(); //force query
            if (items.Count == 0)
                return 0;

            double mone = items.Sum(i => _db.GetRank(a, i).Value * _db.GetRank(u, i).Value);            
            double mechane1 = Math.Sqrt(items.Sum(i => Math.Pow(_db.GetRank(a, i).Value, 2)));
            double mechane2 = Math.Sqrt(items.Sum(i => Math.Pow(_db.GetRank(u, i).Value, 2)));
            return mone / (mechane1 * mechane2);
        }

        private double Pearson(string a, string u)
        {            
            double avg1 = _db.GetRanks(a).Average();
            double avg2 = _db.GetRanks(u).Average();
            var items = _db.getItems(a).Intersect(_db.getItems(u)).ToList(); //forces immidiate execution
            if (items.Count == 0)
                return 0;

            double mone = 0;
            foreach (var item in items)
            {
                double rank1 = _db.GetRank(a, item).Value;
                double rank2 = _db.GetRank(u, item).Value;
                mone += (rank1 - avg1) * (rank2 - avg2);
            }
            if (mone == 0)
                return 0;

            double mechne = sideMechanePearson(a, items, avg1) * sideMechanePearson(u, items, avg2);            
            //TODO: check about happen if one of the users have the same grade for all the items
            return mone / mechne;
        }

        private double sideMechanePearson(string usr, IEnumerable<string> items, double avg)
        {
            double res = 0;
            foreach (var item in items)
            {
                res += Math.Pow(_db.GetRank(usr, item).Value - avg, 2);
            }
            return Math.Sqrt(res);
        }

        public Dictionary<double, double> PredictAllRatings(PredictionMethod m, string sUID, string sIID)
        {
            SimilartyData<string> similarityData = null;
            switch (m)
            {
                case PredictionMethod.Pearson:
                    similarityData = usr2usr[m];
                    break;
                case PredictionMethod.Cosine:
                    similarityData = usr2usr[m];
                    break;
                case PredictionMethod.Random:
                    break;
                case PredictionMethod.SVD:
                    break;
                case PredictionMethod.Stereotypes:
                    break;
                default:
                    break;
            }
            if (similarityData == null)
                throw new NotImplementedException();
            
            IEnumerable<string> users= getUsers(sUID, sIID, similarityData).ToList();

            Dictionary<double, double> rankToSumSimilarity = new Dictionary<double, double>();
            foreach (var usr in users)
            {                
                var rank = _db.GetRank(usr, sIID);                
                if (!rankToSumSimilarity.ContainsKey(rank.Value))
                    rankToSumSimilarity[rank.Value] = 0;
                double sim = similarityData.GetSimilarity(sUID, usr);
                if (sim >0)
                    rankToSumSimilarity[rank.Value] += sim;
            }

            return rankToSumSimilarity;
        }

        private IEnumerable<string> getUsers(string sUID, string sIID, SimilartyData<string> similarityData)
        {
            IEnumerable<string> users;
            if (Mode == FilterMode.MaxUsers)
            {
                users = _db.GetRankersOfItem(sIID).Where(usr => usr != sUID).OrderByDescending(usr=>similarityData.GetSimilarity(sUID, usr)).Take(MaxUsers);
            }
            else
            {
                users = _db.GetRankersOfItem(sIID).
                    Where(usr => usr != sUID &&
                        similarityData.GetSimilarity(sUID, usr) >= MinSimilarity).ToList();
            }
            return users;
        }

        public void setSVD(SVD svd)
        {
            _svd = svd;
        }
    }

    public enum FilterMode
    {
        Similarity,
        MaxUsers
    }
}
