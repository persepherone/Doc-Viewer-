using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TabControlAdv_2005
{
    public partial class DocViewer : Form
    {
        //List<C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl> previewHandlersList 
        //    = new List<C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl>();

        //List<Syncfusion.Windows.Forms.Tools.TabPageAdv> tabList 
        //    = new List<Syncfusion.Windows.Forms.Tools.TabPageAdv>();

        public DocViewer()
        {
            InitializeComponent();
            tabControlAdv1.Multiline = true;
            // Allows the user to move the tabs by simply dragging and dropping
            tabControlAdv1.UserMoveTabs = true;
            tabControlAdv1.HotTrack = true;
            tabControlAdv1.ShowTabCloseButton = true;
            //tabControlAdv1.conte
            
            this.AllowDrop = true;
            //this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            //this.OnDragDrop. += new DragEventHandler(Form1_DragDrop);
            this.DragEnter += new DragEventHandler(treeView1_DragEnter);

        }

        void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            // Sets the effect that will happen when the drop occurs to a value 
            // in the DragDropEffects enumeration. 
            e.Effect = DragDropEffects.Copy;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.Multiselect = true;
                DialogResult dr = dlg.ShowDialog();
                //if (dlg.ShowDialog())
                //{
                if (dr != DialogResult.Cancel)
                {
                    foreach (var item in dlg.FileNames)
                    {
                        OpenFile(item);
                    }

                    tabControlAdv1.SelectedIndex = tabControlAdv1.TabPages.Count - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: "+ex.Message);
                //throw;
            }
            //tabControlAdv1.TabPages.Count
        }

        private void OpenFile(string item)
        {
            C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl control1 = new C4F.DevKit.PreviewHandler.PreviewHandlerHost.PreviewHandlerHostControl();
            control1.Width = tabControlAdv1.Width - 8;
            control1.Height = tabControlAdv1.Height - 35;

            control1.FilePath = item;//  dlg.FileName;
            control1.AllowDrop = true;

            string filename = System.IO.Path.GetFileName(item);
            //filename = getShortFilename(filename);
            string filenameExt = System.IO.Path.GetExtension(item).ToLower();

            //adjust for extra long filenames here

            Syncfusion.Windows.Forms.Tools.TabPageAdv tab1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv(filename);
            tab1.Controls.Add(control1);
            tab1.ToolTipText = filename;
            if (filenameExt == ".doc" || filenameExt == ".docx")
            {
                tab1.Image
                    = Image.FromFile(@".\Word-icon.png");
            }
            else if (filenameExt == ".xls" || filenameExt == ".xlsx")
            {
                tab1.Image
                    = Image.FromFile(@".\Excel-icon.png");
            }
            else if (filenameExt == ".ppt" || filenameExt == ".pptx")
            {
                tab1.Image
                    = Image.FromFile(@".\PowerPoint-icon.png");
            }

            tabControlAdv1.TabPages.Add(tab1);
        }


        void Form1_DragDrop(object sender, DragEventArgs e)
        {

            //string formats = string.Join("\n", e.Data.GetFormats(false));

            //MessageBox.Show(formats);

            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    OpenFile(file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);

                //throw;
            }

            tabControlAdv1.SelectedIndex = tabControlAdv1.TabPages.Count - 1;

            //MessageBox.Show(file);
            //Console.WriteLine(file);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.Run(new DocViewer());
        }

        private void tabControlAdv1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControlAdv1_Resize(object sender, EventArgs e)
        {
            //tabControlAdv1.TabPages[0].Controls[0].Width = tabControlAdv1.Width - 10;            
            //tabControlAdv1.TabPages[0].Controls[0].Height = tabControlAdv1.Height - 10;

            foreach (Syncfusion.Windows.Forms.Tools.TabPageAdv item in tabControlAdv1.TabPages)
            {
                item.Controls[0].Width = tabControlAdv1.Width - 8;
                item.Controls[0].Height = tabControlAdv1.Height - 35;

            }
        }


        private string getShortFilename(string fileName)
        {            
            const int MAX_WIDTH = 45;

            if (fileName.Length <= MAX_WIDTH)
                return fileName;

            
            int i = fileName.LastIndexOf('.');

            string tokenRight = fileName.Substring(i, fileName.Length - i);
            string tokenCenter = @"...";
            string tokenLeft = fileName.Substring(0, MAX_WIDTH - (tokenRight.Length + tokenCenter.Length));

            return tokenLeft + tokenCenter + tokenRight;

        }

        private string getShortFilename2(string path)
        {
            const string pattern = @"^(w+:|)([^]+[^]+).*([^]+[^]+)$";
            const string replacement = "$1$2...$3";
            if (Regex.IsMatch(path, pattern))
            {
                return Regex.Replace(path, pattern, replacement);
            }
            else
            {
                return path;
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        static string PathShortener(string path, int length)
        {
            StringBuilder sb = new StringBuilder();
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }

        #region Custom ContextMenu
        private void contextMenuStripEx1_Opening(object sender, CancelEventArgs e)
        {
            //if (FormTabControl.GetTabRect(FormTabControl.SelectedIndex).Contains(tabPoint)
            //    && contextMenuCheck.Checked)
            //{
                contextMenuStripEx1.Show();
            //    tabPoint = Point.Empty;
            //}
            //else
            //    e.Cancel = true;
        }

        private void FormTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            //if (contextMenuCheck.Checked)
            //    tabPoint = new Point(e.X, e.Y);
        }

        private void addTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TabPageAdv tabPage = new TabPageAdv("New Tab");
            //tabPage.ImageIndex = 3;
            //FormTabControl.TabPages.Add(tabPage);
            //MessageBox.Show("addTabToolStripMenuItem_Click");

            try
            {
                var dlg = new OpenFileDialog();
                dlg.Multiselect = true;
                DialogResult dr = dlg.ShowDialog();
                //if (dlg.ShowDialog())
                //{
                if (dr != DialogResult.Cancel)
                {
                    foreach (var item in dlg.FileNames)
                    {
                        OpenFile(item);
                    }

                    tabControlAdv1.SelectedIndex = tabControlAdv1.TabPages.Count - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
                //throw;
            }

        }
        #endregion

    }
}
