using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PSLADemo
{
    class PSLAGenerator
    {
        //gathered config information
        String configurationFolderPath = @"..\..\..\configFolder";
        int numTiers;
        Boolean useRealRuntimes;
         
        //fixed parameters
        String schemaFile = "SchemaDefinitionTPCH.txt";
        List<String> tierPredictionFolders = new List<String> { "4Workers", "6Workers", "8Workers", "16Workers"};
        int[] tiersWorkers = { 4, 6, 8, 16 };
        List<int> tiers = new List<int>();
      
        //data information
        double[] selectivities = { .1, 1, 10, 100 };
        List<Table> listOfTables = new List<Table>();
        Fact factTable;

        //query information
        List<Query> listOfQueries = new List<Query>();

        //PSLA clusters generated
        static List<Cluster> clusterList = new List<Cluster>();
        
        static void Main(string[] args)
        {
            PSLAGenerator p = new PSLAGenerator();
            p.readConfig();
            p.dataQueryGeneration();
            p.buildPredictionModel();
            p.generatePSLA();
            p.outputPSLAResult();
            Console.ReadLine();
        }

        public void readConfig()
        {
            String currentLine = String.Empty;
            int lineCount = 0;
         
            while(lineCount < 2){
                switch (lineCount) {
                    case 0:
                        Console.Write("Number of Configurations: ");
                        var numValue = Console.ReadLine();
                        numTiers = Int32.Parse(numValue);
                        break;
                    case 1:
                        Console.Write("Use predicted (P) or real (R) runtimes: ");
                        var runtimeValue = Console.ReadLine();
                        useRealRuntimes = String.Equals(runtimeValue, "P", StringComparison.OrdinalIgnoreCase) ? false : true;
                        break;
                }
                lineCount++;
            }

            for (int i = 1; i <= numTiers; i++) {
                tiers.Add(i);
            }
        }

        /*
         * Given a schema, load information about the dataset and generate queries
         */
        public void dataQueryGeneration()
        {
            //PARSE SCHEMA
            StreamReader schemaReader = new StreamReader(Path.Combine(configurationFolderPath,schemaFile));
            String currentLine = schemaReader.ReadLine();
            bool firstLine = true;
            while (currentLine != null) {
                List<Attribute> listofPrimaryKeys = new List<Attribute>();
                List<Attribute> listofAttributes = new List<Attribute>();
                List<String> listofSelectivities = new List<String>();

                //read table info
                String[] tableNameSize = currentLine.Substring(0).Split(',');
                String tableName = tableNameSize[0];
                int tableSize = int.Parse(tableNameSize[1]);
                Attribute foreignKeyAttribute = null;

                //loop through the table attributes
                while ((currentLine = schemaReader.ReadLine()).Contains("*")) {
                    bool isPrimaryKey = false;
                    String[] attributeNameSize = currentLine.Substring(1).Split(',');
                    String attributeName = attributeNameSize[0];
                    int attributeSize;

                    if (currentLine.Contains("#PK")) { //Primary Key
                        attributeSize = int.Parse(attributeNameSize[1].Substring(0, attributeNameSize[1].IndexOf(';')));
                        isPrimaryKey = true;
                    }
                    else {
                        attributeSize = int.Parse(attributeNameSize[1]);
                    }

                    if (attributeNameSize[1].Contains("#FK")) { //Foreign Key
                        String[] foreignKey = currentLine.Split(' ');
                        foreignKeyAttribute = new Attribute(foreignKey[1], attributeSize);
                    }

                    //create the attribute and add it to the lists
                    Attribute currentAttribute = new Attribute(attributeName, attributeSize);
                    if (isPrimaryKey) listofPrimaryKeys.Add(currentAttribute);
                    listofAttributes.Add(currentAttribute);
                }

                //read Selectivities
                listofSelectivities = currentLine.Substring(0).Split(';').ToList();
                listofSelectivities.Add(String.Empty); //adding the "nothing" for 100% 


                if (firstLine) {
                    //composite keys
                    if (listofPrimaryKeys.Count > 1) {
                        String compositeName = String.Empty;
                        int compositeSize = 0;
                        foreach (Attribute att in listofPrimaryKeys) {
                            compositeName += att.attributeName;
                            compositeSize += att.attributeSize;
                            if (!(listofPrimaryKeys.Last() == att)) compositeName += ",";
                        }
                        factTable = new Fact(tableName, listofAttributes, listofSelectivities, new Attribute(compositeName, compositeSize), tableSize);
                    }
                    else {
                        factTable = new Fact(tableName, listofAttributes, listofSelectivities, listofPrimaryKeys.First(), tableSize);
                    }
                    listOfTables.Add(factTable);
                    firstLine = false;
                }
                else {
                    listOfTables.Add(new Dimension(tableName, listofAttributes, listofSelectivities, listofPrimaryKeys.First(), tableSize, foreignKeyAttribute));
                }
                currentLine = schemaReader.ReadLine();
            }
            schemaReader.Close();

            //GENERATE QUERIES
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
                            listOfQueries.Add(new SingleTableQuery(queryID, projectingAttributes, new List<Table> { currentSingleTable }, currentSingleTable.tablePrimaryKey,
                                                                   currentCoverage, selectivities[counter], currentSingleTable, currentTier));
                        }
                        queryID++;
                        counter++;
                    }
                }
            }

            //join queries
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
                            listOfQueries.Add(new JoinQuery(queryID, projectingAttributes, new Attribute(factTable.tablePrimaryKey.attributeName, factTable.tablePrimaryKey.attributeSize, factTable),
                                               currentCoverage, selectivities[counter], tablesToJoin, factTable, currentTiers));
                        }
                        queryID++;
                        counter++;
                    }
                }
            } 
        }

        public void useRealTimes()
        {
            foreach (var t in tiers) {
                String realRuntimesPath = @"predictions\" + tierPredictionFolders[t - 1];
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
                    String predictionPath = @"predictions\" + tierPredictionFolders[t - 1];
                    predictForConfig(Path.Combine(configurationFolderPath, predictionPath), t);
                }
            }
        }

        public void predictForConfig(String configPredictionPath, int tier)
        {
            //predict runtimes using training and testing
            ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
            info.WindowStyle = ProcessWindowStyle.Hidden;
            var predictCommand = String.Format("/C java -classpath \"{0}\"" + " weka.classifiers.rules.M5Rules -M 4.0 -t \"{1}\" -T {2} -p 0 > \"{3}\"", "C:\\Program Files\\Weka-3-6\\weka.jar",
                                                configPredictionPath + "\\TRAINING.arff",
                                                configPredictionPath + "\\TESTING.arff",
                                                configPredictionPath + "\\results.txt");

            info.Arguments = predictCommand;
            Process.Start(info).WaitForExit();

            List<Query> listToModifyPredictions = listOfQueries.Where(x => x.queryTier == tier).ToList();

            foreach (var currentQuery in listToModifyPredictions) {
                currentQuery.queryPredictedTime = 0;
            }

            //parse the results
            StreamReader sr = new StreamReader(configPredictionPath + "\\results.txt");
            string currentline = String.Empty;

            //read header
            for (int i = 0; i < 5; i++) {
                currentline = sr.ReadLine();
            }

            //read the file and store the predictions for this configuration
            List<double> storePredictedTimes = new List<double>();
            while ((currentline = sr.ReadLine()) != null) {
                string[] parts = currentline.Split(null);
                List<double> temp = new List<double>();
                foreach (var p in parts) {
                    if (p != String.Empty) {
                        temp.Add(double.Parse(p));
                    }
                }
                if (temp.Count >= 4) {
                    double predValue = temp[2];
                    storePredictedTimes.Add(predValue);
                }
            }
            sr.Close();

            foreach (var currentQuery in listToModifyPredictions) {
                int id = currentQuery.queryID;
                if (storePredictedTimes[id - 1] < 0) {
                    currentQuery.queryPredictedTime = 0;
                }
                else {
                    currentQuery.queryPredictedTime = storePredictedTimes[id - 1];
                }
            }
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

            int insideToAdd = 1;
            int counter = 0;

            var highInterval = humanIntervals[insideToAdd];
            int i = humanIntervals[insideToAdd - 1];

            bool done = false;

            while (!done) {
                var currentInterval = (from l in clusterList
                                       where l.clusterTier == tier && l.clusterMax >= i && l.clusterMax < highInterval
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
                    insideToAdd++;
                    i = humanIntervals[insideToAdd - 1];
                    highInterval = humanIntervals[insideToAdd];
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
                                            new JProperty("NumWorkers", tiersWorkers[t-1]),
                                            new JProperty("Clusters",
                                                from clusters in clusterList
                                                where clusters.clusterTier == t
                                                select new JObject(
                                                        new JProperty("Threshold", clusters.clusterIntervalHigh),
                                                        new JProperty("Queries",
                                                                from q in clusters.getRootQueries()
                                                                select new JObject(
                                                                        new JProperty("QueryTemplate", q.toStringShortHand()))))))));

            allFile.WriteLine(job.ToString());
            allFile.Close();

            Console.WriteLine("Done.");
        }

    }
}
