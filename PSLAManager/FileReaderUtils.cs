using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLAManager
{
    public class FileReaderUtils
    {

        public static void readTiers(String configPath, String tierFileName)
        {
            StreamReader tierReader = new StreamReader(Path.Combine(configPath, tierFileName));
            String currentLine = tierReader.ReadLine();
            while (currentLine != null) {
                if(currentLine != String.Empty && !currentLine.Contains("#")) {
                    PSLAGenerator.tierPredictionFolders.Add(currentLine);
                }
                currentLine = tierReader.ReadLine();
            }

            for (int i = 1; i <= PSLAGenerator.tierPredictionFolders.Count(); i++) {
                PSLAGenerator.tiers.Add(i);
            }
        }

        public static void readDataSchema(String configPath, String schemaFileName)
        {
            //PARSE SCHEMA
            StreamReader schemaReader = new StreamReader(Path.Combine(configPath, schemaFileName));
            String currentLine = schemaReader.ReadLine();
            bool firstLine = true;

            //read through the header
            if(currentLine.Contains("/*")){
                while (currentLine != "*/") {
                    currentLine = schemaReader.ReadLine();
                }
                currentLine = schemaReader.ReadLine();
            }

            //start reading schema
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
                        PSLAGenerator.factTable = new Fact(tableName, listofAttributes, listofSelectivities, new Attribute(compositeName, compositeSize), tableSize);
                    }
                    else {
                        PSLAGenerator.factTable = new Fact(tableName, listofAttributes, listofSelectivities, listofPrimaryKeys.First(), tableSize);
                    }
                    PSLAGenerator.listOfTables.Add(PSLAGenerator.factTable);
                    firstLine = false;
                }
                else {
                    PSLAGenerator.listOfTables.Add(new Dimension(tableName, listofAttributes, listofSelectivities, listofPrimaryKeys.First(), tableSize, foreignKeyAttribute));
                }
                currentLine = schemaReader.ReadLine();
            }
            schemaReader.Close();
        }

        public static void parsePredictionResults(String configPath, int tier)
        {
            List<Query> listToModifyPredictions = PSLAGenerator.listOfQueries.Where(x => x.queryTier == tier).ToList();

            foreach (var currentQuery in listToModifyPredictions) {
                currentQuery.queryPredictedTime = 0;
            }

            //parse the results
            StreamReader sr = new StreamReader(configPath + "/results.txt");
            string currentline = String.Empty;

            //read header
            for (int i = 0; i < 1; i++) {
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
    }
}
