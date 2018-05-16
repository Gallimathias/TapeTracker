using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConverter
{
    public class SimpleTable
    {
        public string Name { get; set; }
        public List<KeyValuePair<string, Type>> Columns { get; set; }
        public List<object[]> Data { get; set; }

        public SimpleTable(string name)
        {
            Name = name;
            Columns = new List<KeyValuePair<string, Type>>();
            Data = new List<object[]>();
        }
    }
}
