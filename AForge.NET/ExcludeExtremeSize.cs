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
	/// Add fillter - add pixel values of two images.
	/// </summary>
	/// 
	/// <remarks><para>The add filter takes two images (source and overlay images)
	/// of the same size and pixel format and produces an image, where each pixel equals
	/// to the sum value of corresponding pixels from provided images (if sum is greater
	/// than maximum allowed value, 255 or 65535, then it is truncated to that maximum).</para>
	/// 
	/// <para>The filter accepts 8 and 16 bpp grayscale images and 24, 32, 48 and 64 bpp
	/// color images for processing.</para>
	/// 
	/// <para>Sample usage:</para>
	/// <code>
	/// // create filter
	/// Add filter = new Add( overlayImage );
	/// // apply the filter
	/// Bitmap resultImage = filter.Apply( sourceImage );
	/// </code>
	/// 
	/// <para><b>Source image:</b></para>
	/// <img src="img/imaging/sample6.png" width="320" height="240" />
	/// <para><b>Overlay image:</b></para>
	/// <img src="img/imaging/sample7.png" width="320" height="240" />
	/// <para><b>Result image:</b></para>
	/// <img src="img/imaging/add.png" width="320" height="240" />
	/// </remarks>
	/// 
	/// <seealso cref="Merge"/>
	/// <seealso cref="Intersect"/>
	/// <seealso cref="Subtract"/>
	/// <seealso cref="Difference"/>
	/// 
	public sealed class ExcludeExtremeSize : BaseInPlaceFilter2
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
		/// Initializes a new instance of the <see cref="ExcludeExtremeSize"/> class.
		/// </summary>
		public ExcludeExtremeSize()
		{
			InitFormatTranslations();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtraLargeLabelling"/> class.
		/// </summary>
		/// 
		/// <param name="overlayImage">Overlay image.</param>
		/// 
		public ExcludeExtremeSize(Bitmap overlayImage)
			 : base(overlayImage)
		{
			InitFormatTranslations();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtraLargeLabelling"/> class.
		/// </summary>
		/// 
		/// <param name="unmanagedOverlayImage">Unmanaged overlay image.</param>
		/// 
		public ExcludeExtremeSize(UnmanagedImage unmanagedOverlayImage)
			 : base(unmanagedOverlayImage)
		{
			InitFormatTranslations();
		}

		// Initialize format translation dictionary
		private void InitFormatTranslations()
		{
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format8bppIndexed;
			formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format24bppRgb;
			formatTranslations[PixelFormat.Format32bppRgb]=PixelFormat.Format32bppRgb;
			formatTranslations[PixelFormat.Format32bppArgb]=PixelFormat.Format32bppArgb;
			formatTranslations[PixelFormat.Format16bppGrayScale]=PixelFormat.Format16bppGrayScale;
			formatTranslations[PixelFormat.Format48bppRgb]=PixelFormat.Format48bppRgb;
			formatTranslations[PixelFormat.Format64bppArgb]=PixelFormat.Format64bppArgb;
		}

		public int MinimumSize { get; set; }=int.MinValue;
		public int MaximumSize { get; set; }=int.MaxValue;

		/// <summary>
		/// Process the filter on the specified image.
		/// </summary>
		/// 
		/// <param name="image">Source image data.</param>
		/// <param name="overlay">Overlay image data.</param>
		///
		protected override unsafe void ProcessFilter(UnmanagedImage image, UnmanagedImage overlay)
		{
			PixelFormat pixelFormat = image.PixelFormat;
			// process the image
			BlobxCounter blobCounter=new BlobxCounter();
			blobCounter.ProcessImage(overlay);
			int[] labels = blobCounter.ObjectLabels;
			Blobx[] blobs = blobCounter.GetObjectsInformation();

			int width = image.Width;
			int height = image.Height;

			if (
				 (pixelFormat==PixelFormat.Format8bppIndexed)||
				 (pixelFormat==PixelFormat.Format24bppRgb)||
				 (pixelFormat==PixelFormat.Format32bppRgb)||
				 (pixelFormat==PixelFormat.Format32bppArgb)) {

				// initialize other variables
				int pixelSize = (pixelFormat==PixelFormat.Format8bppIndexed) ? 1 :
					 (pixelFormat==PixelFormat.Format24bppRgb) ? 3 : 4;
				int lineSize = width*pixelSize;
				int srcOffset = image.Stride-lineSize;
				int ovrOffset = overlay.Stride-lineSize;

				// do the job
				byte* ptr = (byte*)image.ImageData.ToPointer();
				byte* ovr = (byte*)overlay.ImageData.ToPointer();

				int p=0; // initial position
				// for each line
				for (int y = 0; y<height; y++) {
					// for each pixel
					for (int x = 0; x<lineSize; x++, ptr++, ovr++, p++) {
						if (*ptr==0 && labels[p]!=0 &&
							(blobs[labels[p]-1].Area>MaximumSize || blobs[labels[p]-1].Area<MinimumSize) ) {
							*ptr=(byte)255;
						} 
					}						
					ptr+=srcOffset;
					ovr+=ovrOffset;
				}
			} else {
				// initialize other variables
				int pixelSize = (pixelFormat==PixelFormat.Format16bppGrayScale) ? 1 :
					 (pixelFormat==PixelFormat.Format48bppRgb) ? 3 : 4;
				int lineSize = width*pixelSize;
				int srcStride = image.Stride;
				int ovrStride = overlay.Stride;

				// do the job
				byte* basePtr = (byte*)image.ImageData.ToPointer();
				byte* baseOvr = (byte*)overlay.ImageData.ToPointer();
				int p=0;
				// for each line
				for (int y = 0; y<height; y++) {
					ushort* ptr = (ushort*)(basePtr+y*srcStride);
					ushort* ovr = (ushort*)(baseOvr+y*ovrStride);
					// for each pixel
					for (int x = 0; x<lineSize; x++, ptr++, ovr++, p++) {
						if (*ptr==0 && labels[p]!=0 &&
							(blobs[labels[p]-1].Area>MaximumSize || blobs[labels[p]-1].Area<MinimumSize) ) {
							*ptr=(ushort)65535;	
						}						
					}
				}
			}
						

		}
	}
}
