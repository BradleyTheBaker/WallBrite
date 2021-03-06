﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WallBrite
{
    [Serializable]
    public class WBImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Average brightness calculated (or manually set) for this image
        /// </summary>
        public float AverageBrightness { get; private set; }

        /// <summary>
        /// Whether this WBImage will show up in the library
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creation date of this WBImage
        /// </summary>
        public DateTime AddedDate { get; private set; }

        /// <summary>
        /// Thumbnail image for use in library UI
        /// </summary>
        [JsonConverter(typeof(ThumbnailConverter))]
        public BitmapImage Thumbnail { get; private set; }

        /// <summary>
        /// Background color for use in library UI
        /// </summary>
        public SolidColorBrush BackgroundColor { get; private set; }

        /// <summary>
        /// Path to the original image
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Creates WBImage using data from externally created bitmap and given file path
        /// Calculates its average brightness, creates a proportionate thumbnail, sets creation date, and
        /// stores the given path to the original file
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="path"></param>
        public WBImage(Bitmap bitmap, string path)
        {
            Thumbnail = Helpers.GetThumbnailFromBitmap(bitmap);
            AverageBrightness = CalculateAverageBrightness(bitmap);
            BackgroundColor = GetBackgroundColor();
            AddedDate = DateTime.Now;
            Path = path;
            IsEnabled = true;
        }

        /// <summary>
        /// Creates WBImage from JSON file
        /// </summary>
        /// <param name="averageBrightness"></param>
        /// <param name="isEnabled"></param>
        /// <param name="addedDate"></param>
        /// <param name="thumbnail"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="path"></param>
        [JsonConstructor]
        public WBImage(float averageBrightness, bool isEnabled, DateTime addedDate, BitmapImage thumbnail, SolidColorBrush backgroundColor, string path)
        {
            Thumbnail = thumbnail;
            AverageBrightness = averageBrightness;
            IsEnabled = isEnabled;
            AddedDate = addedDate;
            BackgroundColor = backgroundColor;
            Path = path;
        }

        /// <summary>
        /// Gets appropriate background color for front-end; background color is a shade between black and
        /// white representing this WBImage's brightness value
        /// </summary>
        /// <returns></returns>
        public SolidColorBrush GetBackgroundColor()
        {
            int backgroundBrightness = (int)Math.Round(AverageBrightness * 255);
            SolidColorBrush backgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness,
                                                  (byte)backgroundBrightness));
            return backgroundColor;
        }

        /// <summary>
        /// Sets and returns average brightness level (from fully black at 0.0 to fully white at 1.0) of this
        /// WBImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Average brightness level of given bitmap image</returns>
        public float CalculateAverageBrightness(Bitmap image)
        {
            // Brightness value to be summed and averaged
            float averageBrightness = 0;

            // Get log of width and height of image; to be used when looping over pixels to
            // increase efficiency
            // (Rather than looping over every pixel, using the log will allow taking a sample
            //  of only pixels on every log(width)th column and log(height)th row)
            int widthLog = Convert.ToInt32(Math.Log(image.Width));
            int heightLog = Convert.ToInt32(Math.Log(image.Height));

            // Loop over image's pixels (taking only the logged sample as described above)
            for (int x = 0; x < image.Width; x += widthLog)
            {
                for (int y = 0; y < image.Height; y += heightLog)
                {
                    // For every sampled pixel, get the color value of the pixel and add its brightness to
                    // the running sum
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    averageBrightness += pixelColor.GetBrightness();
                }
            }

            // Divide summed brightness by the number of pixels sampled to get the average
            averageBrightness /= (image.Width / widthLog) * (image.Height / heightLog);

            return averageBrightness;
        }
    }
}