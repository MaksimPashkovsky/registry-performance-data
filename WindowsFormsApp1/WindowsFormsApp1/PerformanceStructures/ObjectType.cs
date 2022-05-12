using System;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public class ObjectType
    {
        public readonly UIntPtr Address;
        public List<InstanceDefinition> Instances;
        public List<CounterDefinition> Counters;
        public readonly int ObjectNameTitleIndex;
        public readonly int TotalByteLength;
        public readonly int ObjectHelpTitleIndex;
        public readonly int DetailLevel;
        public readonly int NumCounters;
        public readonly long DefaultCounter;
        public readonly long NumInstances;
        public readonly int CodePage;
        public readonly long PerfTime;
        public readonly long PerfFreq;

        public ObjectType(UIntPtr address, 
            int objectNameTitleIndex, 
            int totalByteLength, 
            int objectHelpTitleIndex, 
            int detailLevel, 
            int numCounters, 
            long defaultCounter, 
            long numInstances, 
            int codePage, 
            long perfTime, 
            long perfFreq)
        {
            Address = address;
            Instances = new List<InstanceDefinition>();
            Counters = new List<CounterDefinition>();
            ObjectNameTitleIndex = objectNameTitleIndex;
            TotalByteLength = totalByteLength;
            ObjectHelpTitleIndex = objectHelpTitleIndex;
            DetailLevel = detailLevel;
            NumCounters = numCounters;
            DefaultCounter = defaultCounter;
            NumInstances = numInstances;
            CodePage = codePage;
            PerfTime = perfTime;
            PerfFreq = perfFreq;
        }

        public static ObjectType GetFromPointer(UIntPtr PerfObjectTypePntr)
        {
            Externals.GetPerfObjectTypeInfo(
                PerfObjectTypePntr,
                out int ObjectNameTitleIndex,
                out int TotalByteLength,
                out int ObjectHelpTitleIndex,
                out int DetailLevel,
                out int NumCounters,
                out long DefaultCounter,
                out long NumInstances,
                out int CodePage,
                out long PerfTime,
                out long PerfFreq);

            return new ObjectType(
                objectNameTitleIndex: ObjectNameTitleIndex,
                totalByteLength: TotalByteLength,
                objectHelpTitleIndex: ObjectHelpTitleIndex,
                detailLevel: DetailLevel,
                numCounters: NumCounters,
                defaultCounter: DefaultCounter,
                numInstances: NumInstances,
                codePage: CodePage,
                perfTime: PerfTime,
                perfFreq: PerfFreq,
                address: PerfObjectTypePntr);
        }

    }
}
