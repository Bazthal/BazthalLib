
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BazthalLib.Controls;


namespace BazthalLib.UI
{
    public class TintedImageRenderer
    {
        private static readonly Dictionary<(Image, Color), Image> _cashe = new();
        // private static ContentAlignment alignment;

        /// <summary>
        /// Draws a tinted image onto a specified graphics surface within given bounds and alignment.
        /// </summary>
        /// <remarks>If the source image or graphics object is <see langword="null"/>, or if the bounds
        /// have non-positive dimensions, the method returns without drawing.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object on which to draw the image. Cannot be <see langword="null"/>.</param>
        /// <param name="src_Img">The source <see cref="Image"/> to be tinted and drawn. Cannot be <see langword="null"/>.</param>
        /// <param name="color">The <see cref="Color"/> used to tint the image.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that defines the area within which the image should be drawn. Must have positive
        /// width and height.</param>
        /// <param name="alignment">The <see cref="ContentAlignment"/> that specifies how the image should be aligned within the bounds.</param>
        public static void Draw(Graphics g, Image src_Img, Color color, Rectangle bounds, ContentAlignment alignment)
        {
            if (src_Img == null || g == null || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            var tintedImage = GetTintedImage(src_Img, color);
            var imageRect = AlignImage(tintedImage.Size, bounds, alignment);

            g.DrawImage(tintedImage, imageRect);
        }

        /// <summary>
        /// Aligns an image within a specified bounding rectangle according to the given alignment.
        /// </summary>
        /// <param name="imageSize">The size of the image to be aligned.</param>
        /// <param name="bounds">The rectangle within which the image should be aligned.</param>
        /// <param name="alignment">The alignment specification indicating how the image should be positioned within the bounds.</param>
        /// <returns>A <see cref="Rectangle"/> representing the aligned position of the image within the specified bounds.</returns>
        private static Rectangle AlignImage(Size imageSize, Rectangle bounds, ContentAlignment alignment)
        {
            int x = bounds.X, y = bounds.Y;

            if (alignment.HasFlag(ContentAlignment.MiddleCenter) ||
                alignment.HasFlag(ContentAlignment.TopCenter) ||
                alignment.HasFlag(ContentAlignment.BottomCenter))
                x = bounds.X + (bounds.Width - imageSize.Width) / 2;
            else if (alignment.HasFlag(ContentAlignment.MiddleRight) ||
                     alignment.HasFlag(ContentAlignment.TopRight) ||
                     alignment.HasFlag(ContentAlignment.BottomRight))
                x = bounds.Right - imageSize.Width;

            if (alignment.HasFlag(ContentAlignment.MiddleCenter) ||
                alignment.HasFlag(ContentAlignment.MiddleLeft) ||
                alignment.HasFlag(ContentAlignment.MiddleRight))
                y = bounds.Y + (bounds.Height - imageSize.Height) / 2;
            else if (alignment.HasFlag(ContentAlignment.BottomLeft) ||
                     alignment.HasFlag(ContentAlignment.BottomCenter) ||
                     alignment.HasFlag(ContentAlignment.BottomRight))
                y = bounds.Bottom - imageSize.Height;

            return new Rectangle(x, y, imageSize.Width, imageSize.Height);
        }

        /// <summary>
        /// Loads an embedded image from the specified file path within the assembly.
        /// </summary>
        /// <remarks>Ensure that the specified file path is correct and that the image is embedded as a
        /// resource in the assembly.</remarks>
        /// <param name="embededFilePath">The path to the embedded image file within the assembly.</param>
        /// <returns>An <see cref="Image"/> object representing the embedded image if found; otherwise, <see langword="null"/>.</returns>
        public static Image LoadEmbededImage(string embededFilePath)
        {
            var asm = typeof(ThemeColorsSetter).Assembly;
            using var stream = asm.GetManifestResourceStream(embededFilePath);
            return stream != null ? Image.FromStream(stream) : null;
        }

        /// <summary>
        /// Applies a tint to the specified image using the given color and returns the tinted image.
        /// </summary>
        /// <remarks>The method caches the tinted image for the combination of the source image and tint
        /// color to improve performance on subsequent calls with the same parameters.</remarks>
        /// <param name="srcImg">The source image to be tinted. Cannot be null.</param>
        /// <param name="tintColor">The color used to tint the image.</param>
        /// <returns>A new <see cref="Image"/> object with the applied tint. Returns <see langword="null"/> if <paramref
        /// name="srcImg"/> is <see langword="null"/>.</returns>
        private static Image GetTintedImage(Image srcImg, Color tintColor)
        {
            if (srcImg == null)
                return null;

            var key = (srcImg, tintColor);
            if (_cashe.TryGetValue(key, out var cached))
                return cached;

            Bitmap bmp = new Bitmap(srcImg.Width, srcImg.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
            }

            using (Bitmap source = new Bitmap(srcImg))
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color src = source.GetPixel(x, y);
                        if (src.A == 0)
                            continue;

                        float luminance = src.R * 0.3f + src.G * 0.59f + src.B * 0.11f;
                        float brightness = luminance / 255f;

                        float minVisualBrightness = 0.2f;
                        brightness = Math.Max(minVisualBrightness, brightness);

                        Color baseColor = (tintColor.R < 16 && tintColor.G < 16 && tintColor.B < 16)
                            ? Color.FromArgb(40, 40, 40)
                            : Color.Black;

                        int r = (int)(baseColor.R + (tintColor.R - baseColor.R) * brightness);
                        int g = (int)(baseColor.G + (tintColor.G - baseColor.G) * brightness);
                        int b = (int)(baseColor.B + (tintColor.B - baseColor.B) * brightness);

                        Color tinted = Color.FromArgb(src.A, ClampByte(r), ClampByte(g), ClampByte(b));
                        bmp.SetPixel(x, y, tinted);
                    }
                }
            }

            _cashe[key] = bmp;
            return bmp;
        }

        /// <summary>
        /// Clamps an integer value to the range of a byte.
        /// </summary>
        /// <param name="value">The integer value to clamp.</param>
        /// <returns>An integer representing the clamped value, which will be between 0 and 255 inclusive.</returns>
        private static int ClampByte(int value) => Math.Max(0, Math.Min(255, value));

        public static Rectangle GetAlignedImageRect(Image image, Rectangle bounds, ContentAlignment alignment)
        {
            if (image == null || bounds.Width <= 0 || bounds.Height <= 0)
                return Rectangle.Empty;

            return AlignImage(image.Size, bounds, alignment);
        }
        /// <summary>
        /// Generates a <see cref="GraphicsPath"/> representing the opaque outline of an image.
        /// </summary>
        /// <remarks>The method processes the image to determine which pixels are opaque based on the
        /// specified alpha threshold. It then constructs a path outlining these regions, aligns it within the given
        /// bounds, and applies optional padding. The resulting path is clipped to the specified bounds.</remarks>
        /// <param name="srcImg">The source image from which to extract the opaque outline.</param>
        /// <param name="bounds">The bounding rectangle within which the outline should be aligned.</param>
        /// <param name="alignment">The alignment of the image within the specified bounds.</param>
        /// <param name="alphaThreshold">The alpha threshold above which a pixel is considered opaque. Default is 10.</param>
        /// <param name="paddingScale">The scale factor to apply as padding around the outline. Default is 1.02f.</param>
        /// <returns>A <see cref="GraphicsPath"/> representing the outline of the opaque regions of the image, aligned and scaled
        /// according to the specified parameters. Returns <see langword="null"/> if the source image is <see
        /// langword="null"/> or if the bounds are invalid.</returns>
        /// 
        public static GraphicsPath GetOpaqueOutlinePath(
            Image srcImg,
            Rectangle bounds,
            ContentAlignment alignment,
            byte alphaThreshold = 10,
            float paddingScale = 1.02f)
        {
            if (srcImg == null || bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            Bitmap bmp = GetTintedImage(srcImg, Color.White) as Bitmap;
            if (bmp == null)
                return null;

            bool[,] mask = new bool[bmp.Width, bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    mask[x, y] = bmp.GetPixel(x, y).A > alphaThreshold;
                }
            }

            var path = new GraphicsPath();
            for (int y = 1; y < bmp.Height - 1; y++)
            {
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    if (!mask[x, y]) continue;

                    bool edge = !mask[x - 1, y] || !mask[x + 1, y] || !mask[x, y - 1] || !mask[x, y + 1];
                    if (edge)
                    {
                        path.AddRectangle(new Rectangle(x, y, 1, 1));
                    }
                }
            }

            Rectangle imageRect = AlignImage(bmp.Size, bounds, alignment);
            float scaleX = (float)imageRect.Width / bmp.Width;
            float scaleY = (float)imageRect.Height / bmp.Height;

            var alignMatrix = new Matrix();
            alignMatrix.Scale(scaleX, scaleY);
            alignMatrix.Translate(imageRect.X, imageRect.Y, MatrixOrder.Append);
            path.Transform(alignMatrix);

            if (paddingScale != 1f)
            {
                RectangleF boundsF = path.GetBounds();

                var paddingMatrix = new Matrix();

                paddingMatrix.Translate(
                    -boundsF.X - boundsF.Width / 2,
                    -boundsF.Y - boundsF.Height / 2,
                    MatrixOrder.Append);

                paddingMatrix.Scale(paddingScale, paddingScale, MatrixOrder.Append);

                paddingMatrix.Translate(
                    boundsF.X + boundsF.Width / 2,
                    boundsF.Y + boundsF.Height / 2,
                    MatrixOrder.Append);

                path.Transform(paddingMatrix);
            }

            using (var clipRegion = new Region(bounds))
            {
                clipRegion.Intersect(path);
                return GraphicsPathFromRegion(clipRegion);
            }

        }

        /// <summary>
        /// Converts a <see cref="Region"/> into a <see cref="GraphicsPath"/> by extracting its rectangular components.
        /// </summary>
        /// <param name="region">The <see cref="Region"/> to be converted. Cannot be null.</param>
        /// <returns>A <see cref="GraphicsPath"/> containing the rectangular components of the specified <paramref
        /// name="region"/>.</returns>
        private static GraphicsPath GraphicsPathFromRegion(Region region)
        {
            var result = new GraphicsPath();
            foreach (RectangleF rect in region.GetRegionScans(new Matrix()))
            {
                result.AddRectangle(rect);
            }
            return result;
        }


        /// <summary>
        /// Generates a <see cref="GraphicsPath"/> representing the opaque areas of the specified image.
        /// </summary>
        /// <remarks>The method processes the image to identify pixels with an alpha value greater than
        /// the specified threshold, creating a path of rectangles for these pixels. The resulting path is scaled and
        /// aligned according to the specified bounds and alignment.</remarks>
        /// <param name="src_Img">The source image from which to create the opaque mask. Cannot be null.</param>
        /// <param name="bounds">The bounding rectangle within which the image is aligned and scaled.</param>
        /// <param name="alignment">Specifies how the image is aligned within the bounds.</param>
        /// <param name="alphaThreshold">The alpha threshold above which a pixel is considered opaque. Defaults to 10.</param>
        /// <returns>A <see cref="GraphicsPath"/> containing rectangles for each opaque pixel in the image, or <see
        /// langword="null"/> if the source image is null or the bounds are invalid.</returns>
        public static GraphicsPath GetOpaqueMask(Image src_Img, Rectangle bounds, ContentAlignment alignment, byte alphaThreshold = 10)
        {
            if (src_Img == null || bounds.Width <= 0 || bounds.Height <= 0)
                return null;

            Bitmap bmp = GetTintedImage(src_Img, Color.White) as Bitmap;
            if (bmp == null)
                return null;

            var path = new GraphicsPath();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp.GetPixel(x, y).A > alphaThreshold)
                    {
                        path.AddRectangle(new Rectangle(x, y, 1, 1));
                    }
                }
            }

            var imageRect = AlignImage(bmp.Size, bounds, alignment);

            float scaleX = (float)imageRect.Width / bmp.Width;
            float scaleY = (float)imageRect.Height / bmp.Height;

            var matrix = new Matrix();
            matrix.Scale(scaleX, scaleY);
            matrix.Translate(imageRect.X, imageRect.Y, MatrixOrder.Append);

            path.Transform(matrix);

            return path;
        }
    }
}
