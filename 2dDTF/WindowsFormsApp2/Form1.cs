using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        Bitmap Image;
        Bitmap fftshiftImage;
        Bitmap SetImage;
        Bitmap PassImage;
        Bitmap OutImage;
        string img_FileName;
        double[,] pFreqReal = new double[1000, 1000];
        double[,] pFreqImag = new double[1000, 1000];
        bool[,] LP = new bool[1000, 1000];
        bool IsOk = false;
        bool Run = true;
        BackgroundWorker worker = null;
        public Form1()
        {
            InitializeComponent();
            Load += new EventHandler(Form1_Load);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Run)
            {
                MessageBox.Show("還沒完成");
                return;
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.RestoreDirectory = true;
            dlg.Title = "Open Image File";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Png Image|*.png";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlg.FileName != null)
            {
                img_FileName= dlg.FileName;
                Image = new Bitmap(img_FileName);
                fftshiftImage = new Bitmap(img_FileName);
                SetImage = new Bitmap(img_FileName);
                OutImage = new Bitmap(img_FileName);
                PassImage = new Bitmap(img_FileName);
                int height = Image.Height;
                int width = Image.Width;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                if (height>500|| width>500)
                {
                    MessageBox.Show("圖片太大");
                    img_FileName = "";
                    IsOk = false;
                    label1.Text = "";
                }
                else
                {
                    textBox1.Text = ""+(width / 8);
                    textBox2.Text = ""+(height / 8);
                    pictureBox1.Image = Image;
                    IsOk = true;
                    label1.Text = "圖片尺寸 "+ width +" x "+ height +"  "+ img_FileName;
                }
            }
            else
            {
                img_FileName = "";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsOk)
            {
                MessageBox.Show("選擇圖片");
                worker.CancelAsync();
                return;
            }
            if (!Run)
            {
                MessageBox.Show("還沒完成");
                return;
            }
            int height = Image.Height;
            int width = Image.Width;
            if (int.Parse(textBox2.Text)> height / 2|| int.Parse(textBox1.Text) > width / 2)
            {
                MessageBox.Show("截止頻率半徑太大");
                worker.CancelAsync();
                textBox1.Text = "" + (width / 8);
                textBox2.Text = "" + (height / 8);
                return;
            }
            checkBox1.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            progressBar3.Visible = true;
            progressBar3.Minimum = 1;
            progressBar3.Maximum = height* width * 2;
            progressBar3.Value = 1;
            progressBar3.Step = 1;
            worker.RunWorkerAsync();
        }
        double DTF(int height, int width, int u, int v)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //存取圖像的像素
                    Color p = Image.GetPixel(x, y);
                    //灰階化
                    double intensity = 0.299 * p.R + 0.587 * p.G + 0.14 * p.B;
                    if (intensity > 255) intensity = 255;
                    if (intensity < 0) intensity = 0;
                    //傅立葉變換
                    double angleDFT = (-1 * 2 * 3.14159 * ((double)(u * x) / (double)width + (double)(v * y) / (double)height));
                    // 利用歐拉公式式計算傅立葉之實虛數部分
                    pFreqReal[u,v] += (double)intensity * Math.Cos(angleDFT);
                    pFreqImag[u,v] -= (double)intensity * Math.Sin(angleDFT);
                }
            }
            pFreqReal[u,v] = pFreqReal[u,v] * (1 / Math.Sqrt(height * width));
            pFreqImag[u,v] = pFreqImag[u,v] * (1 / Math.Sqrt(height * width));
            // 將計算好的傅立葉實數與虛數部分作結合 
            // 結合後之頻率域返回到影像陣列中顯示 
            return (Math.Sqrt(Math.Pow(pFreqReal[u,v], (double)2.0) + Math.Pow(pFreqImag[u,v], (double)2.0)));
        }
        double InverseDFT(int height, int width, int y, int x)
        {
            double InverseReal = 0.0;
            double InverseImag = 0.0;
            for (int v = 0; v < height; v++)
            {
                for (int u = 0; u < width; u++)
                {
                    //反傅立葉變換
                    double angleIDFT = (2 * 3.14159 * ((double)(u * y) / (double)width + (double)(v * x) / (double)height));
                    double c = Math.Cos(angleIDFT);
                    double s = Math.Sin(angleIDFT);
                    // 利用歐拉公式計算傅立葉之實虛數部分
                    InverseReal += (pFreqReal[u,v] * c + pFreqImag[u,v] * s);
                    InverseImag += (pFreqReal[u,v] * s - pFreqImag[u,v] * c);
                }
            }
            InverseReal = InverseReal * (1 / Math.Sqrt(height * width));
            InverseImag = InverseImag * (1 / Math.Sqrt(height * width));
            // 將計算好的傅立葉實數與虛數部分作結合 
            // 結合後之頻率域返回到影像陣列中顯示
            return (Math.Sqrt(Math.Pow(InverseReal, (double)2.0) + Math.Pow(InverseImag, (double)2.0)));
        }
        void ifftshift(int height, int width)
        {
            int heightHalf = height / 2;
            int widthHalf = width / 2;
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    Color p = SetImage.GetPixel(j, i);
                    fftshiftImage.SetPixel(widthHalf + j, heightHalf + i, Color.FromArgb(255, p.R, p.R, p.R));
                    PassImage.SetPixel(widthHalf + j, heightHalf + i, Color.FromArgb(255, p.R, p.R, p.R));
                }
            }
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    Color p = SetImage.GetPixel(widthHalf + j, heightHalf + i);
                    fftshiftImage.SetPixel(j, i, Color.FromArgb(255, p.R, p.R, p.R));
                    PassImage.SetPixel(j, i, Color.FromArgb(255, p.R, p.R, p.R));
                }
            }
            for (int i = heightHalf; i < height; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    Color p = SetImage.GetPixel(j, i);
                    fftshiftImage.SetPixel(widthHalf + j, i - heightHalf, Color.FromArgb(255, p.R, p.R, p.R));
                    PassImage.SetPixel(widthHalf + j, i - heightHalf, Color.FromArgb(255, p.R, p.R, p.R));
                }
            }
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = widthHalf; j < width; j++)
                {
                    Color p = SetImage.GetPixel(j, i);
                    fftshiftImage.SetPixel(j - widthHalf, heightHalf + i, Color.FromArgb(255, p.R, p.R, p.R));
                    PassImage.SetPixel(j - widthHalf, heightHalf + i, Color.FromArgb(255, p.R, p.R, p.R));
                }
            }
        }
        void circle(int height, int width)
        {
            bool[,] LP2 = new bool[1000, 1000];
            int heightHalf = height / 2;
            int widthHalf = width / 2;
            int heightOvalHalf = int.Parse(textBox2.Text);
            int widthOvalHalf = int.Parse(textBox1.Text);
            for (int v = 0; v < height; v++)
            {
                for (int u = 0; u < width; u++)
                {
                    LP2[v,u] = false;
                }
            }
            for (int y = -heightOvalHalf; y <= heightOvalHalf; y++)
            {
                for (int x = -widthOvalHalf; x <= widthOvalHalf; x++)
                {
                    if (x * x * heightOvalHalf * heightOvalHalf + y * y * widthOvalHalf * widthOvalHalf <= heightOvalHalf * heightOvalHalf * widthOvalHalf * widthOvalHalf)
                    {
                        LP2[heightHalf + y,widthHalf + x] = true;
                    }
                }
            }
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (checkBox1.Checked)
                    {
                        if (LP2[x, y])
                        {
                            PassImage.SetPixel(y, x, Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                    else
                    {
                        if (!LP2[x, y])
                        {
                            PassImage.SetPixel(y, x, Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                }
            }
            pictureBox4.Image = PassImage;
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    LP[heightHalf + i,widthHalf + j] = LP2[i,j];
                }
            }
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    LP[i,j] = LP2[heightHalf + i,widthHalf + j];
                }
            }
            for (int i = heightHalf; i < height; i++)
            {
                for (int j = 0; j < widthHalf; j++)
                {
                    LP[i - heightHalf,widthHalf + j] = LP2[i,j];
                }
            }
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = widthHalf; j < width; j++)
                {
                    LP[heightHalf + i,j - widthHalf] = LP2[i,j];
                }
            }
            for (int i = 0; i < heightHalf; i++)
            {
                for (int j = widthHalf; j < width; j++)
                {
                    LP[heightHalf + i,j - widthHalf] = LP2[i,j];
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Run = false;
            int height = Image.Height;
            int width = Image.Width;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double pFreq = DTF(height, width, j, i);
                    if (pFreq > 255) pFreq = 255;
                    if (pFreq < 0) pFreq = 0;
                    SetImage.SetPixel(j, i, Color.FromArgb(255, (int)pFreq, (int)pFreq, (int)pFreq));
                    worker.ReportProgress(0);
                }
                worker.ReportProgress(0);
            }
            ifftshift(height, width);
            pictureBox2.Image = fftshiftImage;
            circle(height, width);
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (checkBox1.Checked)//高通
                    {
                        if (LP[x, y])
                        {
                            pFreqReal[y, x] = 0;
                            pFreqImag[y, x] = 0;
                        }
                    }
                    else
                    {
                        if (!LP[x, y])//低通
                        {
                            pFreqReal[y, x] = 0;
                            pFreqImag[y, x] = 0;
                        }
                    }
                }
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double pFreq = InverseDFT(height, width, j, i);
                    if (pFreq > 255) pFreq = 255;
                    if (pFreq < 0) pFreq = 0;
                    OutImage.SetPixel(j, i, Color.FromArgb(255, (int)pFreq, (int)pFreq, (int)pFreq));
                    worker.ReportProgress(0);
                }
                worker.ReportProgress(0);
            }
            pictureBox3.Image = OutImage;
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("取消了操作");
            else
            {
                MessageBox.Show("完成了操作");
                checkBox1.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                Run = true;
            }
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar3.PerformStep();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Text = "高通";
            }
            else
            {
                checkBox1.Text = "低通";
            }
        }

        private void progressBar3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }
        private void InvokeFun()
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))
                {
                    e.Handled = true;
                }
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))
                {
                    e.Handled = true;
                }
            }
        }
    }
}
