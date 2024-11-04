using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace HNUDIP
{
    static class ImageProcess
    {

        public static Bitmap CopyImage(Bitmap source)
        {
            Bitmap processed = new Bitmap(source.Width, source.Height, source.PixelFormat);
            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);

            // Lock the bits of both images to work directly with the pixel data
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData processedData = processed.LockBits(rect, ImageLockMode.WriteOnly, processed.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            int widthInBytes = source.Width * bytesPerPixel;

            // Copy the pixel data row by row
            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* processedPtr = (byte*)processedData.Scan0;

                for (int y = 0; y < source.Height; y++)
                {
                    byte* srcRow = sourcePtr + (y * sourceData.Stride);
                    byte* destRow = processedPtr + (y * processedData.Stride);

                    Buffer.MemoryCopy(srcRow, destRow, widthInBytes, widthInBytes);
                }
            }

            // Unlock the bits
            source.UnlockBits(sourceData);
            processed.UnlockBits(processedData);

            return processed;
        }

        public static Bitmap Greyscale(Bitmap source)
        {
            Bitmap processed = new Bitmap(source.Width, source.Height, source.PixelFormat);

            // Lock the bits of both images to work directly with the pixel data
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData processedData = processed.LockBits(rect, ImageLockMode.WriteOnly, processed.PixelFormat);

            // Calculate the number of bytes per pixel
            int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;

            // Convert the pixel data to grayscale
            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* processedPtr = (byte*)processedData.Scan0;

                for (int y = 0; y < source.Height; y++)
                {
                    byte* srcRow = sourcePtr + (y * sourceData.Stride);
                    byte* destRow = processedPtr + (y * processedData.Stride);

                    for (int x = 0; x < source.Width; x++)
                    {
                        int index = x * bytesPerPixel;

                        // Extract RGB components
                        byte b = srcRow[index];       // Blue
                        byte g = srcRow[index + 1];   // Green
                        byte r = srcRow[index + 2];   // Red

                        // Calculate the grayscale value
                        byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

                        // Set the RGB components of the processed image to the grayscale value
                        destRow[index] = gray;       // Blue
                        destRow[index + 1] = gray;   // Green
                        destRow[index + 2] = gray;   // Red

                        if (bytesPerPixel == 4)
                        {
                            destRow[index + 3] = srcRow[index + 3]; // Alpha
                        }
                    }
                }
            }

            // Unlock the bits
            source.UnlockBits(sourceData);
            processed.UnlockBits(processedData);

            return processed;
        }

        public static Bitmap Inverted(Bitmap source)
        {
            // Create a new inverted bitmap with the same dimensions and format as the source
            Bitmap processed = new Bitmap(source.Width, source.Height, source.PixelFormat);

            // Lock the bits of both images to work directly with the pixel data
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData processedData = processed.LockBits(rect, ImageLockMode.WriteOnly, processed.PixelFormat);

            // Calculate the number of bytes per pixel
            int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;

            // Invert the pixel data
            unsafe
            {
                byte* sourcePtr = (byte*)sourceData.Scan0;
                byte* processedPtr = (byte*)processedData.Scan0;

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        int index = y * sourceData.Stride + x * bytesPerPixel;

                        // Invert RGB components
                        processedPtr[index] = (byte)(255 - sourcePtr[index]);       // Blue
                        processedPtr[index + 1] = (byte)(255 - sourcePtr[index + 1]); // Green
                        processedPtr[index + 2] = (byte)(255 - sourcePtr[index + 2]); // Red

                        // If there is an alpha channel, copy it directly
                        if (bytesPerPixel == 4)
                        {
                            processedPtr[index + 3] = sourcePtr[index + 3];
                        }
                    }
                }
            }

            // Unlock the bits
            source.UnlockBits(sourceData);
            processed.UnlockBits(processedData);

            return processed;
        }

        public static void Fliphorizontal(ref Bitmap a, ref Bitmap b)
        {
            b = new Bitmap(a.Width, a.Height);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color data = a.GetPixel(x, y);
                    b.SetPixel(x, (a.Height-1)-y, data);
                }

            }

        }

        public static void FlipVertical(ref Bitmap a, ref Bitmap b)
        {
            b = new Bitmap(a.Width, a.Height);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color data = a.GetPixel(x, y);
                    b.SetPixel((a.Width-1)-x, y, data);
                }

            }

        }

        public static void Scale(ref Bitmap a, ref Bitmap b, int nwidth, int nheight)
        {
            int targetWidth = nwidth;
            int targetHeight = nheight;
            int xTarget, yTarget, xSource, ySource;
            int width = a.Width;
            int height = a.Height;
            b = new Bitmap(targetWidth, targetHeight);

            for (xTarget = 0; xTarget < targetWidth; xTarget++)
            {
                for (yTarget = 0; yTarget < targetHeight; yTarget++)
                {
                    xSource = xTarget * width / targetWidth;
                    ySource = yTarget * height / targetHeight;
                    b.SetPixel(xTarget, yTarget, a.GetPixel(xSource, ySource));
                }
            }
        }

        public static void Subtract(ref Bitmap a, ref Bitmap b,ref Bitmap result, int value)
        {
            result = new Bitmap(a.Width, a.Height);
            byte agraydata = 0;
            byte bgraydata = 0;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color adata = a.GetPixel(x, y);
                    Color bdata = b.GetPixel(x, y);

                    agraydata = (byte)((adata.R + adata.G + adata.B) / 3);
                    bgraydata = (byte)((bdata.R + bdata.G + bdata.B) / 3);
                    if (Math.Abs(agraydata-bgraydata) > value)
                        result.SetPixel(x, y, Color.Red);
                    else
                        result.SetPixel(x, y,bdata);

                }

            }

        }


        public static void Threshold(ref Bitmap a, ref Bitmap b, int value)
        {
            b = new Bitmap(a.Width, a.Height);
            byte graydata = 0;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color data = a.GetPixel(x, y);
                    graydata=(byte)((data.R+data.G+data.B)/3);
                    if(graydata>value)
                    b.SetPixel(x, y, Color.White);
                    else
                    b.SetPixel(x, y, Color.Black);
                    
                }

            }
        
        }

        public static void Rotate(ref Bitmap a, ref Bitmap b, int value)
        {
            float angleRadians = (float)value;
            int xCenter = (int)(a.Width / 2);
            int yCenter = (int)(a.Height / 2);
            int width, height, xs, ys, xp, yp, x0, y0;
            float cosA, sinA;
            cosA = (float)Math.Cos(angleRadians);
            sinA = (float)Math.Sin(angleRadians);
            width = a.Width;
            height = a.Height;
            b = new Bitmap(width, height);
            for (xp = 0; xp < width; xp++)
            {
                for (yp = 0; yp < height; yp++)
                {
                    x0 = xp - xCenter;     // translate to (0,0)
                    y0 = yp - yCenter;
                    xs = (int)(x0 * cosA + y0 * sinA);   // rotate around the origin
                    ys = (int)(-x0 * sinA + y0 * cosA);
                    xs = (int)(xs + xCenter);  // translate back to (xCenter,yCenter)
                    ys = (int)(ys + yCenter);
                    xs = Math.Max(0, Math.Min(width - 1, xs));  // force the source location to within image bounds
                    ys = Math.Max(0, Math.Min(height - 1, ys));
                    b.SetPixel(xp, yp, a.GetPixel(xs, ys));
                }
            }
        }
        
        public static void Brightness(ref Bitmap a, ref Bitmap b, int value)
        {
            b = new Bitmap(a.Width, a.Height);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color temp = a.GetPixel(x, y);
                    Color changed;
                    if(value>0)
                    changed = Color.FromArgb(Math.Min(temp.R + value, 255), Math.Min(temp.G + value, 255), Math.Min(temp.B + value, 255));
                    else
                    changed = Color.FromArgb(Math.Max(temp.R + value, 0), Math.Max(temp.G + value, 0), Math.Max(temp.B + value, 0));
                    
                    b.SetPixel(x, y, changed);
                }
            }
        }
        public static void Equalisation(ref Bitmap a, ref Bitmap b, int degree)
        {
            int height = a.Height;
            int width = a.Width;
            int numSamples, histSum;
            int[] Ymap = new int[256];
            int[] hist = new int[256];
            int percent = degree;
            // compute the histogram from the sub-image
            Color nakuha;
            Color gray;
            Byte graydata;
            //compute greyscale
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    nakuha = a.GetPixel(x, y);
                    graydata = (byte)((nakuha.R + nakuha.G + nakuha.B) / 3);
                    gray = Color.FromArgb(graydata, graydata, graydata);
                    a.SetPixel(x, y, gray);
                }
            }
            //histogram 1d data;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    nakuha = a.GetPixel(x, y);
                    hist[nakuha.B]++;

                }
            }
            // remap the Ys, use the maximum contrast (percent == 100) 
            // based on histogram equalization
            numSamples = (a.Width * a.Height);   // # of samples that contributed to the histogram
            histSum = 0;
            for (int h = 0; h < 256; h++)
            {
                histSum += hist[h];
                Ymap[h] = histSum * 255 / numSamples;
            }

            // if desired contrast is not maximum (percent < 100), then adjust the mapping
            if (percent < 100)
            {
                for (int h = 0; h < 256; h++)
                {
                    Ymap[h] = h + ((int)Ymap[h] - h) * percent / 100;
                }
            }

            b = new Bitmap(a.Width, a.Height);
            // enhance the region by remapping the intensities
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                  // set the new value of the gray value
                    Color temp = Color.FromArgb(Ymap[a.GetPixel(x, y).R], Ymap[a.GetPixel(x, y).G], Ymap[a.GetPixel(x, y).B]);
                   b.SetPixel(x, y, temp);
                }

            }
          

        
        }



        public static Bitmap Histogram(Bitmap source)
        {
            // Grayscale Conversion and Histogram Data Calculation
            int[] histogramData = new int[256];

            Bitmap grayscaleImage = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, source.Width, source.Height);

            // Lock bits for source and grayscale images
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData grayData = grayscaleImage.LockBits(rect, ImageLockMode.WriteOnly, grayscaleImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            int stride = sourceData.Stride;
            int height = source.Height;
            int width = source.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* grayPtr = (byte*)grayData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * stride + x * bytesPerPixel;
                        byte grayValue = (byte)((srcPtr[idx] + srcPtr[idx + 1] + srcPtr[idx + 2]) / 3);

                        // Grayscale pixel write
                        grayPtr[idx] = grayPtr[idx + 1] = grayPtr[idx + 2] = grayValue;

                        // Histogram data update
                        histogramData[grayValue]++;
                    }
                }
            }

            // Unlock bits
            source.UnlockBits(sourceData);
            grayscaleImage.UnlockBits(grayData);

            // Bitmap Graph Generation
            Bitmap histogramImage = new Bitmap(256, 800);
            using (Graphics g = Graphics.FromImage(histogramImage))
            {
                g.Clear(Color.White);
            }

            // Plotting points based on histogram data
            int maxFrequency = histogramData.Max();
            double scale = (double)histogramImage.Height / maxFrequency;

            for (int x = 0; x < 256; x++)
            {
                int barHeight = (int)(histogramData[x] * scale);
                for (int y = 0; y < barHeight; y++)
                {
                    histogramImage.SetPixel(x, histogramImage.Height - 1 - y, Color.Black);
                }
            }

            return histogramImage;
        }

        public static Bitmap Sepia(Bitmap source)
        {
            Bitmap sepiaImage = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);

            // Lock bits for the source and sepia images
            BitmapData sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData sepiaData = sepiaImage.LockBits(rect, ImageLockMode.WriteOnly, sepiaImage.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(source.PixelFormat) / 8;
            int stride = sourceData.Stride;
            int height = source.Height;
            int width = source.Width;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* sepiaPtr = (byte*)sepiaData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * stride + x * bytesPerPixel;

                        // Get original RGB values
                        byte b = srcPtr[idx];
                        byte g = srcPtr[idx + 1];
                        byte r = srcPtr[idx + 2];

                        // Apply sepia tone transformation
                        byte sepiaR = (byte)Math.Min(255, (0.393 * r) + (0.769 * g) + (0.189 * b));
                        byte sepiaG = (byte)Math.Min(255, (0.349 * r) + (0.686 * g) + (0.168 * b));
                        byte sepiaB = (byte)Math.Min(255, (0.272 * r) + (0.534 * g) + (0.131 * b));

                        // Set sepia pixel values
                        sepiaPtr[idx] = sepiaB;
                        sepiaPtr[idx + 1] = sepiaG;
                        sepiaPtr[idx + 2] = sepiaR;
                    }
                }
            }

            // Unlock bits
            source.UnlockBits(sourceData);
            sepiaImage.UnlockBits(sepiaData);

            return sepiaImage;
        }


    }
}
