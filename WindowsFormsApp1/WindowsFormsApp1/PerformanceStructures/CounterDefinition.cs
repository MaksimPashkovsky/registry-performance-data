using System;

namespace WindowsFormsApp1
{
    public class CounterDefinition
    {
        public readonly UIntPtr Address;
        public readonly int ByteLength;
        public readonly int CounterHelpTitleIndex;
        public readonly int CounterNameTitleIndex;
        public readonly int CounterSize;
        public readonly int CounterType;
        public readonly long DefaultScale;
        public readonly int DetailLevel;
        public RawData RawData;
        public string Value;

        public CounterDefinition(UIntPtr address, 
            int byteLength, 
            int counterHelpTitleIndex, 
            int counterNameTitleIndex, 
            int counterSize, 
            int counterType, 
            long defaultScale, 
            int detailLevel)
        {
            Address = address;
            ByteLength = byteLength;
            CounterHelpTitleIndex = counterHelpTitleIndex;
            CounterNameTitleIndex = counterNameTitleIndex;
            CounterSize = counterSize;
            CounterType = counterType;
            DefaultScale = defaultScale;
            DetailLevel = detailLevel;
            RawData = null;
            Value = null;
        }
    }
}
