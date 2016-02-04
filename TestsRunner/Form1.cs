using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;

namespace TestsRunner
{
    public partial class Form1 : Form
    {
        private string path = "";
        public StackPanel stackPanel;
        System.Windows.Controls.TreeView treeV;
        public ScrollViewer scrollView;
        public List<string> selectedCases = new List<string>();
        Window a;
        XmlNodeList xnList = null;
        OpenFileDialog dlg = new OpenFileDialog();
        List<XmlNode> nodes = new List<XmlNode>();
        Window win = null;
        Dictionary <string[],string> list = new Dictionary<string[],string>();
        

        public Form1()
        {
            InitializeComponent();
            try
            {
                string filename = Path.GetFileName("Tests.xml");
                string fullPath = Path.GetFullPath("Tests.xml");
                if (!String.IsNullOrEmpty(filename))
                {
                    path = fullPath;
                    WriteConsole("Document '" + filename + "' Loaded \n");
                }
            }
            catch
            {
                WriteConsole("File Tests.xml not found");
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            if (!String.IsNullOrEmpty(path))
            {
                xml.Load(path);
                string criteria = "/Tests/readyToRun/Test";
                xnList = xml.SelectNodes(criteria);
                foreach (XmlNode xmln in xnList)
                {
                    xmln.Attributes[1].Value = "Y";

                    string[] toAdd = new string[2];
                    if (xmln.OuterXml.Contains("critical=\"Y\""))
                    {
                        toAdd[0] = xmln["Name"].InnerText;
                        toAdd[1] = "critical";
                    }
                    else
                    {
                        toAdd[0] = xmln["Name"].InnerText;
                        toAdd[1] = "normal";
                    }
                    list.Add(toAdd,xmln["Category"].InnerText);
                }
                win = Window2(path, list);
                win.ShowDialog();
            }
        }
        
        public Window Window2(string path, Dictionary<string[], string> list)
        {
            a = new Window();
            stackPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
            scrollView = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
            a.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            a.WindowStyle = WindowStyle.None;
            a.Height = 700;
            a.Width = 500;
            a.Title = path;
            a.ResizeMode = ResizeMode.NoResize;
            treeV = new System.Windows.Controls.TreeView { Name = "Tests" };

            List<TreeViewItem> ltv = new List<TreeViewItem>();
            List<string> containsE = new List<string>();

            foreach (KeyValuePair<string[],string> item in list)
            {
                if (!containsE.Contains(item.Value))
                {
                    containsE.Add(item.Value);
                    TreeViewItem itm = new TreeViewItem();
                    itm.Header = item.Value;
                    ltv.Add(itm);
                }
            }

            foreach (TreeViewItem im in ltv)
            {
                treeV.Items.Add(im);
            }

            foreach (KeyValuePair<string[], string> item in list)
            {
                insertItems(item, treeV);
            }

            var selectBtn = new System.Windows.Controls.Button { Content = "Select" };
            System.Windows.Controls.Button closeBtn = new System.Windows.Controls.Button { Name = "closeBtn", Content = "Close" };
            stackPanel.Children.Add(treeV);
            stackPanel.Children.Add(selectBtn);
            stackPanel.Children.Add(closeBtn);
            scrollView.Content = stackPanel;
            a.Content = scrollView;
            closeBtn.Click += new RoutedEventHandler(onClosing);
            selectBtn.Click += new RoutedEventHandler(selectCases);
            return a;
        }

        public void insertItems(KeyValuePair<string[], string> item, System.Windows.Controls.TreeView trev)
        {
            if (trev.HasItems)
            {
                foreach (TreeViewItem trvit in trev.Items)
                {
                    if (trvit.Header.Equals(item.Value))
                    {
                        if (item.Key[1].Equals("critical"))
                        {
                            trvit.Items.Add(new System.Windows.Controls.CheckBox
                            {
                                FontStyle = FontStyles.Italic,
                                Foreground = new SolidColorBrush(Colors.Red),
                                Content = item.Key[0]
                            });
                        }
                        else
                        {
                            trvit.Items.Add(new System.Windows.Controls.CheckBox { Content = item.Key[0] });
                        }
                    }
                }
            }
        }

        public TreeViewItem addToTreeView(string itemValue,string[] itemKey)
        {
            TreeViewItem treeviewitem = new TreeViewItem { Header = itemValue };
            if (itemKey[1].Equals("critical"))
            {
                treeviewitem.Items.Add(new System.Windows.Controls.CheckBox
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Colors.Red),
                    Content = itemKey[0]
                });
            }
            else
            {
                treeviewitem.Items.Add(new System.Windows.Controls.CheckBox { Content = itemKey[0] });
            }
            return treeviewitem;
        }

        private void selectCases(object sender, RoutedEventArgs e)
        {
            if (selectedCases.Count > 0)
            {
                selectedCases.Clear();
            }
            stackPanel.Children.RemoveAt(stackPanel.Children.Count - 1);
            stackPanel.Children.RemoveAt(stackPanel.Children.Count - 1);

            foreach (TreeViewItem item in treeV.Items)
            {
                foreach (System.Windows.Controls.CheckBox chb in item.Items)
                {
                    if (chb.IsChecked.Equals(true))
                    {
                        selectedCases.Add(chb.Content.ToString());
                    }
                }
                
            }
            WriteConsole(selectedCases.Count + " Cases Added for Running \n");
            CloseWindow();
            button3.Enabled = true;
        }

        private void CloseWindow()
        {
            win.Close();
            a.Close();
            a.Content = null;
            stackPanel.Children.Clear();
            list.Clear();
            treeV.Items.Clear();
            nodes.Clear();
        }

        private void onClosing(object sender, RoutedEventArgs e)
        {
            win.Close();
            a.Close();
            a.Content = null;
            stackPanel.Children.Clear();
            list.Clear();
            treeV.Items.Clear();
            nodes.Clear();
            selectedCases.Clear();
            WriteConsole("Cases Window closed! \n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedCases.Count > 0)
            {
                int count = selectedCases.Count;
                selectedCases.Clear();
                WriteConsole(count + " Cases cleared from list");
            }
            button3.Enabled = false;
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            string xmlPath = @"SeleniumTests\Documents\Tests.xml";

            foreach (XmlNode xmln in xnList)
            {
                foreach (string item in selectedCases)
                {
                    if (xmln["Name"].InnerText.Equals(item))
                    {
                        nodes.Add(xmln);
                    }
                }
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode testsNode = xmlDoc.CreateElement("Tests");
            xmlDoc.AppendChild(testsNode);
            XmlNode readyToRunNode = xmlDoc.CreateElement("readyToRun");
            testsNode.AppendChild(readyToRunNode);
            foreach (XmlNode xmln in nodes)
            {
                try
                {
                    XmlNode import = xmlDoc.ImportNode(xmln, true);
                    readyToRunNode.AppendChild(import);
                }
                catch (Exception ex)
                {
                    WriteConsole("Exception orccured while importing the xml nodes : " + ex.Message + "\n");
                }

            }
            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
            }
            xmlDoc.Save(xmlPath);
            WriteConsole("New Tests.xml document created \n");

            WriteConsole("Starting automation cases");
            RunTests();
            WriteConsole("Finish");
        }

        public void RunTests()
        {
            string exePath = null;
            try
            {
                foreach (string command in Directory.GetFiles("SeleniumTests", "SeleniumTest.exe"))
                {
                    exePath = command;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            if (!String.IsNullOrEmpty(exePath))
            {
                using (Process process = Process.Start(exePath,"Firefox 1 5"))
                {
                    process.WaitForExit();
                }
            }
        }
        
        public void WriteConsole(string text)
        {
            textBox2.AppendText(text + "\n");
        }
    }
}
