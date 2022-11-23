using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using static System.Windows.Forms.LinkLabel;

namespace Homework_5
{
    public partial class Form1 : Form
    {
        Random r = new Random(); 
        Bitmap b;
        Graphics g;
        Rectangle virtualWindow, virtualWindow2;

        SortedDictionary<double, long> dist = new SortedDictionary<double, long>();

        bool vertical = false;

        int x_mouse, y_mouse;
        int x_down, y_down;

        int r_width, r_height;

        bool drag = false;
        bool resizing = false;

        bool pictureBox1_MouseWheel_SR;

        Pen penRectangle = new Pen(Color.Green, 0.2f);


        int interval = 3;
        int repeat = 30000; 

        public Form1()
        {
            InitializeComponent();
            b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(b);

            pictureBox1.Image = b;
            
            virtualWindow = new Rectangle(20, 20, b.Width - 200, b.Height - 200);

            timer1.Enabled = true;
            timer1.Interval = 16;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // if (!virtualWindow.Contains(e.X, e.Y)) return;

            x_mouse = e.X;
            y_mouse = e.Y;

            x_down = virtualWindow.X;
            y_down = virtualWindow.Y;

            r_width = virtualWindow.Width;
            r_height = virtualWindow.Height;

            if (e.Button == MouseButtons.Left)
            {
                drag = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                resizing = true;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            resizing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (b == null) return;

            int delta_x = e.X - x_mouse;
            int delta_y = e.Y - y_mouse;



            if (drag)
            {
                virtualWindow.X = x_down + delta_x;
                virtualWindow.Y = y_down + delta_y;
                // if (virtualWindow.X < 0) virtualWindow.X = 0;
                // if (virtualWindow.Y < 0) virtualWindow.Y = 0;
                // if (virtualWindow.X > pictureBox1.Width - virtualWindow.Width) virtualWindow.X = pictureBox1.Width - virtualWindow.Width;
                // if (virtualWindow.Y > pictureBox1.Height - virtualWindow.Height) virtualWindow.Y = pictureBox1.Height - virtualWindow.Height;
            }
            else if (resizing)
            {

                virtualWindow.Width = r_width + delta_x;
                virtualWindow.Height = r_height + delta_y;
                // if (virtualWindow.Width < 100) virtualWindow.Width = 100;
                // if (virtualWindow.Height < 100) virtualWindow.Height = 100;
                // if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
                // if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            }

        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!(ModifierKeys == Keys.Control)) return;
            if (pictureBox1_MouseWheel_SR) return;

            pictureBox1_MouseWheel_SR = true;

            float stepZoom;
            if (ModifierKeys == (Keys.Shift | Keys.Control))
            {
                stepZoom = 0.01F;
            }
            else
            {
                stepZoom = 0.1F;
            }

            virtualWindow.Inflate((int)(e.Delta * stepZoom), (int)(e.Delta * stepZoom));

            if (virtualWindow.Width < 100) virtualWindow.Width = 100;
            if (virtualWindow.Height < 100) virtualWindow.Height = 100;
            if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
            if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            pictureBox1_MouseWheel_SR = false;

        }

        private void generateHistogram(Rectangle r, SortedDictionary<double, long> d, Graphics g, int interval, bool vertical = false)
        {

            if (d == null || d.Count == 0) return;
            int n = d.Count;


            double maxKey = d.Keys.Max();
            double maxValue = d.Values.Max();
            for (int i = 0; i < n; i++)
            {
                Rectangle rr;
                int left, top, right, bottom;
                if (vertical)
                {
                    // vertical histogram
                    left = fromXRealToXVirtual(0, 0, maxValue, r.Left, r.Width);
                    top = fromYRealToYVirtual(i + 1, 0, n, r.Top, r.Height);
                    right = fromXRealToXVirtual(d.ElementAt(i).Value, 0, maxValue, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(i, 0, n, r.Top, r.Height);
                }
                else
                {
                    //horizontal histogram
                    left = fromXRealToXVirtual(i, 0, n, r.Left, r.Width);
                    top = fromYRealToYVirtual(d.ElementAt(i).Value, 0, maxValue, r.Top, r.Height);
                    right = fromXRealToXVirtual(i + 1, 0, n, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(0, 0, maxValue, r.Top, r.Height);
                }
                rr = Rectangle.FromLTRB(left, top, right, bottom);

                g.DrawRectangle(penRectangle, rr);
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 200, 89, 0)), rr);

                g.DrawString(vertical ? Math.Round(d.ElementAt(i).Key*interval, 1).ToString() : Math.Round((double)d.ElementAt(i).Value, 1).ToString(), DefaultFont, Brushes.Black, r.Right, vertical ? top : top);
                g.DrawString(vertical ? Math.Round((double)d.ElementAt(i).Value, 1).ToString() : Math.Round(d.ElementAt(i).Key*interval, 1).ToString(), DefaultFont, Brushes.Black, vertical ? right: left, r.Bottom);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            redraw();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            vertical = checkBox1.Checked;
        }
        

        // It generates the gaussian distribution 
        private void button2_Click(object sender, EventArgs e)
        {
            dist.Clear(); 

            double num;
            for (int i = 0; i < repeat; i++)
            {
                num = Math.Round((Double)generateGaussian(0, 1), 1);

                if (dist.ContainsKey(num))
                {
                    dist[num]++;
                }
                else
                {
                    dist.Add(num, 1);
                }
            }
        }

        // It generates the Chi square ditribution 
        private void button3_Click(object sender, EventArgs e)
        {
            dist.Clear();

            double num;
            for (int i = 0; i < repeat; i++)
            {
                num = Math.Round((Double)generateGaussian(0, 1), 1);
                num = num * num; 

                if (dist.ContainsKey(num))
                {
                    dist[num]++;
                }
                else
                {
                    dist.Add(num, 1);
                }
            }
        }

        // It generates the Cauchy Distribution 
        private void button4_Click(object sender, EventArgs e)
        {
            dist.Clear();

            double x, y, res;
            for (int i = 0; i < repeat; i++)
            {
                x = Math.Round((Double)generateGaussian(0, 1), 1);
                y = Math.Round((Double)generateGaussian(0, 1), 1);
                if (y != 0) res = Math.Round((double)x / y, 0);
                else continue; 

                if (dist.ContainsKey(res))
                {
                    dist[res]++;
                }
                else
                {
                    dist.Add(res, 1);
                }
            }
        }

        // It generates F-Fisher with 1 and 1 degree of freedom 
        private void button5_Click(object sender, EventArgs e)
        {
            dist.Clear();

            double x, y, res;
            for (int i = 0; i < repeat; i++)
            {
                x = Math.Round((Double)generateGaussian(0, 1), 1);
                y = Math.Round((Double)generateGaussian(0, 1), 1);
                if (y != 0) res = Math.Round((double) Math.Pow(x, 2) / (Math.Pow(y, 2)), 0);
                else continue; 

                if (dist.ContainsKey(res))
                {
                    dist[res]++;
                }
                else
                {
                    dist.Add(res, 1);
                }
            }
        }

        // It generate a T-student distribution with 1 degree of freedom 
        private void button6_Click(object sender, EventArgs e)
        {
            dist.Clear();

            double x, y, res;
            for (int i = 0; i < repeat; i++)
            {
                x = Math.Round((Double)generateGaussian(0, 1), 1);
                y = Math.Round((Double)generateGaussian(0, 1), 1);
                if (y != 0) res = Math.Round((double) Math.Pow(x,2) / y, 0); 
                else continue;

                if (dist.ContainsKey(res))
                {
                    dist[res]++;
                }
                else
                {
                    dist.Add(res, 1);
                }
            }
        }

        // It generates a Gaussian random variable starting from two uniform random variables
        double generateGaussian(double mean, double sigma)
        {
            double u, v, S;

            do
            {
                u = 2.0 * r.NextDouble() - 1.0; // uniform distribution over [-1 1]
                v = 2.0 * r.NextDouble() - 1.0; // uniform distribution over [-1 1]
                S = u * u + v * v;
            }
            while (S >= 1.0 || S == 0);

            double fac = Math.Sqrt(-2.0 * Math.Log(S) / S);
            double z1 = u * fac; 

            return (sigma * z1) + mean;
        }


        private int fromXRealToXVirtual(double x, double minX, double maxX, int left, int w)
        {
            return left + (int)(w * (x - minX) / (maxX - minX));
        }

        private int fromYRealToYVirtual(double y, double minY, double maxY, int top, int h)
        {
            return top + (int)(h - h * (y - minY) / (maxY - minY));
        }

        private void redraw()
        {

            g.Clear(BackColor);
            generateHistogram(virtualWindow, dist, g, interval, vertical);
            g.DrawRectangle(Pens.DarkSlateGray, virtualWindow);

            pictureBox1.Image = b;
        }
    }
}