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
            NodeCollection allTables = doc.GetChildNodes(NodeType.Table, true);
            Aspose.Words.Tables.Table wordTable;
            //获取第一张表
            wordTable = allTables[0] as Aspose.Words.Tables.Table;
            //获取表头
            int index = 0;
            Aspose.Words.Tables.Row rowTen = wordTable.Rows[index];

            DataTable dt = GetDataTable();

            //添加数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //复制表头的那行样式和数据等过来。如果你单独插入一行，你自己试试吧会有惊喜的
                Aspose.Words.Tables.Row row = (Aspose.Words.Tables.Row)rowTen.Clone(true);
                //因为复制的是表头，所以里面的数据是需要我们根据实际数据替换掉的。更改序号
                Aspose.Words.Tables.Cell cell = row.Cells[0];
                Aspose.Words.Paragraph p = new Paragraph(doc);
                p.AppendChild(new Run(doc, (i + 1).ToString()));
                cell.FirstParagraph.Remove();//移除之前的数据
                cell.AppendChild(p);
                //更改为需要填充的数据=XXX
                string str1 = dt.Rows[i]["XXX"].ToString();
                cell = row.Cells[1];
                p = new Paragraph(doc);
                p.AppendChild(new Run(doc, str1));
                cell.FirstParagraph.Remove();
                cell.AppendChild(p);

                //添加一行数据
                wordTable.Rows.Add(row);
            }


            //写图片



            //保存
            doc.Save("F:\\eee\\WriteDoc.docx");
        }

        private DataTable GetDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("sex");
            dt.Columns.Add("year");
            dt.Columns.Add("where");

            var row = dt.NewRow();
            row["name"] = "张三";
            row["sex"] = "男";
            row["year"] = "1999";
            row["where"] = "地点";
            dt.Rows.Add(dt);

            row = dt.NewRow();
            row["name"] = "张三";
            row["sex"] = "男";
            row["year"] = "1999";
            row["where"] = "地点";
            dt.Rows.Add(dt);

            return dt;
        }
    }
}
