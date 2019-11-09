// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework///
// Copyright ?Andrew Kirillov, 2005-2009
// andrew.kirillov@aforgenet.com
//
// Adapted from ConnectedComponentsLabeling //
// AForge.Imaging.Filters.Others.AreaComponentsLabeling.cs

namespace AForge.Imaging.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Area components labeling.
	/// </summary>
	/// 
	/// <remarks><para>The filter performs labeling of objects in the source image. It colors
	/// each separate object using different color. The image processing filter treats all none
	/// black pixels as objects' pixels and all black pixel as background.</para>
	/// 
	/// <para>The filter accepts 8 bpp grayscale images and 24/32 bpp color images and produces
	/// 24 bpp RGB image.</para>
	///
	/// <para>Sample usage:</para>
	/// <code>
	/// // create filter
	/// AreaComponentsLabeling filter = new AreaComponentsLabeling( );
	/// // apply the filter
	/// Bitmap newImage = filter.Apply( image );
	/// // check objects count
	/// int objectCount = filter.ObjectCount;
	/// </code>
	/// 
	/// <para><b>Initial image:</b></para>
	/// <img src="img/imaging/sample2.jpg" width="320" height="240" />
	/// <para><b>Result image:</b></para>
	/// <img src="img/imaging/labeling.jpg" width="320" height="240" />
	/// </remarks>
	/// 
	public class ExcludeSize : BaseFilter
	{
		// private format translation dictionary
		private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

		/// <summary>
		/// Format translations dictionary.
		/// </summary>
		public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
			get { return formatTranslations; }
		}

		// blob counter
		private BlobxCounterBase blobCounter = new BlobxCounter();

		/// <summary>
		/// Blob counter used to locate separate blobs.
		/// </summary>
		/// 
		/// <remarks><para>The property allows to set blob counter to use for blobs' localization.</para>
		/// 
		/// <para>Default value is set to <see cref="BlobCounter"/>.</para>
		/// </remarks>
		/// 
		public BlobxCounterBase BlobCounter {
			get { return blobCounter; }
			set { blobCounter=value; }
		}
		
		/// <summary>
		/// Objects count.
		/// </summary>
		/// 
		/// <remarks>The amount of objects found in the last processed image.</remarks>
		/// 
		public int ObjectCount {
			get { return blobCounter.ObjectsCount; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExcludeSize"/> class.
		/// </summary>
		/// 
		public ExcludeSize()
		{
			// initialize format translation dictionary
			//formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format24bppRgb;
			//formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format24bppRgb;
			//formatTranslations[PixelFormat.Format32bppArgb]=PixelFormat.Format24bppRgb;
			//formatTranslations[PixelFormat.Format32bppPArgb]=PixelFormat.Format24bppRgb;
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format8bppIndexed;
			formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format8bppIndexed;
			formatTranslations[PixelFormat.Format32bppArgb]=PixelFormat.Format8bppIndexed;
			formatTranslations[PixelFormat.Format32bppPArgb]=PixelFormat.Format8bppIndexed;
		}

		public int Low { get; set; } = 0;
		public int High { get; set; } = int.MaxValue;		
		public int LowCount {get; set; }=0;
		public int HighCount {get; set; }=0;

		/// <summary>
		/// Process the filter on the specified image.
		/// </summary>
		/// 
		/// <param name="sourceData">Source image data.</param>
		/// <param name="destinationData">Destination image data.</param>
		/// 
		protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
		{
			// process the image
			blobCounter.ProcessImage(sourceData);

			// get object information
			int[] labels = blobCounter.ObjectLabels;
			Blobx[] blobs = blobCounter.GetObjectsInformation();

			int pixelsize=1; // 1: 8 bbp index 3: 24 bbp rgb
			int width = sourceData.Width;
			int height = sourceData.Height;
			int dstOffset = destinationData.Stride-width*pixelsize;
			byte* dst = (byte*)destinationData.ImageData.ToPointer();
			int p = 0;
			LowCount = HighCount = 0;

			// for each row
			for (int y = 0; y<height; y++) {
				// for each pixel
				for (int x = 0; x<width; x++, dst+=pixelsize, p++) {
					if (labels[p]!=0)	{
						if (blobs[labels[p]-1].Area<Low) {
							*dst=(byte)255; LowCount++;
						} else if (blobs[labels[p]-1].Area>High) {
							*dst=(byte)255; HighCount++;
						}
					}// else *dst=(byte)0;
				}
				dst+=dstOffset;
			}
		}	
	}
}
