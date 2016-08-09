using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RegistrySearch
{
    public partial class Form1 : Form
    {
        public Form1()
        { InitializeComponent(); }

        private void Form1_Load(object sender, EventArgs e)
        { Functions.Init(); }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        { Functions.Close(); }

        private void button1_Click(object sender, EventArgs e)
        { Functions.Search(); }

        private void button2_Click(object sender, EventArgs e)
        { Functions.Stop(); }

        private void button3_Click(object sender, EventArgs e)
        { Functions.Close(); }

        private void button4_Click(object sender, EventArgs e)
        { Functions.Clear(); }

        private void button5_Click(object sender, EventArgs e)
        { Functions.Replace(); }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textBox3.Enabled = (comboBox1.SelectedIndex > 0);
                button5.Enabled = false;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        { Functions.Select(); }

        internal void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Functions.eSearchType type = Functions.eType;

                if (type == Functions.eSearchType.Folder)
                    Functions.SearchFolder();
                else if (type == Functions.eSearchType.Key)
                    Functions.SearchKey();
                else if (type == Functions.eSearchType.Value)
                    Functions.SearchValue();
                else
                    MessageBox.Show("Unknown search type.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                listBox1.Items.Clear();
                for (int i = 0; i < Functions.listKey.Count; i++)
                {
                    listBox1.Items.Add(Functions.listKey[i]);
                }
                listBox1.Refresh();
            }
            else
            {
                if (Program.mainForm.worker.CancellationPending)
                    return;
                else
                    progressBar1.Value = e.ProgressPercentage;
            }
        }

        internal void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Functions.ProcessSearch(false);
            listBox1.Refresh();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = (textBox1.Text.Trim() == "");
            textBox4.Enabled = (textBox1.Text.Trim() == "");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = (textBox2.Text.Trim() == "");
            textBox4.Enabled = (textBox2.Text.Trim() == "");
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = (textBox4.Text.Trim() == "");
            textBox2.Enabled = (textBox4.Text.Trim() == "");
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        { button5.Enabled = (textBox6.Text.Trim() != ""); }

        private void textBox5_TextChanged(object sender, EventArgs e)
        { button5.Enabled = (textBox5.Text.Trim() != ""); }

        private void textBox7_TextChanged(object sender, EventArgs e)
        { button5.Enabled = (textBox7.Text.Trim() != ""); }
    }
}
