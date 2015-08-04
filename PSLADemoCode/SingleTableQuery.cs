using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class SingleTableQuery : Query
    {
        public Table queryTable { get; set; }

        public SingleTableQuery(int qId, List<Attribute> qProjectedAttrs, List<Table> qTables,
                                Attribute qSelectedAttribute, String qSelectedAttributeValue,
                                double qPercentSelection, Table qTable, int qConfig = 0)
            : base(qId, qProjectedAttrs, qTables, qSelectedAttribute, qSelectedAttributeValue, qPercentSelection, qConfig)
        {
            this.queryTable = qTable;
        }
    }
}