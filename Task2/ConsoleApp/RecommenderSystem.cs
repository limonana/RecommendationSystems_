using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    using RankingDB = RankingDB<string, string>;
    public class RecommenderSystem
    {
        public enum PredictionMethod { Pearson, Cosine, Random, SVD, Stereotypes };
        //RankingDB _ratings;
        //RecommenderSystemEngine _engine;
        RecommenderSystemEngine _trainEngine;
        RankingDB _train;
        RankingDB _test;
        private IEnumerable<RankingDB.RankData> _allTest;

        public RecommenderSystem()
        {

        }

        public void Load(string sFileName, double dTrainSetSize)
        {
            RankingDB db = Load(sFileName);
            _test = new RankingDB();
            _train = new RankingDB();
            DivideDB(db, dTrainSetSize, _train, _test);
            _trainEngine = new RecommenderSystemEngine(_train);
        }

        public void TrainBaseModel(int cFeatures)
        {            
            double avg=(_train.SumRanks()+_test.SumRanks())/(_train.NumOfRanks()+_test.NumOfRanks());
            var svd = new SVD(avg, cFeatures);

            RankingDB train = new RankingDB();
            RankingDB validation = new RankingDB();
            DivideDB(_train, 0.95, train, validation);

            var ranks = train.GetAllData();
            double RMSE = double.MaxValue; double LastRMSE = double.MaxValue;
            while (RMSE <= LastRMSE)
            {
                LastRMSE = RMSE;
                svd.Train(ranks);
                _trainEngine.setSVD(svd);
                RMSE = ComputeRMSE(PredictionMethod.SVD, _trainEngine, validation.GetAllData());
            }
            //_engine.setSVD(svd);
        }

        //load a dataset from a file
        public RankingDB Load(string sFileName)
        {
            RankingDB db = new RankingDB();
            //_engine = new RecommenderSystemEngine(_ratings);
            var lines = File.ReadLines(sFileName);
            //var lines = LoadPart(sFileName, 50000);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                db.Add(parts[0], parts[1], int.Parse(parts[2]));
            }
            return db;
        }

        //use for tests
        /*public void Load(string sFileName, int amount)
        {
            _ratings = new RankingDB<string, string, double>();
            _engine = new RecommenderSystemEngine(_ratings);
            //var lines = File.ReadLines(sFileName);
            var lines = LoadPart(sFileName, amount);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                _ratings.Add(parts[0], parts[1], int.Parse(parts[2]));
            }
        }*/

        private string[] LoadPart(string sFileName, int p)
        {
            var reader = new StreamReader(sFileName);
            string[] res = new string[p];
            for (int i = 0; i < p; i++)
            {
                res[i] = reader.ReadLine();
            }

            return res;
        }
        //return an existing rating 
        public double GetRating(string sUID, string sIID)
        {
            double? res = _train.GetRank(sUID, sIID);
            if (res.HasValue)
                return res.Value;
            else
                return _test.GetRank(sUID, sIID).Value;
        }

        //return an histogram of all ratings that the user has used
        /*public Dictionary<double, int> GetRatingsHistogram(string sUID)
        {
            Dictionary<double, int> res = new Dictionary<double, int>();

            IEnumerable<double> AllRanks = _ratings.GetRanks(sUID);
            foreach (var rank in AllRanks)
            {
                if (!res.ContainsKey(rank))
                    res[rank] = 0;
                ++res[rank];
            }
            return res;
        }*/

        //predict the rating that a user will give to an item using one of the methods "Pearson", "Cosine", "Random"
        public double PredictRating(PredictionMethod m, string sUID, string sIID)
        {
            return _trainEngine.PredictRating(m, sUID, sIID);
        }

        //return the predicted weights of all ratings that a user may give an item using one of the methods "Pearson", "Cosine", "Random"
        public Dictionary<double, double> PredictAllRatings(PredictionMethod m, string sUID, string sIID)
        {
            return _trainEngine.PredictAllRatings(m, sUID, sIID);
        }

        //Compute the hit ratio of all the methods in the list for a given train-test split (e.g. 0.95 train set size)
        /*public Dictionary<string, double> ComputeHitRatio(List<PredictionMethod> lMethods, double dTrainSetSize)
        {
            Dictionary<string, double> res = new Dictionary<string, double>();
            foreach (var m in lMethods)
            {
                res.Add(m.ToString(), CalcHitRatio(m, dTrainSetSize));
            }
            return res;
        }*/



        /*private int GetGrade(PredictionMethod m, RankingDB train, RankingDB test)
        {
            var engineTrain = SetEngine(train);


            int grade = 0;
            var allTest = test.GetAllData();
            foreach (var rankData in allTest)
            {
                double pretictedRating = Math.Round(engineTrain.PredictRating(m, rankData.ranker, rankData.item));
                if (pretictedRating == rankData.rank)
                    ++grade;
            }
            return grade;
        }

        private RecommenderSystemEngine SetEngine(RankingDB train)
        {
            var engineTrain = new RecommenderSystemEngine(train);
            if (_engine.Mode == FilterMode.MaxUsers)
                engineTrain.MaxUsers = _engine.MaxUsers;
            else
                engineTrain.MinSimilarity = _engine.MinSimilarity;
            return engineTrain;
        }*/

        // the train and test are initiliazed
        private void DivideDB(RankingDB allRatings, double dTrainSetSize, RankingDB train, RankingDB test)
        {
            var users = allRatings.GetRankers();
            //quick add and contains
            HashSet<string> chosenUsers = new HashSet<string>();
            int testSize = (int)Math.Round((1 - dTrainSetSize) * allRatings.NumOfRanks());
            while (test.NumOfRanks() < testSize)
            {
                var currentUser = chooseNewRandomUser(users, chosenUsers);
                chosenUsers.Add(currentUser);
                IEnumerable<RankingDB.RankData> randomRanks = chooseSomeRandomRanks(currentUser, allRatings);
                train.Add(randomRanks);
                var other = otherRanks(currentUser, randomRanks, allRatings);
                int otherCount = allRatings.NumOfRanks(currentUser) - randomRanks.Count();
                int diff = otherCount - (testSize - test.NumOfRanks());
                if (diff > 0) //other count > free space in test
                {
                    train.Add(other.Take(diff));
                    test.Add(other.Skip(diff));
                }
                else
                    test.Add(other);
            }

            foreach (var usr in users)
            {
                if (!chosenUsers.Contains(usr))
                {
                    train.AddUser(usr,allRatings.GetUserData(usr));
                }
            }
        }

        private IEnumerable<RankingDB.RankData> otherRanks(string currentUser, IEnumerable<RankingDB.RankData> ranks, RankingDB allRatings)
        {
            return allRatings.GetRanksAndItems(currentUser).Except(ranks);
        }

        private IEnumerable<RankingDB.RankData> chooseSomeRandomRanks(string user, RankingDB allRatings)
        {
            //var usrRanks = allRatings.GetRanksAndItems(user);           
            var items = allRatings.getItems(user);
            Random rand = new Random();
            int k = rand.Next(1, items.Count() + 1);
            RankingDB.RankData[] res = new RankingDB.RankData[k];
            for (int i = 0; i < k; i++)
            {
                int index = rand.Next(items.Count());
                string item = items.ElementAt(index);
                res[i] = new RankingDB.RankData(user, item, allRatings.GetRank(user, item).Value);
            }
            return res;
        }

        private string chooseNewRandomUser(IEnumerable<string> users, HashSet<string> chosenUsers)
        {
            Random rand = new Random();
            string curUser;
            do
            {
                int index = rand.Next(users.Count());
                curUser = users.ElementAt(index);
            }
            while (chosenUsers.Contains(curUser));
            return curUser;
        }

        /*public void SetMaxUsers(int i)
        {
            _engine.MaxUsers = i;
        }

        public void SetSimilaritySaf(double p)
        {
            _engine.MinSimilarity = p;
        }*/

        //TODO
        public Dictionary<PredictionMethod, double> ComputeRMSE(List<PredictionMethod> lMethods, out Dictionary<PredictionMethod, Dictionary<PredictionMethod, double>> dConfidence)
        {
            _allTest = _test.GetAllData();
            Dictionary<PredictionMethod, double> res = new Dictionary<PredictionMethod, double>();
            foreach (var m in lMethods)
            {
                res.Add(m, ComputeRMSE(m, _trainEngine, _allTest));
            }

            //TODO
            dConfidence = new Dictionary<PredictionMethod, Dictionary<PredictionMethod, double>>();
            foreach (var m1 in lMethods)
            {
                dConfidence.Add(m1, new Dictionary<PredictionMethod, double>());
                foreach (var m2 in lMethods)
                {
                    if (m1 != m2)
                    {
                        dConfidence[m1].Add(m2, SignTest(m1, m2));
                    }
                }
            }
            return res;

        }

        private double SignTest(PredictionMethod A, PredictionMethod B)
        {
            double winA; double winB;
            Contest(A, B, out winA, out winB);
            return 1 - pAnotBetterThanB((int)Math.Round(winA), (int)Math.Round(winB));
        }

        private double pAnotBetterThanB(int nA, int nB)
        {
            int n = nA + nB;
            //n!
            double nAzeret = Azeret(n);

            double sum = 0;
            for (int k = nA; k <= n; k++)
            {
                //n!/(k!(n-k)!)*0.5^n                
                double kAzeret = Azeret(k);
                double nMinusKAzeret = Azeret(n - k);

                sum += Math.Pow(2, nAzeret - kAzeret - nMinusKAzeret - n); //-n <=> -log (2^n)<=>+log (1/2^n)<=>*(1/2)^n
            }
            return sum;
        }

        private double Azeret(int x)
        {
            double sum = 0;
            for (int i = 1; i <= x; i++)
            {
                sum += Math.Log(i, 2);
            }
            return sum;
        }

        private void Contest(PredictionMethod A, PredictionMethod B, out double winA, out double winB)
        {
            winA = 0; winB = 0;
            foreach (var rankData in _allTest)
            {
                double eA = Math.Abs(rankData.rank - PredictRating(A, rankData.ranker, rankData.item));
                double eB = Math.Abs(rankData.rank - PredictRating(B, rankData.ranker, rankData.item));
                if (eA < eB)
                    ++winA;
                if (eA > eB)
                    ++winB;

                if (eA == eB)
                {
                    winA += 0.5;
                    winB += 0.5;
                }

            }
        }

        private double ComputeRMSE(PredictionMethod m, RankingDB db)
        {
            return ComputeRMSE(m, new RecommenderSystemEngine(db), db.GetAllData());
        }

        private double ComputeRMSE(PredictionMethod m, RecommenderSystemEngine engineTrain, IEnumerable<RankingDB.RankData> allTest)
        {
            double sum = 0;
            foreach (var rankData in allTest)
            {
                double pretictedRating = Math.Round(engineTrain.PredictRating(m, rankData.ranker, rankData.item));
                sum += Math.Pow((rankData.rank - pretictedRating), 2);
            }
            return Math.Sqrt(sum / allTest.Count());
        }
    }
}

