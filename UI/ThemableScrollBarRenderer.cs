using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BazthalLib.Controls
{
    [ToolboxItem(false)] //Don't want this to appear in the toolbox
    public class ThemableScrollBarRenderer
    {
        #region Fields
        public Color BackColor { get; set; } = SystemColors.Control;
        public Color BorderColor { get; set; } = SystemColors.ActiveBorder;
        public Color AccentColor { get; set; } = Color.DodgerBlue;
        public bool Hovering { get; set; } = false;
        public bool HoverArrows { get; set; } = true;
        public bool DrawOuterBorder { get; set; } = true;



        private enum ArrowDirection { Up, Down, Left, Right }

        //Don't think I will need this but it's here while I work on it 
        public Color ForeColor { get; set; } = SystemColors.ControlText;
        #endregion Fields

        /// <summary>
        /// Draws a custom scrollbar on the specified graphics surface within the given client area.
        /// </summary>
        /// <remarks>This method uses anti-aliasing to smooth the drawing of the scrollbar components. It
        /// draws the background, arrow buttons, thumb, and optionally an outer border based on the current settings.
        /// Ensure that the <paramref name="g"/> parameter is properly initialized and disposed of by the
        /// caller.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the scrollbar.</param>
        /// <param name="clientRect">The <see cref="Rectangle"/> that defines the client area where the scrollbar is drawn.</param>
        /// <param name="orientation">The <see cref="Orientation"/> of the scrollbar, either horizontal or vertical.</param>
        /// <param name="upButtonRect">The <see cref="Rectangle"/> that defines the area for the up (or left) arrow button.</param>
        /// <param name="downButtonRect">The <see cref="Rectangle"/> that defines the area for the down (or right) arrow button.</param>
        /// <param name="trackArea">The <see cref="Rectangle"/> that defines the track area of the scrollbar.</param>
        /// <param name="thumbRect">The <see cref="Rectangle"/> that defines the area for the scrollbar thumb.</param>
        public void DrawScrollBar(Graphics g, Rectangle clientRect, Orientation orientation, Rectangle upButtonRect,
            Rectangle downButtonRect, Rectangle trackArea, Rectangle thumbRect)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var bg = new SolidBrush(BackColor);
            g.FillRectangle(bg, clientRect);


            DrawArrowButton(g, upButtonRect, GetArrowDirection(orientation, true));
            DrawArrowButton(g, downButtonRect, GetArrowDirection(orientation, false));


            // Draw thumb

            DrawThumb(g, orientation, trackArea, thumbRect);

            using (var borderPen = new Pen(BorderColor))
                if (DrawOuterBorder)
                {
                    var oldSmoothing = g.SmoothingMode;
                    g.SmoothingMode = SmoothingMode.None;

                    Rectangle borderRect = new Rectangle(
                        clientRect.Left,
                        clientRect.Top,
                        clientRect.Width - 1,
                        clientRect.Height - 1
                    );
                    g.DrawRectangle(borderPen, borderRect);

                    g.SmoothingMode = oldSmoothing;
                }
            //     g.DrawRectangle(borderPen, clientRect.Left, clientRect.Top, clientRect.Width - 1, clientRect.Height - 1);

        }

        /// <summary>
        /// Draws the thumb of a scrollbar within the specified track area.
        /// </summary>
        /// <remarks>The method uses anti-aliasing to smooth the edges of the thumb. If
        /// <c>DrawOuterBorder</c> is <see langword="true"/>, an outer border is drawn around the track area using the
        /// specified border color.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the thumb.</param>
        /// <param name="orientation">The orientation of the scrollbar, either horizontal or vertical.</param>
        /// <param name="trackArea">The <see cref="Rectangle"/> that defines the area of the track where the thumb is drawn.</param>
        /// <param name="thumbRect">The <see cref="Rectangle"/> that defines the bounds of the thumb to be drawn.</param>
        public void DrawThumb(Graphics g, Orientation orientation, Rectangle trackArea, Rectangle thumbRect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var thumbBrush = new SolidBrush(AccentColor))
                g.FillRectangle(thumbBrush, thumbRect);

            if (DrawOuterBorder)
            {
                using var borderPen = new Pen(BorderColor);
                var oldSmoothing = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;

                Rectangle borderRect = new Rectangle(
                    trackArea.Left,
                    trackArea.Top,
                    trackArea.Width - 1,
                    trackArea.Height - 1
                );
                g.DrawRectangle(borderPen, borderRect);

                g.SmoothingMode = oldSmoothing;
            }
        }

        /// <summary>
        /// Determines the arrow direction based on the specified orientation and direction flag.
        /// </summary>
        /// <param name="orientation">The orientation to consider, either vertical or horizontal.</param>
        /// <param name="isUpOrLeft">A boolean flag indicating the desired direction.  If <see langword="true"/>, returns Up for vertical or Left
        /// for horizontal orientation;  otherwise, returns Down for vertical or Right for horizontal orientation.</param>
        /// <returns>An <see cref="ArrowDirection"/> value representing the calculated direction based on the  provided
        /// orientation and direction flag.</returns>
        private ArrowDirection GetArrowDirection(Orientation orientation, bool isUpOrLeft)
        {
            return orientation switch
            {
                Orientation.Vertical => isUpOrLeft ? ArrowDirection.Up : ArrowDirection.Down,
                Orientation.Horizontal => isUpOrLeft ? ArrowDirection.Left : ArrowDirection.Right,
                _ => ArrowDirection.Down
            };
        }

        /// <summary>
        /// Calculates the rectangle representing the thumb of a scrollbar based on the specified parameters.
        /// </summary>
        /// <remarks>The method calculates the thumb rectangle differently based on the orientation. For
        /// vertical scrollbars,  the thumb height is determined by the large change, while for horizontal scrollbars,
        /// the thumb width is  calculated based on the visible content width and total content width.</remarks>
        /// <param name="orientation">The orientation of the scrollbar, either vertical or horizontal.</param>
        /// <param name="trackArea">The area of the track on which the thumb can move.</param>
        /// <param name="min">The minimum value of the scrollbar range.</param>
        /// <param name="max">The maximum value of the scrollbar range.</param>
        /// <param name="value">The current value of the scrollbar, indicating the position of the thumb.</param>
        /// <param name="largeChange">The amount by which the scrollbar value changes when the thumb is moved a large distance.</param>
        /// <returns>A <see cref="Rectangle"/> representing the position and size of the scrollbar thumb.  Returns <see
        /// cref="Rectangle.Empty"/> if the maximum value is less than or equal to the minimum value plus the large
        /// change.</returns>
        public Rectangle GetThumbRectangle(Orientation orientation, Rectangle trackArea, int min, int max, int value, int largeChange)
        {
            if (max <= min + largeChange)
                return Rectangle.Empty;

            float range = max - min;
            float percent = (float)(value - min) / (range - largeChange);

            if (orientation == Orientation.Vertical)
            {
                float moveSpace = trackArea.Height - largeChange;
                int top = trackArea.Top + (int)(percent * moveSpace);
                return new Rectangle(trackArea.Left, top, trackArea.Width, largeChange);
            }
            else // Orientation.Horizontal
            {
                int viewportWidth = largeChange;         // visible width of content
                int contentWidth = max;                  // total content width (e.g. _maxItemWidth)

                float viewRatio = (float)viewportWidth / contentWidth;
                int thumbWidth = Math.Max(15, (int)(trackArea.Width * viewRatio));

                float maxScroll = contentWidth - viewportWidth;
                float scrollPercent = maxScroll > 0 ? (float)(value - min) / maxScroll : 0;

                int left = trackArea.Left + (int)((trackArea.Width - thumbWidth) * scrollPercent);

                return new Rectangle(left, trackArea.Top, thumbWidth, trackArea.Height);
            }
        }

        /// <summary>
        /// Draws an arrow button within the specified rectangle on the provided graphics surface.
        /// </summary>
        /// <remarks>The button is drawn with a border and background color, and the arrow is filled with
        /// an accent color. The arrow is only drawn if the <c>HoverArrows</c> property is <see langword="false"/> or if
        /// it is <see langword="true"/> and the button is being hovered over.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the button.</param>
        /// <param name="rect">The <see cref="Rectangle"/> that defines the bounds of the button.</param>
        /// <param name="direction">The <see cref="ArrowDirection"/> indicating the direction of the arrow to be drawn.</param>
        private void DrawArrowButton(Graphics g, Rectangle rect, ArrowDirection direction)
        {
            using var borderPen = new Pen(BorderColor);
            using var bgBrush = new SolidBrush(BackColor);
            using var arrowBrush = new SolidBrush(AccentColor);

            g.FillRectangle(bgBrush, rect);
            g.DrawRectangle(borderPen, rect);

            Point center = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            Point[] arrow = direction switch
            {
                ArrowDirection.Up => new[] {
            new Point(center.X, center.Y - 3),
            new Point(center.X - 4, center.Y + 2),
            new Point(center.X + 4, center.Y + 2),
        },
                ArrowDirection.Down => new[] {
            new Point(center.X, center.Y + 3),
            new Point(center.X - 4, center.Y - 2),
            new Point(center.X + 4, center.Y - 2),
        },
                ArrowDirection.Left => new[] {
            new Point(center.X - 3, center.Y),
            new Point(center.X + 2, center.Y - 4),
            new Point(center.X + 2, center.Y + 4),
        },
                ArrowDirection.Right => new[] {
            new Point(center.X + 3, center.Y),
            new Point(center.X - 2, center.Y - 4),
            new Point(center.X - 2, center.Y + 4),
        },
                _ => Array.Empty<Point>()
            };
            if (!HoverArrows || (HoverArrows && Hovering))
                g.FillPolygon(arrowBrush, arrow);
        }

    }
}