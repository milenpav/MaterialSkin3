using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialDatePicker : MaterialForm, IMaterialControl
    {

        public override Color BackColor { get { return Parent == null ? SkinManager.BackgroundColor : Parent.BackColor; } }
        private MaterialButton btnToday;
        private RectangleF TopDayRect;
        private RectangleF TopDateRect;
        private RectangleF MonthRect;
        private RectangleF DayRect;
        private RectangleF YearRect;

        private RectangleF CurrentCal_Header;

        private Font TopDayFont, MonthFont, DayFont, YeahrFont;

        private RectangleF CurrentCal;
        private RectangleF PreviousCal;
        private RectangleF NextCal;
        private GraphicsPath ShadowPath;
        private DateTime CurrentDate;
        public DateTime Date { get { return CurrentDate; } set { CurrentDate = value; Invalidate(); } }
        private List<List<DateRect>> DateRectangles;

        private int DateRectDefaultSize;
        private int HoverX;
        private int HoverY;
        private int SelectedX;
        private int SelectedY;
        private bool recentHovered;
        private bool nextHovered;

        public delegate void DateChanged(DateTime newDateTime);
        public event DateChanged onDateChanged;


        private Brush HoverBrush;

        public MaterialDatePicker(DateTime date)
        {
            CurrentDate =date;

            InitializeComponent();

            Width = 250;
            Height = 425;
            TopDayRect = new RectangleF(0f, 0f, Width, 20f);
            TopDateRect = new RectangleF(0f, TopDayRect.Bottom, Width, (float)(Height * 0.3));
            MonthRect = new RectangleF(0f, TopDayRect.Bottom, Width, (float)(TopDateRect.Height * 0.3));
            DayRect = new RectangleF(0f, MonthRect.Bottom, Width, (float)(TopDateRect.Height * 0.4));
            YearRect = new RectangleF(0f, DayRect.Bottom, Width, (float)(TopDateRect.Height * 0.3));
            CurrentCal = new RectangleF(0f, TopDateRect.Bottom, Width, (float)(Height * 0.75));
            CurrentCal_Header = new RectangleF(0f, TopDateRect.Bottom + 3, Width, (float)(CurrentCal.Height * 0.1));
            PreviousCal = new RectangleF(0f, CurrentCal_Header.Y, CurrentCal_Header.Height, CurrentCal_Header.Height);
            NextCal = new RectangleF(Width - CurrentCal_Header.Height, CurrentCal_Header.Y, CurrentCal_Header.Height, CurrentCal_Header.Height);
            ShadowPath = new GraphicsPath();
            ShadowPath.AddLine(-5, TopDateRect.Bottom, Width, TopDateRect.Bottom);
            TopDayFont = SkinManager.getFontByType(MaterialSkinManager.fontType.H6);
            MonthFont = SkinManager.getFontByType(MaterialSkinManager.fontType.H5);
            DayFont = SkinManager.getFontByType(MaterialSkinManager.fontType.H3);
            YeahrFont = SkinManager.getFontByType(MaterialSkinManager.fontType.H5);
            DoubleBuffered = true;
            DateRectDefaultSize = (Width - 10) / 7;
            KeyDown+=(sender, e) =>
            {
                if (e.KeyCode==Keys.Escape)
                    this.Close();
            };
            HoverX = -1;
            HoverY = -1;
            CalculateRectangles();

            // Инициализация и настройка на бутона
            btnToday = new MaterialButton();
            btnToday.Text = "Днес";
            btnToday.Type = MaterialButton.MaterialButtonType.Tonal;
            btnToday.HighEmphasis = true;
            btnToday.Size = new Size(Width - 20, 30);
            btnToday.Dock = DockStyle.Bottom;
            btnToday.Location = new Point(10, Height - 40); // Разполагаме бутона в долната част
            btnToday.Click += BtnToday_Click;
            this.Controls.Add(btnToday);
            Invalidate();



        }
        private void BtnToday_Click(object sender, EventArgs e)
        {
            CurrentDate = DateTime.Now;
            CalculateRectangles();
            Invalidate();
        }
   
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            for (int i = 1; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (DateRectangles[i][j].Drawn)
                    {
                        if (DateRectangles[i][j].Rect.Contains(e.Location))
                        {
                            if (HoverX != i || HoverY != j)
                            {
                                HoverX = i;
                                recentHovered = false;
                                nextHovered = false;
                                HoverY = j;
                                Invalidate();
                            }
                            return;
                        }
                    }
                }
            }

            if (PreviousCal.Contains(e.Location))
            {
                recentHovered = true;
                HoverX = -1;
                Invalidate();
                return;
            }
            if (NextCal.Contains(e.Location))
            {
                nextHovered = true;
                HoverX = -1;
                Invalidate();
                return;
            }
            if (HoverX >= 0 || recentHovered || nextHovered)
            {
                HoverX = -1;
                recentHovered = false;
                nextHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
          

            if (HoverX >= 0)
            {
                SelectedX = HoverX;
                SelectedY = HoverY;
                CurrentDate = DateRectangles[SelectedX][SelectedY].Date;
                Invalidate();
                if (onDateChanged != null)
                {
                    onDateChanged(CurrentDate);
                }
                return;
            }
            if (recentHovered)
            {
                CurrentDate = FirstDayOfMonth(CurrentDate.AddMonths(-1));
                CalculateRectangles();
                Invalidate();
      
                return;
            }
            if (nextHovered)
            {
                CurrentDate = FirstDayOfMonth(CurrentDate.AddMonths(1));
                CalculateRectangles();
                Invalidate();
            
                return;
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            HoverX = -1;
            HoverY = -1;
            nextHovered = false;
            recentHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }


        private void CalculateRectangles()
        {
            DateRectangles = new List<List<DateRect>>();
            for (int i = 0; i < 7; i++)
            {
                DateRectangles.Add(new List<DateRect>());
                for (int j = 0; j < 7; j++)
                {
                    DateRectangles[i].Add(new DateRect(new Rectangle(5 + (j * DateRectDefaultSize), (int)(CurrentCal_Header.Bottom + (i * DateRectDefaultSize)), DateRectDefaultSize, DateRectDefaultSize)));
                }
            }
            DateTime FirstDay = FirstDayOfMonth(CurrentDate);
            for (DateTime date = FirstDay; date <= LastDayOfMonth(CurrentDate); date = date.AddDays(1))
            {
                int WeekOfMonth = GetWeekNumber(date, FirstDay);
                int DayOfWeek = (int)date.DayOfWeek - 1;
                if (DayOfWeek < 0) DayOfWeek = 6;
                if (date.DayOfYear == CurrentDate.DayOfYear && date.Year == CurrentDate.Year)
                {
                    SelectedX = WeekOfMonth;
                    SelectedY = DayOfWeek;
                }
                DateRectangles[WeekOfMonth][DayOfWeek].Drawn = true;
                DateRectangles[WeekOfMonth][DayOfWeek].Date = date;
            }

        }


        protected override void OnResize(EventArgs e)
        {
            Width = 250;
            Height = 425;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
            var roles = SkinManager.ActiveColorRoles;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            HoverBrush = new SolidBrush(isM3
                ? Color.FromArgb(52, roles.Primary)
                : Color.FromArgb(100, SkinManager.ColorScheme.PrimaryColor));

            using (var topDayBrush = new SolidBrush(isM3 ? roles.SurfaceContainerHigh : SkinManager.ColorScheme.DarkPrimaryColor))
            using (var topDateBrush = new SolidBrush(isM3 ? roles.PrimaryContainer : SkinManager.ColorScheme.PrimaryColor))
            {
                g.FillRectangle(topDayBrush, TopDayRect);
                g.FillRectangle(topDateBrush, TopDateRect);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var topDayTextColor = isM3 ? roles.OnSurfaceVariant : SkinManager.TextMediumEmphasisColor;
            var topDateTextColor = isM3 ? roles.OnPrimaryContainer : SkinManager.TextMediumEmphasisColor;
            var yearTextColor = isM3 ? roles.OnSurfaceVariant : Color.FromArgb(80, SkinManager.DividersAlternativeColor);
            var calendarHeaderTextColor = isM3 ? roles.OnSurface : SkinManager.TextMediumEmphasisColor;

            using (var topDayTextBrush = new SolidBrush(topDayTextColor))
            using (var topDateTextBrush = new SolidBrush(topDateTextColor))
            using (var yearTextBrush = new SolidBrush(yearTextColor))
            using (var calendarHeaderTextBrush = new SolidBrush(calendarHeaderTextColor))
            {
                g.DrawString(CurrentDate.ToString("dddd"),
                    SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.LabelLarge : MaterialSkinManager.fontType.H6),
                    topDayTextBrush, TopDayRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                g.DrawString(CurrentDate.ToString("MMMM"),
                    SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.TitleLarge : MaterialSkinManager.fontType.H5),
                    topDateTextBrush, MonthRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
                g.DrawString(CurrentDate.ToString("dd"),
                    SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.DisplaySmall : MaterialSkinManager.fontType.H3),
                    topDateTextBrush, DayRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                g.DrawString(CurrentDate.ToString("yyyy"),
                    SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.TitleMedium : MaterialSkinManager.fontType.H5),
                    yearTextBrush, YearRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                g.DrawString(CurrentDate.ToString("MMMM"),
                    SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.TitleMedium : MaterialSkinManager.fontType.Body1),
                    calendarHeaderTextBrush, CurrentCal_Header, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            if (HoverX >= 0)
            {
                g.FillEllipse(HoverBrush, DateRectangles[HoverX][HoverY].Rect);
            }

            using (var selectedDateBrush = new SolidBrush(isM3 ? roles.Primary : SkinManager.ColorScheme.PrimaryColor))
            {
                g.FillEllipse(selectedDateBrush, DateRectangles[SelectedX][SelectedY].Rect);
            }
            if (recentHovered) g.FillEllipse(HoverBrush, PreviousCal);

            if (nextHovered) g.FillEllipse(HoverBrush, NextCal);

            using (var ButtonPen = new Pen(isM3 ? roles.OnSurfaceVariant : SkinManager.TextMediumEmphasisColor, 2))
            {

                g.DrawLine(ButtonPen,
                        (int)(PreviousCal.X + PreviousCal.Width * 0.6),
                        (int)(PreviousCal.Y + PreviousCal.Height * 0.4),
                        (int)(PreviousCal.X + PreviousCal.Width * 0.4),
                        (int)(PreviousCal.Y + PreviousCal.Height * 0.5));

                g.DrawLine(ButtonPen,
                        (int)(PreviousCal.X + PreviousCal.Width * 0.6),
                        (int)(PreviousCal.Y + PreviousCal.Height * 0.6),
                        (int)(PreviousCal.X + PreviousCal.Width * 0.4),
                        (int)(PreviousCal.Y + PreviousCal.Height * 0.5));

                g.DrawLine(ButtonPen,
                       (int)(NextCal.X + NextCal.Width * 0.4),
                       (int)(NextCal.Y + NextCal.Height * 0.4),
                       (int)(NextCal.X + NextCal.Width * 0.6),
                       (int)(NextCal.Y + NextCal.Height * 0.5));

                g.DrawLine(ButtonPen,
                        (int)(NextCal.X + NextCal.Width * 0.4),
                        (int)(NextCal.Y + NextCal.Height * 0.6),
                        (int)(NextCal.X + NextCal.Width * 0.6),
                        (int)(NextCal.Y + NextCal.Height * 0.5));
            }

            DateTime FirstDay = FirstDayOfMonth(CurrentDate);
            for (int i = 0; i < 7; i++)
            {
                string strName;
                int DayOfWeek = (int)DateTime.Now.DayOfWeek - 1;
                if (DayOfWeek < 0) DayOfWeek = 6;

                strName = DateTime.Now.AddDays(-DayOfWeek+i).ToString("ddd");
                using (var weekDayBrush = new SolidBrush(isM3 ? roles.OnSurfaceVariant : SkinManager.TextMediumEmphasisColor))
                {
                    g.DrawString(strName,
                        SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.LabelMedium : MaterialSkinManager.fontType.Body1),
                        weekDayBrush,
                        DateRectangles[0][i].Rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }
            for (DateTime date = FirstDay; date <= LastDayOfMonth(CurrentDate); date = date.AddDays(1))
            {
                int WeekOfMonth = GetWeekNumber(date, FirstDay);
                int DayOfWeek = (int)date.DayOfWeek - 1;
                if (DayOfWeek < 0) DayOfWeek = 6;

                bool isSelected = WeekOfMonth == SelectedX && DayOfWeek == SelectedY;
                using (var dateBrush = new SolidBrush(isSelected
                    ? (isM3 ? roles.OnPrimary : Color.White)
                    : (isM3 ? roles.OnSurface : SkinManager.TextMediumEmphasisColor)))
                {
                    g.DrawString(date.Day.ToString(),
                        SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.BodyMedium : MaterialSkinManager.fontType.Body1),
                        dateBrush,
                        DateRectangles[WeekOfMonth][DayOfWeek].Rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

            }

        }

        public DateTime FirstDayOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public DateTime LastDayOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        public static int GetWeekNumber(DateTime CurrentDate, DateTime FirstDayOfMonth)
        {

            while (CurrentDate.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                CurrentDate = CurrentDate.AddDays(1);

            return (int)Math.Truncate((double)CurrentDate.Subtract(FirstDayOfMonth).TotalDays / 7f) + 1;
        }

        private class DateRect
        {
            public Rectangle Rect;
            public bool Drawn = false;
            public DateTime Date;

            public DateRect(Rectangle pRect)
            {
                Rect = pRect;
            }
        }
    }
}
