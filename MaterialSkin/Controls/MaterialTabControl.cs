namespace MaterialSkin.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class MaterialTabControl : TabControl, IMaterialControl
    {
        public MaterialTabControl()
        {
            Multiline = false;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(0, 1);
            Padding = new Point(0, 0);
        }

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode) m.Result = (IntPtr)1;
            else base.WndProc(ref m);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!DesignMode)
            {
                Multiline = false;
                SizeMode = TabSizeMode.Fixed;
                ItemSize = new Size(0, 1);
                Padding = new Point(0, 0);
            }
        }
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control is TabPage tabPage)
            {
                tabPage.BackColor = SkinManager.BackgroundColor;
                tabPage.ForeColor = SkinManager.TextHighEmphasisColor;
            }
        }
    }
}
