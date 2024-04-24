namespace MaterialSkin
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// Defines the <see cref="DrawHelper" />
    /// </summary>
    internal static class DrawHelper
    {
        /// <summary>
        /// The CreateRoundRect
        /// </summary>
        /// <param name="x">The x<see cref="float"/></param>
        /// <param name="y">The y<see cref="float"/></param>
        /// <param name="width">The width<see cref="float"/></param>
        /// <param name="height">The height<see cref="float"/></param>
        /// <param name="radius">The radius<see cref="float"/></param>
        /// <returns>The <see cref="GraphicsPath"/></returns>
        public static GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            var gp = new GraphicsPath();
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            gp.CloseFigure();
            return gp;
        }

        /// <summary>
        /// The CreateRoundRect
        /// </summary>
        /// <param name="rect">The rect<see cref="Rectangle"/></param>
        /// <param name="radius">The radius<see cref="float"/></param>
        /// <returns>The <see cref="GraphicsPath"/></returns>
        public static GraphicsPath CreateRoundRect(Rectangle rect, float radius)
        {
            return CreateRoundRect(rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        /// <summary>
        /// The CreateRoundRect
        /// </summary>
        /// <param name="rect">The rect<see cref="RectangleF"/></param>
        /// <param name="radius">The radius<see cref="float"/></param>
        /// <returns>The <see cref="GraphicsPath"/></returns>
        public static GraphicsPath CreateRoundRect(RectangleF rect, float radius)
        {
            return CreateRoundRect(rect.X, rect.Y, rect.Width, rect.Height, radius);
        }

        /// <summary>
        /// The BlendColor
        /// </summary>
        /// <param name="backgroundColor">The backgroundColor<see cref="Color"/></param>
        /// <param name="frontColor">The frontColor<see cref="Color"/></param>
        /// <param name="blend">The blend<see cref="double"/></param>
        /// <returns>The <see cref="Color"/></returns>
        public static Color BlendColor(Color backgroundColor, Color frontColor, double blend)
        {
            var ratio = blend / 255d;
            var invRatio = 1d - ratio;
            var r = (int)((backgroundColor.R * invRatio) + (frontColor.R * ratio));
            var g = (int)((backgroundColor.G * invRatio) + (frontColor.G * ratio));
            var b = (int)((backgroundColor.B * invRatio) + (frontColor.B * ratio));
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// The BlendColor
        /// </summary>
        /// <param name="backgroundColor">The backgroundColor<see cref="Color"/></param>
        /// <param name="frontColor">The frontColor<see cref="Color"/></param>
        /// <returns>The <see cref="Color"/></returns>
        public static Color BlendColor(Color backgroundColor, Color frontColor)
        {
            return BlendColor(backgroundColor, frontColor, frontColor.A);
        }


        public static void DrawSquareShadow(Graphics g, Rectangle bounds)
        {
            int radius = Math.Min(bounds.Width, bounds.Height) / 2;
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(12, 0, 0, 0)))
            {
                GraphicsPath path;
                path = DrawHelper.CreateRoundRect(new RectangleF(bounds.X - 2f, bounds.Y - 1.5f, bounds.Width + 2, bounds.Height + 2), radius+6);
                g.FillPath(shadowBrush, path);
                path = DrawHelper.CreateRoundRect(new RectangleF(bounds.X - 1.5f, bounds.Y - 1.5f, bounds.Width + 1, bounds.Height + 1), radius+4);
                g.FillPath(shadowBrush, path);
                path = DrawHelper.CreateRoundRect(new RectangleF(bounds.X - 0.5f, bounds.Y - 0.5f, bounds.Width + 0.5f, bounds.Height + 0.5f), radius+2);
                g.FillPath(shadowBrush, path);
                path = DrawHelper.CreateRoundRect(new RectangleF(bounds.X - 0.5f, bounds.Y + 1.5f, bounds.Width + 0, bounds.Height + 0), radius);
                g.FillPath(shadowBrush, path);
                path = DrawHelper.CreateRoundRect(new RectangleF(bounds.X - 0.5f, bounds.Y + 2.5f, bounds.Width + 0, bounds.Height + 0), radius);
                g.FillPath(shadowBrush, path);
                path.Dispose();
            }
        }
        public static void DrawElevatedShadow(Graphics g, Rectangle bounds)
        {
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(12, 0, 0, 0)))
            {
                GraphicsPath path = new GraphicsPath();
                float cornerRadius = 18;
                path.AddRoundRectangle(bounds, cornerRadius);

                float shadowWidth = 10;
                // Рисуване на сянката с различни размери и цветове
                for (float i = 0; i < shadowWidth; i++)
                {
                    GraphicsPath shadowPath = new GraphicsPath();
                    RectangleF shadowBounds = new RectangleF(bounds.X - i, bounds.Y - i, bounds.Width + 2 * i, bounds.Height + 2 * i);
                    path.Reset();
                    path.AddRoundRectangle(shadowBounds, cornerRadius);

                    // Рисуване на елиптична маска
                    GraphicsPath maskPath = new GraphicsPath();
                    maskPath.AddRoundRectangle(shadowBounds, cornerRadius);

                    // Прозрачност на текущия път спрямо размера на маската
                    Region currentRegion = new Region(path);
                    currentRegion.Exclude(maskPath);

                    // Рисуване на сянката
                    g.FillRegion(shadowBrush, currentRegion);

                    // Освобождаване на паметта, заета от пътя на сянката и маската
                    shadowPath.Dispose();
                    maskPath.Dispose();
                }
            }
        }

        public static void DrawRoundShadow(Graphics g, Rectangle bounds)
        {
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(12, 0, 0, 0)))
            {
                g.FillEllipse(shadowBrush, new Rectangle(bounds.X - 2, bounds.Y - 1, bounds.Width + 4, bounds.Height + 6));
                g.FillEllipse(shadowBrush, new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 4));
                g.FillEllipse(shadowBrush, new Rectangle(bounds.X - 0, bounds.Y - 0, bounds.Width + 0, bounds.Height + 2));
                g.FillEllipse(shadowBrush, new Rectangle(bounds.X - 0, bounds.Y + 2, bounds.Width + 0, bounds.Height + 0));
                g.FillEllipse(shadowBrush, new Rectangle(bounds.X - 0, bounds.Y + 1, bounds.Width + 0, bounds.Height + 0));
            }
        }
        public static void AddRoundRectangle(this GraphicsPath path, RectangleF bounds, float cornerRadius)
        {
            float diameter = cornerRadius * 2;
            SizeF size = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
        }
    }
}