using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialRatingBar : ProgressBar, IMaterialControl
    {
        private readonly Icon _starIcon;

        public int CurrentRating
        {
            get => this.Value;
            set => this.Value = Math.Max(0, Math.Min(5, value));
        }
        public MaterialRatingBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            _starIcon = SkinManager.GetIcon(MaterialIcon.Star);

        }
        [Browsable(false)]
        public int Depth { get; set; }

        [Browsable(false)]
        public MaterialSkinManager SkinManager => MaterialSkinManager.Instance;

        [Browsable(false)]
        public MouseState MouseState { get; set; }
       
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int starCount = 5;
            int starWidth = Width / starCount;
            int starHeight = Height;

            for (int i = 0; i < starCount; i++)
            {
                Rectangle starRect = new Rectangle(i * starWidth, 0, starWidth, starHeight);
                DrawStarIcon(e.Graphics, starRect, i < Value);
            }
        }
        private void DrawStarIcon(Graphics g, Rectangle bounds, bool filled)
        {
            if (_starIcon != null)
            {
                // Изчертаване на иконата на звездичката във всяка област на рейтинг бара
                g.DrawIcon(_starIcon, bounds);
            }
        }
    }
}

