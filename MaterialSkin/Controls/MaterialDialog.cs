namespace MaterialSkin.Controls
{
    using MaterialSkin.Animations;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    public class MaterialDialog : MaterialForm
    {

        private const int LEFT_RIGHT_PADDING = 24;
        private const int BUTTON_PADDING = 8;
        private const int BUTTON_HEIGHT = 36;
        private const int TEXT_TOP_PADDING = 17;
        private const int TEXT_BOTTOM_PADDING = 28;
        private int _header_Height = 40;

        private MaterialButton _validationButton = new MaterialButton();
        private MaterialButton _cancelButton = new MaterialButton();
        private AnimationManager _AnimationManager;
        private bool CloseAnimation = false;
        private Form _formOverlay;
        private Panel _contentHost;
        private Panel _userControlScrollPanel;
        private String _text;
        private String _title;
        private readonly string title;
        private UserControl _userControl;
        private bool _userControlShellSized;
        private int CornerRadius => SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? 28 : 6;
        private MaterialSkinManager.fontType DialogTitleFont => SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? MaterialSkinManager.fontType.HeadlineSmall : MaterialSkinManager.fontType.H6;
        private MaterialSkinManager.fontType DialogBodyFont => SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3 ? MaterialSkinManager.fontType.BodyLarge : MaterialSkinManager.fontType.Body1;
        private int CompactHeaderHeight => 24;

        /// <summary>
        /// The Collection for the Buttons
        /// </summary>
        //public ObservableCollection<MaterialButton> Buttons { get; set; }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        /// <summary>
        /// Constructer Setting up the Layout
        /// </summary>
        public MaterialDialog(Form ParentForm, string Title, string Text, string ValidationButtonText, bool ShowCancelButton, string CancelButtonText, bool UseAccentColor)
        {
            _formOverlay = new Form
            {
                BackColor = Color.Black,
                Opacity = 0.5,
                MinimizeBox = false,
                MaximizeBox = true,
                Text = "",
                ShowIcon = false,
                ControlBox = false,
                FormBorderStyle = FormBorderStyle.None,
                Size = new Size(ParentForm.Width, ParentForm.Height),
                ShowInTaskbar = false,
                Owner = ParentForm,
                Visible = true,
                Location = new Point(ParentForm.Location.X, ParentForm.Location.Y),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            };

            _title = Title;
            if (Title.Length == 0)
                _header_Height = 0;
            else
                _header_Height = 40;

            _text = Text;
            ShowInTaskbar = false;
            Sizable = false;
            ControlBox=true;

            BackColor = SkinManager.BackgroundColor;
            FormStyle = FormStyles.StatusAndActionBar_None;
            _AnimationManager = new AnimationManager();
            _AnimationManager.AnimationType = AnimationType.EaseOut;
            _AnimationManager.Increment = 0.03;
            _AnimationManager.OnAnimationProgress += _AnimationManager_OnAnimationProgress;

            _validationButton = new MaterialButton
            {
                AutoSize = false,
                DialogResult = DialogResult.OK,
                DrawShadows = false,
                Type = MaterialButton.MaterialButtonType.Text,
                UseAccentColor = UseAccentColor,
                Text = ValidationButtonText
            };
            _cancelButton = new MaterialButton
            {
                AutoSize = false,
                DialogResult = DialogResult.Cancel,
                DrawShadows = false,
                Type = MaterialButton.MaterialButtonType.Text,
                UseAccentColor = UseAccentColor,
                Visible = ShowCancelButton,
                Text = CancelButtonText
            };

            this.AcceptButton = _validationButton;
            this.CancelButton = _cancelButton;

            if (!Controls.Contains(_validationButton))
                Controls.Add(_validationButton);
            if (!Controls.Contains(_cancelButton))
                Controls.Add(_cancelButton);

            Width = 560;
            int TextWidth = TextRenderer.MeasureText(_text, SkinManager.getFontByType(DialogBodyFont)).Width;
            int RectWidth = Width - (2 * LEFT_RIGHT_PADDING) - BUTTON_PADDING;
            int RectHeight = ((TextWidth / RectWidth) + 1) * 19;
            Rectangle textRect = new Rectangle(
                LEFT_RIGHT_PADDING,
                _header_Height + TEXT_TOP_PADDING,
                RectWidth,
                RectHeight + 9);

            Height = _header_Height + TEXT_TOP_PADDING + textRect.Height + TEXT_BOTTOM_PADDING + 52; //560;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, CornerRadius, CornerRadius));

            int _buttonWidth = ((TextRenderer.MeasureText(ValidationButtonText, SkinManager.getFontByType(MaterialSkinManager.fontType.Button))).Width + 32);
            Rectangle _validationbuttonBounds = new Rectangle((Width) - BUTTON_PADDING - _buttonWidth, Height - BUTTON_PADDING - BUTTON_HEIGHT, _buttonWidth, BUTTON_HEIGHT);
            _validationButton.Width = _validationbuttonBounds.Width;
            _validationButton.Height = _validationbuttonBounds.Height;
            _validationButton.Top = _validationbuttonBounds.Top;
            _validationButton.Left = _validationbuttonBounds.Left;  //Button minimum width management
            _validationButton.Visible = true;

            _buttonWidth = ((TextRenderer.MeasureText(CancelButtonText, SkinManager.getFontByType(MaterialSkinManager.fontType.Button))).Width + 32);
            Rectangle _cancelbuttonBounds = new Rectangle((_validationbuttonBounds.Left) - BUTTON_PADDING - _buttonWidth, Height - BUTTON_PADDING - BUTTON_HEIGHT, _buttonWidth, BUTTON_HEIGHT);
            _cancelButton.Width = _cancelbuttonBounds.Width;
            _cancelButton.Height = _cancelbuttonBounds.Height;
            _cancelButton.Top = _cancelbuttonBounds.Top;
            _cancelButton.Left = _cancelbuttonBounds.Left;  //Button minimum width management

            Owner = ParentForm;

            SkinManager.AddFormToManage(this);

            //this.ShowDialog();
            //this Dispose();
            //return materialDialogResult;
        }
        public MaterialDialog(Form ParentForm, string title, UserControl userControl)
        {
            this.title=title;
            _userControl = userControl;
            _formOverlay = new Form
            {
                BackColor = Color.Black,
                Opacity = 0.5,
                MinimizeBox = false,
                MaximizeBox = false,
                Text = "",
                ShowIcon = false,
                ControlBox = false,
                FormBorderStyle = FormBorderStyle.None,
                Size = new Size(ParentForm.Width, ParentForm.Height),
                ShowInTaskbar = false,
                Owner = ParentForm,
                Visible = true,
                Location = new Point(ParentForm.Location.X, ParentForm.Location.Y),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                
            };
            Owner = ParentForm;
            _title = title;
            if (title.Length == 0)
                _header_Height = 0;
            else
                _header_Height = 40;

            Text=title;
            ShowInTaskbar = false;
            Sizable = false;
            ControlBox=true;
            MaximizeBox=false;
            MinimizeBox=false;
            StartPosition = FormStartPosition.CenterParent;

            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            
            BackColor = SkinManager.BackgroundColor;
            FormStyle = FormStyles.ActionBar_None;
            _AnimationManager = new AnimationManager();
            _AnimationManager.AnimationType = AnimationType.EaseOut;
            _AnimationManager.Increment = 0.03;
            _AnimationManager.OnAnimationProgress += _AnimationManager_OnAnimationProgress;

            _userControl.Margin = Padding.Empty;
            _userControl.Location = Point.Empty;
            _userControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _userControl.PerformLayout();

            _contentHost = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = SkinManager.BackgroundColor,
                Margin = Padding.Empty,
                Padding = new Padding(24, 16, 24, 24),
                Dock = DockStyle.Top
            };
            _contentHost.Controls.Add(_userControl);

            _userControlScrollPanel = new Panel
            {
                AutoScroll = true,
                BackColor = SkinManager.BackgroundColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _userControlScrollPanel.Controls.Add(_contentHost);

            Rectangle workArea = Screen.FromControl(ParentForm).WorkingArea;

            MinimumSize = new Size(Math.Min(560, workArea.Width - 48), Math.Min(360, workArea.Height - 48));
            MaximumSize = new Size(workArea.Width - 24, workArea.Height - 24);
            Size = MinimumSize;

            Controls.Add(_userControlScrollPanel);

            SkinManager.AddFormToManage(this);

        }

        public MaterialDialog(Form ParentForm) : this(ParentForm, "Title", "Dialog box", "OK", false, "Cancel", false)
        {
        }
    
        public MaterialDialog(Form ParentForm, string Text) : this(ParentForm, "Title", Text, "OK", false, "Cancel", false)
        {
        }

        public MaterialDialog(Form ParentForm, string Title, string Text) : this(ParentForm, Title, Text, "OK", false, "Cancel", false)
        {
        }

        public MaterialDialog(Form ParentForm, string Title, string Text, string ValidationButtonText) : this(ParentForm, Title, Text, ValidationButtonText, false, "Cancel", false)
        {
        }

        public MaterialDialog(Form ParentForm, string Title, string Text, bool ShowCancelButton) : this(ParentForm, Title, Text, "OK", ShowCancelButton, "Cancel", false)
        {
        }

        public MaterialDialog(Form ParentForm, string Title, string Text, bool ShowCancelButton, string CancelButtonText) : this(ParentForm, Title, Text, "OK", ShowCancelButton, CancelButtonText, false)
        {
        }

       public MaterialDialog(Form ParentForm, string Title, string Text, string ValidationButtonText, bool ShowCancelButton, string CancelButtonText) : this(ParentForm, Title, Text, ValidationButtonText, ShowCancelButton, CancelButtonText, false)
        {
        }
        


        /// <summary>
        /// Sets up the Starting Location and starts the Animation
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_userControl != null)
            {
                UpdateUserControlShellSize();
            }

            Location = new Point(Convert.ToInt32(Owner.Location.X + (Owner.Width / 2) - (Width / 2)), Convert.ToInt32(Owner.Location.Y + (Owner.Height/2) - (Height / 2)));
            _AnimationManager.StartNewAnimation(AnimationDirection.In);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_userControl != null)
            {
                BeginInvoke(new Action(UpdateUserControlShellSize));
            }
        }

        /// <summary>
        /// Animates the Form slides
        /// </summary>
        void _AnimationManager_OnAnimationProgress(object sender)
        {
            if (CloseAnimation)
            {
                Opacity = _AnimationManager.GetProgress();
            }
        }

        /// <summary>
        /// Ovverides the Paint to create the solid colored backcolor
        /// </summary>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (_userControl!=null)
            {
                base.OnPaint(e);

                using (NativeTextRenderer NativeText = new NativeTextRenderer(e.Graphics))
                {
                    Rectangle compactTitleRect = new Rectangle(16, 0, Math.Max(0, ClientSize.Width - 76), CompactHeaderHeight);
                    NativeText.DrawTransparentText(
                        _title,
                        SkinManager.getLogFontByType(SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3
                            ? MaterialSkinManager.fontType.TitleMedium
                            : MaterialSkinManager.fontType.Subtitle1),
                        SkinManager.TextHighEmphasisColor,
                        compactTitleRect.Location,
                        compactTitleRect.Size,
                        NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
                }

                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.Clear(BackColor);

            
            // Calc title Rect
            Rectangle titleRect = new Rectangle(
                LEFT_RIGHT_PADDING,
                0,
                Width - (2 * LEFT_RIGHT_PADDING) ,
                _header_Height);

            //Draw title
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                // Draw header text
                NativeText.DrawTransparentText(
                    _title,
                    SkinManager.getLogFontByType(DialogTitleFont),
                    SkinManager.TextHighEmphasisColor,
                    titleRect.Location,
                    titleRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Bottom);
            }

            // Calc text Rect

            int TextWidth = TextRenderer.MeasureText(_text, SkinManager.getFontByType(DialogBodyFont)).Width;
            int RectWidth = Width - (2 * LEFT_RIGHT_PADDING) - BUTTON_PADDING;
            int RectHeight = ((TextWidth / RectWidth) + 1) * 19;

            Rectangle textRect = new Rectangle(
                LEFT_RIGHT_PADDING,
                _header_Height+17,
                RectWidth,
                RectHeight +19);

            //Draw  Text
            using (NativeTextRenderer NativeText = new NativeTextRenderer(g))
            {
                // Draw header text
                NativeText.DrawMultilineTransparentText(
                    _text,
                    SkinManager.getLogFontByType(DialogBodyFont),
                    SkinManager.TextHighEmphasisColor,
                    textRect.Location,
                    textRect.Size,
                    NativeTextRenderer.TextAlignFlags.Left | NativeTextRenderer.TextAlignFlags.Middle);
            }
           

        }

        /// <summary>
        /// Overrides the Closing Event to Animate the Slide Out
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SkinManager.RemoveFormToManage(this);
            _formOverlay.Visible = false;
            _formOverlay.Close();
            _formOverlay.Dispose();

            DialogResult res = this.DialogResult;

            base.OnClosing(e);
        }

        private void UpdateUserControlShellSize()
        {
            if (_userControl == null || Owner == null)
            {
                return;
            }

            _userControl.PerformLayout();
            _contentHost?.PerformLayout();
            _userControlScrollPanel?.PerformLayout();

            Rectangle workArea = Screen.FromControl(Owner).WorkingArea;
            Size contentSize = MeasureControlContent(_userControl);

            int horizontalChrome = 48;
            int verticalChrome = CompactHeaderHeight + 40;
            int width = Math.Min(Math.Max(contentSize.Width + horizontalChrome, MinimumSize.Width), MaximumSize.Width);
            int height = Math.Min(Math.Max(contentSize.Height + verticalChrome, MinimumSize.Height), MaximumSize.Height);

            _userControlScrollPanel.AutoScrollMinSize = new Size(
                Math.Max(0, contentSize.Width + 48),
                Math.Max(0, contentSize.Height + 40));

            Size = new Size(width, height);
            Location = new Point(
                workArea.Left + Math.Max(0, (workArea.Width - Width) / 2),
                workArea.Top + Math.Max(0, (workArea.Height - Height) / 2));

            if (!_userControlShellSized)
            {
                _userControlShellSized = true;
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed)
                    {
                        UpdateUserControlShellSize();
                    }
                }));
            }
        }

        private static Size MeasureControlContent(Control control)
        {
            Rectangle bounds = Rectangle.Empty;
            MeasureVisibleChildBounds(control, control, ref bounds);

            if (bounds.IsEmpty)
            {
                Size fallback = control.PreferredSize;
                if (fallback.Width <= 0 || fallback.Height <= 0)
                {
                    fallback = control.Size;
                }

                return new Size(
                    Math.Max(0, fallback.Width + control.Padding.Horizontal),
                    Math.Max(0, fallback.Height + control.Padding.Vertical));
            }

            return new Size(
                Math.Max(0, bounds.Right + control.Padding.Right),
                Math.Max(0, bounds.Bottom + control.Padding.Bottom));
        }

        private static void MeasureVisibleChildBounds(Control root, Control parent, ref Rectangle bounds)
        {
            foreach (Control child in parent.Controls)
            {
                if (!child.Visible)
                {
                    continue;
                }

                Rectangle childBounds = root.RectangleToClient(child.RectangleToScreen(child.ClientRectangle));
                childBounds = Rectangle.FromLTRB(
                    childBounds.Left - child.Margin.Left,
                    childBounds.Top - child.Margin.Top,
                    childBounds.Right + child.Margin.Right,
                    childBounds.Bottom + child.Margin.Bottom);

                bounds = bounds.IsEmpty ? childBounds : Rectangle.Union(bounds, childBounds);

                if (child.Controls.Count > 0)
                {
                    MeasureVisibleChildBounds(root, child, ref bounds);
                }
            }
        }

        /// <summary>
        /// Closes the Form after the pull out animation
        /// </summary>
        void _AnimationManager_OnAnimationFinished(object sender)
        {
            Close();
        }
    
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MaterialDialog
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(560, 182);
            this.Name = "MaterialDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Prevents the Form from beeing dragged
        /// </summary>
        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (message.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = message.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                        return;
                    break;
            }

            base.WndProc(ref message);
        }

    }
}
