// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright ?AForge.NET, 2005-2011
// contacts@aforgenet.com
//

namespace AForge.Imaging.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Merge filter - get MAX of pixels in two images.
	/// </summary>
	/// 
	/// <remarks><para>The merge filter takes two images (source and overlay images)
	/// of the same size and pixel format and produces an image, where each pixel equals
	/// to the maximum value of corresponding pixels from provided images.</para>
	/// 
	/// <para>The filter accepts 8 and 16 bpp grayscale images and 24, 32, 48 and 64 bpp
	/// color images for processing.</para>
	/// 
	/// <para>Sample usage:</para>
	/// <code>
	/// // create filter
	/// Merge filter = new Merge( overlayImage );
	/// // apply the filter
	/// Bitmap resultImage = filter.Apply( sourceImage );
	/// </code>
	///
	/// <para><b>Source image:</b></para>
	/// <img src="img/imaging/sample6.png" width="320" height="240" />
	/// <para><b>Overlay image:</b></para>
	/// <img src="img/imaging/sample7.png" width="320" height="240" />
	/// <para><b>Result image:</b></para>
	/// <img src="img/imaging/merge.png" width="320" height="240" />
	/// </remarks>
	/// 
	
	public sealed class TissueMapping : BaseInPlaceFilter2
	{
		// private format translation dictionary
		private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

		/// <summary>
		/// Format translations dictionary.
		/// </summary>
		public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
			get { return formatTranslations; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Merge"/> class
		/// </summary>
		public TissueMapping()
		{
			InitFormatTranslations();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Merge"/> class.
		/// </summary>
		/// 
		/// <param name="overlayImage">Overlay image.</param>
		/// 
		public TissueMapping(Bitmap overlayImage)
				: base(overlayImage)
		{
			InitFormatTranslations();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Merge"/> class.
		/// </summary>
		/// 
		/// <param name="unmanagedOverlayImage">Unmanaged overlay image.</param>
		/// 
		public TissueMapping(UnmanagedImage unmanagedOverlayImage)
			 : base(unmanagedOverlayImage)
		{
			InitFormatTranslations();
		}

		// Initialize format translation dictionary
		private void InitFormatTranslations()
		{
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format8bppIndexed;
			formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format24bppRgb;

		}

		public double ROffset {get;set;}=0.0d;
		public double RMultiplier {get;set;}=1.0d;
		public double GOffset {get;set;}=0.0d;
		public double GMultiplier {get;set;}=1.0d;
		public double BOffset {get;set;}=0.0d;
		public double BMultiplier {get;set;}=1.0d;

		/// <summary>
		/// Process the filter on the specified image.
		/// </summary>
		/// 
		/// <param name="image">Source image data.</param>
		/// <param name="overlay">Overlay image data.</param>
		///
		protected override unsafe void ProcessFilter(UnmanagedImage image, UnmanagedImage overlay)
		{
			PixelFormat srcpixelFormat = image.PixelFormat;
			int srcpixelSize = (srcpixelFormat==PixelFormat.Format8bppIndexed) ? 1 :
				 (srcpixelFormat==PixelFormat.Format24bppRgb) ? 3 : 4;
			PixelFormat ovrpixelFormat = overlay.PixelFormat;
			int ovrpixelSize = (ovrpixelFormat==PixelFormat.Format8bppIndexed) ? 1 :
				 (ovrpixelFormat==PixelFormat.Format24bppRgb) ? 3 : 4;
			

			// should have the same image dimension
			int width = image.Width; int height = image.Height;
			// initialize other variables
			int srclineSize = width*srcpixelSize;
			int ovrlineSize = width*ovrpixelSize;
			int srcOffset = image.Stride-srclineSize;
			int ovrOffset = overlay.Stride-ovrlineSize;
			byte* src = (byte*)image.ImageData.ToPointer();
			byte* ovr = (byte*)overlay.ImageData.ToPointer();

			for (int y = 0; y<height; y++) // each line
			{
				for (int x = 0; x<srclineSize; x++, src++, ovr++) // each pixel
				{					
					if (*ovr!=0) src[RGB.R]=(byte)Math.Round(RMultiplier*(ROffset+src[RGB.R]));
					if (*ovr!=0) src[RGB.G]=(byte)Math.Round(GMultiplier*(GOffset+src[RGB.G]));
					if (*ovr!=0) src[RGB.B]=(byte)Math.Round(BMultiplier*(BOffset+src[RGB.B]));
				}
				src+=srcOffset; ovr+=ovrOffset;
			}

		}
	}
}
