namespace MaterialSkin.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    public class MaterialCard : Panel, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public MaterialCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Paint += new PaintEventHandler(paintControl);
            BackColor = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? SkinManager.ActiveColorRoles.SurfaceContainer : SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            Margin = new Padding(SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? 12 : SkinManager.FORM_PADDING);
            Padding = new Padding(SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? 16 : SkinManager.FORM_PADDING);
        }

        private void drawShadowOnParent(object sender, PaintEventArgs e)
        {
            if (Parent == null)
            {
                RemoveShadowPaintEvent((Control)sender, drawShadowOnParent);
                return;
            }

            if (SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3)
            {
                return;
            }

            // paint shadow on parent
            Graphics gp = e.Graphics;
            Rectangle rect = new Rectangle(Location, ClientRectangle.Size);
            gp.SmoothingMode = SmoothingMode.AntiAlias;
            DrawHelper.DrawSquareShadow(gp, rect);
        }

        protected override void InitLayout()
        {
            LocationChanged += (sender, e) => { Parent?.Invalidate(); };
            ForeColor = SkinManager.TextHighEmphasisColor;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null) AddShadowPaintEvent(Parent, drawShadowOnParent);
            if (_oldParent != null) RemoveShadowPaintEvent(_oldParent, drawShadowOnParent);
            _oldParent = Parent;
        }

        private Control _oldParent;

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Parent == null) return;
            if (Visible)
                AddShadowPaintEvent(Parent, drawShadowOnParent);
            else
                RemoveShadowPaintEvent(Parent, drawShadowOnParent);
        }

        private bool _shadowDrawEventSubscribed = false;

        private void AddShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (_shadowDrawEventSubscribed) return;
            control.Paint += shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = true;
        }

        private void RemoveShadowPaintEvent(Control control, PaintEventHandler shadowPaintEvent)
        {
            if (!_shadowDrawEventSubscribed) return;
            control.Paint -= shadowPaintEvent;
            control.Invalidate();
            _shadowDrawEventSubscribed = false;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            BackColor = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? SkinManager.ActiveColorRoles.SurfaceContainer : SkinManager.BackgroundColor;
        }

        private void paintControl(Object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(Parent.BackColor);

            // card rectangle path
            RectangleF cardRectF = new RectangleF(ClientRectangle.Location, ClientRectangle.Size);
            cardRectF.X -= 0.5f;
            cardRectF.Y -= 0.5f;
            int radius = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? 16 : 4;
            using (GraphicsPath cardPath = DrawHelper.CreateRoundRect(cardRectF, radius))
            {
                if (SkinManager.DesignVersion != MaterialSkinManager.MaterialDesignVersion.Material3)
                {
                    DrawHelper.DrawSquareShadow(g, ClientRectangle);
                }

                // Draw card
                using (SolidBrush normalBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(normalBrush, cardPath);
                }

                if (SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3)
                {
                    using (Pen outlinePen = new Pen(SkinManager.ActiveColorRoles.OutlineVariant, 1))
                    {
                        g.DrawPath(outlinePen, cardPath);
                    }
                }
            }
        }
    }
}
