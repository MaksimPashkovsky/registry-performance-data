using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class RAW_DATA
    {
        public int CounterType;
        public ulong Data;          // Raw counter data
        public long Time;           // Is a time value or a base value
        public int MultiCounterData;  // Second raw counter value for multi-valued counters
        public long Frequency;
    }
}
