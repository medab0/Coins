using ImageProcess2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coins
{
    public class CoinCounter
    {
        //Returns a sequence of processes (Grayscale -> Smooth -> Resize to 500 -> Binary) to make the image
        //simpler to count the coins
        public static Bitmap ProcessCoin(Bitmap b, int threshold)
        {
            Bitmap newImage = new Bitmap(b);
            float aspectRatio = (float)b.Width / b.Height;
            int newWidth, newHeight;

            BitmapFilter.GrayScale(newImage);
            BitmapFilter.Smooth(newImage, 1);

            if (500 / (float)500 > aspectRatio)
            {
                newWidth = (int)(500 * aspectRatio);
                newHeight = 500;
            }
            else
            {
                newWidth = 500;
                newHeight = (int)(500 / aspectRatio);
            }

            BitmapFilter.Resize(newImage, newWidth, newHeight, false);

            //System.Diagnostics.Debug.WriteLine(b.Width + " " + b.Height);

            BitmapData bmData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - newImage.Width * 3;

                for (int y = 0; y < newImage.Height; y++)
                {
                    for (int x = 0; x < newImage.Width; x++)
                    {
                        byte pixelValue = p[0];

                        byte binaryValue = (pixelValue >= threshold) ? (byte)255 : (byte)0;

                        p[0] = binaryValue; // Blue
                        p[1] = binaryValue; // Green
                        p[2] = binaryValue; // Red

                        p += 3;
                    }
                    p += nOffset;
                }

            }

            newImage.UnlockBits(bmData);

            return newImage;
        }

        public static double CountCoins(Bitmap processed)
        {
            int fiveCentCount = 0, tenCentCount = 0, twentyFiveCentCount = 0, onePesoCount = 0, fivePesoCount = 0;
            bool[,] visited = new bool[processed.Width, processed.Height];

            for (int y = 0; y < processed.Height; y++)
            {
                for (int x = 0; x < processed.Width; x++)
                {
                    if (!visited[x, y] && IsBlackPixel(processed, x, y))
                    {
                        List<Point> circlePixels = new List<Point>();
                        FloodFill(processed, x, y, visited, circlePixels);

                        Rectangle boundingBox = GetBoundingBox(circlePixels);
                        int area = boundingBox.Width * boundingBox.Height;

                        DrawRectangle(processed, boundingBox, area);

                        if (area <= 2000) // none
                        {
                            continue;
                        }
                        else if (area <= 3000) // 5 cent
                        {
                            fiveCentCount++;
                        }
                        else if (area <= 4020)  // 10 cent
                        {
                            tenCentCount++;
                        }
                        else if (area <= 5000)  // 25 cent
                        {
                            twentyFiveCentCount++;
                        }
                        else if (area <= 6000)  // 1 peso
                        {
                            onePesoCount++;
                        }
                        else // 5 peso
                        {
                            fivePesoCount++;
                        }
                    }
                }
            }

            double amount = 0.0;
            amount += fiveCentCount * 0.05 + tenCentCount * 0.1 + twentyFiveCentCount * 0.25 + onePesoCount + fivePesoCount * 5;
            return amount;
        }

        // Helper method to check if the pixel is black
        public static bool IsBlackPixel(Bitmap processed, int x, int y)
        {
            Color pixelColor = processed.GetPixel(x, y);
            return pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0;
        }

        // Flood fill (BFS) to find all connected black pixels of a circle
        public static void FloodFill(Bitmap processed, int x, int y, bool[,] visited, List<Point> circlePixels)
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x, y));

            while (stack.Count > 0)
            {
                Point current = stack.Pop();
                int cx = current.X;
                int cy = current.Y;

                if (cx < 0 || cy < 0 || cx >= processed.Width || cy >= processed.Height)
                    continue;  // Out of bounds

                if (visited[cx, cy] || !IsBlackPixel(processed, cx, cy))
                    continue;  // Already visited or not black

                visited[cx, cy] = true;
                circlePixels.Add(current);

                // Push neighbors (4 directions: up, down, left, right)
                stack.Push(new Point(cx - 1, cy)); // Left
                stack.Push(new Point(cx + 1, cy)); // Right
                stack.Push(new Point(cx, cy - 1)); // Up
                stack.Push(new Point(cx, cy + 1)); // Down
            }
        }

        // Get bounding box for a group of points
        public static Rectangle GetBoundingBox(List<Point> points)
        {
            int minX = points.Min(p => p.X);
            int maxX = points.Max(p => p.X);
            int minY = points.Min(p => p.Y);
            int maxY = points.Max(p => p.Y);

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        // Draw a rectangle with different colors based on the size of the circle
        public static void DrawRectangle(Bitmap processed, Rectangle boundingBox, int area)
        {
            using (Graphics g = Graphics.FromImage(processed))
            {
                // Define colors based on size categories
                Color rectangleColor = Color.Black;  // Default color

                if (area <= 2000) // none
                {
                    rectangleColor = Color.White;
                }
                else if (area <= 3000) // 5 cent
                {
                    rectangleColor = Color.Red;
                }
                else if (area <= 4000)  // 10 cent
                {
                    rectangleColor = Color.Yellow;
                }
                else if (area <= 5000)  // 25 cent
                {
                    rectangleColor = Color.Green;
                }
                else if (area <= 6000)  // 1 peso
                {
                    rectangleColor = Color.Blue;
                }
                else // 5 peso
                {
                    rectangleColor = Color.Violet;
                }

                using (Pen pen = new Pen(rectangleColor, 2))
                {
                    g.DrawRectangle(pen, boundingBox);
                }
            }
        }
    }
}
