using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsFormsApp1
{
    public class DataBlock
    {
        public readonly UIntPtr Address;
        private List<ObjectType> Objects;
        public readonly string SystemName;
        public readonly string SystemTime;
        public readonly long DefaultObject;
        public readonly int NumObjectTypes;
        public readonly long PerfFreq;
        public readonly long PerfTime;
        public readonly long PerfTime100nSec;
        public readonly int Revision;
        public readonly string Signature;
        public readonly int TotalByteLength;
        public readonly int Version;
        
        public List<ObjectType> GetObjects()
        {
            return Objects;
        }

        public DataBlock(UIntPtr address, 
            string systemName, 
            string systemTime, 
            long defaultObject, 
            int numObjectTypes, 
            long perfFreq, 
            long perfTime, 
            long perfTime100nSec, 
            int revision, 
            string signature, 
            int totalByteLength, 
            int version)
        {
            Objects = new List<ObjectType>();
            Address = address;
            SystemName = systemName;
            SystemTime = systemTime;
            DefaultObject = defaultObject;
            NumObjectTypes = numObjectTypes;
            PerfFreq = perfFreq;
            PerfTime = perfTime;
            PerfTime100nSec = perfTime100nSec;
            Revision = revision;
            Signature = signature;
            TotalByteLength = totalByteLength;
            Version = version;
        }

        public static DataBlock GetPerformanceData()
        {
            Externals.GetDataBlockPointer(out UIntPtr PerfDataBlockPntr);
            DataBlock PerfDataBlock = DataBlock.GetFromPointer(PerfDataBlockPntr);
            Externals.GetFirstObjectTypePointer(PerfDataBlockPntr, out UIntPtr PerfObjectTypePntr);

            for (int i = 0; i < PerfDataBlock.NumObjectTypes; i++)
            {
                ObjectType PerfObjectType = ObjectType.GetFromPointer(PerfObjectTypePntr);
                Externals.GetCounterDefinitionPointer(PerfObjectTypePntr, out UIntPtr CounterDefinitionPntr);

                if (PerfObjectType.NumInstances > 0)
                {
                    Externals.GetFirstInstancePointer(PerfObjectTypePntr, out UIntPtr PerfInstanceDefinitionPntr);

                    for (int j = 0; j < PerfObjectType.NumInstances; j++)
                    {
                        InstanceDefinition PerfInstDef = InstanceDefinition.GetFromPointer(PerfInstanceDefinitionPntr, PerfObjectType.CodePage);
                        UIntPtr CurrentCounterPntr = CounterDefinitionPntr;
                        Externals.GetCounterBlock_InstPointer(PerfInstanceDefinitionPntr, out UIntPtr CounterBlockPntr);

                        for (int k = 0; k < PerfObjectType.NumCounters; k++)
                        {
                            CounterDefinition PerfCounterDef = CounterDefinition.GetFromPointer(CurrentCounterPntr);

                            RawData rd = RawData.GetFromPointers(
                                PerfDataBlockPntr,
                                PerfObjectTypePntr,
                                CurrentCounterPntr,
                                CounterBlockPntr,
                                PerfCounterDef.CounterType);

                            PerfCounterDef.RawData = rd;
                            PerfInstDef.Counters.Add(PerfCounterDef);
                            Externals.GetNextCounterPointer(CurrentCounterPntr, out CurrentCounterPntr);
                        }
                        PerfObjectType.GetInstances().Add(PerfInstDef);
                        Externals.GetNextInstancePointer(CounterBlockPntr, out PerfInstanceDefinitionPntr);
                    }
                }
                else
                {
                    Externals.GetCounterBlock_ObjPointer(PerfObjectTypePntr, out UIntPtr CounterBlockPntr);

                    for (int j = 0; j < PerfObjectType.NumCounters; j++)
                    {
                        CounterDefinition PerfCounterDef = CounterDefinition.GetFromPointer(CounterDefinitionPntr);

                        RawData rd = RawData.GetFromPointers(
                                PerfDataBlockPntr,
                                PerfObjectTypePntr,
                                CounterDefinitionPntr,
                                CounterBlockPntr,
                                PerfCounterDef.CounterType);

                        PerfCounterDef.RawData = rd;
                        PerfObjectType.GetCounters().Add(PerfCounterDef);
                        Externals.GetNextCounterPointer(CounterDefinitionPntr, out CounterDefinitionPntr);
                    }
                }
                PerfDataBlock.Objects.Add(PerfObjectType);
                Externals.GetNextObjectTypePointer(PerfObjectTypePntr, out PerfObjectTypePntr);
            }
            return PerfDataBlock;
        }

        public static void CalculateCounterValues(DataBlock PerfDataBlock1, DataBlock PerfDataBlock2)
        {
            foreach (ObjectType obj in PerfDataBlock1.Objects)
            {
                foreach (InstanceDefinition inst in obj.GetInstances())
                {
                    foreach (CounterDefinition counter in inst.Counters)
                    {
                        CounterDefinition counter2 = PerfDataBlock2.GetCounter(counter.CounterNameTitleIndex, inst.Name, obj.ObjectNameTitleIndex);
                        if (counter2 != null)
                            counter.SetValueUsingTwo(counter2.CounterType, counter2.RawData);
                        else
                            counter.SetValueUsingOne();
                    }

                }

                foreach (CounterDefinition counter in obj.GetCounters())
                {
                    CounterDefinition counter2 = PerfDataBlock2.GetCounter(counter.CounterNameTitleIndex, obj.ObjectNameTitleIndex);
                    if (counter2 != null)
                        counter.SetValueUsingTwo(counter2.CounterType, counter2.RawData);
                    else
                        counter.SetValueUsingOne();
                }

            }
        }

        public static DataBlock GetFromPointer(UIntPtr PerfDataBlockPntr)
        {
            StringBuilder Signature = new StringBuilder(5);
            StringBuilder SystemName = new StringBuilder(20);
            StringBuilder SystemTime = new StringBuilder(100);

            Externals.GetDataBlockInfo(PerfDataBlockPntr,
                out long DefaultObject,
                out int NumObjectTypes,
                out long PerfFreq,
                out long PerfTime,
                out long PerfTime100nSec,
                out int Revision,
                Signature,
                SystemName,
                SystemTime,
                out int TotalByteLength,
                out int Version);

            return new DataBlock(
                systemName: SystemName.ToString(),
                systemTime: SystemTime.ToString(),
                defaultObject: DefaultObject,
                numObjectTypes: NumObjectTypes,
                perfFreq: PerfFreq,
                perfTime: PerfTime,
                perfTime100nSec: PerfTime100nSec,
                revision: Revision,
                signature: Signature.ToString(),
                totalByteLength: TotalByteLength,
                version: Version,
                address: PerfDataBlockPntr);
        }

        public CounterDefinition GetCounter(int counterIndex, string InstanceName, int ObjectIndex)
        {
            foreach (ObjectType obj in Objects)
            {
                if (obj.ObjectNameTitleIndex != ObjectIndex)
                    continue;
                foreach (InstanceDefinition inst in obj.GetInstances())
                {
                    if (!inst.Name.Equals(InstanceName))
                        continue;
                    foreach (CounterDefinition counter in inst.Counters)
                    {
                        if (counter.CounterNameTitleIndex == counterIndex)
                            return counter;
                    }
                }
            }
            return null;
        }

        public CounterDefinition GetCounter(int counterIndex, int ObjectIndex)
        {
            foreach (ObjectType obj in Objects)
            {
                if (obj.ObjectNameTitleIndex != ObjectIndex)
                    continue;
                foreach (CounterDefinition counter in obj.GetCounters())
                {
                    if (counter.CounterNameTitleIndex == counterIndex)
                        return counter;
                }
            }
            return null;
        }
    }
}
