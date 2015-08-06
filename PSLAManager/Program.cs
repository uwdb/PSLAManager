using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PSLAManager
{
    class PSLAGenerator
    {
        //gathered config information
        String configurationFolderPath = @"..\..\..\PSLAFiles";
        static Boolean generateQueriesOnly;
        Boolean useRealRuntimes;

        //fixed parameters
        String schemaFile = "SchemaDefinition.txt";
        String tierFile = @"predictions_for_tiers\tiers.txt";
        public static List<String> tierPredictionFolders = new List<String>();
        int[] tiersWorkers = { 4, 6, 8, 16 };
        public static List<int> tiers = new List<int>();
      
        //data information
        double[] selectivities = { .1, 1, 10, 100 };
        public static List<Table> listOfTables = new List<Table>();
        public static Fact factTable;

        //query information
        public static List<Query> listOfQueries = new List<Query>();

        //PSLA clusters generated
        public static List<Cluster> clusterList = new List<Cluster>();
        
        static void Main(string[] args)
        {
            PSLAGenerator p = new PSLAGenerator();
            p.readConfig();
            p.dataQueryGeneration();

            if (!generateQueriesOnly) {
                p.buildPredictionModel();
                p.generatePSLA();
                p.outputPSLAResult();
            }
            else {
                Console.WriteLine(String.Empty);
                Console.WriteLine("Finished Generating Queries");
            }

            Console.ReadLine();
        }

        /*
         * Reads user input before generating the PSLA
         */
        public void readConfig()
        {
            String currentLine = String.Empty;

            Console.WriteLine("*******************************");
            Console.WriteLine("PSLAManager");
            Console.WriteLine("*******************************");

            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Empty);

            Console.WriteLine("Please select an option (either type 1 or 2 and then press <ENTER>)");
            Console.WriteLine(String.Empty);

            Console.WriteLine("Option 1: Generate a PSLA");
            Console.WriteLine("Option 2: Generate New Queries (for a custom schema)");

            Boolean selectedOption = false;
            int answer = 0;
            while (!selectedOption) {
                try {
                        answer = Int32.Parse(Console.ReadLine().Trim());
                        if (answer == 1 || answer == 2) {
                            selectedOption = true;
                            if (answer == 2) generateQueriesOnly = true;
                        }
                        else {
                            throw new Exception();
                        }  
                    }
                catch(Exception){
                    Console.WriteLine("Invalid answer. Please try again (1 or 2)");
                }
            }

            //creating a PSLA option
            if (answer == 1) {
                Boolean selectedRuntimesOption = false;
                String runtimeValue = String.Empty;
                while (!selectedRuntimesOption) {
                    try {
                        Console.WriteLine(String.Empty);
                        Console.WriteLine("Please select which runtimes to use for the PSLA: ");
                        Console.WriteLine(String.Empty);
                        Console.WriteLine("* (Press P) -- predicted runtimes  ");
                        Console.WriteLine("* (Press R) -- real runtimes <this is assuming perfect predictions> ");

                        //reading input
                        runtimeValue = Console.ReadLine().Trim();
                        Console.WriteLine(String.Empty);

                        if (String.Equals(runtimeValue, "P", StringComparison.OrdinalIgnoreCase) 
                            || String.Equals(runtimeValue, "R", StringComparison.OrdinalIgnoreCase)) {
                            selectedRuntimesOption = true;
                        }
                        else {
                            throw new Exception();
                        }
                    }
                    catch (Exception) {
                        Console.WriteLine("Invalid answer. Please try again (P or R)");
                    }
                }
                useRealRuntimes = String.Equals(runtimeValue, "p", StringComparison.OrdinalIgnoreCase) ? false : true;
            }
        }

        /*
         * Given a schema, load information about the dataset and generate queries
         */
        public void dataQueryGeneration()
        {
            //finding number of tiers
            FileReaderUtils.readTiers(configurationFolderPath, tierFile);
            Console.WriteLine("Number of Tiers found: " + tiers.Count());

            //read the data schema
            Console.WriteLine("Reading the Data Schema from " + schemaFile + "...");
            FileReaderUtils.readDataSchema(configurationFolderPath, schemaFile);

            int queryID = 1;
            List<double> allList = new List<double>();

            //first generate single table queries
            foreach (var currentSingleTable in listOfTables) {
                List<String> selectionattributesForTables = currentSingleTable.tableSelectivityList;
                for (int i = 1; i <= currentSingleTable.tableAttributeList.Count(); i++) { //possible projections
                    int counter = 0;
                    List<Attribute> projectingAttributes = (from a in currentSingleTable.tableAttributeList
                                                            orderby a.attributeSize descending
                                                            select a).Take(i).ToList();
                    foreach (var currentCoverage in selectionattributesForTables) { //possible selectivities
                        foreach (var currentTier in tiers) {
                            listOfQueries.Add(new SingleTableQuery(queryID, projectingAttributes, new List<Table> { currentSingleTable }, 
                                                                   currentSingleTable.tablePrimaryKey, currentCoverage, selectivities[counter],
                                                                   currentSingleTable, currentTier));
                        }
                        queryID++;
                        counter++;
                    }
                }
            }

            //generate join queries
            List<Table> totalDimensions = (from a in listOfTables
                                            where a is Dimension
                                            orderby a.tableSize descending
                                            select a).ToList();

            for (int i = 1; i < listOfTables.Count(); i++) {
                List<Table> tablesToJoin = (totalDimensions).Take(i).ToList();
                tablesToJoin.Add(factTable);

                //attributes from fact
                List<Attribute> completeAttributeList = new List<Attribute>();
                foreach (var currentAttribute in factTable.tableAttributeList.ToList()) {
                    completeAttributeList.Add(new Attribute(currentAttribute.attributeName, currentAttribute.attributeSize, factTable));
                }

                //attributes from the dimension tables
                var test = tablesToJoin.Where(x => !(x is Fact));
                foreach (var currentTable in tablesToJoin.Where(x => !(x is Fact))) {
                    foreach (var currentAttr in currentTable.tableAttributeList) {
                        if (!completeAttributeList.Contains(currentAttr))
                            completeAttributeList.Add(new Attribute(currentAttr.attributeName, currentAttr.attributeSize, currentTable));
                    }
                }

                List<String> selectionattributesForTables = tablesToJoin.Contains(factTable) ?
                             factTable.tableSelectivityList : tablesToJoin.First().tableSelectivityList;


                for (int j = 1; j <= completeAttributeList.Count(); j++) {
                    int counter = 0;
                    List<Attribute> projectingAttributes = (from a in completeAttributeList
                                                            orderby a.attributeSize descending
                                                            select a).Take(j).ToList();


                    foreach (var currentCoverage in selectionattributesForTables) {
                        foreach (var currentTiers in tiers) {
                            listOfQueries.Add(new JoinQuery(queryID, projectingAttributes, new Attribute(factTable.tablePrimaryKey.attributeName, 
                                                            factTable.tablePrimaryKey.attributeSize, factTable),
                                                            currentCoverage, selectivities[counter], tablesToJoin, factTable, currentTiers));
                        }
                        queryID++;
                        counter++;
                    }
                }
            }

            //output queries
            StreamWriter queryOutput = new StreamWriter(Path.Combine(configurationFolderPath, "SQLQueries-Generated.txt"));
            foreach (var currentQuery in listOfQueries.Where(l => l.queryTier == 1)) {
                queryOutput.WriteLine(currentQuery.ToString());
            }
            queryOutput.Close();

        }

        public void useRealTimes()
        {
            foreach (var t in tiers) {
                String realRuntimesPath = @"predictions_for_tiers\" + tierPredictionFolders[t - 1];
                StreamReader r = new StreamReader(Path.Combine(configurationFolderPath, realRuntimesPath + @"\realtimes.txt"));

                string currentLine = string.Empty;
                List<double> listoftimes = new List<double>();
                while ((currentLine = r.ReadLine()) != null) {
                    listoftimes.Add(double.Parse(currentLine));
                }

                int count = 0;
                foreach (var q in listOfQueries.Where(l => l.queryTier == t).OrderBy(l => l.queryID)) {
                    q.queryPredictedTime = listoftimes[count];
                    count++;
                }
            }
        }

        /*
         * Given the training data and testing data, predict the runtime for the generated queries
         */
        public void buildPredictionModel()
        {
            if (useRealRuntimes) {
                Console.WriteLine("Using real runtimes...");
                useRealTimes();
            }
            else {
                Console.WriteLine("Predicting runtimes...");
                foreach (var t in tiers) {
                    String predictionPath = @"predictions_for_tiers\" + tierPredictionFolders[t - 1];
                    predictForTier(Path.Combine(configurationFolderPath, predictionPath), t);
                }
            }
        }

        public void predictForTier(String configPredictionPath, int tier)
        {
            //predict runtimes using training and testing
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
            info.WindowStyle = ProcessWindowStyle.Hidden;

            var predictCommand = String.Format("/C java -classpath \"{0}\"" + " weka.classifiers.rules.M5Rules -M 4.0 -t \"{1}\" -T {2} -p 0 > \"{3}\"",
                                                configurationFolderPath + @"\predictions_for_tiers\weka.jar",
                                                configPredictionPath + "\\TRAINING.arff",
                                                configPredictionPath + "\\TESTING.arff",
                                                configPredictionPath + "\\results.txt");

            info.Arguments = predictCommand;
            Process.Start(info).WaitForExit();

            //parse prediction results
            FileReaderUtils.parsePredictionResults(configPredictionPath, tier);
        }

        /*
         * Generate a PSLA given predicted runtimes for the amount of configurations
         */
        public void generatePSLA()
        {
            createSingletons();

            //intra-cluster and cross-tier compressions
            Console.WriteLine("Preparing Queries through Intra-Cluster and Cross-Tier Compression ...");
            foreach (int currentTier in tiers)
            {
                intraClusterCompression(currentTier);
                foreach (int largestTier in tiers.Where(x => x > currentTier)) {
                    crossTierCompression(currentTier, largestTier);
                }
            }
        }

        public void createSingletons()
        {
            List<Query> currentInterval = new List<Query>();
            int counter = 0;

            foreach (var currentTier in tiers) {
                foreach (var currentQuery in listOfQueries.Where(x => x.queryTier == currentTier)) {
                    Cluster c = new Cluster("Cluster" + counter, currentTier, currentQuery.queryPredictedTime, currentQuery.queryPredictedTime,
                                            currentQuery.queryPredictedTime, currentQuery.queryPredictedTime);
                    c.setSingleton();
                    c.addQuery(currentQuery);
                    c.clusterTier = currentTier;
                    clusterList.Add(c);

                    counter++;
                }
            }
        }

        public void intraClusterCompression(int tier)
        {
            if (clusterList.Where(l => l.clusterTier == tier).Select(l => l.clusterMax).Count() == 0) return; //no more queries in this configuration

            var maxValue = clusterList.Where(l => l.clusterTier == tier).Select(l => l.clusterMax).Max();
            int[] humanIntervals = new int[] { 0, 10, 60, 300, 600, 1800, 3600, 7200 };

            int intervalMarker = 1;
            int counter = 0;

            var highInterval = humanIntervals[intervalMarker];
            int i = humanIntervals[intervalMarker - 1];

            bool done = false;

            while (!done) {
                var currentInterval = (from l in clusterList
                                       where l.clusterTier == tier && l.clusterMax >= i 
                                             && l.clusterMax < highInterval
                                       select l).ToList();

                if (currentInterval.Count() > 0) {
                    Cluster c = new Cluster("Cluster" + counter, tier,
                        currentInterval.Select(l => l.clusterMax).Min(),
                        currentInterval.Select(l => l.clusterMax).Max(), i, highInterval);
                    counter++;

                    foreach (var q in currentInterval) {
                        foreach (var qInner in q.getAllQueries()) {
                            c.addQuery(qInner);
                        }
                        clusterList.Remove(q);
                    }
                    clusterList.Add(c);
                    c.findRoots();
                }

                //check if finished
                if (highInterval > maxValue) {
                    var lit = clusterList.Where(l => l.clusterTier == tier && l.clusterSingleton == true);
                    done = true;
                }
                else {
                    intervalMarker++;
                    i = humanIntervals[intervalMarker - 1];
                    highInterval = humanIntervals[intervalMarker];
                }
            }
        }

        public void crossTierCompression(int to, int from)
        {
            List<ClusterPair> pairs = new List<ClusterPair>();
            List<ClusterPair> combinedPairs = new List<ClusterPair>();

            //converge once all possible clusters have been subsumed
            bool converged = false;
            while (!converged)
            {
                pairs = crossTierConvergeHelper(to, from);
                int countPairs = 0;
                foreach (var i in pairs) {
                    if (i.cluster1.subsumes(i.cluster2)) {
                        combinedPairs.Add(i); 
                        clusterList.Remove(i.cluster2);
                        break; //we found a subsumption
                    }
                    else {
                        countPairs++;
                    }
                }
                if (pairs.Count() == countPairs) converged = true;
            }
        
        }
  
        public class ClusterPair
        {
            //helper class for cluster pairs
            public Cluster cluster1;
            public Cluster cluster2;
            public ClusterPair(Cluster d, Cluster s)
            {
                foreach (var c in clusterList) {
                    this.cluster1 = d;
                    this.cluster2 = s;
                }
            }
        }

        public List<ClusterPair> crossTierConvergeHelper(int tier1, int tier2)
        {
            List<ClusterPair> possibleSubsumptionPairs = new List<ClusterPair>();

            foreach (var p in clusterList.Where(l => l.clusterTier == tier1 && l.clusterSingleton == false)) {
                foreach (var q in clusterList.Where(l => l.clusterTier == tier2)) {
                    double high;
                    double low;
                    if (p.clusterIntervalHigh == 0 & p.clusterIntervalLow == 0) {
                        high = p.clusterMax;
                        low = p.clusterMin;
                    }
                    else {
                        high = p.clusterIntervalHigh;
                        low = p.clusterIntervalLow;
                    }
                    if (high >= q.clusterMax && low <= q.clusterMin && !p.Equals(q))
                        possibleSubsumptionPairs.Add(new ClusterPair(p, q));
                }
            }
            return possibleSubsumptionPairs;
        }

        /*
         * Generate a PSLA given predicted runtimes
         */
        public void outputPSLAResult()
        {
            Console.WriteLine("Writing output to FinalPSLA.json...");
            StreamWriter allFile = new StreamWriter(Path.Combine(configurationFolderPath,"FinalPSLA.json"));

            JObject job = new JObject(
                              new JProperty("Tiers",
                                    from t in tiers
                                    select new JObject(
                                            new JProperty("Tier", t),
                                            new JProperty("NumberWorkers", tiersWorkers[t-1]),
                                            new JProperty("Query Groupings",
                                                from clusters in clusterList
                                                where clusters.clusterTier == t
                                                select new JObject(
                                                        new JProperty("SLA Threshold", clusters.clusterIntervalHigh),
                                                        new JProperty("Queries",
                                                                from q in clusters.getRootQueries()
                                                                select new JObject(
                                                                        new JProperty("Query", q.toStringShortHand()))))))));

            allFile.WriteLine(job.ToString());
            allFile.Close();

            Console.WriteLine("Done.");
        }

    }
}
