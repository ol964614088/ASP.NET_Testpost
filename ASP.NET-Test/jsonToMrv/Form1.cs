using chemaxon.formats;
using chemaxon.struc;
using chemaxon.struc.graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jsonToMrv
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

        private void Button1_Click(object sender, EventArgs e)
        {
            string constr = "server=192.168.1.113;database=BusinessData_New_Test;uid=sa;pwd=123456";
            SqlConnection con = new SqlConnection(constr);
            string id = this.textBox1.Text;
            string sql = $"select Result from SearchResult where Id ='{id}'";
            SqlCommand com = new SqlCommand(sql, con);
            try
            {
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                reader.Read();
                //获取路线参数
                string result = reader["Result"].ToString();
                //导出Mrv数据
                WriteMrv writeMrv = new WriteMrv();
                string mrv = writeMrv.WriteToMrvWithBranchWithStr(result);
                this.richTextBox1.Text = mrv;
                Clipboard.SetText(mrv);
                reader.Close();
                
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                con.Close();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string constr = "server=192.168.1.113;database=BusinessData_New_Test;uid=sa;pwd=123456";
            SqlConnection con = new SqlConnection(constr);
            string id = this.textBox1.Text;
            string sql = $"select Result from SearchResult where Id ='{id}'";
            SqlCommand com = new SqlCommand(sql, con);
            try
            {
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                reader.Read();
                //获取路线参数
                string result = reader["Result"].ToString();
                //导出Mrv数据
                WriteMrv writeMrv = new WriteMrv();
                string mrv = writeMrv.WriteToMrvWithAllRouteWithStr(result);
                this.richTextBox1.Text = mrv;
                Clipboard.SetText(mrv);
                reader.Close();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                con.Close();
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            string constr = "server=192.168.1.113;database=BusinessData_QuotedPrice_Test;uid=sa;pwd=123456";
            SqlConnection con = new SqlConnection(constr);
            string id = this.textBox1.Text;
            string sql = $"select Result from SearchResult where Id ='{id}'";
            SqlCommand com = new SqlCommand(sql, con);
            try
            {
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                reader.Read();
                //获取路线参数
                string result = reader["Result"].ToString();
                //导出Mrv数据
                WriteMrv writeMrv = new WriteMrv();
                MemoryStream mrv = writeMrv.WriteToMrvWithAllRoute(result);
                //MemoryStream mrv = writeq();
                string path = "F:\\eee\\MolTest.cdx";

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = mrv.ToArray();//转化为byte格式存储
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                    buffer = null;
                }
                reader.Close();
                MessageBox.Show("success");
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                con.Close();
            }
        }

        private MemoryStream writeq()
        {
            //树绘图
            MDocument md = new MDocument(MolImporter.importMol("O"));
            MPoint p1 = new MPoint(1,1);
            MPoint p2 = new MPoint(1,2);
            MPolyline arrow = new MRectangle(p1, p2);
            md.addObject(arrow);
            MemoryStream stream = new MemoryStream(MolExporter.exportToBinFormat(md, "mrv"));
            return stream;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            string constr = "server=192.168.1.113;database=BusinessData_QuotedPrice_Test;uid=sa;pwd=123456";
            SqlConnection con = new SqlConnection(constr);
            string id = this.textBox1.Text;
            string sql = $"select Result from SearchResult where Id ='{id}'";
            SqlCommand com = new SqlCommand(sql, con);
            try
            {
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                reader.Read();
                //获取路线参数
                string result = reader["Result"].ToString();
                //导出Mrv数据
                WriteMrv writeMrv = new WriteMrv();
                MemoryStream mrv = writeMrv.WriteToMrvWithBranch(result);
                //MemoryStream mrv = writeq();
                string path = "F:\\eee\\MolTest.cdx";

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = mrv.ToArray();//转化为byte格式存储
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                    buffer = null;
                }
                reader.Close();
                MessageBox.Show("success");
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
