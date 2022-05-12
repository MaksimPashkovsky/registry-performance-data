using System;

namespace WindowsFormsApp1
{
    public class RawData
    {
        public readonly int CounterType;
        public readonly ulong Data;          // Raw counter data
        public readonly long Time;           // Is a time value or a base value
        public readonly int MultiCounterData;  // Second raw counter value for multi-valued counters
        public readonly long Frequency;

        public RawData(int counterType, ulong data, long time, int multiCounterData, long frequency)
        {
            CounterType = counterType;
            Data = data;
            Time = time;
            MultiCounterData = multiCounterData;
            Frequency = frequency;
        }

        public static RawData GetFromPointers(
            UIntPtr PerfDataBlockPntr, 
            UIntPtr PerfObjectTypePntr, 
            UIntPtr CurrentCounterPntr, 
            UIntPtr CounterBlockPntr,
            int counterType)
        {
            Externals.GetCounterValue(
                PerfDataBlockPntr,
                PerfObjectTypePntr,
                CurrentCounterPntr,
                CounterBlockPntr,
                out ulong Data,
                out long Time,
                out int MultiCounterData,
                out long Frequency);

            return new RawData(
                counterType: counterType,
                data: Data,
                time: Time,
                multiCounterData: MultiCounterData,
                frequency: Frequency);
        }
    }
}
