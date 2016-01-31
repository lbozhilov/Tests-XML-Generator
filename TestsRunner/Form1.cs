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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename = "";
            dlg.DefaultExt = ".xml";
            Nullable<DialogResult> result = dlg.ShowDialog();
            if (!result.HasValue.Equals(null))
            {
                path = dlg.FileName;
                label1.Text = path;
                if (dlg.CheckPathExists)
                {
                    filename = Path.GetFileName(path);
                }
                if (!String.IsNullOrEmpty(filename))
                {
                    WriteConsole("Document '" + filename + "' Loaded \n");
                }
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
                                Content = item.Key[0] + " (Critical)"
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
                    Content = itemKey[0] + " (Critical)"
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
            button4_Click(sender, e);
        }

        private void onClosing(object sender, RoutedEventArgs e)
        {
            win.Close();
            a.Close();
            a.Content = null;
            stackPanel.Children.Clear();
            path = "";
            list.Clear();
            label1.Text = "";
            treeV.Items.Clear();
            nodes.Clear();
            selectedCases.Clear();
            WriteConsole("Cases Window closed! \n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(label1.Text))
            {
                win.Close();
                a.Close();
                a.Content = null;
                stackPanel.Children.Clear();
                path = "";
                list.Clear();
                label1.Text = "";
                treeV.Items.Clear();
                nodes.Clear();
                selectedCases.Clear();
                WriteConsole("Path cleared \n");
            }
            else
            {
                WriteConsole("Path already cleared \n");
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text.Equals("Run Tests"))
            {
                WriteConsole("Starting automation cases");
                RunTests();
                WriteConsole("Finish");
                button3.Text = "Copy Project and create XML";
            }
            else
            {
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
                try
                {
                    if (File.Exists("Tests.xml"))
                    {
                        WriteConsole("Deleting old document \n");
                        File.Delete("Tests.xml");
                    }
                }
                catch (Exception ex)
                {
                    WriteConsole("Exception orccured  while saving the xml file : " + ex.Message + "\n");
                }

                Copy();

                xmlDoc.Save(@"Project\Documents\Tests.xml");
                WriteConsole("New document created \n");

                a.Content = null;
                stackPanel.Children.Clear();
                selectedCases.Clear();
                path = "";
                list.Clear();
                label1.Text = "";
                treeV.Items.Clear();
                nodes.Clear();

                button3.Text = "Run Tests";
            }
        }

        public void RunTests()
        {
            string exePath = null;
            try
            {
                foreach (string command in Directory.GetFiles("Project", "SeleniumTest.exe"))
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
                using (Process process = Process.Start(exePath))
                    process.WaitForExit();
            }
        }

        public void Copy()
        {
            if (Directory.Exists("Project"))
            {
                Directory.Delete("Project", true);
            }
            System.Windows.Forms.MessageBox.Show("Select Selenium Tests Bin/Release or Bin/Deubg folder!");
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowDialog();
            string path = dlg.SelectedPath;

            DirectoryInfo dest = Directory.CreateDirectory("Project");
            DirectoryInfo source = new DirectoryInfo(path);
            copyAll(source,dest);
        }

        public void copyAll(DirectoryInfo source, DirectoryInfo target)
        {
            WriteConsole("Starting copy of the directory");
            foreach (DirectoryInfo dir in source.GetDirectories())
                copyAll(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            WriteConsole("Directory copied");
        }

        public void WriteConsole(string text)
        {
            textBox2.AppendText(text + "\n");
        }
    }
}
