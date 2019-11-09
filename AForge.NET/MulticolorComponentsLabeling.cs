﻿// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright ?Andrew Kirillov, 2005-2009
// andrew.kirillov@aforgenet.com
//

namespace AForge.Imaging.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Connected components labeling.
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
	/// MulticolorComponentsLabeling filter = new MulticolorComponentsLabeling( );
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
	public class MulticolorComponentsLabeling : BaseFilter
	{
		// Color table for coloring objects
		private static Color[] colorTable = new Color[]
		{
			Color.Red,     Color.Green,   Color.Blue,       Color.Yellow,
			Color.Violet,  Color.Brown,   Color.Olive,      Color.Cyan,

			Color.Magenta, Color.Gold,    Color.Indigo,     Color.Ivory,
			Color.HotPink, Color.DarkRed, Color.DarkGreen,  Color.DarkBlue,

			Color.DarkSeaGreen,  Color.Gray, Color.DarkKhaki,  Color.DarkGray,
			Color.LimeGreen, Color.Tomato,   Color.SteelBlue,  Color.SkyBlue,

			Color.Silver,  Color.Salmon,  Color.SaddleBrown,   Color.RosyBrown,
			Color.PowderBlue, Color.Plum, Color.PapayaWhip, Color.Orange
		};

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
		/// Colors used to color the binary image.
		/// </summary>
		public static Color[] ColorTable {
			get { return colorTable; }
			set { colorTable=value; }
		}

		/// <summary>
		/// Specifies if blobs should be filtered.
		/// </summary>
		/// 
		/// <remarks><para>See documentation for <see cref="BlobCounterBase.FilterBlobs"/> property
		/// of <see cref="BlobCounterBase"/> class for more information.</para></remarks>
		/// 
		public bool FilterBlobs {
			get { return blobCounter.FilterBlobs; }
			set { blobCounter.FilterBlobs=value; }
		}

		/// <summary>
		/// Specifies if size filetering should be coupled or not.
		/// </summary>
		/// 
		/// <remarks><para>See documentation for <see cref="BlobCounterBase.CoupledSizeFiltering"/> property
		/// of <see cref="BlobCounterBase"/> class for more information.</para></remarks>
		/// 
		public bool CoupledSizeFiltering {
			get { return blobCounter.CoupledSizeFiltering; }
			set { blobCounter.CoupledSizeFiltering=value; }
		}

		/// <summary>
		/// Minimum allowed width of blob.
		/// </summary>
		/// 
		public int MinWidth {
			get { return blobCounter.MinWidth; }
			set { blobCounter.MinWidth=value; }
		}

		/// <summary>
		/// Minimum allowed height of blob.
		/// </summary>
		/// 
		public int MinHeight {
			get { return blobCounter.MinHeight; }
			set { blobCounter.MinHeight=value; }
		}

		/// <summary>
		/// Maximum allowed width of blob.
		/// </summary>
		/// 
		public int MaxWidth {
			get { return blobCounter.MaxWidth; }
			set { blobCounter.MaxWidth=value; }
		}

		/// <summary>
		/// Maximum allowed height of blob.
		/// </summary>
		/// 
		public int MaxHeight {
			get { return blobCounter.MaxHeight; }
			set { blobCounter.MaxHeight=value; }
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

		/// <summary>Minimum area size to be counted and colored </summary>
		public int Low { get; set; } = 0;

		/// <summary>Maximum area size to be counted and colored </summary>
		public int High { get; set; } = int.MaxValue;

		/// <summary>True: fill in the shape; False: only fill the perimeter, the outline.</summary>
		public bool FillShape { get; set; } = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="MulticolorComponentsLabeling"/> class.
		/// </summary>
		/// 
		public MulticolorComponentsLabeling()
		{
			// initialize format translation dictionary
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format24bppRgb;
			formatTranslations[PixelFormat.Format24bppRgb]=PixelFormat.Format24bppRgb;
			formatTranslations[PixelFormat.Format32bppArgb]=PixelFormat.Format24bppRgb;
			formatTranslations[PixelFormat.Format32bppPArgb]=PixelFormat.Format24bppRgb;
		}

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

			// get object labels
			int[] labels = blobCounter.ObjectLabels;
			Blobx[] blobs = blobCounter.GetObjectsInformation();
			// get width and height
			int width = sourceData.Width;
			int height = sourceData.Height;

			int dstOffset = destinationData.Stride-width*3;

			// do the job
			byte* dst = (byte*)destinationData.ImageData.ToPointer();
			int p = 0;

			// for each row
			for (int y = 0; y<height; y++) {
				// for each pixel
				for (int x = 0; x<width; x++, dst+=3, p++) {
					if (labels[p]!=0 && blobs[labels[p]-1].Area>=Low && blobs[labels[p]-1].Area<=High) {
						if (FillShape||(p-width-1<0)||(p+width+1>(width*height-1))||
									  labels[p-width-1]==0||labels[p-width]==0||labels[p-width+1]==0||
									  labels[p-1]==0||labels[p+1]==0||
									  labels[p+width-1]==0||labels[p+width]==0||labels[p+width+1]==0) {
							Color c = colorTable[(labels[p]-1)%colorTable.Length];
							dst[RGB.R]=c.R; dst[RGB.G]=c.G; dst[RGB.B]=c.B;
						}
					}
				}
				dst+=dstOffset;
			}
		}
	}
}
