using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLAManager
{
    class Fact : Table
    {
        public Fact(String tabName, List<Attribute> tabAttr, List<String> tabListOfSel, Attribute tabPrimaryKey, int tabSize)
            : base(tabName, tabAttr, tabListOfSel, tabPrimaryKey, tabSize) {}

        public Fact(String tabName, List<Attribute> tabAttr, List<String> tabListOfSel, List<Attribute> tabCompositeKeys, int tabSize)
            : base(tabName, tabAttr, tabListOfSel, tabCompositeKeys, tabSize) {}

        //if composite key, return the string
        public string getCompositeKeys()
        {
            string stringConcat = String.Empty;
            foreach (var v in this.tableCompositeKeys) {
                stringConcat += v;
                if (v != this.tableCompositeKeys.Last())
                    stringConcat += ",";
            }
            return stringConcat;
        }

    }
}
