namespace MaterialSkin.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class MaterialListView : ListView, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }

        [Browsable(false)]
        public Point MouseLocation { get; set; }

        private bool _autoSizeTable;

        [Category("Appearance"), Browsable(true)]
        public bool AutoSizeTable
        {
            get
            {
                return _autoSizeTable;
            }
            set
            {
                _autoSizeTable = value;
                Scrollable = !value;
            }
        }

        [Browsable(false)]
        private ListViewItem HoveredItem { get; set; }

        private const int PAD = 16;
        private const int ITEMS_HEIGHT = 52;

        public MaterialListView()
        {
            GridLines = false;
            FullRowSelect = true;
            View = View.Details;
            OwnerDraw = true;
            ResizeRedraw = true;
            BorderStyle = BorderStyle.None;
            MinimumSize = new Size(200, 100);

            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            Font = SkinManager.getFontByType(SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? MaterialSkinManager.fontType.BodyMedium : MaterialSkinManager.fontType.Body1);
            BackColor = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? SkinManager.ActiveColorRoles.Surface : SkinManager.BackgroundColor;

            // Fix for hovers, by default it doesn't redraw
            MouseLocation = new Point(-1, -1);
            MouseState = MouseState.OUT;
            MouseEnter += delegate
            {
                MouseState = MouseState.HOVER;
            };
            MouseLeave += delegate
            {
                MouseState = MouseState.OUT;
                MouseLocation = new Point(-1, -1);
                HoveredItem = null;
                Invalidate();
            };
            MouseDown += delegate
            {
                MouseState = MouseState.DOWN;
            };
            MouseUp += delegate
            {
                MouseState = MouseState.HOVER;
            };
            MouseMove += delegate (object sender, MouseEventArgs args)
            {
                MouseLocation = args.Location;
                var currentHoveredItem = GetItemAt(MouseLocation.X, MouseLocation.Y);
                if (HoveredItem != currentHoveredItem)
                {
                    HoveredItem = currentHoveredItem;
                    Invalidate();
                }
            };
        }
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            Graphics g = e.Graphics;
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var headerBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.SurfaceContainer : BackColor))
            {
                g.FillRectangle(headerBrush, e.Bounds);
            }
            // Draw Text
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                NativeText.DrawTransparentText(
                    e.Header.Text,
                    SkinManager.getLogFontByType(isM3 ? MaterialSkinManager.fontType.TitleSmall : MaterialSkinManager.fontType.Subtitle2),
                    Enabled ? (isM3 ? SkinManager.ActiveColorRoles.OnSurfaceVariant : SkinManager.TextHighEmphasisNoAlphaColor) : SkinManager.TextDisabledOrHintColor,
                    new Point(e.Bounds.Location.X + PAD, e.Bounds.Location.Y),
                    new Size(e.Bounds.Size.Width - PAD * 2, e.Bounds.Size.Height),
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }
            using (var dividerPen = new Pen(isM3 ? SkinManager.ActiveColorRoles.OutlineVariant : SkinManager.DividersColor))
            {
                g.DrawLine(dividerPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            Graphics g = e.Graphics;
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Always draw default background
            using (var rowBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.Surface : SkinManager.BackgroundColor))
            {
                g.FillRectangle(rowBrush, e.Bounds);
            }

            if (e.Item.Selected)
            {
                // Selected background
                using (var selectedBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.SecondaryContainer : SkinManager.BackgroundFocusColor))
                {
                    g.FillRectangle(selectedBrush, e.Bounds);
                }
            }
            else if (e.Bounds.Contains(MouseLocation) && MouseState == MouseState.HOVER)
            {
                // Hover background
                using (var hoverBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.SurfaceContainerHigh : SkinManager.BackgroundHoverColor))
                {
                    g.FillRectangle(hoverBrush, e.Bounds);
                }
            }

                // Draw separator line
            using (var dividerPen = new Pen(isM3 ? SkinManager.ActiveColorRoles.OutlineVariant : SkinManager.DividersColor))
            {
                g.DrawLine(dividerPen, e.Bounds.Left, e.Bounds.Y, e.Bounds.Right, e.Bounds.Y);
            }

            foreach (ListViewItem.ListViewSubItem subItem in e.Item.SubItems)
            {
                // Draw Text
                using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
                {
                    NativeText.DrawTransparentText(
                        subItem.Text,
                        SkinManager.getLogFontByType(isM3 ? MaterialSkinManager.fontType.BodyMedium : MaterialSkinManager.fontType.Body2),
                        Enabled ? (e.Item.Selected && isM3 ? SkinManager.ActiveColorRoles.OnSecondaryContainer : SkinManager.TextHighEmphasisNoAlphaColor) : SkinManager.TextDisabledOrHintColor,
                        new Point(subItem.Bounds.X + PAD, subItem.Bounds.Y),
                        new Size(subItem.Bounds.Width - PAD * 2, subItem.Bounds.Height),
                        NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }
            }
        }

        // Resize
        protected override void OnColumnWidthChanging(ColumnWidthChangingEventArgs e)
        {
            base.OnColumnWidthChanging(e);
            AutoResize();
        }

        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            AutoResize();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AutoResize();
        }

        private void AutoResize()
        {
            if (!AutoSizeTable) return;

            // Width
            int w = 0;
            foreach (ColumnHeader col in Columns)
            {
                w += col.Width;
            }

            // Height
            int h = 50; //Header size
            if (Items.Count > 0) h = TopItem.Bounds.Top;
            foreach (ListViewItem item in Items)
            {
                h += item.Bounds.Height;
            }

            Size = new Size(w, h);
        }

        protected override void InitLayout()
        {
            base.InitLayout();

            // enforce settings
            GridLines = false;
            FullRowSelect = true;
            View = View.Details;
            OwnerDraw = true;
            ResizeRedraw = true;
            BorderStyle = BorderStyle.None;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            BackColor = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? SkinManager.ActiveColorRoles.Surface : SkinManager.BackgroundColor;
        }
    }
}
