using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Custom label control that renders text with an outline for better visibility.
    /// Supports transparent background with optional semi-transparent overlay.
    /// Used for OSD (On-Screen Display) text overlays on video panels.
    /// </summary>
    public class OutlinedLabel : Label
    {
        #region Private Fields

        private Color _outlineColor = Color.Black;
        private float _outlineWidth = 2f;
        private int _backgroundOpacity = 0;  // 0 = transparent, 255 = fully opaque
        private int _padding = 6;

        #endregion

        #region Public Properties

        /// <summary>
        /// Color of the text outline. Default is Black.
        /// </summary>
        public Color OutlineColor
        {
            get { return _outlineColor; }
            set { _outlineColor = value; Invalidate(); }
        }

        /// <summary>
        /// Width of the text outline in pixels. Default is 2.
        /// </summary>
        public float OutlineWidth
        {
            get { return _outlineWidth; }
            set { _outlineWidth = value; Invalidate(); }
        }

        /// <summary>
        /// Opacity of the background rectangle (0-255).
        /// 0 = fully transparent, 255 = fully opaque black.
        /// </summary>
        public int BackgroundOpacity
        {
            get { return _backgroundOpacity; }
            set { _backgroundOpacity = Math.Max(0, Math.Min(255, value)); Invalidate(); }
        }

        /// <summary>
        /// Padding around text in pixels. Default is 6.
        /// </summary>
        public int TextPadding
        {
            get { return _padding; }
            set { _padding = value; Invalidate(); }
        }

        #endregion

        #region Constructor

        public OutlinedLabel()
        {
            // Enable double buffering and transparent background for flicker-free rendering
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Custom paint handler that renders text with outline using GraphicsPath.
        /// Supports multi-line text and optional semi-transparent background.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Draw semi-transparent background if opacity > 0
            if (_backgroundOpacity > 0)
            {
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(_backgroundOpacity, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(bgBrush, 0, 0, Width, Height);
                }
            }

            // Calculate font size in em units for GraphicsPath
            float emSize = e.Graphics.DpiY * Font.Size / 72f;
            string[] lines = Text.Split('\n');
            float y = _padding;

            // Configure text alignment and outline
            using (StringFormat sf = new StringFormat())
            using (Pen outlinePen = new Pen(_outlineColor, _outlineWidth))
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Near;
                outlinePen.LineJoin = LineJoin.Round;

                // Render each line with outline
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        y += emSize;
                        continue;
                    }

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddString(
                            line,
                            Font.FontFamily,
                            (int)Font.Style,
                            emSize,
                            new PointF(_padding, y),
                            sf);

                        // Draw outline first, then fill for crisp text
                        e.Graphics.DrawPath(outlinePen, path);
                        e.Graphics.FillPath(brush, path);
                    }

                    y += emSize + 4;  // Line spacing
                }
            }
        }

        /// <summary>
        /// Auto-sizes the control based on text content.
        /// Handles multi-line text and accounts for outline width.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // Calculate required size for all lines
            using (Graphics g = CreateGraphics())
            {
                string[] lines = Text.Split('\n');
                float maxWidth = 0;
                float totalHeight = 0;
                float emSize = g.DpiY * Font.Size / 72f;

                foreach (string line in lines)
                {
                    SizeF size = g.MeasureString(line, Font);
                    if (size.Width > maxWidth) maxWidth = size.Width;
                    totalHeight += emSize + 4;
                }

                // Add padding and outline width to final size
                Width = (int)(maxWidth + _outlineWidth * 2 + _padding * 2 + 20);
                Height = (int)(totalHeight + _outlineWidth * 2 + _padding * 2 + 8);
            }
        }

        #endregion
    }
}
