using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class ObjectType
    {
        public UIntPtr address;

        public List<InstanceDefinition> instances = new List<InstanceDefinition>();
        public List<CounterDefinition> counters = new List<CounterDefinition>();

        public int ObjectNameTitleIndex;
        public int TotalByteLength;
        public int ObjectHelpTitleIndex;
        public int DetailLevel;
        public int NumCounters;
        public long DefaultCounter;
        public long NumInstances;
        public int CodePage;
        public long PerfTime;
        public long PerfFreq;
    }
}
