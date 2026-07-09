namespace MaterialSkin.Controls
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public sealed class MaterialDivider : Control, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public MaterialDivider()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 1;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color dividerColor = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3
                ? SkinManager.ActiveColorRoles.OutlineVariant
                : SkinManager.DividersColor;
            using (var brush = new SolidBrush(dividerColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, ClientRectangle.Width, 1);
            }
        }
    }
}
