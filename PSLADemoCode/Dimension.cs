using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class Dimension : Table
    {
        public Attribute dimensionForeignKey { get; set; }
        public List<Attribute> dimensionForeignKeyList { get; set; }

        public Dimension(String tabName, List<Attribute> tabAttr, List<String> tabListofSel,
                         Attribute tabPrimaryKey, int tabSize, Attribute dimForeignKey)
            : base(tabName, tabAttr, tabListofSel, tabPrimaryKey, tabSize)
        {
            this.dimensionForeignKey = dimForeignKey;
        }

        public Dimension(String tabName, List<Attribute> tabAttr, List<String> tabListofSel, 
                         Attribute tabPrimaryKey, int tabSize, List<Attribute> dimForeignKeyList)
            : base(tabName, tabAttr, tabListofSel, tabPrimaryKey, tabSize)
        {
            this.dimensionForeignKeyList = dimForeignKeyList;
        }

    }
}
