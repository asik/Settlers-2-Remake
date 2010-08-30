using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AssetImporter {
    public partial class Form1 : Form {

        string sourceFolder;
        string targetFolder;

        public Form1() {
            InitializeComponent();
        }

        bool promptForS2_EXE() {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Where is your original Settlers 2 executable?";
            openFileDialog.Filter = "|S2.EXE";
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                sourceFolder = Path.GetDirectoryName(openFileDialog.FileName);
                return true;
            }
            return false;
        }

        void convertAllAssets() {
            backgroundWorker1.ProgressChanged += (sender, e) => {
                if (!String.IsNullOrEmpty(e.UserState as string)) {
                    textBox3.AppendText((string)e.UserState);
                }
            };
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerAsync(null);
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            var result = e.Result as string;
            if (result == "success") {
                MessageBox.Show("All assets converted successfully!\nThe asset importer will now close.");
                Close();
            }
            else {
                MessageBox.Show("An error occured. See message log for details.");
                textBox3.AppendText(result);
            }
        }
 

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            try {
                new Converter().Convert(sourceFolder, targetFolder, sender as BackgroundWorker);
                e.Result = "success";
            }
            catch (Exception exception) {
                e.Result = exception.Message;
                button3.Invoke((Delegate)(Action)(() => button3.Enabled = true));
            }

        }

        void button3_Click(object sender, EventArgs e) {
            targetFolder = "../assets";
            promptForS2_EXE();
            button3.Invoke((Delegate)(Action)(() => button3.Enabled = false));
            convertAllAssets();
        }
    }
}
