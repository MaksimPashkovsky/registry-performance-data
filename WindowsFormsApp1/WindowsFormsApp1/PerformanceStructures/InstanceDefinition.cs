using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class InstanceDefinition
    {
        public UIntPtr address;
        public List<CounterDefinition> counters = new List<CounterDefinition>();
        public int ByteLength;
        public int ParentObjectTitleIndex;
        public int ParentObjectInstance;
        public string Name;
    }
}
