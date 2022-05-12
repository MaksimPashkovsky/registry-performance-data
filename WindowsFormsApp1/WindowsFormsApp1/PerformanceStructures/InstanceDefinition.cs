using System;
using System.Collections.Generic;
using System.Text;

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

        public static InstanceDefinition GetFromPointer(UIntPtr PerfInstanceDefinitionPntr, int codePage)
        {
            StringBuilder Name = new StringBuilder(300);

            Externals.GetInstanceInfo(
                PerfInstanceDefinitionPntr,
                codePage,
                out int ByteLength,
                out int ParentObjectTitleIndex,
                out int ParentObjectInstance,
                Name);

            return new InstanceDefinition(
                address: PerfInstanceDefinitionPntr,
                byteLength: ByteLength,
                parentObjectTitleIndex: ParentObjectTitleIndex,
                parentObjectInstance: ParentObjectInstance,
                name: Name.ToString());
        }
    }
}