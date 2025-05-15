// adapted from https://stackoverflow.com/questions/30103425/find-dominant-color-in-an-image

using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR_WIN
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Graphics = System.Drawing.Graphics;
#endif
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace AssetInventory
{
    public static class ImageUtils
    {
        public static readonly List<string> SYSTEM_IMAGE_TYPES = new List<string> {"jpg", "jpeg", "png", "bmp", "gif", "tiff", "tif"};

        // palette adapted from http://eastfarthing.com/blog/2016-05-06-palette/
        private static Color[] PALETTE_32 =
        {
            FromHex("#d6a090"),
            FromHex("#fe3b1e"),
            FromHex("#a12c32"),
            FromHex("#fa2f7a"),
            FromHex("#fb9fda"),
            FromHex("#e61cf7"),
            FromHex("#992f7c"),
            FromHex("#47011f"),
            FromHex("#051155"),
            FromHex("#4f02ec"),
            FromHex("#2d69cb"),
            FromHex("#00a6ee"),
            FromHex("#6febff"),
            FromHex("#08a29a"),
            FromHex("#2a666a"),
            FromHex("#063619"),
            FromHex("#000000"),
            FromHex("#4a4957"),
            FromHex("#8e7ba4"),
            FromHex("#b7c0ff"),
            FromHex("#ffffff"),
            FromHex("#acbe9c"),
            FromHex("#827c70"),
            FromHex("#5a3b1c"),
            FromHex("#ae6507"),
            FromHex("#f7aa30"),
            FromHex("#f4ea5c"),
            FromHex("#9b9500"),
            FromHex("#566204"),
            FromHex("#11963b"),
            FromHex("#51e113"),
            FromHex("#08fdcc")
        };

        private static Color UNITY_BACKGROUND = new Color(0.321568638f, 0.321568638f, 0.321568638f, 1f);

        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color result))
            {
                return result;
            }
            return Color.clear;
        }

        public static Color GetNearestColor(Color inputColor)
        {
            double inputRed = Convert.ToDouble(inputColor.r);
            double inputGreen = Convert.ToDouble(inputColor.g);
            double inputBlue = Convert.ToDouble(inputColor.b);

            Color nearestColor = Color.clear;
            double distance = 500.0;
            foreach (Color color in PALETTE_32)
            {
                // Compute Euclidean distance between the two colors
                double testRed = Math.Pow(Convert.ToDouble(color.r) - inputRed, 2.0);
                double testGreen = Math.Pow(Convert.ToDouble(color.g) - inputGreen, 2.0);
                double testBlue = Math.Pow(Convert.ToDouble(color.b) - inputBlue, 2.0);
                double tempDistance = Math.Sqrt(testBlue + testGreen + testRed);
                if (tempDistance == 0.0) return color;
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    nearestColor = color;
                }
            }
            return nearestColor;
        }

        public static Color GetMostUsedColor(Texture2D texture)
        {
            Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color pixelColor = texture.GetPixel(x, y);
                    if (pixelColor == UNITY_BACKGROUND) continue;
                    if (pixelColor.a == 0) continue;

                    if (colorCount.Keys.Contains(pixelColor))
                    {
                        colorCount[pixelColor]++;
                    }
                    else
                    {
                        colorCount.Add(pixelColor, 1);
                    }
                }
            }
            if (colorCount.Count == 0) return Color.clear;

            return colorCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).First().Key;
        }

        public static float GetHue(Texture2D source)
        {
            if (source == null) return -1f;

            Color32[] texColors = source.GetPixels32();
            int total = texColors.Length;
            int r = 0;
            int g = 0;
            int b = 0;
            int count = 0;
            byte alphaThreshold = (byte)(0.25f * 255);

            for (int i = 0; i < total; i++)
            {
                Color32 pixelColor = texColors[i];
                if (pixelColor.a > alphaThreshold)
                {
                    count++;
                    r += pixelColor.r;
                    g += pixelColor.g;
                    b += pixelColor.b;
                }
            }
            if (count == 0) return -1f;

            float inverseCount255 = 1f / (count * 255f);
            float avgR = r * inverseCount255;
            float avgG = g * inverseCount255;
            float avgB = b * inverseCount255;

            return RGBToHue(avgR, avgG, avgB);
        }

        public static float ToHue(this Color color) => RGBToHue(color.r, color.g, color.b);

        // adapted from https://stackoverflow.com/questions/23090019/fastest-formula-to-get-hue-from-rgb
        public static float RGBToHue(float r, float g, float b)
        {
            float min = Mathf.Min(Mathf.Min(r, g), b);
            float max = Mathf.Max(Mathf.Max(r, g), b);
            float delta = max - min;
            if (delta == 0) return 0;

            float hue = 0;
            if (r == max)
            {
                hue = (g - b) / delta;
            }
            else if (g == max)
            {
                hue = (b - r) / delta + 2f;
            }
            else if (b == max)
            {
                hue = (r - g) / delta + 4f;
            }
            hue *= 60f;

            if (hue < 0.0f) hue += 360f;

            return hue;
        }

        public static Texture FillTexture(this Texture2D texture, Color color)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public static Tuple<int, int> GetDimensions(string file, bool ignoreErrors = false, string extOverride = null)
        {
            try
            {
                string path = IOUtils.ToLongPath(file);
#if UNITY_2021_2_OR_NEWER
                string ext = extOverride != null ? extOverride : Path.GetExtension(file).ToLowerInvariant();
                if (ext == ".png")
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        // PNG header: 8 bytes signature, 4 bytes length, 4 bytes "IHDR"
                        // Then the IHDR chunk: width (4 bytes, big-endian) and height (4 bytes, big-endian)
                        fs.Position = 16;
                        Span<byte> buffer = stackalloc byte[8];
                        if (fs.Read(buffer) == 8)
                        {
                            int width = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
                            int height = (buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | buffer[7];
                            return Tuple.Create(width, height);
                        }
                    }
                }
                else if (ext == ".jpg" || ext == ".jpeg")
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        // Validate JPEG SOI marker (0xFFD8)
                        if (fs.ReadByte() != 0xFF || fs.ReadByte() != 0xD8)
                        {
                            Debug.LogWarning($"Not a valid JPEG file: {file}. Trying fallback.");
                        }
                        else
                        {
                            while (fs.Position < fs.Length)
                            {
                                if (fs.ReadByte() != 0xFF) break;
                                int marker = fs.ReadByte();
                                int length = (fs.ReadByte() << 8) | fs.ReadByte();
                                // SOF markers: 0xC0 to 0xC3
                                if (marker >= 0xC0 && marker <= 0xC3)
                                {
                                    fs.ReadByte(); // sample precision
                                    int height = (fs.ReadByte() << 8) | fs.ReadByte();
                                    int width = (fs.ReadByte() << 8) | fs.ReadByte();
                                    return Tuple.Create(width, height);
                                }
                                fs.Position += length - 2;
                            }
                        }
                    }
                }
#endif
                // Fallback to full image loading
#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR_WIN
                using (Image originalImage = Image.FromFile(path))
                {
                    return Tuple.Create(originalImage.Width, originalImage.Height);
                }
#else
                Texture2D tmpTexture = new Texture2D(1, 1);
                byte[] assetContent = File.ReadAllBytes(path);
                if (tmpTexture.LoadImage(assetContent))
                {
                    return Tuple.Create(tmpTexture.width, tmpTexture.height);
                }
                return null;
#endif
            }
            catch (Exception e)
            {
                if (!ignoreErrors && AI.Config.LogImageExtraction)
                {
                    Debug.LogWarning($"Could not determine image dimensions for '{file}': {e.Message}");
                }
                return null;
            }
        }

#if UNITY_2021_2_OR_NEWER && UNITY_EDITOR_WIN
        public static bool ResizeImage(string originalFile, string outputFile, int maxSize, bool scaleBeyondSize = true, ImageFormat format = null)
        {
            Image originalImage; // leave here as otherwise temp files will be created by FromFile() for yet unknown reasons 
            try
            {
                using (originalImage = Image.FromFile(IOUtils.ToLongPath(originalFile)))
                {
                    int originalWidth = originalImage.Width;
                    int originalHeight = originalImage.Height;

                    // Calculate the scaling
                    double ratioX = (double)maxSize / originalWidth;
                    double ratioY = (double)maxSize / originalHeight;
                    double ratio = Math.Min(ratioX, ratioY);

                    int newWidth = Math.Max(1, (int)(originalWidth * ratio));
                    int newHeight = Math.Max(1, (int)(originalHeight * ratio));

                    if (!scaleBeyondSize && (newWidth > originalWidth || newHeight > originalHeight))
                    {
                        newWidth = originalWidth;
                        newHeight = originalHeight;
                    }

                    // Create a new empty image with the new dimensions
                    using (Bitmap newImage = new Bitmap(newWidth, newHeight))
                    {
                        using (Graphics graphics = Graphics.FromImage(newImage))
                        {
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                        }

                        // Save the resized image
                        string dir = Path.GetDirectoryName(outputFile);
                        Directory.CreateDirectory(dir);
                        newImage.Save(IOUtils.ToLongPath(outputFile), format != null ? format : ImageFormat.Png); // Adjust the format based on your needs
                    }
                }
            }
            catch (Exception e)
            {
                if (AI.Config.LogImageExtraction)
                {
                    Debug.LogWarning($"Could not resize image '{originalFile}': {e.Message}");
                }
                return false;
            }
            return true;
        }
#endif

        public static Texture2D Resize(this Texture2D source, int size)
        {
            int targetX = size;
            int targetY = size;

            if (source.width > source.height) targetY = (int)(targetX * ((float)source.height / source.width));
            if (source.height > source.width) targetX = (int)(targetY * ((float)source.width / source.height));

            RenderTexture rt = RenderTexture.GetTemporary(targetX, targetY, 24, RenderTextureFormat.Default);
            RenderTexture.active = rt;
            UnityEngine.Graphics.Blit(source, rt);
            Texture2D result = new Texture2D(targetX, targetY, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.filterMode = FilterMode.Trilinear;
            result.wrapMode = TextureWrapMode.Clamp;
            result.hideFlags = source.hideFlags;
            result.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}