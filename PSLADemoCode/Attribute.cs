using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSLADemo
{
    class Attribute
    {
        public string attributeName { get; set; }
        public int attributeSize { get; set; }
        public Table attributeTable { get; set; }

        public Attribute(string attName, int attSize, Table attTable = null)
        {
            attributeName = attName;
            attributeSize = attSize;
            attributeTable = attTable;
        }

    }
}
