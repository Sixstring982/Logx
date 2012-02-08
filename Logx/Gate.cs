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
        public int renderCode = 0;
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
        private bool On = false;
        public ButtonGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null, null };
            HasInput = false;
            renderCode = 4;
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

    class BulbGate : Gate
    {
        public BulbGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null };
            renderCode = 7;
        }

        public override bool Output()
        {
            if (inputs[0] != null)
            {
                if (inputs[0].Output()) renderCode = 8;
                else renderCode = 7;
                return inputs[0].Output();
            }
            return false;
        }
    }

    class NotGate : Gate
    {
        public NotGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null };
            renderCode = 9;
        }

        public override bool Output()
        {
            if (inputs[0] != null) return !inputs[0].Output();
            else return false;
        }
    }

    class LinkGate : Gate
    {
        public LinkGate(int x, int y)
            : base(x, y)
        {
            inputs = new Gate[] { null };
            renderCode = 10;
        }

        public override bool Output()
        {
            if (inputs[0] != null) return inputs[0].Output();
            else return false;
        }
    }
}
