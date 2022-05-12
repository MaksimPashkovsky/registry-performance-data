using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public DataBlock PerfDataBlock1;
        public DataBlock PerfDataBlock2;

        Dictionary<int, string> names;
        Dictionary<int, string> helps;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImageList il = new ImageList();
            il.Images.Add(Image.FromFile("Images\\open.png"));
            il.Images.Add(Image.FromFile("Images\\data.png"));
            il.Images.Add(Image.FromFile("Images\\key.jpg"));
            il.Images.Add(Image.FromFile("Images\\file.png"));
            treeView1.ImageList = il;

            ImageList il2 = new ImageList();
            il2.Images.Add(Image.FromFile("Images\\param.png"));
            il2.Images.Add(Image.FromFile("Images\\counter.png"));
            listView1.SmallImageList = il2;

            names = GetNamesFromRegistry();
            helps = GetHelpsFromRegistry();

            PerfDataBlock1 = GetPerformanceData();
            PerfDataBlock2 = GetPerformanceData();

            CalculateCounterValues(PerfDataBlock1, PerfDataBlock2);

            treeView1.Nodes.Add("DataBlock", "DataBlock", 1);
            
            foreach (ObjectType obj in PerfDataBlock1.Objects)
            {
                TreeNode objNode = new TreeNode();
                int index = obj.ObjectNameTitleIndex;
                if (names.ContainsKey(index))
                {
                    string name = names[index];
                    objNode.Text = name;
                }
                else
                {
                    objNode.Text = index.ToString();
                }
                
                if (obj.Instances.Count > 0)
                {
                    foreach (InstanceDefinition inst in obj.Instances)
                    {
                        objNode.Nodes.Add(inst.Name, inst.Name, 3);
                    }
                }
                objNode.ImageIndex = 2;
                treeView1.Nodes[0].Nodes.Add(objNode);
            }
        }

        private void CalculateCounterValues(DataBlock PerfDataBlock1, DataBlock PerfDataBlock2)
        {
            foreach (ObjectType obj in PerfDataBlock1.Objects)
            {
                foreach (InstanceDefinition inst in obj.Instances)
                {
                    foreach (CounterDefinition counter in inst.Counters)
                    {
                        CounterDefinition counter2 = GetCounter(PerfDataBlock2, counter.CounterNameTitleIndex, inst.Name, obj.ObjectNameTitleIndex);
                        RawData rd = counter2.RawData;
                        if (counter2 != null)
                            counter.SetValueUsingTwo(counter2.CounterType, rd);
                        else
                            counter.SetValueUsingOne();
                    }

                }
                
                foreach (CounterDefinition counter in obj.Counters)
                {
                    CounterDefinition counter2 = GetCounter(PerfDataBlock2, counter.CounterNameTitleIndex, obj.ObjectNameTitleIndex);
                    RawData rd = counter2.RawData;
                    if (counter2 != null)
                        counter.SetValueUsingTwo(counter2.CounterType, rd);
                    else
                        counter.SetValueUsingOne();
                }

            }
        }

        private CounterDefinition GetCounter(DataBlock PerfDataBlock, int counterIndex, string InstanceName, int ObjectIndex)
        {
            foreach (ObjectType obj in PerfDataBlock.Objects)
            {
                if (obj.ObjectNameTitleIndex != ObjectIndex) 
                    continue;
                foreach (InstanceDefinition inst in obj.Instances)
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

        private CounterDefinition GetCounter(DataBlock PerfDataBlock, int counterIndex, int ObjectIndex)
        {
            foreach (ObjectType obj in PerfDataBlock.Objects)
            {
                if (obj.ObjectNameTitleIndex != ObjectIndex) 
                    continue;
                foreach (CounterDefinition counter in obj.Counters)
                {
                    if (counter.CounterNameTitleIndex == counterIndex)
                        return counter;
                }
            }
            return null;
        }

        private DataBlock GetPerformanceData()
        {   
            UIntPtr PerfDataBlockPntr = new UIntPtr();
            UIntPtr PerfObjectTypePntr = new UIntPtr();
            UIntPtr CounterDefinitionPntr = new UIntPtr();
            UIntPtr CurrentCounterPntr = new UIntPtr();
            UIntPtr CounterBlockPntr = new UIntPtr();
            UIntPtr PerfInstanceDefinitionPntr = new UIntPtr();

            int b = Externals.GetDataBlock(out PerfDataBlockPntr);
            DataBlock PerfDataBlock = DataBlock.GetFromPointer(PerfDataBlockPntr);
            int t = Externals.GetFirstObjectType(PerfDataBlockPntr, out PerfObjectTypePntr);

            for (int i = 0; i < PerfDataBlock.NumObjectTypes; i++)
            {
                ObjectType PerfObjectType = ObjectType.GetFromPointer(PerfObjectTypePntr);
                int q = Externals.GetCounterDefinition(PerfObjectTypePntr, out CounterDefinitionPntr);

                if (PerfObjectType.NumInstances > 0)
                {
                    int w = Externals.GetFirstInstance(PerfObjectTypePntr, out PerfInstanceDefinitionPntr);

                    for (int j = 0; j < PerfObjectType.NumInstances; j++)
                    {
                        InstanceDefinition PerfInstDef = InstanceDefinition.GetFromPointer(PerfInstanceDefinitionPntr, PerfObjectType.CodePage);
                        CurrentCounterPntr = CounterDefinitionPntr;
                        int u = Externals.GetCounterBlock_Inst(PerfInstanceDefinitionPntr, out CounterBlockPntr);

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
                            int s = Externals.GetNextCounter(CurrentCounterPntr, out CurrentCounterPntr);
                        }
                        PerfObjectType.Instances.Add(PerfInstDef);
                        int y = Externals.GetNextInstance(CounterBlockPntr, out PerfInstanceDefinitionPntr);
                    }
                }
                else
                {
                    int x = Externals.GetCounterBlock_Obj(PerfObjectTypePntr, out CounterBlockPntr);

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
                        PerfObjectType.Counters.Add(PerfCounterDef);
                        int m = Externals.GetNextCounter(CounterDefinitionPntr, out CounterDefinitionPntr);
                    }
                }
                PerfDataBlock.Objects.Add(PerfObjectType);
                int v = Externals.GetNextObjectType(PerfObjectTypePntr, out PerfObjectTypePntr);
            }
            return PerfDataBlock;
        }

        private Dictionary<int, string> GetNamesFromRegistry()
        {
            Dictionary<int, string> names = new Dictionary<int, string>();
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\019");
            string[] va = (string[])hKey.GetValue("CounterDefinition");
            for (int i = 0; i < va.Length - 1; i += 2)
            {
                names.Add(int.Parse(va[i]), va[i + 1]);
            }
            return names;
        }

        private Dictionary<int, string> GetHelpsFromRegistry()
        {
            Dictionary<int, string> helps = new Dictionary<int, string>();
            RegistryKey hKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Perflib\\019");
            string[] va = (string[])hKey.GetValue("Help");
            for (int i = 0; i < va.Length - 1; i = i + 2)
            {
                helps.Add(int.Parse(va[i]), va[i + 1]);
            }
            return helps;
        }
        
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            textBox1.Text = "";
            listView1.Items.Clear();
            TreeNode t = e.Node;

            if (t.Parent == null)
            {
                ListViewItem[] items = new ListViewItem[]
                {
                    new ListViewItem(new string[] { "SystemName", PerfDataBlock1.SystemName }),
                    new ListViewItem(new string[] { "SystemTime", PerfDataBlock1.SystemTime }),
                    new ListViewItem(new string[] { "DefaultObject", names[(int)PerfDataBlock1.DefaultObject] }),
                    new ListViewItem(new string[] { "NumObjectTypes", PerfDataBlock1.NumObjectTypes.ToString() }),
                    new ListViewItem(new string[] { "PerfFreq", PerfDataBlock1.PerfFreq.ToString() }),
                    new ListViewItem(new string[] { "PerfTime", PerfDataBlock1.PerfTime.ToString() }),
                    new ListViewItem(new string[] { "PerfTime100nSec", PerfDataBlock1.PerfTime100nSec.ToString() }),
                    new ListViewItem(new string[] { "Revision", PerfDataBlock1.Revision.ToString() }),
                    new ListViewItem(new string[] { "Signature", PerfDataBlock1.Signature }),
                    new ListViewItem(new string[] { "TotalByteLength", PerfDataBlock1.TotalByteLength.ToString() }),
                    new ListViewItem(new string[] { "Version", PerfDataBlock1.Version.ToString() })
                };

                foreach (ListViewItem lvi in items)
                {
                    lvi.ImageIndex = 0;
                    listView1.Items.Add(lvi);
                }
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                ObjectType obj = PerfDataBlock1.Objects.ElementAt(t.Index);

                ListViewItem[] items = new ListViewItem[]
                {
                    new ListViewItem(new string[] { "TotalByteLength", obj.TotalByteLength.ToString() }),
                    new ListViewItem(new string[] { "ObjectNameTitleIndex", obj.ObjectNameTitleIndex.ToString() }),
                    new ListViewItem(new string[] { "ObjectHelpTitleIndex", obj.ObjectHelpTitleIndex.ToString() }),
                    new ListViewItem(new string[] { "DetailLevel", obj.DetailLevel.ToString() }),
                    new ListViewItem(new string[] { "NumCounters", obj.NumCounters.ToString() }),
                    new ListViewItem(new string[] { "NumInstances", obj.NumInstances.ToString() }),
                    new ListViewItem(new string[] { "CodePage", obj.CodePage.ToString() }),
                    new ListViewItem(new string[] { "PerfTime", obj.PerfTime.ToString() }),
                    new ListViewItem(new string[] { "PerfFreq", obj.PerfFreq.ToString() })
                };
                foreach (ListViewItem lvi in items)
                {
                    lvi.ImageIndex = 0;
                    listView1.Items.Add(lvi);
                }
                foreach (CounterDefinition counter in obj.Counters)
                {
                    ListViewItem lvi = new ListViewItem(new string[] { 
                        names.ContainsKey(counter.CounterNameTitleIndex) ? 
                        names[counter.CounterNameTitleIndex] : counter.CounterNameTitleIndex.ToString(), counter.Value 
                    });
                    lvi.ImageIndex = 1;
                    listView1.Items.Add(lvi);
                }
                if (helps.ContainsKey(obj.ObjectHelpTitleIndex))
                {
                    textBox1.Text += helps[obj.ObjectHelpTitleIndex];
                }
            }
            else
            {
                ObjectType obj = PerfDataBlock1.Objects.ElementAt(t.Parent.Index);
                InstanceDefinition inst = obj.Instances.ElementAt(t.Index);
                ListViewItem lvi = new ListViewItem(new string[] { "Name", inst.Name });
                lvi.ImageIndex = 0;
                listView1.Items.Add(lvi);

                foreach (CounterDefinition counter in inst.Counters)
                {
                    lvi = new ListViewItem(new string[] {
                        names.ContainsKey(counter.CounterNameTitleIndex) ?
                        names[counter.CounterNameTitleIndex] : counter.CounterNameTitleIndex.ToString(), counter.Value
                    });
                    lvi.ImageIndex = 1;
                    listView1.Items.Add(lvi);
                }
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
            int index = listView1.SelectedItems[0].Index;

            TreeNode t = treeView1.SelectedNode;
            TreeNode tp = t.Parent;

            if (t.Parent == null)
            {
                switch (index)
                {
                    case 0: //SystemName
                        textBox1.Text += "System name\r\n";
                        break;
                    case 1: //SystemTime
                        textBox1.Text += "System time (UTC)\r\n";
                        textBox1.Text += "Time at the system under measurement\r\n";
                        textBox1.Text += "Type: SYSTEMTIME\r\n";
                        break;
                    case 2: //DefaultObject
                        textBox1.Text += "Object Title Index of default object to " +
                            "display when data from this system is retrieved (-1 = " +
                            "none, but this is not expected to be used)\r\n";
                        textBox1.Text += "Type: LONG\r\n";
                        break;
                    case 3: //NumObjectTypes
                        textBox1.Text += "Number of types of objects being reported\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 4: //PerfFreq
                        textBox1.Text += "Performance counter frequency at the system " +
                            "under measurement\r\n";
                        textBox1.Text += "Type: LARGE_INTEGER\r\n";
                        break;
                    case 5: //PerfTime
                        textBox1.Text += "Performance counter value at the system " +
                            "under measurement \r\n";
                        textBox1.Text += "Type: LARGE_INTEGER\r\n";
                        break;
                    case 6: //PerfTime100nSec
                        textBox1.Text += "Performance counter time in 100 nsec units " +
                            "at the system under measurement\r\n";
                        textBox1.Text += "Type: LARGE_INTEGER\r\n";
                        break;
                    case 7: //Revision
                        textBox1.Text += "Revision of these data structures starting " +
                            "at 0 for each Version\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 8: //Signature
                        textBox1.Text += "Signature: Unicode \"PERF\"\r\n";
                        textBox1.Text += "Type: WCHAR\r\n";
                        break;
                    case 9: //TotalByteLength
                        textBox1.Text += "Total length of data block\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 10:    //Version
                        textBox1.Text += "Version of these data structures starting at 1\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                }
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                switch (index)
                {
                    case 0: //TotalByteLength
                        textBox1.Text += "Length of this object definition including this " +
                            "structure, the counter definitions, and the instance definitions " +
                            "and the counter blocks for each instance: This is the offset from " +
                            "this structure to the next object, if any\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 1: //ObjectNameTitleIndex
                        textBox1.Text += "Index to name in Title Database\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 2: //ObjectHelpTitleIndex
                        textBox1.Text += "Index to Help in Title Database\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 3: //DetailLevel
                        textBox1.Text += "Object level of detail (for controlling display complexity); " +
                            "will be min of detail levels for all this object's counters\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 4: // NumCounters
                        textBox1.Text += "Number of counters in each counter block (one " +
                            "counter block per instance)\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 5: //NumInstances
                        textBox1.Text += "Number of object instances for which counters are " +
                            "being returned from the system under measurement. If the object defined " +
                            "will never have any instance data structures (InstanceDefinition) then " +
                            "this value should be -1, if the object can have 0 or more instances, " +
                            "but has none present, then this should be 0, otherwise this field contains " +
                            "the number of instances of this counter.\r\n";
                        textBox1.Text += "Type: LONG\r\n";
                        break;
                    case 6: //CodePage
                        textBox1.Text += "0 if instance strings are in UNICODE, else the Code Page " +
                            "of the instance names\r\n";
                        textBox1.Text += "Type: DWORD\r\n";
                        break;
                    case 7: //PerfTime
                        textBox1.Text += "Sample Time in \"Object\" units\r\n";
                        textBox1.Text += "Type: LARGE_INTEGER\r\n";
                        break;
                    case 8: //PerfFreq
                        textBox1.Text += "Frequency of \"Object\" units in counts per second.\r\n";
                        textBox1.Text += "Type: LARGE_INTEGER\r\n";
                        break;
                    default:
                        int newindex = index - 9;
                        int objindex = t.Index;
                        ObjectType obj = PerfDataBlock1.Objects.ElementAt(objindex);
                        CounterDefinition counter = obj.Counters.ElementAt(newindex);

                        StringBuilder name = new StringBuilder(50);
                        StringBuilder description = new StringBuilder(500);

                        int a = Externals.GetCounterType(counter.CounterType, name, description);
                        textBox1.Text += "Size: " + counter.CounterSize + " bytes" + "\r\n";
                        textBox1.Text += "Counter type: " + counter.CounterType + " = " + name.ToString() + "\r\n";
                        textBox1.Text += "Counter type description: " + description + "\r\n";
                        textBox1.Text += "Default scale: " + Math.Pow(10,counter.DefaultScale) + "\r\n";
                        textBox1.Text += "Detail level: " + counter.DetailLevel + " = " + GetDetailLevel(counter.DetailLevel) + "\r\n";

                        if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                        {
                            textBox1.Text += helps[counter.CounterHelpTitleIndex];
                        }
                        break;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        textBox1.Text += "Object name\r\n";
                        break;
                    default:
                        int newindex = index - 1;
                        ObjectType obj = PerfDataBlock1.Objects.ElementAt(tp.Index);
                        InstanceDefinition inst = obj.Instances.ElementAt(t.Index);
                        CounterDefinition counter = inst.Counters.ElementAt(newindex);

                        StringBuilder name = new StringBuilder(50);
                        StringBuilder description = new StringBuilder(500);

                        int a = Externals.GetCounterType(counter.CounterType, name, description);
                        textBox1.Text += "Size: " + counter.CounterSize + " bytes" + "\r\n";
                        textBox1.Text += "Counter type: " + counter.CounterType + " = " + name.ToString() + "\r\n";
                        textBox1.Text += "Counter type description: " + description + "\r\n";
                        textBox1.Text += "Default scale: " + Math.Pow(10, counter.DefaultScale) + "\r\n";
                        textBox1.Text += "Detail level: " + counter.DetailLevel + " = " + GetDetailLevel(counter.DetailLevel) + "\r\n";

                        if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                        {
                            textBox1.Text += helps[counter.CounterHelpTitleIndex];
                        }
                        break;
                }
            }
        }

        private string GetDetailLevel(int num)
        {
            switch (num)
            {
                case 100:
                    return "Novice";
                case 200:
                    return "Advanced";
                case 300:
                    return "Expert";
                case 400:
                    return "Wizard";
            }
            return "Unknown";
        }
    }
}