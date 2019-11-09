// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2005-2010
// contacts@aforgenet.com
//

namespace AForge.Imaging.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Fill holes in objects in binary image.
	/// </summary>
	/// 
	/// <remarks><para>The filter allows to fill black holes in white object in a binary image.
	/// It is possible to specify maximum holes' size to fill using <see cref="MaxHoleWidth"/>
	/// and <see cref="MaxHoleHeight"/> properties.</para>
	/// 
	/// <para>The filter accepts binary image only, which are represented  as 8 bpp images.</para>
	/// 
	/// <para>Sample usage:</para>
	/// <code>
	/// // create and configure the filter
	/// FillHoles filter = new FillHoles( );
	/// filter.MaxHoleHeight = 20;
	/// filter.MaxHoleWidth  = 20;
	/// filter.CoupledSizeFiltering = false;
	/// // apply the filter
	/// Bitmap result = filter.Apply( image );
	/// </code>
	/// 
	/// <para><b>Initial image:</b></para>
	/// <img src="img/imaging/sample19.png" width="320" height="240" />
	/// <para><b>Result image:</b></para>
	/// <img src="img/imaging/filled_holes.png" width="320" height="240" />
	/// </remarks>
	/// 
	public class FillHolesArea : BaseInPlaceFilter
	{
		// private format translation dictionary
		private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

		// maximum hole area to fill
		private int maxArea = int.MaxValue;
		/// <summary>Maximum Area of a hole to fill.</summary>
		/// <remarks><para>All holes, which have area greater to this value, are kept unfilled.       
		/// <para>Default value is set to <see cref="int.MaxValue"/>.</para></remarks>
		public int MaxHoleArea {
			get { return maxArea; }
			set { maxArea=Math.Max(value, 0); }
		}

		public int NumFilled { get;set; } = 0;
		public int AreaFilled { get;set; } = 0;
		public int NumUnfilled { get;set; } = 0;
		public int AreaUnfilled { get;set; } = 0;

		/// <summary>
		/// Format translations dictionary.
		/// </summary>
		public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
			get { return formatTranslations; }
		}

		/// <summary>   
		/// Initializes a new instance of the <see cref="FillHoles"/> class.
		/// </summary>
		public FillHolesArea()
		{
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format8bppIndexed;
		}

		/// <summary>
		/// Process the filter on the specified image.
		/// </summary>
		/// 
		/// <param name="image">Source image data.</param>
		///
		protected override unsafe void ProcessFilter(UnmanagedImage image)
		{
			int width = image.Width;
			int height = image.Height;

			// 1 - invert the source image
			Invert invertFilter = new Invert();
			UnmanagedImage invertedImage = invertFilter.Apply(image);

			// 2 - use blob counter to find holes (they are white objects now on the inverted image)
			BlobCounter blobCounter = new BlobCounter();
			blobCounter.ProcessImage(invertedImage);
			Blob[] blobs = blobCounter.GetObjectsInformation();
						
			// 3 - check all blobs and determine which should be filtered
			byte[] newObjectColors = new byte[blobs.Length+1];
			newObjectColors[0]=255; // don't touch the objects, which have 0 ID

			for (int i = 0, n = blobs.Length; i<n; i++) {
				Blob blob = blobs[i];

				if ((blob.Rectangle.Left==0)||(blob.Rectangle.Top==0)||
					  (blob.Rectangle.Right==width)||(blob.Rectangle.Bottom==height)) {
					newObjectColors[blob.ID]=0; // background image
				} else {
					if (blob.Area<=maxArea) {
						newObjectColors[blob.ID]=255; // fill hole, set to bright 255
						NumFilled++; AreaFilled+=blob.Area;
					} else {
						newObjectColors[blob.ID]=0; // do no fill, remain dark zero
						NumUnfilled++; AreaUnfilled+=blob.Area;
					}
				}
			}

			// 4 - process the source image image and fill holes
			byte* ptr = (byte*)image.ImageData.ToPointer();
			int offset = image.Stride-width;

			int[] objectLabels = blobCounter.ObjectLabels;

			for (int y = 0, i = 0; y<height; y++) {
				for (int x = 0; x<width; x++, i++, ptr++) {
					*ptr=newObjectColors[objectLabels[i]];
				}
				ptr+=offset;
			}
		}
	}
}
