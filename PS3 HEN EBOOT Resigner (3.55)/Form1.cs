using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;

namespace PS3_HEN_EBOOT_Resigner__3._55_
{
    public partial class Form1 : Form
    {
        string[] files;
        string directory, elfPath, text;
        string newEbootPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            try
            {
                File.WriteAllBytes(Environment.CurrentDirectory + "\\data.zip", Properties.Resources.data);
                if (File.Exists(Environment.CurrentDirectory + "\\data.zip"))
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(Environment.CurrentDirectory + "\\data.zip", Environment.CurrentDirectory);
                    File.Delete(Environment.CurrentDirectory + "\\data.zip");
                }
            }
            catch
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Directory.Exists(Environment.CurrentDirectory + "\\data\\"))
            {
                Directory.Delete(Environment.CurrentDirectory + "\\data\\", true);
            }

            if (File.Exists("scetool.exe"))
            {
                File.Delete("scetool.exe");
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Process scetool = new Process();
                scetool.StartInfo.FileName = "scetool.exe";
                scetool.StartInfo.Arguments = " -i \"" + directory + "\"";
                scetool.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                scetool.StartInfo.UseShellExecute = false;
                scetool.StartInfo.RedirectStandardOutput = true;
                scetool.StartInfo.CreateNoWindow = true;
                scetool.Start();

                text = scetool.StandardOutput.ReadToEnd();

                //richTextBox1.Text = "Eboot path = " + txtGame.Text + "\n\n";
                //richTextBox1.Text += "Elf path = " + elfPath + "\n\n";
                //richTextBox1.Text += "Args = " + scetool.StartInfo.FileName + scetool.StartInfo.Arguments + "\n\n";
                //richTextBox1.Text += text + "\n\n";



                if (text.Contains("SELF-Type [Application]"))
                {
                    richTextBox1.Text += "Decrypting EBOOT.BIN to EBOOT.ELF.." + "\n";

                    Process decrypt = new Process();
                    decrypt.StartInfo.FileName = "scetool.exe";
                    decrypt.StartInfo.Arguments = " -d \"" + directory + "\" \"" + elfPath + "\"";
                    decrypt.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    decrypt.StartInfo.UseShellExecute = false;
                    decrypt.StartInfo.RedirectStandardOutput = true;
                    decrypt.StartInfo.CreateNoWindow = true;
                    decrypt.Start();
                    text = decrypt.StandardOutput.ReadToEnd();


                    //richTextBox1.Text += "Args = " + decrypt.StartInfo.FileName + decrypt.StartInfo.Arguments + "\n\n";
                    //richTextBox1.Text += text;


                    if (File.Exists(elfPath))
                    {
                        richTextBox1.Text += "Backing up original EBOOT.BIN.." + "\n";

                        string path = Path.GetDirectoryName(txtGame.Text);
                        try
                        {
                            System.IO.File.Move(txtGame.Text, path + "\\EBOOT.BAK");

                        }
                        catch
                        {

                        }


                        richTextBox1.Text += "Encrypting and compressing EBOOT.BIN.." + "\n";

                        Process encrypt = new Process();
                        encrypt.StartInfo.FileName = "scetool.exe";
                        encrypt.StartInfo.Arguments = " --verbose --sce-type=SELF --skip-sections=FALSE --compress-data=TRUE --self-add-shdrs=TRUE --key-revision=0A --self-auth-id=1010000001000003 --self-vendor-id=01000002 --self-ctrl-flags=0000000000000000000000000000000000000000000000000000000000000000 --self-cap-flags=00000000000000000000000000000000000000000000003B0000000100040000 --self-app-version=0001000000000000 --self-type=APP --self-fw-version=0003005500000000 --encrypt " + "\"" + elfPath + "\" " + "\"" + path + "\\EBOOT.BIN" + "\"";
                        encrypt.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        encrypt.StartInfo.UseShellExecute = false;
                        encrypt.StartInfo.RedirectStandardOutput = true;
                        encrypt.StartInfo.CreateNoWindow = true;
                        encrypt.Start();
                        text = encrypt.StandardOutput.ReadToEnd();

                        //richTextBox1.Text += "Args = " + encrypt.StartInfo.FileName + encrypt.StartInfo.Arguments + "\n\n";
                        //richTextBox1.Text += text;

                        richTextBox1.Text += "Deleting files.." + "\n";

                        if (File.Exists(elfPath))
                        {
                            File.Delete(elfPath);
                        }
                        richTextBox1.Text += "EBOOT.BIN resigned to 3.55 FW" + "\n";
                        label5.ForeColor = Color.Green;
                        label5.Text = "DONE!";
                    }
                    else
                    {
                        label5.ForeColor = Color.Red;
                        label5.Text = "FAIL!";
                    }
                }
                else
                {

                }
            }
            catch
            {

            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            label5.ForeColor = Color.Black;
            label5.Text = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length != 0)
                {

                   
                    richTextBox1.Clear();
                    txtGame.Text = files[0];
                    directory = txtGame.Text;
                    elfPath = Path.GetDirectoryName(txtGame.Text) + "\\EBOOT.ELF";
                    newEbootPath = Path.GetDirectoryName(txtGame.Text) + "\\EBOOT355.BIN";

                    richTextBox1.Text = "Reading EBOOT.BIN Info.." + "\n";

                    if (backgroundWorker1.IsBusy)
                        backgroundWorker1.CancelAsync();
                    backgroundWorker1.RunWorkerAsync();
                    
                }
                else
                {
                    MessageBox.Show("INVALID FILE", "3.55 EBOOT RESIGNER");
                }
            }
        }
    }
}
