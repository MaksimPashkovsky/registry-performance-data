using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class CounterDefinition
    {
        public UIntPtr address;

        public int ByteLength;
        public int CounterHelpTitleIndex;
        public int CounterNameTitleIndex;
        public int CounterSize;
        public int CounterType;
        public long DefaultScale;
        public int DetailLevel;

        public RAW_DATA RawData;

        public string VALUE;
    }
}
