using Aspose.Words;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditWordByAspose
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void FileChoose_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if(file.ShowDialog() == DialogResult.OK)
            {
                this.filePath.Text = file.FileName;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.filePath.Text = "F:\\eee\\Test.docx";
            //载入模板
            var doc = new Document(this.filePath.Text);

            //基本属性
            DocumentBuilder builder = new DocumentBuilder(doc);
            builder.MoveToMergeField("txt1");
            builder.Write("被我写入了值11！");
            builder.MoveToMergeField("txt2");
            builder.Write("被我写入了值22！");

            //写表格
            var list = doc.GetChildNodes(NodeType.Table, true);
            
            
            //写图片



            //保存
            doc.Save("F:\\eee\\WriteDoc.docx");
        }
    }
}
