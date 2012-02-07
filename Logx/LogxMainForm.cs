using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Logx
{
    class LogxMainForm : Form
    {
        private Bitmap field;
        private MenuStrip menuStrip;
        private int screenWidth = 512;
        private int screenHeight = 512;
        private bool running = true;

        public LogxMainForm()
        {
            SetupField();
            this.Controls.Add(menuStrip);
            this.ClientSize = field.Size;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.SetStyle(ControlStyles.Opaque, true);
            this.Paint += new PaintEventHandler(LogxMainForm_Paint);
            this.Show();
            Run();
        }

        private void Run(double fps = 60.0)
        {
            DateTime startOfFrame;
            while (running)
            {
                startOfFrame = DateTime.Now;
                Update();
                if ((DateTime.Now - startOfFrame).TotalMilliseconds < (1000 / fps))
                    Thread.Sleep((int)((1000 / fps) - (DateTime.Now - startOfFrame).TotalMilliseconds));
                Render();
                Application.DoEvents();
                if (this.IsDisposed) running = false;
            }
        }

        private void Update()
        {

        }

        private void Render()
        {
            FillScreen(Color.BlanchedAlmond);
        }

        private void FillScreen(Color c)
        {
            BitmapData bmpdata = field.LockBits(new Rectangle(new Point(0), field.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* start = (byte*)bmpdata.Scan0;
                int times = field.Width * field.Height * 4;
                for (int i = 0; i < times; i += 4)
                {
                    *(start + i + 3) = c.A;
                    *(start + i + 2) = c.R;
                    *(start + i + 1) = c.G;
                    *(start + i) = c.B;
                }
            }

            field.UnlockBits(bmpdata);
        }

        private void SetupField()
        {
            field = new Bitmap(screenWidth, screenHeight);
            FillScreen(Color.White);
        }

        private void LogxMainForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(field, new Rectangle(new Point(0), field.Size), new Rectangle(new Point(0), field.Size), GraphicsUnit.Pixel);
        }

        public static void Main(string[] args)
        {
            LogxMainForm x = new LogxMainForm();
        }
    }
}
