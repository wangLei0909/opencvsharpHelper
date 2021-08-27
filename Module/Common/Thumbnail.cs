using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OpenCVSharpHelper.Common
{
    class Thumbnail
    {
		private static bool ThumbnailCallback()
		{
			return false;
		}

		/// <summary>
		/// 创建低内存的缩略图
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="panelWidth"></param>
		/// <param name="panelHeight"></param>
		/// <returns></returns>
		public static BitmapImage CreateThumbnailLowMemory(string fileName, double panelWidth, double panelHeight)
		{
			Image image = null;
			Image image2 = null;
			try
			{
				Image.GetThumbnailImageAbort callback = ThumbnailCallback;
				image2 = Image.FromStream(new MemoryStream(File.ReadAllBytes(fileName)));
				if (image2 == null)
				{
					return null;
				}
				double val = panelWidth / image2.Width;
				double val2 = panelHeight / image2.Height;
				float num = (float)Math.Min(val, val2);
				int thumbWidth = image2.Width;
				int thumbHeight = image2.Height;
				if (num < 1f)
				{
					thumbWidth = (int)Math.Round(image2.Width * num);
					thumbHeight = (int)Math.Round(image2.Height * num);
				}
				var memoryStream = new MemoryStream();
				image = image2.GetThumbnailImage(thumbWidth, thumbHeight, callback, IntPtr.Zero);
				using (Bitmap bitmap = new (image.Width, image.Height))
				{
					bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.Clear(Color.White);
						graphics.DrawImageUnscaled(image, 0, 0);
					}
					bitmap.Save(memoryStream, ImageFormat.Bmp);
				}
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.DecodePixelHeight = (int)panelHeight;
				bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				bitmapImage.Freeze();
				return bitmapImage;
			}
			catch
			{
				return null;
			}
			finally
			{
				image2?.Dispose();
				image?.Dispose();
			}
		}

		public static void GetImageSize(string fileName, out int imageWidth, out int imageHeight)
		{
			using Stream bitmapStream = new MemoryStream(File.ReadAllBytes(fileName));
			BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
			imageWidth = bitmapFrame.PixelWidth;
			imageHeight = bitmapFrame.PixelHeight;
		}

		public static BitmapImage CreateRoiThumbnail(string fileName, double panelWidth, double panelHeight, Int32Rect rect)
		{
			try
			{
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.SourceRect = rect;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				using (Stream streamSource = new MemoryStream(File.ReadAllBytes(fileName)))
				{
					bitmapImage.StreamSource = streamSource;
					bitmapImage.DecodePixelHeight = (int)panelHeight;
					bitmapImage.DecodePixelWidth = (int)panelWidth;
					bitmapImage.EndInit();
				}
				return bitmapImage;
			}
			catch
			{
				return null;
			}
			finally {

                Console.WriteLine("finally");
			}
		}

		public static Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
		{
			var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			int stride = bitmapData.Stride;
			int num = stride - width;
			IntPtr scan = bitmapData.Scan0;
			int num2 = stride * height;
			int num3 = 0;
			int num4 = 0;
			byte[] array = new byte[num2];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					array[num3++] = rawValues[num4++];
				}
				num3 += num;
			}
			Marshal.Copy(array, 0, scan, num2);
			bitmap.UnlockBits(bitmapData);
			ColorPalette palette;
			using (Bitmap bitmap2 = new  (1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
			{
				palette = bitmap2.Palette;
			}
			for (int k = 0; k < 256; k++)
			{
				palette.Entries[k] = System.Drawing.Color.FromArgb(k, k, k);
			}
			bitmap.Palette = palette;
			return bitmap;
		}
	}
}
