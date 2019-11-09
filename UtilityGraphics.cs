using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Data;
using System.Globalization;
using Xceed.Wpf.AvalonDock;
using AForge.Imaging.Filters;

namespace LungMorphApp
{
	public static class UtilityGraphics
	{
		public static Bitmap BitmapImage2Bitmap(this BitmapImage bitmapImage)
		{
			try {
				using (var outStream = new MemoryStream()) {
					var enc = new BmpBitmapEncoder();
					enc.Frames.Add(BitmapFrame.Create(bitmapImage));
					enc.Save(outStream);
					return new Bitmap(outStream);
				}
			} catch { throw new Exception("Failed to convert from BitmapImage to Bitmap."); }
		}
		public static BitmapSource Bitmap2BitmapImage(this Bitmap bitmap)
		{
			if (bitmap==null) return null;
			IntPtr hBitmap = bitmap.GetHbitmap();
			try {
				return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());				
			} catch {
				throw new Exception("Failed to convert from Bitmap to BitmapImage.");
			} finally { // will temp store bitmapsource, deleteobject, then return 
				DeleteObject(hBitmap);
			}
		}
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);


		public static ImageCodecInfo getEncoder(this ImageFormat format)
		{
			try {
				ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
				foreach (ImageCodecInfo codec in codecs) {
					if (codec.FormatID==format.Guid) { return codec; }
				}
				return null;
			} catch { throw new Exception("Failed to get Encoder."); }
		}
		public static EncoderParameters jpgEncoderCQ(this long quality)
		{
			try {
				Encoder myEncoder = Encoder.Quality;
				EncoderParameters myEncoderParameters = new EncoderParameters(1);
				EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality); // compression quality
				myEncoderParameters.Param[0]=myEncoderParameter; // set up jpeg encoder and compression level
				return myEncoderParameters;
			} catch { throw new Exception("Failed to adjust jpg quality."); }
		}
		public static Bitmap To24bbpRgb(this Bitmap tempOri, double ResizeRatio = 1.0d, double CropRatio = 1.0d)
		{
			try {
				Bitmap Original = new Bitmap((int)Math.Round(ResizeRatio*tempOri.Width), (int)Math.Round(ResizeRatio*tempOri.Height), PixelFormat.Format24bppRgb);
				using (Graphics graphics = Graphics.FromImage(Original)) {
					graphics.CompositingQuality=CompositingQuality.HighQuality;
					graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode=SmoothingMode.HighQuality;
					graphics.DrawImage(tempOri, (int)Math.Round(ResizeRatio*(CropRatio-1.0d)*tempOri.Width/-2.0d), (int)Math.Round(ResizeRatio*(CropRatio-1.0d)*tempOri.Height/-2.0d), (int)Math.Round(ResizeRatio*CropRatio*tempOri.Width), (int)Math.Round(ResizeRatio*CropRatio*tempOri.Height));
				} 
				return Original;
			} catch { throw new Exception("Failed to convert to 24bbpRgb bitmap image."); }
		}
		public static Bitmap FileTo24bbpRgb(this string file, double ResizeRatio = 1.0d, double FrameCrop = 1.0d, double ImageZoom = 1.0d, int RotateDegree = 0)
		{
			try {
				StreamReader streamReader = new StreamReader(file);
				Bitmap tempOri = (Bitmap)Image.FromStream(streamReader.BaseStream, true);
				int CanvasWidth=(int)Math.Round(ResizeRatio*tempOri.Width);
				int CanvasHeight=(int)Math.Round(ResizeRatio*tempOri.Height);
				Bitmap Original = new Bitmap(CanvasWidth, CanvasHeight, PixelFormat.Format24bppRgb);
				using (Graphics graphics = Graphics.FromImage(Original)) {
					graphics.CompositingQuality=CompositingQuality.HighQuality;
					graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode=SmoothingMode.HighQuality;
					graphics.DrawImage(tempOri, (int)Math.Round(ResizeRatio*(ImageZoom-1.0d)*tempOri.Width/-2.0d), (int)Math.Round(ResizeRatio*(ImageZoom-1.0d)*tempOri.Height/-2.0d), (int)Math.Round(ResizeRatio*ImageZoom*tempOri.Width), (int)Math.Round(ResizeRatio*ImageZoom*tempOri.Height));
				} tempOri.Dispose(); streamReader.Dispose();
				if (FrameCrop!=1.0d) {
				    Crop cropfilter=new Crop(new Rectangle((int)Math.Round(FrameCrop*(1-FrameCrop)*CanvasWidth),
					    (int)Math.Round(FrameCrop*(1-FrameCrop)*CanvasHeight),
					    (int)Math.Round(FrameCrop*CanvasWidth), (int)Math.Round(FrameCrop*CanvasHeight)));
				    Original=cropfilter.Apply(Original);
                }
                if (RotateDegree!=0) {
                    RotateBicubic rotatefilter=new RotateBicubic(RotateDegree, keepSize:true);
                    Original=rotatefilter.Apply(Original);
                }
                return Original;
			} catch { throw new Exception("Failed to import the image files."); }
		}
		public static int Center2QuantileValue(this AForge.Math.Histogram hist, double center)
		{
			if (center>0.5) {
				return hist.GetRange((0.5-center)*2).Min;
			} else {
				return hist.GetRange((center-0.5)*2).Max;
			}
		}
		
	}


}
