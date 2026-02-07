using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using LibVLCSharp.Shared;

namespace datvreceiver
{
    /// <summary>
    /// Generates OSD overlay images for VLC's logo filter.
    /// Uses PNG with per-pixel alpha for text on semi-transparent background.
    /// Scales font size based on video resolution to maintain consistent apparent size.
    /// </summary>
    public class VlcOsdHelper : IDisposable
    {
        private MediaPlayer mediaPlayer;
        private string tempImagePath;
        private string currentText = "";
        private int backgroundOpacity = 128;
        private bool disposed = false;

        // Target apparent font size on screen (in pixels at display resolution)
        private const float TargetApparentFontSize = 24f;

        // Current video dimensions
        private int videoWidth = 1280;
        private int videoHeight = 720;

        // Display dimensions (VideoView size)
        private int displayWidth = 800;
        private int displayHeight = 450;

        public VlcOsdHelper(MediaPlayer player, int rxNumber)
        {
            mediaPlayer = player;

            // Create unique temp file for this receiver
            string tempDir = Path.Combine(Path.GetTempPath(), "datvreceiver");
            Directory.CreateDirectory(tempDir);
            tempImagePath = Path.Combine(tempDir, $"osd_rx{rxNumber}.png");
        }

        /// <summary>
        /// Update the video dimensions. Call this when video resolution changes.
        /// </summary>
        public void SetVideoSize(int width, int height)
        {
            if (width > 0 && height > 0 && (width != videoWidth || height != videoHeight))
            {
                videoWidth = width;
                videoHeight = height;
                // Regenerate OSD with new scale if we have text
                if (!string.IsNullOrEmpty(currentText))
                {
                    UpdateLogo();
                }
            }
        }

        /// <summary>
        /// Update the display dimensions (VideoView size). Call this when the control is resized.
        /// </summary>
        public void SetDisplaySize(int width, int height)
        {
            if (width > 0 && height > 0 && (width != displayWidth || height != displayHeight))
            {
                displayWidth = width;
                displayHeight = height;
                // Regenerate OSD with new scale if we have text
                if (!string.IsNullOrEmpty(currentText))
                {
                    UpdateLogo();
                }
            }
        }

        public void SetOsdText(string text)
        {
            if (currentText == text) return;
            currentText = text;
            UpdateLogo();
        }

        public void SetBackgroundOpacity(int opacity)
        {
            backgroundOpacity = Math.Max(0, Math.Min(255, opacity));
            UpdateLogo();
        }

        private void UpdateLogo()
        {
            if (mediaPlayer == null || disposed) return;

            try
            {
                if (string.IsNullOrEmpty(currentText))
                {
                    // Disable logo when no text
                    mediaPlayer.SetLogoInt(VideoLogoOption.Enable, 0);
                    return;
                }

                // Generate the overlay image
                GenerateOsdImage();

                // Configure VLC logo filter
                mediaPlayer.SetLogoInt(VideoLogoOption.Enable, 1);
                mediaPlayer.SetLogoString(VideoLogoOption.File, tempImagePath);
                // Position 6 = top-right, 5 = top-left, 4 = top-center (but VLC uses different values)
                // VLC position: 8=top-center, 0=center, 2=bottom-center
                // Actually: 0=center, 1=left, 2=right, 4=top, 8=bottom (combined: 4=top-center)
                mediaPlayer.SetLogoInt(VideoLogoOption.Position, 4); // Top-center
                mediaPlayer.SetLogoInt(VideoLogoOption.Opacity, 255); // Use PNG's alpha
            }
            catch
            {
                // Ignore errors
            }
        }

        private void GenerateOsdImage()
        {
            // Calculate font size to achieve consistent apparent size on screen
            // The video will be scaled from videoHeight to displayHeight
            // So we need to generate text that, after scaling, appears as TargetApparentFontSize
            // Formula: actualFontSize = targetSize * (videoHeight / displayHeight)
            float scaleFactor = (float)videoHeight / displayHeight;
            float scaledFontSize = TargetApparentFontSize * scaleFactor;

            // Ensure minimum readable size
            scaledFontSize = Math.Max(scaledFontSize, 12f);

            // Scale padding and outline proportionally
            int padding = (int)(6 * scaleFactor);
            float outlineWidth = Math.Max(2f, 2f * scaleFactor);

            using (Font scaledFont = new Font("Arial", scaledFontSize, FontStyle.Bold))
            {
                // Measure text size first
                SizeF textSize;
                using (Bitmap measureBitmap = new Bitmap(1, 1))
                using (Graphics measureGraphics = Graphics.FromImage(measureBitmap))
                {
                    textSize = measureGraphics.MeasureString(currentText, scaledFont);
                }

                int width = (int)textSize.Width + padding * 2 + (int)(4 * scaleFactor);
                int height = (int)textSize.Height + padding * 2 + (int)(4 * scaleFactor);

                using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TextRenderingHint = TextRenderingHint.AntiAlias;
                        g.Clear(Color.Transparent);

                        // Draw semi-transparent black background
                        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(backgroundOpacity, 0, 0, 0)))
                        {
                            g.FillRectangle(bgBrush, 0, 0, width, height);
                        }

                        // Draw text with outline for better visibility
                        float x = padding;
                        float y = padding;

                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddString(currentText, scaledFont.FontFamily, (int)scaledFont.Style,
                                g.DpiY * scaledFont.Size / 72f, new PointF(x, y), StringFormat.GenericDefault);

                            // Draw outline
                            using (Pen outlinePen = new Pen(Color.Black, outlineWidth))
                            {
                                outlinePen.LineJoin = LineJoin.Round;
                                g.DrawPath(outlinePen, path);
                            }

                            // Fill text
                            using (SolidBrush textBrush = new SolidBrush(Color.White))
                            {
                                g.FillPath(textBrush, path);
                            }
                        }
                    }

                    // Save as PNG with alpha
                    bitmap.Save(tempImagePath, ImageFormat.Png);
                }
            }
        }

        public void ClearOsd()
        {
            currentText = "";
            if (mediaPlayer != null && !disposed)
            {
                try
                {
                    mediaPlayer.SetLogoInt(VideoLogoOption.Enable, 0);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            ClearOsd();

            // Clean up temp file
            try
            {
                if (File.Exists(tempImagePath))
                    File.Delete(tempImagePath);
            }
            catch { }
        }
    }
}
