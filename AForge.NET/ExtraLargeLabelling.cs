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
	public sealed class ExtraLargeLabelling : BaseInPlaceFilter2
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
		/// Initializes a new instance of the <see cref="ExtraLargeLabelling"/> class.
		/// </summary>
		public ExtraLargeLabelling()
		{
			InitFormatTranslations();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtraLargeLabelling"/> class.
		/// </summary>
		/// 
		/// <param name="overlayImage">Overlay image.</param>
		/// 
		public ExtraLargeLabelling(Bitmap overlayImage)
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
		public ExtraLargeLabelling(UnmanagedImage unmanagedOverlayImage)
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

		public int MaximumSize {get; set; }=int.MaxValue;

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
						if (*ptr==0 && labels[p]!=0 && blobs[labels[p]-1].Area>MaximumSize) {						
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
						if (*ptr==0 && labels[p]!=0 && blobs[labels[p]-1].Area>MaximumSize) {						
							*ptr=(ushort)65535;	
						}						
					}
				}
			}
						

		}
	}
}

//// AForge Image Processing Library
//// AForge.NET framework
//// http://www.aforgenet.com/framework///
//// Copyright ?Andrew Kirillov, 2005-2009
//// andrew.kirillov@aforgenet.com
////
//// Adapted from ConnectedComponentsLabeling //
//// AForge.Imaging.Filters.Others.ExtraLargeLabeling.cs

//namespace AForge.Imaging.Filters
//{
//	using System;
//	using System.Collections.Generic;
//	using System.Drawing;
//	using System.Drawing.Imaging;

//	/// <summary>
//	/// Area components labeling.
//	/// </summary>
//	/// 
//	/// <remarks><para>The filter performs labeling of objects in the source image. It colors
//	/// each separate object using different color. The image processing filter treats all none
//	/// black pixels as objects' pixels and all black pixel as background.</para>
//	/// 
//	/// <para>The filter accepts 8 bpp grayscale images and 24/32 bpp color images and produces
//	/// 24 bpp RGB image.</para>
//	///
//	/// <para>Sample usage:</para>
//	/// <code>
//	/// // create filter
//	/// ExtraLargeLabeling filter = new ExtraLargeLabeling( );
//	/// // apply the filter
//	/// Bitmap newImage = filter.Apply( image );
//	/// // check objects count
//	/// int objectCount = filter.ObjectCount;
//	/// </code>
//	/// 
//	/// <para><b>Initial image:</b></para>
//	/// <img src="img/imaging/sample2.jpg" width="320" height="240" />
//	/// <para><b>Result image:</b></para>
//	/// <img src="img/imaging/labeling.jpg" width="320" height="240" />
//	/// </remarks>
//	/// 
//	public class ExtraLargeLabeling : BaseFilter
//	{		
//		// private format translation dictionary
//		private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

//		/// <summary>
//		/// Format translations dictionary.
//		/// </summary>
//		public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
//			get { return formatTranslations; }
//		}

//		// blob counter
//		private BlobxCounterBase blobCounter = new BlobxCounter();

//		/// <summary>
//		/// Blob counter used to locate separate blobs.
//		/// </summary>
//		/// 
//		/// <remarks><para>The property allows to set blob counter to use for blobs' localization.</para>
//		/// 
//		/// <para>Default value is set to <see cref="BlobCounter"/>.</para>
//		/// </remarks>
//		/// 
//		public BlobxCounterBase BlobCounter {
//			get { return blobCounter; }
//			set { blobCounter=value; }
//		}

//		/// <summary>
//		/// Initializes a new instance of the <see cref="ExtraLargeLabeling"/> class.
//		/// </summary>
//		/// 
//		public ExtraLargeLabeling()
//		{
//			// initialize format translation dictionary
//			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format24bppRgb;
//			formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format24bppRgb;
//			formatTranslations[PixelFormat.Format32bppArgb]=PixelFormat.Format24bppRgb;
//			formatTranslations[PixelFormat.Format32bppPArgb]=PixelFormat.Format24bppRgb;
//		}

//		/// <summary>Larger than this size will be colored white.</summary>
//		public int MaximumSize { get; set; } = int.MaxValue;
//		/// <summary>True: fill in the shape; False: only fill the perimeter, the outline.</summary>
//		public bool FillShape { get; set; } = false;

//		/// <summary>
//		/// Process the filter on the specified image.
//		/// </summary>
//		/// 
//		/// <param name="sourceData">Source image data.</param>
//		/// <param name="destinationData">Destination image data.</param>
//		/// 
//		protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
//		{
//			// process the image
//			blobCounter.ProcessImage(sourceData);

//			// get object information
//			int[] labels = blobCounter.ObjectLabels;
//			Blobx[] blobs = blobCounter.GetObjectsInformation();
//			// get width and height
//			int width = sourceData.Width;
//			int height = sourceData.Height;

//			int dstOffset = destinationData.Stride-width*3;

//			// do the job
//			byte* dst = (byte*)destinationData.ImageData.ToPointer();
//			int p = 0;

//			Color c = Color.White;
//			// for each row
//			for (int y = 0; y<height; y++) {
//				// for each pixel
//				for (int x = 0; x<width; x++, dst+=3, p++) {
//					if (labels[p]!=0&&blobs[labels[p]-1].Area>MaximumSize) {
//						if (FillShape||(p-width-1<0)||(p+width+1>(width*height-1))||
//							  labels[p-width-1]==0||labels[p-width]==0||labels[p-width+1]==0||
//							  labels[p-1]==0||labels[p+1]==0||
//							  labels[p+width-1]==0||labels[p+width]==0||labels[p+width+1]==0) {
//							dst[RGB.R]=c.R; dst[RGB.G]=c.G; dst[RGB.B]=c.B; // assign color
//						}
//					}
//				}
//				dst+=dstOffset;
//			}
//		}
//	}
//}
