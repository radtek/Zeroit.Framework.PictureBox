﻿// ***********************************************************************
// Assembly         : Zeroit.Framework.PictureBox
// Author           : ZEROIT
// Created          : 12-20-2018
//
// Last Modified By : ZEROIT
// Last Modified On : 12-20-2018
// ***********************************************************************
// <copyright file="Utils.cs" company="Zeroit Dev Technologies">
//    This program is for creating Image controls.
//    Copyright ©  2017  Zeroit Dev Technologies
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
//    You can contact me at zeroitdevnet@gmail.com or zeroitdev@outlook.com
// </copyright>
// <summary></summary>
// ***********************************************************************
#region Imports

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
//using System.Windows.Forms.VisualStyles;
using System.Windows.Forms;

#endregion

namespace Zeroit.Framework.PictureBox
{

    #region Util

    static public class Util
    {
        /// <summary>
        /// Load colored cursor handle from a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "LoadCursorFromFileW", CharSet = CharSet.Unicode)]
        static public extern IntPtr LoadCursorFromFile(string fileName);

        /// <summary>
        /// Create cursor from embedded cursor
        /// </summary>
        /// <param name="cursorResourceName">embedded cursor resource name</param>
        /// <returns>cursor</returns>
        public static Cursor CreateCursorFromFile(String cursorResourceName)
        {
            // read cursor resource binary data
            Stream inputStream = GetEmbeddedResourceStream(cursorResourceName);
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, buffer.Length);
            inputStream.Close();

            // create temporary cursor file
            String tmpFileName = System.IO.Path.GetRandomFileName();
            FileInfo tempFileInfo = new FileInfo(tmpFileName);
            FileStream outputStream = tempFileInfo.Create();
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Close();

            // create cursor
            IntPtr cursorHandle = LoadCursorFromFile(tmpFileName);
            Cursor cursor = new Cursor(cursorHandle);

            tempFileInfo.Delete();  // delete temporary cursor file
            return cursor;
        }

        /// <summary>
        /// Get image from embedded resource in the given assembly
        /// </summary>
        /// <param name="resourceName">resouce name</param>
        /// <returns>embedded image</returns>
        public static Image GetImageFromEmbeddedResource(string resourceName)
        {
            Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);
            return new Bitmap(stream);
        }

        /// <summary>
        /// create a thumbnail from the original image
        /// </summary>
        /// <param name="originalImage">the original image from which the thumbnail is created</param>
        /// <param name="imageHeight">the height of the thumbnail</param>
        /// <returns></returns>
        static public Image CreateThumbnail(Image originalImage, int imageHeight)
        {
            // create thumbnail
            float ratio = (float)originalImage.Width / originalImage.Height;
            int imageWidth = (int)(imageHeight * ratio);

            // set the thumbnail image
            Image thumbnailImage = originalImage.GetThumbnailImage(imageWidth, imageHeight,
                new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);

            return thumbnailImage;
        }

        /// <summary>
        /// Required, but not used
        /// </summary>
        /// <returns>true</returns>
        static private bool ThumbnailCallback()
        {
            return true;
        }

        /// <summary>
        /// calculate the correct rectangle according to the image size and targetArea.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="targetArea"></param>
        /// <returns></returns>
        static public Rectangle ScaleToFit(Image image, Rectangle targetArea, bool strechToFit)
        {
            Rectangle result;
            if (image.Width < targetArea.Width && image.Height < targetArea.Height)
            {
                if (strechToFit)
                {
                    float widthRatio = (float)targetArea.Width / (float)image.Width;
                    float heightRatio = (float)targetArea.Height / (float)image.Height;
                    float minRatio = Math.Min(widthRatio, heightRatio);
                    result = new Rectangle(targetArea.X, targetArea.Y, (int)(image.Width * minRatio), (int)(image.Height * minRatio));
                    if (result.Width < targetArea.Width)
                    {
                        result.X += (targetArea.Width - result.Width) / 2;
                    }
                    if (result.Height < targetArea.Height)
                    {
                        result.Y += (targetArea.Height - result.Height) / 2;
                    }
                }
                else
                {
                    // the image size is less than the targetArea size
                    result = new Rectangle(targetArea.Location, image.Size);
                    result.X += (targetArea.Width - result.Width) / 2;
                    result.Y += (targetArea.Height - result.Height) / 2;
                }
            }
            else	// the width or height of the image is greater than the targetArea
            {
                result = new Rectangle(targetArea.Location, targetArea.Size);
                // determine best fit: width or height
                if (image.Width * result.Height > image.Height * result.Width)
                {
                    // final width should match target, determine and center height
                    result.Height = result.Width * image.Height / image.Width;
                    result.Y += (targetArea.Height - result.Height) / 2;
                }
                else
                {
                    // final height should match target, determine and center width
                    result.Width = result.Height * image.Width / image.Height;
                    result.X += (targetArea.Width - result.Width) / 2;
                }
            }

            return result;
        }

        /// <summary>
        /// Get image from embedded resource in excuting assembly
        /// </summary>
        /// <param name="resourceName">resouce name</param>
        /// <returns>embedded image</returns>
        internal static Image GetImageFromZeroitScalablePicBoxEmbeddedResource(string resourceName)
        {
            return new Bitmap(GetEmbeddedResourceStream(resourceName));
        }

        /// <summary>
        /// Get embedded resource stream
        /// </summary>
        /// <param name="resourceName">resource name</param>
        /// <returns>the stream of embedded resource</returns>
        private static Stream GetEmbeddedResourceStream(string resourceName)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }
    }

    #endregion

}
