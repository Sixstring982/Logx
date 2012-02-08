using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Logx
{
    abstract class Gate
    {
        public Point location;
        public Gate[] inputs;
        public abstract bool Output();
        protected int renderCode = 0;
        public bool HasInput = true;
        private bool EvaluatedOutput = false;

        public static Bitmap[] images;

        public void Evaluate()
        {
            EvaluatedOutput = Output();
        }

        public Gate(int x, int y)
        {
            location.X = x;
            location.Y = y;
        }

        public void Render(Graphics g, bool active = false)
        {
            g.DrawImage(images[renderCode], location.X, location.Y);
            if (EvaluatedOutput)
                g.FillRectangle(new SolidBrush(Color.Green), new Rectangle(location.X, location.Y, 10, 10));
            if (active) g.DrawEllipse(new Pen(new SolidBrush(Color.Red)), new Rectangle(location.X + 13, location.Y + 13, 6, 6));
        }

        public static void LoadImages(string filename)
        {
            Bitmap tileSheet = new Bitmap(filename);
            images = new Bitmap[256];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    images[y * 16 + x] = new Bitmap(32, 32);
                    Graphics g = Graphics.FromImage(images[y * 16 + x]);
                    g.DrawImage(tileSheet, new Rectangle(0, 0, 32, 32), new Rectangle(x * 32, y * 32, 32, 32), GraphicsUnit.Pixel);
                }
            }
        }
    }

    class AndGate : Gate
    {
        public AndGate(int x, int y) : base(x, y)
        {
            inputs = new Gate[] { null, null };
            renderCode = 0;
        }

        public override bool Output()
        {
            if(inputs[0] != null && inputs[1] != null)
                return inputs[0].Output() && inputs[1].Output();
            return false;
        }
    }

    class OrGate : Gate
    {
        public OrGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null, null };
            renderCode = 1;
        }

        public override bool Output()
        {
            if (inputs[0] != null && inputs[1] != null)
                return inputs[0].Output() || inputs[1].Output();
            return false;
        }
    }

    class XorGate : Gate
    {
        public XorGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null, null };
            renderCode = 5;
        }

        public override bool Output()
        {
            if (inputs[0] != null && inputs[1] != null)
                return inputs[0].Output() ^ inputs[1].Output();
            return false;
        }
    }

    class OnGate : Gate
    {
        public OnGate(int x, int y) : base(x, y)
        {
            inputs = new Gate[] { null, null };
            HasInput = false;
            renderCode = 2;
        }

        public override bool Output()
        {
            return true;
        }
    }

    class OffGate : Gate
    {
        public OffGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null, null };
            HasInput = false;
            renderCode = 6;
        }

        public override bool Output()
        {
            return false;
        }
    }

    class ButtonGate : Gate
    {
        private bool On = true;
        public ButtonGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null, null };
            HasInput = false;
            renderCode = 3;
        }

        public void Toggle()
        {
            On = !On;
            if (renderCode == 3) renderCode = 4;
            else renderCode = 3;
        }

        public override bool Output()
        {
            return On;
        }
    }
}
