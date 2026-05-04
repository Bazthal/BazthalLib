
using BazthalLib.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;


namespace BazthalLib.UI
{

    /// <summary>
    /// Specifies the quality or characteristic of an image, typically used in rendering, processing, or analysis
    /// contexts.
    /// </summary>
    /// <remarks>The <see cref="ImageQuality"/> enumeration provides predefined values to represent different
    /// image quality states or characteristics. These values can be used to configure image processing operations,
    /// rendering settings, or other domain-specific behaviors.</remarks>
    public enum ImageQuality
    {
       /// <summary>
       /// Represents a sharp musical note or symbol, typically used in music theory or notation.
       /// </summary>
       /// <remarks>This class or member may be used to define or manipulate sharp notes in a musical
       /// context. Ensure proper usage in accordance with the musical scale or notation system being
       /// implemented.</remarks>
        Sharp,

       /// <summary>
       /// Represents a smoothing operation or behavior.
       /// </summary>
       /// <remarks>The specific meaning of "Smooth" depends on the context in which it is used.  It may
       /// refer to a smoothing algorithm, a graphical rendering option, or another domain-specific operation.</remarks>
        Smooth,

        /// <summary>
        /// Represents a balanced state or condition.
        /// </summary>
        /// <remarks>This enumeration value is typically used to indicate that a system, process, or
        /// entity is in a state of equilibrium or balance.</remarks>
        Balanced
    }

    public class TintedImageRenderer
    {
        private static readonly Dictionary<(Image, Color), Image> _cache = new();
        // private static ContentAlignment alignment;

        /// <summary>
        /// Draws a tinted and optionally scaled image onto a specified graphics surface within the given bounds.
        /// </summary>
        /// <remarks>If the source image or graphics object is <see langword="null"/>, or if the bounds
        /// have a width or height less than or equal to zero, the method does nothing. The method applies a tint to the
        /// source image using the specified color, scales the image if a scale factor is provided, and aligns it within
        /// the bounds based on the specified alignment. The interpolation mode is adjusted based on the specified image
        /// quality for optimal rendering.</remarks>
        /// <param name="g">The <see cref="Graphics"/> object used to draw the image. Cannot be <see langword="null"/>.</param>
        /// <param name="src_Img">The source <see cref="Image"/> to be drawn. Cannot be <see langword="null"/>.</param>
        /// <param name="color">The <see cref="Color"/> used to tint the image.</param>
        /// <param name="bounds">The <see cref="Rectangle"/> that defines the area within which the image will be drawn.</param>
        /// <param name="alignment">The <see cref="ContentAlignment"/> that specifies how the image should be aligned within the bounds.</param>
        /// <param name="scale">The scaling factor to apply to the image. Defaults to 1.0 (no scaling). Must be greater than 0.</param>
        /// <param name="quality">The <see cref="ImageQuality"/> that determines the interpolation mode used for rendering the image. Defaults
        /// to <see cref="ImageQuality.Balanced"/>.</param>
        public static void Draw(Graphics g, Image src_Img, Color color, Rectangle bounds, ContentAlignment alignment, float scale = 1.0f, ImageQuality quality = ImageQuality.Balanced)
        {
            if (src_Img == null || g == null || bounds.Width <= 0 || bounds.Height <= 0)
                return;

            var tintedImage = GetTintedImage(src_Img, color);

            Size scaledSize = tintedImage.Size;
            if (scale != 1.0f)
            {
                scaledSize = new Size(
                    (int)(tintedImage.Width * scale),
                    (int)(tintedImage.Height * scale)
                );
            }

            var imageRect = AlignImage(scaledSize, bounds, alignment);

            var oldMode = g.InterpolationMode;
            switch (quality)
            {
                case ImageQuality.Sharp:
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    break;
                case ImageQuality.Smooth:
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    break;
                default:
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    break;
            }
            g.DrawImage(tintedImage, imageRect);
            g.InterpolationMode = oldMode;
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
        /// Loads an embedded image resource from the specified assembly.
        /// </summary>
        /// <remarks>This method attempts to retrieve the specified embedded image resource from the
        /// provided assembly. If the resource cannot be found, a warning is logged, and the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="resourcePath">The fully qualified name of the embedded resource to load.</param>
        /// <param name="assembly">The assembly containing the embedded resource. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="Image"/> object representing the loaded embedded image, or <see langword="null"/> if the
        /// resource is not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static Image LoadEmbeddedImage(string resourcePath, Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            using Stream stream = assembly.GetManifestResourceStream(resourcePath);
            DebugUtils.LogIf(stream == null, "UI.TintedImageRenderer", "LoadEmbededImage", $"Embedded image not found: {resourcePath}", false, logLevel: DebugUtils.LogLevel.Warning);

            return stream != null ? Image.FromStream(stream) : null;
        }
        /// <summary>
        /// Loads an embedded image resource from the specified resource path.
        /// </summary>
        /// <remarks>This method retrieves an embedded image resource from the calling assembly. Ensure
        /// that the resource path is correct and that the image is properly embedded in the assembly's
        /// resources.</remarks>
        /// <param name="resourcePath">The path to the embedded image resource within the assembly.</param>
        /// <returns>An <see cref="Image"/> object representing the loaded embedded image.</returns>
        public static Image LoadEmbeddedImage(string resourcePath)
        {
            return LoadEmbeddedImage(resourcePath, Assembly.GetCallingAssembly());
        }

        /*
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
            DebugUtils.LogIf(stream == null, "UI.TintedImageRenderer", "LoadEmbededImage", $"Embedded image not found: {embededFilePath}", false, logLevel:DebugUtils.LogLevel.Warning);
            return stream != null ? Image.FromStream(stream) : null;
        }
        */
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
            if (_cache.TryGetValue(key, out var cached))
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

            _cache[key] = bmp;
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

            Rectangle imageRect = AlignImage(srcImg.Size, bounds, alignment);
            Bitmap scaledBmp = new Bitmap(imageRect.Width, imageRect.Height);
            using (Graphics g = Graphics.FromImage(scaledBmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcImg, 0, 0, imageRect.Width, imageRect.Height);
            }

            Bitmap bmp = GetTintedImage(scaledBmp, Color.White) as Bitmap;
            if (bmp == null) return null;

            bool[,] mask = new bool[bmp.Width, bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                    mask[x, y] = bmp.GetPixel(x, y).A > alphaThreshold;
            }

            var path = new GraphicsPath();
            for (int y = 1; y < bmp.Height - 1; y++)
            {
                for (int x = 1; x < bmp.Width - 1; x++)
                {
                    if (!mask[x, y]) continue;
                    bool edge = !mask[x - 1, y] || !mask[x + 1, y] || !mask[x, y - 1] || !mask[x, y + 1];
                    if (edge)
                        path.AddRectangle(new Rectangle(x, y, 1, 1));
                }
            }

            if (paddingScale != 1f)
            {
                RectangleF pathBounds = path.GetBounds();

                float maxScaleX = bounds.Width / pathBounds.Width;
                float maxScaleY = bounds.Height / pathBounds.Height;
                float safeScale = Math.Min(paddingScale, Math.Min(maxScaleX, maxScaleY));

                var paddingMatrix = new Matrix();
                paddingMatrix.Translate(-pathBounds.X - pathBounds.Width / 2, -pathBounds.Y - pathBounds.Height / 2, MatrixOrder.Append);
                paddingMatrix.Scale(safeScale, safeScale, MatrixOrder.Append);
                paddingMatrix.Translate(pathBounds.X + pathBounds.Width / 2, pathBounds.Y + pathBounds.Height / 2, MatrixOrder.Append);
                path.Transform(paddingMatrix);
            }

            var translateMatrix = new Matrix();
            translateMatrix.Translate(imageRect.X, imageRect.Y, MatrixOrder.Append);
            path.Transform(translateMatrix);

            using (var clipRegion = new Region(bounds))
            using (var pathRegion = new Region(path))
            {
                clipRegion.Intersect(pathRegion);
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
