using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class Query
    {
        public int queryID { get; set; }
        public int queryTier { get; set; }

        public List<Attribute> queryProjectedAttributes { get; set; }
        public List<Table> queryTables { get; set; }
        public Attribute querySelectionAttribute { get; set; }
        public String querySelectionAttributeValue { get; set; }
        public double queryPercentSelection { get; set; }
        
        public double queryPredictedTime { get; set; }

        public Query(int qid, List<Attribute> qProjAttr, List<Table> qTables, Attribute qSelectionAttr,
                     String qSelectionAttrValue, double qPercentSelection, int qTier = 0)
        {
            this.queryID = qid;
            this.queryProjectedAttributes = qProjAttr;
            this.queryTables = qTables;
            this.querySelectionAttribute = qSelectionAttr;
            this.querySelectionAttributeValue = qSelectionAttrValue;
            this.queryPercentSelection = qPercentSelection;
            this.queryTier = qTier;
        }

        public string getAttributeCount()
        {
            return queryProjectedAttributes.Count().ToString();
        }
        
        /*
         * Subsumption for attributes
         */
        public Boolean SubsumesOrEqual(List<Attribute> firstList, List<Attribute> secondList)
        {
            var first = (from v in firstList select v.attributeSize);
            var second = (from v in secondList select v.attributeSize);

            if (firstList.Count() >= secondList.Count() && first.Sum() >= second.Sum()) return true;

            return false;
        }
        /*
        * Subsumption for number of tables
        */
        public Boolean SubsumesOrEqual(List<Table> firstList, List<Table> secondList)
        {
            var first = (from v in firstList select v.tableSize);
            var second = (from v in secondList select v.tableSize);

            if (firstList.Count() >= secondList.Count() && first.Sum() >= second.Sum()) return true;

            return false;
        }

       /*
        * Check if one query subsumes another
        */
        public Boolean SubsumesOrEqual(Query q2)
        {
            return  SubsumesOrEqual(this.queryTables, q2.queryTables) //table check
                    && SubsumesOrEqual(this.queryProjectedAttributes, q2.queryProjectedAttributes) //attribute check
                    && (this.querySelectionAttribute.attributeName == null 
                        || this.queryPercentSelection >= q2.queryPercentSelection); //selectivity check
        }

        /*
         * Convert the query into a SQL String
         */
        public override String ToString()
        {
            String fromString = String.Empty;
            String projectString = String.Empty;
            String whereJoinString = String.Empty;
            String whereSelectionString = String.Empty;

            fromString += " FROM ";
            foreach (var currentTable in queryTables)
            {
                fromString += currentTable.tableName;
                if (currentTable != queryTables.Last()) fromString += ",";
            }

            projectString += "SELECT ";
            foreach (var currentAttribute in queryProjectedAttributes)
            {
                projectString += currentAttribute.attributeName;
                if (currentAttribute != queryProjectedAttributes.Last()) projectString += ",";
            }

            //special case for joins
            if (queryTables.Count() > 1)
            {
                whereJoinString += " WHERE ";
                foreach (var t in (from j in queryTables where j is Dimension select j))
                {
                    string factTableName = String.Empty;
                    var tName = (queryTables.Where(l => l is Fact));
                    if (tName.Count() != 0) factTableName = tName.Single().tableName;

                    Dimension dim = (Dimension)t;
                    if (t != (from j in queryTables where j is Dimension select j).Last())
                    {
                        whereJoinString += String.Format(" {0}.{1}={2}.{3} AND ",
                                                          dim.tableName, dim.tablePrimaryKey.attributeName,
                                                          factTableName, dim.dimensionForeignKey.attributeName);
                    }
                    else
                    {
                        whereJoinString += String.Format(" {0}.{1}={2}.{3} ",
                                                          dim.tableName, dim.tablePrimaryKey.attributeName,
                                                          factTableName, dim.dimensionForeignKey.attributeName);
                    }
                }
            }

            //adding selectivities if any
            if (querySelectionAttributeValue != String.Empty)
            {
                whereSelectionString = queryTables.Count() == 1 ? " WHERE " : " AND ";
                //composite key check
                String[] splitSelection = querySelectionAttribute.attributeName.Split(',');
                if (splitSelection.Length > 0)
                {
                    String[] splitValue = querySelectionAttributeValue.Split(',');
                    for (int i = 0; i < splitSelection.Count(); i++)
                    {
                        whereSelectionString = String.Format("{0} <= {1}", splitSelection[i], splitValue[i]);
                        if (i != splitSelection.Count() - 1) whereSelectionString += " AND ";
                    }
                }
                else
                {
                    whereSelectionString = String.Format("{0} <= {1}", 
                                                            querySelectionAttribute.attributeName,
                                                            querySelectionAttributeValue);
                }
            }
            return projectString + fromString + whereJoinString + whereSelectionString + ";";
        }

        /*
        * Condensed Query Description
        */
        public String toStringShortHand()
        {
            string queryPrint = String.Empty;

            queryPrint = String.Format("SELECT ({0} ATTR.) FROM ({1} {2}) WHERE {3}%",
                                        this.getAttributeCount(),
                                        this.queryTables.Count != 1 ? this.queryTables.Count().ToString() : this.queryTables.Single().tableName,
                                        this.queryTables.Count != 1 ? "TABLES" : String.Empty,
                                        this.queryPercentSelection.ToString());

            return queryPrint;
        }
    }
}