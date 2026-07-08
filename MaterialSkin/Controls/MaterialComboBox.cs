namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Data;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;

    public class MaterialComboBox : ComboBox, IMaterialControl
    {
        // For some reason, even when overriding the AutoSize property, it doesn't appear on the properties panel, so we have to create a new one.
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Layout")]
        private bool _AutoResize;

        public bool AutoResize
        {
            get { return _AutoResize; }
            set
            {
                _AutoResize = value;
                recalculateAutoSize();
            }
        }

        //Properties for managing the material design properties
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        private bool _UseTallSize;

        [Category("Material Skin"), DefaultValue(true), Description("Using a larger size enables the hint to always be visible")]
        public bool UseTallSize
        {
            get { return _UseTallSize; }
            set
            {
                _UseTallSize = value;
                setHeightVars();
                Invalidate();
            }
        }

        [Category("Material Skin"), DefaultValue(true)]
        public bool UseAccent { get; set; }

        private string _hint = string.Empty;

        [Category("Material Skin"), DefaultValue(""), Localizable(true)]
        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                hasHint = !String.IsNullOrEmpty(Hint);
                Invalidate();
            }
        }

        private int _startIndex;
        public int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
                try
                {
                    if (base.Items.Count > 0)
                    {
                        base.SelectedIndex = value;
                    }
                }
                catch
                {
                }
                Invalidate();
            }
        }

        private const int TEXT_SMALL_SIZE = 18;
        private const int TEXT_SMALL_Y = 4;
        private const int BOTTOM_PADDING = 3;
        private int HEIGHT = 50;
        private int LINE_Y;

        private bool hasHint;

        private readonly AnimationManager _animationManager;

        public MaterialComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            // Material Properties
            Hint = "";
            UseAccent = true;
            UseTallSize = true;
            MaxDropDownItems = 4;

            Font = SkinManager.getFontByType(MaterialSkinManager.fontType.Subtitle2);
            BackColor = SkinManager.BackgroundColor;
            ForeColor = SkinManager.TextHighEmphasisColor;
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DropDownWidth = Width;

            // Animations
            _animationManager = new AnimationManager(true)
            {
                Increment = 0.08,
                AnimationType = AnimationType.EaseInOut
            };
            _animationManager.OnAnimationProgress += sender => Invalidate();
            _animationManager.OnAnimationFinished += sender => _animationManager.SetProgress(0);
            DropDownClosed += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0 && !Focused) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };
            LostFocus += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                if (SelectedIndex < 0) _animationManager.StartNewAnimation(AnimationDirection.Out);
            };
            DropDown += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
            };
            GotFocus += (sender, args) =>
            {
                _animationManager.StartNewAnimation(AnimationDirection.In);
                Invalidate();
            };
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
                Invalidate();
            };
            MouseLeave += (sender, args) =>
            {
                MouseState = MouseState.OUT;
                Invalidate();
            };
            SelectedIndexChanged += (sender, args) =>
            {
                Invalidate();
            };
            KeyUp += (sender, args) =>
            { 
                if (Enabled && DropDownStyle == ComboBoxStyle.DropDownList && (args.KeyCode == Keys.Delete || args.KeyCode == Keys.Back))
                {
                    SelectedIndex = -1;
                    Invalidate();
                }
            };
        }

    

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;

            g.Clear(Parent.BackColor);
            using (GraphicsPath roundedRectPath = DrawHelper.CreateRoundRect(
                ClientRectangle.X,
                ClientRectangle.Y,
                ClientRectangle.Width,
                LINE_Y,
                isM3 ? 12 : 4))
            using (SolidBrush fillBrush = new SolidBrush(!Enabled
                ? SkinManager.BackgroundDisabledColor
                : Focused
                    ? SkinManager.BackgroundFocusColor
                    : MouseState == MouseState.HOVER
                        ? SkinManager.BackgroundHoverColor
                        : SkinManager.BackgroundAlternativeColor))
            {
                g.FillPath(fillBrush, roundedRectPath);

                if (isM3)
                {
                    using (Pen outlinePen = new Pen(Focused || DroppedDown ? (UseAccent ? SkinManager.ActiveColorRoles.Secondary : SkinManager.ActiveColorRoles.Primary) : SkinManager.ActiveColorRoles.OutlineVariant, Focused || DroppedDown ? 2 : 1))
                    {
                        g.DrawPath(outlinePen, roundedRectPath);
                    }
                }
            }

            //Set color and brush
            Color SelectedColor = new Color();
            if (UseAccent)
                SelectedColor = isM3 ? SkinManager.ActiveColorRoles.Secondary : SkinManager.ColorScheme.AccentColor;
            else
                SelectedColor = isM3 ? SkinManager.ActiveColorRoles.Primary : SkinManager.ColorScheme.PrimaryColor;
            using (SolidBrush SelectedBrush = new SolidBrush(SelectedColor))
            {

                // Create and Draw the arrow
                using (System.Drawing.Drawing2D.GraphicsPath pth = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    PointF TopRight = new PointF(this.Width - 0.5f - SkinManager.FORM_PADDING, (this.Height >> 1) - 2.5f);
                    PointF MidBottom = new PointF(this.Width - 4.5f - SkinManager.FORM_PADDING, (this.Height >> 1) + 2.5f);
                    PointF TopLeft = new PointF(this.Width - 8.5f - SkinManager.FORM_PADDING, (this.Height >> 1) - 2.5f);
                    pth.AddLine(TopLeft, TopRight);
                    pth.AddLine(TopRight, MidBottom);

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (SolidBrush arrowBrush = Enabled ? new SolidBrush(DroppedDown || Focused ? SelectedColor : (isM3 ? SkinManager.ActiveColorRoles.OnSurfaceVariant : SkinManager.TextHighEmphasisColor)) : new SolidBrush(DrawHelper.BlendColor(SkinManager.TextHighEmphasisColor, SkinManager.SwitchOffDisabledThumbColor, 197)))
                    {
                        g.FillPath(arrowBrush, pth);
                    }
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                }

                // HintText
                bool userTextPresent = SelectedIndex >= 0;
                Rectangle hintRect = new Rectangle(SkinManager.FORM_PADDING, ClientRectangle.Y, Width, LINE_Y);
                int hintTextSize = 16;

                // bottom line base
                if (!isM3)
                {
                    g.FillRectangle(SkinManager.DividersAlternativeBrush, 0, LINE_Y, Width, 1);
                }

                if (!_animationManager.IsAnimating())
                {
                    // No animation
                    if (hasHint && UseTallSize && (DroppedDown || Focused || SelectedIndex >= 0))
                    {
                        // hint text
                        hintRect = new Rectangle(SkinManager.FORM_PADDING, TEXT_SMALL_Y, Width, TEXT_SMALL_SIZE);
                        hintTextSize = 12;
                    }

                    // bottom line
                    if (!isM3 && (DroppedDown || Focused))
                    {
                        g.FillRectangle(SelectedBrush, 0, LINE_Y, Width, 2);
                    }
                }
                else
                {
                    // Animate - Focus got/lost
                    double animationProgress = _animationManager.GetProgress();

                    // hint Animation
                    if (hasHint && UseTallSize)
                    {
                        hintRect = new Rectangle(
                            SkinManager.FORM_PADDING,
                            userTextPresent && !_animationManager.IsAnimating() ? (TEXT_SMALL_Y) : ClientRectangle.Y + (int)((TEXT_SMALL_Y - ClientRectangle.Y) * animationProgress),
                            Width,
                            userTextPresent && !_animationManager.IsAnimating() ? (TEXT_SMALL_SIZE) : (int)(LINE_Y + (TEXT_SMALL_SIZE - LINE_Y) * animationProgress));
                        hintTextSize = userTextPresent && !_animationManager.IsAnimating() ? 12 : (int)(16 + (12 - 16) * animationProgress);
                    }

                    // Line Animation
                    if (!isM3)
                    {
                        int LineAnimationWidth = (int)(Width * animationProgress);
                        int LineAnimationX = (Width / 2) - (LineAnimationWidth / 2);
                        g.FillRectangle(SelectedBrush, LineAnimationX, LINE_Y, LineAnimationWidth, 2);
                    }
                }

                // Calc text Rect
                Rectangle textRect = new Rectangle(
                    SkinManager.FORM_PADDING,
                    hasHint && UseTallSize ? (hintRect.Y + hintRect.Height) - 2 : ClientRectangle.Y,
                    ClientRectangle.Width - SkinManager.FORM_PADDING * 3 - 8,
                    hasHint && UseTallSize ? LINE_Y - (hintRect.Y + hintRect.Height) : LINE_Y);

                g.Clip = new Region(textRect);

                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    // Draw user text
                    NativeText.DrawTransparentText(
                        Text,
                        SkinManager.getLogFontByType(isM3 ? MaterialSkinManager.fontType.BodyLarge : MaterialSkinManager.fontType.Subtitle1),
                        Enabled ? SkinManager.TextHighEmphasisColor : SkinManager.TextDisabledOrHintColor,
                        textRect.Location,
                        textRect.Size,
                        NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }

                g.ResetClip();

                // Draw hint text
                if (hasHint && (UseTallSize || String.IsNullOrEmpty(Text)))
                {
                    using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                    {
                        NativeText.DrawTransparentText(
                        Hint,
                        SkinManager.getTextBoxFontBySize(hintTextSize),
                        Enabled ? DroppedDown || Focused ? 
                        SelectedColor : // Focus 
                        (isM3 ? SkinManager.ActiveColorRoles.OnSurfaceVariant : SkinManager.TextMediumEmphasisColor) : // not focused
                        SkinManager.TextDisabledOrHintColor, // Disabled
                        hintRect.Location,
                        hintRect.Size,
                        NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                    }
                }
            }
        }

        private void CustomMeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            e.ItemHeight = HEIGHT - 7;
        }

        private void CustomDrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index > Items.Count || !Focused) return;

            Graphics g = e.Graphics;
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;

            // Draw the background of the item.
            using (Brush backgroundBrush = isM3 ? new SolidBrush(SkinManager.ActiveColorRoles.SurfaceContainer) : SkinManager.BackgroundBrush)
            {
                g.FillRectangle(backgroundBrush, e.Bounds);
            }

            // Hover
            if (e.State.HasFlag(DrawItemState.Focus)) // Focus == hover
            {
                using (Brush hoverBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.SecondaryContainer : SkinManager.BackgroundHoverColor))
                {
                    g.FillRectangle(hoverBrush, e.Bounds);
                }
            }
            
            string Text = "";
            if (!string.IsNullOrWhiteSpace(DisplayMember))
            {
                if (!Items[e.Index].GetType().Equals(typeof(DataRowView)))
                {
                    var item = Items[e.Index].GetType().GetProperty(DisplayMember).GetValue(Items[e.Index]);
                    Text = item.ToString();
                }
                else
                {
                    var table = ((DataRow)Items[e.Index].GetType().GetProperty("Row").GetValue(Items[e.Index])).Table;
                    Text = table.Rows[e.Index][DisplayMember].ToString();
                }
            }
            else
            {
                Text = Items[e.Index].ToString();
            }

            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(
                Text,
                SkinManager.getFontByType(SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? MaterialSkinManager.fontType.BodyLarge : MaterialSkinManager.fontType.Subtitle1),
                SkinManager.TextHighEmphasisNoAlphaColor,
                new Point(e.Bounds.Location.X + SkinManager.FORM_PADDING, e.Bounds.Location.Y),
                new Size(e.Bounds.Size.Width - SkinManager.FORM_PADDING * 2, e.Bounds.Size.Height),
                NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle); ;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            MouseState = MouseState.OUT;
            MeasureItem += CustomMeasureItem;
            DrawItem += CustomDrawItem;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawVariable;
            recalculateAutoSize();
            setHeightVars();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            recalculateAutoSize();
            setHeightVars();
        }

        private void setHeightVars()
        {
            HEIGHT = UseTallSize ? 50 : 36;
            Size = new Size(Size.Width, HEIGHT);
            LINE_Y = HEIGHT - BOTTOM_PADDING;
            ItemHeight = HEIGHT - 7;
            DropDownHeight = ItemHeight * MaxDropDownItems + 2;
        }

        public void recalculateAutoSize()
        {
            if (!AutoResize) return;

            int w = DropDownWidth;
            int padding = SkinManager.FORM_PADDING * 3;
            int vertScrollBarWidth = (Items.Count > MaxDropDownItems) ? SystemInformation.VerticalScrollBarWidth : 0;

            Graphics g = CreateGraphics();
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                var itemsList = this.Items.Cast<object>().Select(item => item.ToString());
                foreach (string s in itemsList)
                {
                    int newWidth = NativeText.MeasureLogString(s, SkinManager.getLogFontByType(MaterialSkinManager.fontType.Subtitle1)).Width + vertScrollBarWidth + padding;
                    if (w < newWidth) w = newWidth;
                }
            }

            if (Width != w)
            {
                DropDownWidth = w;
                Width = w;
            }
        }
    }
}
