using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BejeweledSolver
{
    public partial class Form1 : Form
    {
        const int SQUARE = 52;
        const int BOARDSIZE = 8;
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);   
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        enum Color { Orange, Red, White, Blue, Yellow, Green, Purple, Null }
        Dictionary<uint, Color> colors;

        Color[,] board = new Color[8, 8];
        int fault;

        const int TOPLEFT_X = 531, TOPLEFT_Y = 170;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                fault = 0;
                timer1.Enabled = !timer1.Enabled;
                //Screencap();
            }
            base.WndProc(ref m);
        }

        bool SimilarColor(uint color1, uint color2)
        {
            int t1 = (int)color1 >> 24,
                t2 = (int)color2 >> 24,
                t = t1 - t2;
            if (Math.Abs(t) > 25)     // 10% of 256 
                return false;
            t1 = (int)(color1 & 0x00ffffff) >> 16;
            t2 = (int)(color2 & 0x00ffffff) >> 16;
            t = t1 - t2;
            if (Math.Abs(t1 - t2) > 25)
                return false;
            t1 = (int)(color1 & 0x0000ffff) >> 8;
            t2 = (int)(color2 & 0x0000ffff) >> 8;
            t = t1 - t2;
            if (Math.Abs(t1 - t2) > 25)
                return false;
            t1 = (int)(color1 & 0x000000ff);
            t2 = (int)(color2 & 0x000000ff);
            t = t1 - t2;
            if (Math.Abs(t1 - t2) > 25)
                return false;
            return true;
        }

        bool SimiarColor(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            if (Math.Abs(r1 - r2) > 25)
                return false;
            if (Math.Abs(b1 - b2) > 25)
                return false;
            if (Math.Abs(g1 - g2) > 25)
                return false;
            return true;
        }

        private bool IsBackground(System.Drawing.Color color)
        {
            if (SimilarColor((uint)color.ToArgb(), 0xff313129) ||
                SimilarColor((uint)color.ToArgb(), 0xff4c4c36) ||
                SimilarColor((uint)color.ToArgb(), 0xff3e3e2f) ||
                SimilarColor((uint)color.ToArgb(), 0xff37372c) ||
                SimilarColor((uint)color.ToArgb(), 0xffffffff))
                return true;
            return false;
        }

        bool IsBackgroundOrBlack(byte r, byte g, byte b)
        {
            return (SimiarColor(r, g, b, 0x31, 0x31, 0x29) ||
                    SimiarColor(r, g, b, 0x4c, 0x4c, 0x36) ||
                    SimiarColor(r, g, b, 0x3e, 0x3e, 0x3f) ||
                    SimiarColor(r, g, b, 0x37, 0x37, 0x3c) ||
                    SimiarColor(r, g, b, 0xff, 0xff, 0xff));
        }

        private bool Screencap()
        {
            Size boardDimension = new Size(419, 419);
            Bitmap img = new Bitmap(boardDimension.Width, boardDimension.Height);
            Graphics screencap = Graphics.FromImage(img);
            screencap.CopyFromScreen(new Point(TOPLEFT_X, TOPLEFT_Y), new Point(0, 0), boardDimension);
            screencap.Dispose();
            //Bitmap img = new Bitmap("123.bmp");
            //for (int y = 0; y < img.Height; ++y )
            //    for (int x = 0; x < img.Width; ++x )
            //        if (IsBackground(img.GetPixel(x, y)))
            //            img.SetPixel(x, y, System.Drawing.Color.Black);
            //        else
            //            img.SetPixel(x, y, System.Drawing.Color.White);
            const int newSq = 3, newW = BOARDSIZE * newSq, newH = BOARDSIZE * newSq;
            Bitmap newBM = new Bitmap(newW, newH);
            screencap = Graphics.FromImage(newBM);
            //screencap.CopyFromScreen(new Point(531, 170), new Point(0, 0), boardDimension);
            screencap.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            screencap.DrawImage(img, 0, 0, newW, newH);
            screencap.Dispose();
            
            //screencap.DrawImage(img, 0, 0, 420, 420);
            //img.Save("123.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            label1.Text = "Colors: " + Environment.NewLine;

            for (int y = newSq-1; y < newH; y+= newSq )
            {
                for (int x = newSq-1; x < newW; x+= newSq )
                {
                    uint Z = (uint)newBM.GetPixel(x, y).ToArgb();
                    bool found = false;
                    foreach (KeyValuePair<uint, Color> pair in colors)
                    {
                        if (SimilarColor(Z, pair.Key))
                        {
                            board[x / newSq, y / newSq] = pair.Value;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return false;
                }
                label1.Text += Environment.NewLine;
                pictureBox1.Image = img;
            }
            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x)
                    label1.Text += board[x, y].ToString() + " ";
                label1.Text += Environment.NewLine;
            }
            return true;
        }

        private bool IsBlack(System.Drawing.Color color)
        {
            if (color.A != 0xff)
                return false;
            if (color.R != 0x00)
                return false;
            if (color.B != 0x00)
                return false;
            if (color.G != 0x00)
                return false;
            return true;
        }

        public Form1()
        {
            InitializeComponent();
            colors = new Dictionary<uint, Color>();
            fault = 0;

            colors.Add(0xffff9226, Color.Orange);
            colors.Add(0xffffe809, Color.Yellow);
            colors.Add(0xffef3110, Color.Red);
            colors.Add(0xffffffff, Color.White);
            colors.Add(0xffaddea5, Color.Green);
            colors.Add(0xff1cd0f5, Color.Blue);
            colors.Add(0xffeab5dd, Color.Purple);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool hooked = RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 3, (int)'C');
            if (!hooked)
            {
                MessageBox.Show("Can't register hotkey, program exits!");
                this.Close();
            }
            button1.Text = "Unload!";
            button1.Click += new EventHandler(Unload);
        }

        void Unload(object sender, EventArgs e)
        {
            if (timer1.Enabled)
                timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (Screencap())
            {
                ExamineBoard();
                fault = 0;
            }
            else
            {
                ++fault;
                if (fault > 150)
                {
                    MessageBox.Show("Program can't continue. Please take screenshot and report!");
                    this.Close();
                }
            }
            timer1.Enabled = true;
        }

        private void ExamineBoard()
        {
            bool debug = false;
            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                {
                    if (TrySwap(x, y, -1, 0))
                    {
                        if (debug)
                            MessageBox.Show(x.ToString("X") + y.ToString("X") + Environment.NewLine + "-1" + "0");
                        Swap(x, y, -1, 0);
                        return;
                    }
                    else if (TrySwap(x, y, 0, -1))
                    {
                        if (debug)
                            MessageBox.Show(x.ToString("X") + y.ToString("Y") + Environment.NewLine + "0" + "-1");
                        Swap(x, y, 0, -1);
                        return;
                    }
                    else if (TrySwap(x, y, 1, 0))
                    {
                        if (debug)
                            MessageBox.Show(x.ToString("X") + y.ToString("X") + Environment.NewLine + "1" + "0");
                        Swap(x, y, 1, 0);
                        return;
                    }
                    else if (TrySwap(x, y, 0, 1))
                    {
                        if (debug)
                            MessageBox.Show(x.ToString("X") + y.ToString("X") + Environment.NewLine + "0" + "1");
                        Swap(x, y, 0, 1);
                        return;
                    }
                }
        }

        private void Swap(int x, int y, int x_delta, int y_delta)
        {
            int X1 = TOPLEFT_X + x * 52 + 26,
                Y1 = TOPLEFT_Y + y * 52 + 26,
                X2 = TOPLEFT_X + (x + x_delta) * 52 + 26,
                Y2 = TOPLEFT_Y + (y + y_delta) * 52 + 26;
            SetCursorPos(X1, Y1);
            System.Threading.Thread.Sleep(20);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            SetCursorPos(X2, Y2);
            System.Threading.Thread.Sleep(20);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            SetCursorPos(TOPLEFT_X + 500, TOPLEFT_Y + 500);
        }

        private bool TrySwap(int x, int y, int x_delta, int y_delta)
        {
            try
            {
                Color temp = board[x, y];
                board[x, y] = board[x + x_delta, y + y_delta];
                board[x + x_delta, y + y_delta] = temp;
                bool retVal = Check(x + x_delta, y + y_delta);
                board[x + x_delta, y + y_delta] = board[x, y];
                board[x, y] = temp;
                return retVal;
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }
        }

        private bool Check(int x, int y)
        {
            try
            {
                if (((x - 2 > 0) && (CheckLineX(x - 2, y))) ||
                    ((x - 1 > 0) && (x + 1 < 8) && (CheckLineX(x - 1, y))) ||
                    (CheckLineX(x, y)) ||
                    ((y - 2 > 0) && (CheckLineY(x, y - 2))) ||
                    ((y - 1 > 0) && (y + 1 < 8) && (CheckLineY(x, y - 1))) ||
                    (CheckLineY(x, y)))
                    return true;
                else
                    return false;
                    
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }
        }

        private bool CheckLineY(int x, int y)
        {
            if ((board[x, y] == board[x, y + 1]) && (board[x, y] == board[x, y + 2]))
                return true;
            else
                return false;
        }

        private bool CheckLineX(int x, int y)
        {
            if ((board[x,y] == board[x+1,y]) && (board[x,y] == board[x+2,y]))
                return true;
            else
                return false;
        }
    }

    public class CannotRecognizeBoard : Exception
    {
        public CannotRecognizeBoard(string s)
            : base(s)
        {
        }

        public CannotRecognizeBoard()
            : base()
        {
        }
    }
}
