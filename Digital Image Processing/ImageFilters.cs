using ImageProcess2;
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
    // CONVOLUTION MATRIX
    internal class ConvMatrix
    {
        public int TopLeft = 0, TopMid = 0, TopRight = 0;
        public int MidLeft = 0, Pixel = 1, MidRight = 0;
        public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
        public int Factor = 1;
        public int Offset = 0;
        public void SetAll(int nVal)
        {
            TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight = BottomLeft = BottomMid = BottomRight = nVal;
        }
    }
    internal class ImageFilters
    {
        // 3x3 CONVOLUTION
        public Bitmap Conv3x3(Bitmap inputImage, ConvMatrix m)
        {
            // Avoid divide by zero errors
            if (m.Factor == 0) return null;

            // Clone the input image as source
            Bitmap bSrc = (Bitmap)inputImage.Clone();

            // Create an output bitmap with the same size and pixel format
            Bitmap outputImage = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);

            // Lock bits of both the source and the output image
            BitmapData bmData = outputImage.LockBits(new Rectangle(0, 0, outputImage.Width, outputImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            int stride2 = stride * 2;
            IntPtr scan0 = bmData.Scan0;
            IntPtr srcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)scan0;
                byte* pSrc = (byte*)(void*)srcScan0;

                int nOffset = stride - inputImage.Width * 3;
                int nWidth = inputImage.Width - 1;
                int nHeight = inputImage.Height - 1;

                int nPixel;

                for (int y = 1; y < nHeight; ++y)
                {
                    for (int x = 1; x < nWidth; ++x)
                    {
                        // Apply the convolution matrix to each color channel
                        // RED channel
                        nPixel = (((pSrc[2] * m.TopLeft) + (pSrc[5] * m.TopMid) + (pSrc[8] * m.TopRight) +
                                   (pSrc[2 + stride] * m.MidLeft) + (pSrc[5 + stride] * m.Pixel) + (pSrc[8 + stride] * m.MidRight) +
                                   (pSrc[2 + stride2] * m.BottomLeft) + (pSrc[5 + stride2] * m.BottomMid) + (pSrc[8 + stride2] * m.BottomRight))
                                   / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[2] = (byte)nPixel;

                        // GREEN channel
                        nPixel = (((pSrc[1] * m.TopLeft) + (pSrc[4] * m.TopMid) + (pSrc[7] * m.TopRight) +
                                   (pSrc[1 + stride] * m.MidLeft) + (pSrc[4 + stride] * m.Pixel) + (pSrc[7 + stride] * m.MidRight) +
                                   (pSrc[1 + stride2] * m.BottomLeft) + (pSrc[4 + stride2] * m.BottomMid) + (pSrc[7 + stride2] * m.BottomRight))
                                   / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[1] = (byte)nPixel;

                        // BLUE channel
                        nPixel = (((pSrc[0] * m.TopLeft) + (pSrc[3] * m.TopMid) + (pSrc[6] * m.TopRight) +
                                   (pSrc[0 + stride] * m.MidLeft) + (pSrc[3 + stride] * m.Pixel) + (pSrc[6 + stride] * m.MidRight) +
                                   (pSrc[0 + stride2] * m.BottomLeft) + (pSrc[3 + stride2] * m.BottomMid) + (pSrc[6 + stride2] * m.BottomRight))
                                   / m.Factor) + m.Offset;

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[0] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }
                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            // Unlock bits for both images
            outputImage.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);

            // Return the processed image
            return outputImage;
        }
        //
        //
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
        //
        //
        // SMOOTH
        public Bitmap Smooth(Bitmap inputImage, int nWeight)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = nWeight;
            m.Factor = nWeight + 8;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // GAUSSIAN BLUR
        public Bitmap GaussianBlur(Bitmap inputImage, int nWeight)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = nWeight;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
            m.Factor = nWeight + 12;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // SHARPEN
        public Bitmap Sharpen(Bitmap inputImage, int nWeight)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(0);
            m.Pixel = nWeight;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = -2;
            m.Factor = nWeight - 8;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // MEAN REMOVAL
        public Bitmap MeanRemoval(Bitmap inputImage, int nWeight)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-1);
            m.Pixel = nWeight;
            m.Factor = nWeight - 8;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // EMBOSS LAPLACIAN
        public Bitmap EmbossLaplacian(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-1);
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 0;
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // EMBOSS HORIZONTAL / VERTICAL
        public Bitmap EmbossHV(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-1);
            m.TopRight = m.TopLeft = m.BottomRight = m.BottomLeft = 0;
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // EMBOSS ALL DIRECTION
        public Bitmap EmbossAD(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-1);
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // EMBOSS Lossy
        public Bitmap EmbossLossy(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(-2);
            m.TopLeft = m.TopRight = m.BottomMid = 1;
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
        //
        //
        // EMBOSS HORIZONTAL
        public Bitmap EmbossHorizontal(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(0);
            m.MidLeft = m.MidRight = -1;
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
        // EMBOSS HORIZONTAL
        public Bitmap EmbossVertical(Bitmap inputImage, int nWeight, int nOffset)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(0);
            m.TopMid = -1;
            m.BottomMid = 1;
            m.Pixel = nWeight;
            m.Offset = nOffset;

            return Conv3x3(inputImage, m);
        }
    }
}
