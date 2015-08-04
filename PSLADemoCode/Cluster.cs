using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class Cluster
    {
        public String clusterName { get; set; }
        public int clusterTier { get; set; }

        public double clusterMax { get; set; }
        public double clusterMin { get; set; }
        public double clusterIntervalLow { get; set; }
        public double clusterIntervalHigh { get; set; }

        public Boolean clusterSingleton { get; set; }

        Dictionary<Query, List<Query>> clusterQueryMapper = new Dictionary<Query, List<Query>>();

        public Cluster()
        {
            this.clusterName = String.Empty;
            this.clusterMin = 0;
            this.clusterMax = 0;
        }

        public Cluster(String cName, int cConfig, double cMin, double cMax, double intervalLow, double intervalMax)
        {
            this.clusterName = cName;
            this.clusterTier = cConfig;
            this.clusterMin = cMin;
            this.clusterMax = cMax;
            this.clusterIntervalLow = intervalLow;
            this.clusterIntervalHigh = intervalMax;
            clusterSingleton = false;
        }

        public void setSingleton()
        {
            clusterSingleton = true;
        }

        public void addQuery(Query q)
        {
            if (!isRootQuery(q)) {
                clusterQueryMapper.Add(q, new List<Query>());
            }
        }
        public List<Query> getRootQueries()
        {
            return clusterQueryMapper.Keys.Distinct().ToList();
        }

        // returns a list of all the queries in the cluster
        public List<Query> getAllQueries()
        {
            List<Query> temp = new List<Query>();
            foreach (var q in clusterQueryMapper) {
                if (!temp.Contains(q.Key)) {
                    temp.Add(q.Key);
                    foreach (var qInner in q.Value) {
                        if (!temp.Contains(qInner)) temp.Add(qInner);
                    }
                }
            }
            return temp;
        }

        public void pushQueriesToCluster(List<Query> listOfQueries)
        {
            foreach (var currentQuery in listOfQueries) {
                if (!isRootQuery(currentQuery)) clusterQueryMapper.Add(currentQuery, new List<Query>());
            }
        }

        public void dropQueriesInCluster()
        {
            clusterQueryMapper.Clear();
        }

        /*
         * Determines whether one cluster subsumes another 
         */
        public bool subsumes(Cluster other)
        {
            var thisClusterRoots = this.getRootQueries();
            var OtherClusterRoots = other.getRootQueries();

            //if both clusters are in the same configuration
            if (this.clusterTier == other.clusterTier) {
                return ((other.clusterMax >= this.clusterMin && other.clusterMax <= this.clusterMax)
                      || (other.clusterMin <= this.clusterMax && other.clusterMin >= this.clusterMax));
            }

            else {
                if (this.clusterTier < other.clusterTier) {
                    //if other cluster is within bounds
                    if (other.clusterMin >= this.clusterIntervalLow && other.clusterMax <= this.clusterIntervalHigh) { 

                        /*comparison of queries from both clusters, if flagged then "this" cluster does not subsume "other"*/
                        foreach (var otherRoot in OtherClusterRoots) {
                            bool subsumed = false;
                            foreach (var thisRoot in thisClusterRoots) {
                                if (thisRoot.SubsumesOrEqual(otherRoot)) {
                                    subsumed = true;
                                }
                            }
                            if (!subsumed) return false;
                        }
                        return true; 
                    }
                    else //not within bounds
                    {
                        return false;
                    }
                }
                else //this cluster config is larger
                {
                    return false;
                }
            }
        }

        public bool isRootQuery(Query q){
            return clusterQueryMapper.Keys.Contains(q);
        }

        /*
         * When clusters merge, this method helps to find the "new" roots
         */
        public void findRoots()
        {
            int count;

            //creating copy of the root queries
            List<Query> outerList = clusterQueryMapper.Keys.ToList();
            List<Query> innerList = clusterQueryMapper.Keys.ToList();

            //pair-wise comparison between all queries in the cluster
            foreach (var outerQuery in outerList) {
                count = 0;
                foreach (var innerQuery in innerList) {
                    if (innerQuery.SubsumesOrEqual(outerQuery) && isRootQuery(innerQuery)) {
                        List<Query> subsumedQueryListFromInner = clusterQueryMapper[innerQuery];

                        if (!subsumedQueryListFromInner.Contains(outerQuery)) {
                            subsumedQueryListFromInner.Add(outerQuery);
                            //add all the subsumed queries from outer into the inner query
                            List<Query> subsumedQueryListFromOuter = clusterQueryMapper[outerQuery];
                            foreach (var v in subsumedQueryListFromOuter) {
                                if (!subsumedQueryListFromInner.Contains(v)) subsumedQueryListFromInner.Add(v);
                            }
                        }
                        //finally, remove the subsumed query
                        count++;
                        if (count > 1) {
                            clusterQueryMapper.Remove(outerQuery);
                            break;
                        }
                    }
                }
            }
        }
    }
}
