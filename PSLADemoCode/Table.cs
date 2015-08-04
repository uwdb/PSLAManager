using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class Table
    {
        public string tableName { get; set; }
        public List<Attribute> tableAttributeList { get; set; }
        public List<String> tableSelectivityList { get; set; }
        public Attribute tablePrimaryKey { get; set; }
        public int tableSize { get; set; }
        public List<Attribute> tableCompositeKeys { get; set; }

        public Table(String tabName, List<Attribute> tabAttrList, List<String> tabSelectivityList, Attribute tabPrimaryKey, int tabSize)
        {
            this.tableName = tabName;
            this.tableAttributeList = tabAttrList;
            this.tablePrimaryKey = tabPrimaryKey;
            this.tableSize = tabSize;
            this.tableSelectivityList = tabSelectivityList;
        }

        public Table(String tabName, List<Attribute> tabAttrList, List<String> tabSelectivityList, List<Attribute> tabCompositeKeys, int tabSize)
        {
            this.tableName = tabName;
            this.tableAttributeList = tabAttrList;
            this.tableCompositeKeys = tabCompositeKeys;
            this.tableSize = tabSize;
            this.tableSelectivityList = tabSelectivityList;
        }
    }
}