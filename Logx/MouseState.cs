using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Logx
{
    class MouseState
    {
        public int X = 0;
        public int Y = 0;
        public Dictionary<MouseButtons, bool> Buttons = new Dictionary<MouseButtons, bool>();

        private static MouseState currentMouseState = new MouseState();

        public static MouseState GetMouseState()
        {
            MouseState m = new MouseState();
            m.X = currentMouseState.X;
            m.Y = currentMouseState.Y;
            m.Buttons = new Dictionary<MouseButtons, bool>();
            m.Buttons.Add(MouseButtons.Left, currentMouseState.Buttons[MouseButtons.Left]);
            m.Buttons.Add(MouseButtons.Right, currentMouseState.Buttons[MouseButtons.Right]);
            m.Buttons.Add(MouseButtons.Middle, currentMouseState.Buttons[MouseButtons.Middle]);
            return m;
        }

        public static void UpdateCMSUp(MouseEventArgs m)
        {
            currentMouseState.X = m.X;
            currentMouseState.Y = m.Y;
            currentMouseState.Buttons[m.Button] = false;
        }

        public static void UpdateCMSDown(MouseEventArgs m)
        {
            currentMouseState.X = m.X;
            currentMouseState.Y = m.Y;
            currentMouseState.Buttons[m.Button] = true;
        }

        public static void UpdateCMSPos(int x, int y)
        {
            currentMouseState.X = x;
            currentMouseState.Y = y;
        }

        public static void Setup()
        {
            currentMouseState = new MouseState();
            currentMouseState.X = 0;
            currentMouseState.Y = 0;
            currentMouseState.Buttons = new Dictionary<MouseButtons, bool>();
            currentMouseState.Buttons.Add(MouseButtons.Left, false);
            currentMouseState.Buttons.Add(MouseButtons.Right, false);
            currentMouseState.Buttons.Add(MouseButtons.Middle, false);
        }
    }
}
