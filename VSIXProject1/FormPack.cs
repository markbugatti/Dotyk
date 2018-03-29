using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSIXProject1
{
    public partial class FormPack : Form
    {
        String outputDir, projDir;
        public FormPack()
        {
            InitializeComponent();
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                projDir= openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void textBox2_DoubleClick(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputDir = folderBrowserDialog1.SelectedPath;
                textBox2.Text = outputDir;
            }
        }
    }
}
