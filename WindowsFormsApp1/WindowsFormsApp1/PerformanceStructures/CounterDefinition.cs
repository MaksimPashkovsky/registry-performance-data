using System;
using System.Text;

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

        public static CounterDefinition GetFromPointer(UIntPtr CurrentCounterPntr)
        {
            Externals.GetCounterInfo(
                CurrentCounterPntr,
                out int ByteLength,
                out int CounterHelpTitleIndex,
                out int CounterNameTitleIndex,
                out int CounterSize,
                out int CounterType,
                out long DefaultScale,
                out int DetailLevel);

            return new CounterDefinition(
                address: CurrentCounterPntr,
                byteLength: ByteLength,
                counterHelpTitleIndex: CounterHelpTitleIndex,
                counterNameTitleIndex: CounterNameTitleIndex,
                counterSize: CounterSize,
                counterType: CounterType,
                defaultScale: (DefaultScale & 2147483648) != 0 ? DefaultScale - 4294967295 - 1 : DefaultScale,
                detailLevel: DetailLevel);
        }

        public void SetValueUsingOne()
        {
            StringBuilder Value = new StringBuilder(255);
            Externals.GetCalculatedValue(
                CounterType,
                RawData.Data,
                RawData.Time,
                RawData.MultiCounterData,
                RawData.Frequency,
                0, 0, 0, 0, 0,
                Value);
            this.Value = Value.ToString();
        }

        public void SetValueUsingTwo(int counterType, RawData rd)
        {
            StringBuilder Value = new StringBuilder(255);
            Externals.GetCalculatedValue(
                CounterType,
                RawData.Data,
                RawData.Time,
                RawData.MultiCounterData,
                RawData.Frequency,
                counterType,
                rd.Data,
                rd.Time,
                rd.MultiCounterData,
                rd.Frequency,
                Value);
            this.Value = Value.ToString();
        }

        public string GetDescription()
        {
            return "Size: " + CounterSize + " bytes" + "\r\n" +
                "Counter type: " + CounterType + " = {0}\r\n" +
                "Counter type description: {1}\r\n" +
                "Default scale: " + Math.Pow(10, DefaultScale) + "\r\n" +
                "Detail level: " + DetailLevel + " = " + Utils.GetDetailLevel(DetailLevel) + "\r\n";
        }
    }
}
