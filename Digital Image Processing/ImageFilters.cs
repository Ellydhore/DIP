using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace PBH
{
    internal class ImageFilters
    {
        // COPY IMAGE
        public Bitmap copyImage(Bitmap inputImage)
        {
            Bitmap processed = new Bitmap(inputImage.Width, inputImage.Height, inputImage.PixelFormat);
            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of both images to work directly with the pixel data
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, inputImage.PixelFormat);
            BitmapData processedData = processed.LockBits(rect, ImageLockMode.WriteOnly, processed.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(inputImage.PixelFormat) / 8;
            int widthInBytes = inputImage.Width * bytesPerPixel;

            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* processedPtr = (byte*)processedData.Scan0;

                for (int y = 0; y < inputImage.Height; y++)
                {
                    byte* srcRow = sourcePtr + (y * sourceData.Stride);
                    byte* destRow = processedPtr + (y * processedData.Stride);

                    Buffer.MemoryCopy(srcRow, destRow, widthInBytes, widthInBytes);
                }
            }

            // Unlock the bits
            inputImage.UnlockBits(sourceData);
            processed.UnlockBits(processedData);

            return processed;
        }
        //
        //
        // GRAYSCALE
        public Bitmap grayscale(Bitmap inputImage)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image and output image
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outputData = outputImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* outPtr = (byte*)outputData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;
                    byte* outRow = outPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte b = srcRow[idx];
                        byte g = srcRow[idx + 1];
                        byte r = srcRow[idx + 2];

                        byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                        outRow[idx] = gray;
                        outRow[idx + 1] = gray;
                        outRow[idx + 2] = gray;
                    }
                }
            }

            // Unlock bits for both images
            inputImage.UnlockBits(sourceData);
            outputImage.UnlockBits(outputData);

            return outputImage;
        }
        //
        //
        // INVERT IMAGE
        public Bitmap invert(Bitmap inputImage)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock bits for both the input and output images
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outputData = outputImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* outPtr = (byte*)outputData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;
                    byte* outRow = outPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        outRow[idx] = (byte)(255 - srcRow[idx]);
                        outRow[idx + 1] = (byte)(255 - srcRow[idx + 1]);
                        outRow[idx + 2] = (byte)(255 - srcRow[idx + 2]);
                    }
                }
            }

            // Unlock bits for both images
            inputImage.UnlockBits(sourceData);
            outputImage.UnlockBits(outputData);

            return outputImage;
        }
        //
        //
        // HISTOGRAM
        public Bitmap histogram(Bitmap inputImage)
        {
            int[] histogramData = new int[256];
            var rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the input image's bits for reading
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, inputImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(inputImage.PixelFormat) / 8;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * stride + x * bytesPerPixel;
                        byte grayValue = (byte)((srcPtr[idx] + srcPtr[idx + 1] + srcPtr[idx + 2]) / 3);
                        histogramData[grayValue]++;
                    }
                }
            }

            // Unlock bits
            inputImage.UnlockBits(sourceData);

            Bitmap outputImage = new Bitmap(256, 800);
            using (Graphics g = Graphics.FromImage(outputImage))
            {
                g.Clear(Color.White);
            }

            int maxFrequency = histogramData.Max();
            double scale = (double)outputImage.Height / maxFrequency;

            // Draw the histogram bars
            for (int x = 0; x < 256; x++)
            {
                int barHeight = (int)(histogramData[x] * scale);
                for (int y = 0; y < barHeight; y++)
                {
                    outputImage.SetPixel(x, outputImage.Height - 1 - y, Color.Black);
                }
            }

            return outputImage;
        }
        //
        //
        // SEPIA
        public Bitmap sepia(Bitmap inputImage)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);

            Rectangle rect = new Rectangle(0, 0, inputImage.Width, inputImage.Height);

            // Lock the bits of the source image and output image
            BitmapData sourceData = inputImage.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData outputData = outputImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int bytesPerPixel = 3;
            int stride = sourceData.Stride;
            int height = inputImage.Height;
            int width = inputImage.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* outPtr = (byte*)outputData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcPtr + y * stride;
                    byte* outRow = outPtr + y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int idx = x * bytesPerPixel;

                        byte b = srcRow[idx];
                        byte g = srcRow[idx + 1];
                        byte r = srcRow[idx + 2];

                        int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                        int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                        int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);

                        outRow[idx + 2] = (byte)(tr > 255 ? 255 : tr);
                        outRow[idx + 1] = (byte)(tg > 255 ? 255 : tg);
                        outRow[idx] = (byte)(tb > 255 ? 255 : tb);
                    }
                }
            }

            // Unlock bits for both images
            inputImage.UnlockBits(sourceData);
            outputImage.UnlockBits(outputData);

            return outputImage;
        }
        //
        //
        // SUBTRACT
        public Bitmap subtract(Bitmap inputImage, Bitmap backgroundImage)
        {
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height);
            int greenThreshold = 100;

            for (int x = 0; x < inputImage.Width; x++)
            {
                for (int y = 0; y < inputImage.Height; y++)
                {
                    Color pixel = inputImage.GetPixel(x, y);
                    Color backpixel = backgroundImage.GetPixel(x, y);

                    if (pixel.G > greenThreshold && pixel.G > pixel.R * 1.5 && pixel.G > pixel.B * 1.5)
                    {
                        outputImage.SetPixel(x, y, backpixel);
                    }
                    else
                    {
                        outputImage.SetPixel(x, y, pixel);
                    }
                }
            }
            return outputImage;
        }
    }
}
