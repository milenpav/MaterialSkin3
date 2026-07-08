namespace MaterialSkin.Controls
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class MaterialProgressBar : ProgressBar, IMaterialControl
    {
        public MaterialProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, 5, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
            var doneProgress = (int)(Width * ((double)Value / Maximum));
            using (var progressBrush = new SolidBrush(Enabled
                ? (isM3 ? SkinManager.ActiveColorRoles.Primary : SkinManager.ColorScheme.PrimaryColor)
                : DrawHelper.BlendColor(
                    isM3 ? SkinManager.ActiveColorRoles.Primary : SkinManager.ColorScheme.PrimaryColor,
                    SkinManager.SwitchOffDisabledThumbColor,
                    197)))
            using (var trackBrush = new SolidBrush(isM3
                ? SkinManager.ActiveColorRoles.SurfaceContainerHigh
                : SkinManager.BackgroundFocusColor))
            {
                e.Graphics.FillRectangle(progressBrush, 0, 0, doneProgress, Height);
                e.Graphics.FillRectangle(trackBrush, doneProgress, 0, Width - doneProgress, Height);
            }
        }
    }
}
