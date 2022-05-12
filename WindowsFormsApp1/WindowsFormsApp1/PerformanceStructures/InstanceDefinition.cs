using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class InstanceDefinition
    {
        public readonly UIntPtr Address;
        public List<CounterDefinition> Counters;
        public readonly int ByteLength;
        public readonly int ParentObjectTitleIndex;
        public readonly int ParentObjectInstance;
        public readonly string Name;

        public InstanceDefinition(UIntPtr address,
            int byteLength,
            int parentObjectTitleIndex,
            int parentObjectInstance,
            string name)
        {
            Address = address;
            Counters = new List<CounterDefinition>();
            ByteLength = byteLength;
            ParentObjectTitleIndex = parentObjectTitleIndex;
            ParentObjectInstance = parentObjectInstance;
            Name = name;
        }
    }
}