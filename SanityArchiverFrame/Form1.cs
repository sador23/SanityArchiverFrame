using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanityArchiverFrame
{
    public partial class Form1 : Form
    {

        private bool toolOn;
        private FileBrowser browserInstance;
        private GrammarBuilder gb = new GrammarBuilder();
        private string command ="";

        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private SpeechRecognitionEngine sre = new SpeechRecognitionEngine();

        public Form1()
        {
            InitializeComponent();
            toolOn = false;
            toolbar.Hide();
            Image fileImage = Image.FromFile(@"C:\Users\sador23\Documents\Visual Studio 2017\Projects\SanityArchiverFrame\SanityArchiverFrame\files.png");
            Image folderImage = Image.FromFile(@"C:\Users\sador23\Documents\Visual Studio 2017\Projects\SanityArchiverFrame\SanityArchiverFrame\folder-icon.jpg");

            fileFolder.Images.Add(folderImage);
            fileFolder.Images.Add(fileImage);

            fileBrowser.SmallImageList = fileFolder;
            fileBrowser.LargeImageList = fileFolder;

            browserInstance = new FileBrowser();
            this.ContextMenuStrip = contextMenuStrip1;
            FillRoot();
            VoiceCommand();
        }

        private void toolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolOn = !toolOn;
            if (toolOn)
            {
                toolbar.Show();
            }
            else
            {
                toolbar.Hide();
            }
        }

        public void FillRoot()
        {

            foreach (DirectoryInfo info in browserInstance.GetRootDirs())
            {
                fileBrowser.Items.Add(new ListViewItem { ImageIndex = 0, Text = info.Name });
            }

            foreach (FileInfo info in browserInstance.GetRootFiles())
            {
                string output = String.Format("{0}", info.Name);
                fileBrowser.Items.Add(new ListViewItem { ImageIndex = 1, Text = output });
            }

        }

        public void FillCurrent()
        {
            int i = 0;
            fileBrowser.Items.Clear();
            Choices choices = new Choices();
            string[] choice = new string[browserInstance.GetCurrentDirs().Count];

            foreach (DirectoryInfo info in browserInstance.GetCurrentDirs())
            {
                choice[i] = info.Name;
                fileBrowser.Items.Add(new ListViewItem { ImageIndex = 0, Text = info.Name });
                i++;
            }
            choices.Add(choice);

            choice = new string[browserInstance.GetCurrentFiles().Count];
            i = 0;
            foreach (FileInfo info in browserInstance.GetCurrentFiles())
            {
                string output = String.Format("{0}", info.Name);
                choice[i] = output;
                fileBrowser.Items.Add(new ListViewItem { ImageIndex = 1, Text = output });
                i++;
            }
            choices.Add(choice);
            gb.Append(choices);
            Grammar g = new Grammar(gb);
            sre.LoadGrammarAsync(g);
        }

        private void ListView_MouseDoubleClick(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                String text = fileBrowser.SelectedItems[0].Text;
                if (fileBrowser.SelectedItems[0].ImageIndex != 1)
                {
                    browserInstance.SetPath(true, text);
                    FillCurrent();
                }
            }
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browserInstance.SetPath(false);
            FillCurrent();
        }

        private void compressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                try
                {
                    browserInstance.Zip(fileBrowser.SelectedItems[0].Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    FillCurrent();
                }
            }
            FillCurrent();
        }

        private void unzipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                try
                {
                    browserInstance.UnZip(fileBrowser.SelectedItems[0].Text);
                }
                catch (Exception ex)
                {

                }

            }
            FillCurrent();
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                String psw = Microsoft.VisualBasic.Interaction.InputBox("Enter Password!", "password", "Enter password here");
                browserInstance.Encrypt(fileBrowser.SelectedItems[0].Text, psw);
                FillCurrent();
            }


        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {


                if (fileBrowser.SelectedItems.Count == 1)
                {
                    String psw = Microsoft.VisualBasic.Interaction.InputBox("Enter Password!", "password", "Enter password here");
                    browserInstance.Decrypt(fileBrowser.SelectedItems[0].Text, psw);
                    FillCurrent();
                }
            } catch (CryptographicException ex)
            {
                MessageBox.Show("Wrong password!");
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                String name = fileBrowser.SelectedItems[0].Text;
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String name = Microsoft.VisualBasic.Interaction.InputBox("Enter directory name!", "Name", "");
            browserInstance.CreateFolder(name);
            FillCurrent();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                browserInstance.DeleteFile(fileBrowser.SelectedItems[0].Text);
                FillCurrent();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            DisableContextMenu();

            if (!browserInstance.copyPath.Equals(""))
            {
                contextMenuStrip1.Items[7].Enabled = true;
            }

            if (fileBrowser.SelectedItems.Count == 1)
            {
                contextMenuStrip1.Items[5].Enabled = true;
                contextMenuStrip1.Items[4].Enabled = true;
                contextMenuStrip1.Items[6].Enabled = true;
                contextMenuStrip1.Items[7].Enabled = false;
                String selected = fileBrowser.SelectedItems[0].Text;
                if (selected.Contains("zip") && !selected.Contains("Crypt"))
                {
                    contextMenuStrip1.Items[1].Enabled = true;
                    contextMenuStrip1.Items[2].Enabled = true;
                } else if (selected.Contains("zip") && selected.Contains("Crypt"))
                {
                    contextMenuStrip1.Items[3].Enabled = true;
                }
                else if (browserInstance.copyPath != "")
                {
                    contextMenuStrip1.Items[7].Enabled = true;
                }
                else
                {
                    contextMenuStrip1.Items[0].Enabled = true;
                }
            }
        }

        private void DisableContextMenu()
        {
            foreach (ToolStripMenuItem item in contextMenuStrip1.Items)
            {
                item.Enabled = false;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileBrowser.SelectedItems.Count == 1)
            {
                browserInstance.SetCopyPath(fileBrowser.SelectedItems[0].Text);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        { 
          
            try{

                if (fileBrowser.SelectedItems.Count == 1)
                    browserInstance.CopyFile(fileBrowser.SelectedItems[0].Text);
                else browserInstance.CopyFile();
                    }
            catch(UnauthorizedAccessException ex)
            {
                MessageBox.Show("Dear user! I'd like to inform you, in this formal letter, that I am unable to solve this problem. It's very weird, and I've tried many ways to figure this out, but nah. ");  
            }
        }


        

        public void  VoiceCommand()
        {
            synthesizer.Volume = 100;
            synthesizer.Rate = -2;
            synthesizer.SpeakAsync("Welcome, how are you today?");
            Choices choices = new Choices();
            choices.Add(new string[] { "selelct","copy","paste","compress","unzip","encrypt","show view", "enter", "back" });
            gb.Append(choices);
            Grammar g = new Grammar(gb);
            sre.LoadGrammar(g);
            sre.SpeechRecognized +=
            new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            sre.SetInputToDefaultAudioDevice();
            //sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            synthesizer.SpeakAsync("Speech recognized: " + e.Result.Text);
            if (!command.Equals("select"))
            {

                 switch (e.Result.Text)
                {
                    case ("show view"):
                        toolbarToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("copy"):
                        copyToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("paste"):
                        pasteToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("back"):
                        backToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("compress"):
                        compressToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("encrypt"):
                        encryptToolStripMenuItem_Click(sender, new EventArgs());
                        break;
                    case ("select"):
                        command = "select";
                        break;
                    case ("enter"):
                        ListView_MouseDoubleClick(sender, new EventArgs());
                        break;
                }

            }
            else
            {
                for(int i=0;i> fileBrowser.Items.Count;i++)
                {
                   if(e.Result.Text.Equals(fileBrowser.Items[i].Text))
                    {
                        fileBrowser.Items[i].Selected = true;
                        fileBrowser.Items[i].Focused = true;
                        fileBrowser.Select();
                        command = "";
                        break;
                    }
                }
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String name = Microsoft.VisualBasic.Interaction.InputBox("Enter name!", "Search", "");
            MessageBox.Show("found file : " + browserInstance.RecursiveSearch(name, new DirectoryInfo(@"C:\")));
        }
    }
}
