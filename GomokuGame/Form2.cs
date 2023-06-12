using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GomokuGame
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        OpenFileDialog openFileDialog;
        string[] filePaths;
        string[] fileNames;
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mp3 files, Mp4 flies (*.mp3, *.mp4)|*.mp*";
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "Open";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePaths = openFileDialog.FileNames;
                fileNames = openFileDialog.SafeFileNames;
                foreach (var item in fileNames)
                {
                    this.listBox1.Items.Add(item);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                int choose = listBox1.SelectedIndex;
                axWindowsMediaPlayer1.URL = filePaths[choose];
                this.textBox1.Text = fileNames[choose];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có muốn thoát không?", "Thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                this.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
