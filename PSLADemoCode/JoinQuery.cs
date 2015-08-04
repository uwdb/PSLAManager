using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class JoinQuery : Query
    {

        public List<Table> joinQueryTables { get; set; }
        public Fact joinQueryFactTable { get; set; }

        public JoinQuery(int qId, List<Attribute> qProjAttr, Attribute qSelectionAttr, String qSelectionAttrValue, double qPercentSelection, List<Table> qTablesToJoin, Fact qFactTable, int qConfig = 0)
            : base(qId, qProjAttr, qTablesToJoin, qSelectionAttr, qSelectionAttrValue, qPercentSelection, qConfig)
        {
            this.joinQueryTables = qTablesToJoin;
            this.joinQueryFactTable = qFactTable;
        }

    }
}
