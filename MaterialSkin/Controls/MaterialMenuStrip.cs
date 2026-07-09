using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
	public class MaterialMenuStrip : MenuStrip, IMaterialControl
	{
		public int Depth { get; set; }
		public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
		public MouseState MouseState { get; set; }

		public MaterialMenuStrip()
		{
			Renderer = new MaterialMenuStripRender();

			if (DesignMode)
			{
				Dock = DockStyle.None;
				Anchor |= AnchorStyles.Right;
				AutoSize = false;
				Location = new Point(0, 28);
			}
		}
		
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
			Font = SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.TitleSmall : MaterialSkinManager.fontType.Button);
			BackColor = isM3 ? SkinManager.ActiveColorRoles.SurfaceContainerHigh : SkinManager.PrimaryColor;
		}
	}

	internal class MaterialMenuStripRender : ToolStripProfessionalRenderer, IMaterialControl
	{
		//Properties for managing the material design properties
		public int Depth { get; set; }
		public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
		public MouseState MouseState { get; set; }

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			var g = e.Graphics;
			bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;

			if (e.Item.IsOnDropDown)
			{
				var itemRect = GetItemRect(e.Item);
				var textRect = new Rectangle(24, itemRect.Y, itemRect.Width - (24 + 16), itemRect.Height);
				using (var textBrush = new SolidBrush(e.Item.Enabled
					? (isM3 ? SkinManager.ActiveColorRoles.OnSurface : SkinManager.TextHighEmphasisColor)
					: SkinManager.TextDisabledOrHintColor))
				{
					g.DrawString(e.Text, SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.BodyMedium : MaterialSkinManager.fontType.Button), textBrush, textRect, new StringFormat() { LineAlignment = StringAlignment.Center });
				}
			}
			else
			{
				using (var textBrush = new SolidBrush(isM3
					? (e.Item.Enabled ? SkinManager.ActiveColorRoles.OnSurface : SkinManager.TextDisabledOrHintColor)
					: Color.White))
				{
					g.DrawString(e.Text, SkinManager.getFontByType(isM3 ? MaterialSkinManager.fontType.TitleSmall : MaterialSkinManager.fontType.Button), textBrush, e.TextRectangle, new StringFormat() { LineAlignment = StringAlignment.Center });
				}
			}
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			var g = e.Graphics;
			bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
			g.Clear(isM3 ? SkinManager.ActiveColorRoles.SurfaceContainerHigh : SkinManager.PrimaryColor);

			//Draw background
			var itemRect = GetItemRect(e.Item);
			if (e.Item.IsOnDropDown)
			{
				using (var itemBrush = new SolidBrush(
					e.Item.Selected && e.Item.Enabled
						? (isM3 ? SkinManager.ActiveColorRoles.SecondaryContainer : SkinManager.BackgroundHoverColor)
						: (isM3 ? SkinManager.ActiveColorRoles.Surface : SkinManager.GetApplicationBackgroundColor())))
				{
					g.FillRectangle(itemBrush, itemRect);
				}
			}
			else
			{
				using (var itemBrush = new SolidBrush(
					e.Item.Selected
						? (isM3 ? SkinManager.ActiveColorRoles.SecondaryContainer.WithAlpha(140) : SkinManager.ColorScheme.PrimaryColor.Lighten(0.15f))
						: (isM3 ? SkinManager.ActiveColorRoles.SurfaceContainerHigh : SkinManager.PrimaryColor)))
				{
					g.FillRectangle(itemBrush, itemRect);
				}
			}

			//Ripple animation
			var toolStrip = e.ToolStrip as MaterialContextMenuStrip;
			if (toolStrip != null)
			{
				var animationManager = toolStrip.animationManager;
				var animationSource = toolStrip.animationSource;
				if (toolStrip.animationManager.IsAnimating() && e.Item.Bounds.Contains(animationSource))
				{
					for (int i = 0; i < animationManager.GetAnimationCount(); i++)
					{
						var animationValue = animationManager.GetProgress(i);
						var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationValue * 50)), isM3 ? SkinManager.ActiveColorRoles.OnSecondaryContainer : Color.Black));
						var rippleSize = (int)(animationValue * itemRect.Width * 2.5);
						g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, itemRect.Y - itemRect.Height, rippleSize, itemRect.Height * 3));
					}
				}
			}
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			//base.OnRenderImageMargin(e);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			var g = e.Graphics;
			bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;
			using (var backBrush = new SolidBrush(isM3 ? SkinManager.ActiveColorRoles.Surface : SkinManager.GetApplicationBackgroundColor()))
			using (var pen = new Pen(isM3 ? SkinManager.ActiveColorRoles.OutlineVariant : SkinManager.GetDividersColor()))
			{
				g.FillRectangle(backBrush, e.Item.Bounds);
				g.DrawLine(pen, new Point(e.Item.Bounds.Left, e.Item.Bounds.Height / 2), new Point(e.Item.Bounds.Right, e.Item.Bounds.Height / 2));
			}
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			//var g = e.Graphics;

			//g.DrawRectangle(new Pen(SkinManager.GetDividersColor()), new Rectangle(e.AffectedBounds.X, e.AffectedBounds.Y, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			var g = e.Graphics;
			const int ARROW_SIZE = 4;
			bool isM3 = SkinManager.DesignVersion == MaterialSkinManager.MaterialDesignVersion.Material3;

			var arrowMiddle = new Point(e.ArrowRectangle.X + e.ArrowRectangle.Width / 2, e.ArrowRectangle.Y + e.ArrowRectangle.Height / 2);
			using (var arrowBrush = new SolidBrush(e.Item.Enabled
				? (isM3 ? SkinManager.ActiveColorRoles.OnSurfaceVariant : SkinManager.TextHighEmphasisColor)
				: SkinManager.TextDisabledOrHintColor))
			using (var arrowPath = new GraphicsPath())
			{
				arrowPath.AddLines(new[] { new Point(arrowMiddle.X - ARROW_SIZE, arrowMiddle.Y - ARROW_SIZE), new Point(arrowMiddle.X, arrowMiddle.Y), new Point(arrowMiddle.X - ARROW_SIZE, arrowMiddle.Y + ARROW_SIZE) });
				arrowPath.CloseFigure();

				g.FillPath(arrowBrush, arrowPath);
			}
		}

		private Rectangle GetItemRect(ToolStripItem item)
		{
			return new Rectangle(0, item.ContentRectangle.Y, item.ContentRectangle.Width + 4, item.ContentRectangle.Height);
		}
	}
}
