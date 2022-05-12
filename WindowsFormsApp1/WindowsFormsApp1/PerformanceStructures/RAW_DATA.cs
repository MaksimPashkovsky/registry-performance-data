using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
