using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Logx
{
    class LogxMainForm : Form
    {
        private Bitmap field;
        private int screenWidth = 512;
        private int screenHeight = 512;
        private bool running = true;
        private MouseState currentMS;
        private MouseState prevMS;
        private int selectedGate = -1;
        private int pickedGate = -1;
        private Point pickedOffset = Point.Empty;
        private GateTie wiringAnchor = null;
        private bool ShiftHeld = false, CtrlHeld = false;
        private bool DrawLogic = true;

        private Pen YellowPen = new Pen(new SolidBrush(Color.Yellow));
        private Pen BlackPen = new Pen(new SolidBrush(Color.Black));

        class GateTie
        {
            public Gate gateptr;
            public int inputNum;

            public static GateTie Empty = new GateTie();
        }

        private List<Gate> gateList = new List<Gate>();

        private Font debugFont = new Font("Courier", 12.0f);

        public LogxMainForm()
        {
            MouseState.Setup();
            prevMS = MouseState.GetMouseState();
            Gate.LoadImages("Content\\TileSheet.png");
            SetupField();
            this.ClientSize = field.Size;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.SetStyle(ControlStyles.Opaque, true);
            this.Paint += new PaintEventHandler(LogxMainForm_Paint);
            this.MouseMove += new MouseEventHandler(LogxMainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(LogxMainForm_MouseUp);
            this.MouseDown += new MouseEventHandler(LogxMainForm_MouseDown);
            this.KeyDown += new KeyEventHandler(LogxMainForm_KeyDown);
            this.KeyUp += new KeyEventHandler(LogxMainForm_KeyUp);
            this.Show();
            Run();
        }

        void LogxMainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.ShiftKey)
            {
                ShiftHeld = false;
            }
            if (e.KeyData == (Keys.LButton | Keys.ShiftKey))
            {
                CtrlHeld = false;
            }
        }

        void LogxMainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                switch (e.KeyCode - Keys.D0)
                {
                    case 0:
                        gateList.Add(new AndGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 1:
                        gateList.Add(new OrGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 2:
                        gateList.Add(new OnGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 3:
                        gateList.Add(new OffGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 4:
                        gateList.Add(new ButtonGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 5:
                        gateList.Add(new XorGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 6:
                        gateList.Add(new BulbGate(screenWidth / 2, screenHeight / 2));
                        break;
                    case 7:
                        gateList.Add(new NotGate(screenWidth / 2, screenHeight / 2));
                        break;
                }
            }
            if (e.KeyCode == Keys.Space)
            {
                EvaluateCircuit();
            }
            if (e.KeyData == (Keys.ShiftKey | Keys.Shift))
            {
                ShiftHeld = true;
            }
            if (e.KeyData == (Keys.LButton | Keys.ShiftKey | Keys.Control))
            {
                CtrlHeld = true;
            }
            if (e.KeyCode == Keys.H)
            {
                DrawLogic = !DrawLogic;
            }

            if (CtrlHeld)
            {
                if ((int)e.KeyCode == 79)
                {
                    OpenFileDialog odialog = new OpenFileDialog();
                    odialog.Filter = "Logx Maps (*.lmp)|*.lmp";
                    if (odialog.ShowDialog() == DialogResult.OK)
                    {
                        FileStream fStream = new FileStream(odialog.FileName, FileMode.OpenOrCreate);
                        BinaryReader reader = new BinaryReader(fStream);
                        int gateCount = reader.ReadInt32();
                        gateList.Clear();
                        for (int i = 0; i < gateCount; i++)
                        {
                            int lx = reader.ReadInt32();
                            int ly = reader.ReadInt32();
                            int rcode = reader.ReadInt32();
                            Gate gate = null;
                            switch (rcode)
                            {
                                case 0:
                                    gate = new AndGate(lx, ly);
                                    break;
                                case 1:
                                    gate = new OrGate(lx, ly);
                                    break;
                                case 2:
                                    gate = new OnGate(lx, ly);
                                    break;
                                case 3:
                                case 4:
                                    gate = new ButtonGate(lx, ly);
                                    break;
                                case 5:
                                    gate = new XorGate(lx, ly);
                                    break;
                                case 6:
                                    gate = new OffGate(lx, ly);
                                    break;
                                case 7:
                                case 8:
                                    gate = new BulbGate(lx, ly);
                                    break;
                                case 9:
                                    gate = new NotGate(lx, ly);
                                    break;
                            }
                            gateList.Add(gate);
                        }
                        while (fStream.Position < fStream.Length)
                        {
                            int fromGate = reader.ReadInt32();
                            int inputNum = reader.ReadInt32();
                            int toGate = reader.ReadInt32();

                            gateList[fromGate].inputs[inputNum] = gateList[toGate];
                        }
                        fStream.Close();
                        EvaluateCircuit();
                    }
                }
                if ((int)e.KeyCode == 83)
                {
                    SaveFileDialog sdialog = new SaveFileDialog();
                    sdialog.Filter = "Logx Maps (*.lmp)|*.lmp";
                    if (sdialog.ShowDialog() == DialogResult.OK)
                    {
                        FileStream fStream = new FileStream(sdialog.FileName, FileMode.OpenOrCreate);
                        BinaryWriter writer = new BinaryWriter(fStream);
                        writer.Write(gateList.Count);
                        for (int i = 0; i < gateList.Count; i++)
                        {
                            writer.Write(gateList[i].location.X);
                            writer.Write(gateList[i].location.Y);
                            writer.Write(gateList[i].renderCode);
                        }
                        for (int i = 0; i < gateList.Count; i++)
                        {
                            if (gateList[i].HasInput)
                            {
                                for (int j = 0; j < gateList[i].inputs.Length; j++)
                                {
                                    if (gateList[i].inputs[j] != null)
                                    {
                                        writer.Write(i);
                                        writer.Write(j);
                                        writer.Write(gateList.IndexOf(gateList[i].inputs[j]));
                                    }
                                }
                            }
                        }

                        fStream.Close();
                    }
                }
            }
        }

        private void EvaluateCircuit()
        {
            for (int i = 0; i < gateList.Count; i++)
                gateList[i].Evaluate();
        }

        void LogxMainForm_MouseDown(object sender, MouseEventArgs e)
        {
            MouseState.UpdateCMSDown(e);
        }

        void LogxMainForm_MouseUp(object sender, MouseEventArgs e)
        {
            MouseState.UpdateCMSUp(e);
        }

        void LogxMainForm_MouseMove(object sender, MouseEventArgs e)
        {
            MouseState.UpdateCMSPos(e.X, e.Y);
        }

        private void Run(double fps = 60.0)
        {
            DateTime startOfFrame;
            while (running)
            {
                startOfFrame = DateTime.Now;
                FrameUpdate();
                if ((DateTime.Now - startOfFrame).TotalMilliseconds < (1000 / fps))
                    Thread.Sleep((int)((1000 / fps) - (DateTime.Now - startOfFrame).TotalMilliseconds));
                FrameRender();
                Application.DoEvents();
                if (this.IsDisposed) running = false;
            }
        }

        private void FrameUpdate()
        {
            currentMS = MouseState.GetMouseState();

            if (currentMS.Buttons[MouseButtons.Left] && !prevMS.Buttons[MouseButtons.Left])
            {
                for (int i = 0; i < gateList.Count; i++)
                {
                    if (gateList[i].location.X < currentMS.X &&
                        gateList[i].location.X + 32 > currentMS.X &&
                        gateList[i].location.Y < currentMS.Y &&
                        gateList[i].location.Y + 32 > currentMS.Y)
                    {
                        if (ShiftHeld)
                        {
                            gateList.RemoveAt(i);
                        }
                        else if (CtrlHeld)
                        {
                            if (gateList[i] is ButtonGate)
                            {
                                ((ButtonGate)gateList[i]).Toggle();
                                EvaluateCircuit();
                            }
                        }
                        else
                        {
                            pickedOffset.X = currentMS.X - gateList[i].location.X;
                            pickedOffset.Y = currentMS.Y - gateList[i].location.Y;
                            pickedGate = i;
                            selectedGate = i;
                        }
                        break;
                    }
                    else
                        pickedGate = -1;
                }
            }
            if (!currentMS.Buttons[MouseButtons.Left] && prevMS.Buttons[MouseButtons.Left])
            {
                pickedGate = -1;
            }

            if (pickedGate != -1)
            {
                gateList[pickedGate].location.X = currentMS.X - pickedOffset.X;
                gateList[pickedGate].location.Y = currentMS.Y - pickedOffset.Y;
            }

            if (currentMS.Buttons[MouseButtons.Right] && !prevMS.Buttons[MouseButtons.Right])
            {
                for (int i = 0; i < gateList.Count; i++)
                {
                    if (currentMS.X > gateList[i].location.X + 5 &&
                        currentMS.X < gateList[i].location.X + 15) //Selected Input Block
                    {
                        if (gateList[i].HasInput)
                        {
                            if (currentMS.Y > gateList[i].location.Y &&
                                currentMS.Y < gateList[i].location.Y + 16) //Selected Top Input
                            {
                                if (wiringAnchor == null)
                                {
                                    wiringAnchor = new GateTie();
                                    wiringAnchor.inputNum = 0;
                                    wiringAnchor.gateptr = gateList[i];
                                }
                            }
                            else if (currentMS.Y > gateList[i].location.Y + 16 &&
                                currentMS.Y < gateList[i].location.Y + 32) //Selected Bottom Input
                            {
                                if (wiringAnchor == null)
                                {
                                    wiringAnchor = new GateTie();
                                    if (gateList[i].inputs.Length < 2) wiringAnchor.inputNum = 0;
                                    else wiringAnchor.inputNum = 1;
                                    wiringAnchor.gateptr = gateList[i];
                                }
                            }
                        }
                    }
                    if (currentMS.X >= gateList[i].location.X + 18 &&
                        currentMS.X <= gateList[i].location.X + 23 &&
                        currentMS.Y >= gateList[i].location.Y &&
                        currentMS.Y <= gateList[i].location.Y + 32) //Selected Output Block
                    {
                        if (wiringAnchor != null)
                        {
                            if (wiringAnchor.gateptr != gateList[i])
                            {
                                wiringAnchor.gateptr.inputs[wiringAnchor.inputNum] = gateList[i];
                                wiringAnchor = null;
                            }
                        }
                    }
                }
            }

            prevMS = MouseState.GetMouseState();
        }

        private void FrameRender()
        {
            FillScreen(Color.DarkGray);
            Graphics g = Graphics.FromImage(field);
            for (int i = 0; i < gateList.Count; i++)
            {
                if (i == selectedGate)
                    gateList[i].Render(g, true);
                else
                    gateList[i].Render(g);
                for (int j = 0; j < gateList[i].inputs.Length; j++)
                {
                    if (gateList[i].inputs[j] != null)
                    {
                        Point[] pts = new Point[4];
                        pts[0] = new Point(gateList[i].location.X + 5, gateList[i].location.Y + 16);
                        pts[1] = new Point(((gateList[i].inputs[j].location.X + 22) + (gateList[i].location.X + 5)) / 2, gateList[i].location.Y + 16);
                        pts[2] = new Point(((gateList[i].inputs[j].location.X + 22) + (gateList[i].location.X + 5)) / 2, gateList[i].inputs[j].location.Y + 16);
                        pts[3] = new Point(gateList[i].inputs[j].location.X + 22, gateList[i].inputs[j].location.Y + 16);
                        if (gateList[i].inputs.Length > 1)
                        {
                            int partLength = (8 / (gateList[i].inputs.Length + 1)) + 8;
                            pts[0].Y = (gateList[i].location.Y + partLength) + (partLength * j);
                            pts[1].Y = (gateList[i].location.Y + partLength) + (partLength * j);
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            if (gateList[i].inputs[j].Output())
                                g.DrawLine(YellowPen, pts[k], pts[k + 1]);
                            else
                                g.DrawLine(BlackPen, pts[k], pts[k + 1]);
                        }
                    }
                }
            }

            if (wiringAnchor != null)
            {
                if (wiringAnchor.gateptr.inputs.Length > 1)
                    g.DrawLine(YellowPen, new Point(wiringAnchor.gateptr.location.X + 5, wiringAnchor.gateptr.location.Y + 10 + (10 * wiringAnchor.inputNum)),
                        new Point(currentMS.X, currentMS.Y));
                else
                    g.DrawLine(YellowPen, new Point(wiringAnchor.gateptr.location.X + 5, wiringAnchor.gateptr.location.Y + 16),
                        new Point(currentMS.X, currentMS.Y));
            }

            g.DrawString("( " + currentMS.X + ", " + currentMS.Y + ")", debugFont, new SolidBrush(Color.Black), new PointF(0.0f, 490.0f));
            this.Invalidate();
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

        [STAThread]
        public static void Main(string[] args)
        {
            LogxMainForm x = new LogxMainForm();
        }
    }
}
