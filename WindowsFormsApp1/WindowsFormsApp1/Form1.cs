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

            names = Utils.GetNamesFromRegistry();
            helps = Utils.GetHelpsFromRegistry();

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
                        if (counter2 != null)
                            counter.SetValueUsingTwo(counter2.CounterType, counter2.RawData);
                        else
                            counter.SetValueUsingOne();
                    }

                }
                
                foreach (CounterDefinition counter in obj.Counters)
                {
                    CounterDefinition counter2 = GetCounter(PerfDataBlock2, counter.CounterNameTitleIndex, obj.ObjectNameTitleIndex);
                    if (counter2 != null)
                        counter.SetValueUsingTwo(counter2.CounterType, counter2.RawData);
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
                        PerfObjectType.Instances.Add(PerfInstDef);
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
                        PerfObjectType.Counters.Add(PerfCounterDef);
                        Externals.GetNextCounterPointer(CounterDefinitionPntr, out CounterDefinitionPntr);
                    }
                }
                PerfDataBlock.Objects.Add(PerfObjectType);
                Externals.GetNextObjectTypePointer(PerfObjectTypePntr, out PerfObjectTypePntr);
            }
            return PerfDataBlock;
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
                textBox1.Text = Utils.GetDataBlockPropertiesDescriptionByIndex(index);
            }
            else if (t.Parent.Text.Equals("DataBlock"))
            {
                if (index <= 8) {
                    textBox1.Text = Utils.GetObjectTypePropertiesDescriptionByIndex(index);
                    return;
                }

                ObjectType obj = PerfDataBlock1.Objects.ElementAt(t.Index);
                CounterDefinition counter = obj.Counters.ElementAt(index - 9);
                StringBuilder name = new StringBuilder(50);
                StringBuilder description = new StringBuilder(500);

                Externals.GetCounterType(counter.CounterType, name, description);

                textBox1.Text = string.Format(counter.GetDescription(), name.ToString(), description.ToString());

                if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                    textBox1.Text += helps[counter.CounterHelpTitleIndex];
            }
            else
            {
                if (index == 0) {
                    textBox1.Text += "Object name\r\n";
                    return;
                }
                
                ObjectType obj = PerfDataBlock1.Objects.ElementAt(tp.Index);
                InstanceDefinition inst = obj.Instances.ElementAt(t.Index);
                CounterDefinition counter = inst.Counters.ElementAt(index - 1);
                StringBuilder name = new StringBuilder(50);
                StringBuilder description = new StringBuilder(500);
                
                Externals.GetCounterType(counter.CounterType, name, description);

                textBox1.Text = string.Format(counter.GetDescription(), name.ToString(), description.ToString());

                if (helps.ContainsKey(counter.CounterHelpTitleIndex))
                    textBox1.Text += helps[counter.CounterHelpTitleIndex];
            }
        }
    }
}