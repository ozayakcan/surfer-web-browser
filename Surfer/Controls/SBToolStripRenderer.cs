﻿using System.Drawing;
using System.Windows.Forms;

namespace Surfer.Controls
{
    public class SBToolStripRenderer : ToolStripSystemRenderer
    {
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.GetType() == typeof(MyIconDropdownButton))
            {
                MyIconDropdownButton sbIconButton = (MyIconDropdownButton)e.Item;
                if (sbIconButton.VisualDisabled)
                {
                    DrawTransparent(e);
                    return;
                }
            }
            if (e.Item.GetType() == typeof(MyIconSplitButton))
            {
                MyIconSplitButton sbIconButton = (MyIconSplitButton)e.Item;
                if (sbIconButton.VisualDisabled)
                {
                    DrawTransparent(e);
                    return;
                }
            }
            if (e.Item.GetType() == typeof(MyIconToolStripButton))
            {
                MyIconToolStripButton sbIconButton = (MyIconToolStripButton)e.Item;
                if (sbIconButton.VisualDisabled)
                {
                    DrawTransparent(e);
                    return;
                }
            }
            if (!e.Item.Selected)
            {
                base.OnRenderButtonBackground(e);
                return;
            }
            else
            {
                DrawHover(e);
                return;
            }
        }
        private void DrawTransparent(ToolStripItemRenderEventArgs e)
        {
            Rectangle rectangle = new Rectangle(0, 0, e.Item.Size.Width - 1, e.Item.Size.Height - 1);
            e.Graphics.FillRectangle(Brushes.Transparent, rectangle);
            e.Graphics.DrawRectangle(Pens.Transparent, rectangle);
        }
        private void DrawHover(ToolStripItemRenderEventArgs e)
        {
            Rectangle rectangle = new Rectangle(0, 0, e.Item.Size.Width - 1, e.Item.Size.Height - 1);
            SolidBrush hoverBrush = new SolidBrush(Theme.Get.ColorButtonHover);
            e.Graphics.FillRectangle(hoverBrush, rectangle);
            e.Graphics.DrawRectangle(new Pen(hoverBrush), rectangle);
        }
    }
}
