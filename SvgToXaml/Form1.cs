using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SvgToXaml
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.Text = Path.Combine(Path.GetDirectoryName(s[0]), "svgtoxaml");
            }
            foreach (string path in s)
            {
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    foreach (string p in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    //foreach (string p in FileUtility.GetFiles(path))
                    {
                        AddFileToList(p, path);
                    }
                }
                else
                {
                    AddFileToList(path, Path.GetDirectoryName(path));
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            FileDragEnter(e);
        }

        private static void FileDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        public static string FileExtension = ".svg";
        private void AddFileToList(string path, string dir)
        {
            if (FileExtension.Split('|').Any(f => f.Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)))
            {
                AddFileToListSkipExtgensionCheck(path, dir);
            }
        }


        private void AddFileToListSkipExtgensionCheck(string path, string dir)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = (listView1.Items.Count + 1).ToString();
            lvi.SubItems.Add(Path.GetFileName(path));//.Name = "FileName";
            lvi.SubItems.Add(Path.GetDirectoryName(path));//.Name = "Path";
            lvi.Tag = path;
            listView1.Items.Add(lvi);
        }

        private void btnOutputPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "svg文件|*.svg|所有文件|*";
            var result = ofd.ShowDialog();


            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fullname in ofd.FileNames)
                {
                    AddFileToListSkipExtgensionCheck(fullname, Path.GetDirectoryName(fullname));
                }
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string outputpath = textBox1.Text;
            if (!Directory.Exists(outputpath))
            {
                Directory.CreateDirectory(outputpath);
            }
            foreach (ListViewItem lvi in listView1.Items)
            {
                string filename = lvi.Tag as string;
                var svg = File.ReadAllText(filename);
                var xaml = SvgToXaml.Convert(svg,Path.GetFileNameWithoutExtension(filename));

                File.WriteAllText(Path.Combine(outputpath, Path.GetFileNameWithoutExtension(filename) + ".xaml"), xaml);
            }
        }
    }
}
