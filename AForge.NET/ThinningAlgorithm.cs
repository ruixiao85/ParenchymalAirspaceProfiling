using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace AForge.Imaging.Filters
{


	/// <summary>
	/// Guo-Hall Thinning sub-iterations
	/// </summary>
	public class Thinning3x3 : BaseUsingCopyPartialFilter
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
		/// Initializes a new instance of the <see cref="Thinning3x3"/> class.
		/// </summary>
		/// 
		public Thinning3x3()
		{
			// initialize format translation dictionary
			formatTranslations[PixelFormat.Format8bppIndexed]=PixelFormat.Format8bppIndexed;
		}
		/// <summary>
		/// false: not even number of sub-iteration (1st); true: even number of sub-iteration (2nd).
		/// </summary>
		public bool Even { get; set; } = false;
		/// <summary>
		/// How many changes occured is this iteration?
		/// </summary>
		public int Count { get; set; } = 0;

		/// <summary>
		/// Process the filter on the specified image.
		/// </summary>
		/// 
		/// <param name="sourceData">Source image data.</param>
		/// <param name="destinationData">Destination image data.</param>
		/// <param name="rect">Image rectangle for processing by the filter.</param>
		/// 
		/// <exception cref="InvalidImagePropertiesException">Processing rectangle mast be at least 3x3 in size.</exception>
		/// 
		protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData, Rectangle rect)
		{
			if ((rect.Width<3)||(rect.Height<3)) {
				throw new InvalidImagePropertiesException("Processing rectangle mast be at least 3x3 in size.");
			}

			// processing start and stop X,Y positions
			int startX = rect.Left+1;
			int startY = rect.Top+1;
			int stopX = rect.Right-1;
			int stopY = rect.Bottom-1;

			int dstStride = destinationData.Stride;
			int srcStride = sourceData.Stride;

			int dstOffset = dstStride-rect.Width+1;
			int srcOffset = srcStride-rect.Width+1;

			// image pointers
			byte* src = (byte*)sourceData.ImageData.ToPointer();
			byte* dst = (byte*)destinationData.ImageData.ToPointer();

			// allign pointers by X and Y
			src+=(startX-1)+(startY-1)*srcStride;
			dst+=(startX-1)+(startY-1)*dstStride;

			// --- process the first line setting all to black
			for (int x = startX-1; x<stopX; x++, src++, dst++) {
				*dst=0;
			}
			*dst=0;
			src+=srcOffset;
			dst+=dstOffset;

			// --- process all lines except the last one
			for (int y = startY; y<stopY; y++) {
				// set edge pixel to black
				*dst=0;
				src++;
				dst++;

				// for each pixel
				for (int x = startX; x<stopX; x++, src++, dst++) {

					//if (*dst!=0 && Thinning3x3Alg.ZhangSuen(Even, src[-srcStride]!=0, src[-srcStride+1]!=0, src[1]!=0, src[srcStride+1]!=0,
					//						src[srcStride]!=0, src[srcStride-1]!=0, src[-1]!=0, src[-srcStride-1]!=0)) {
					//	Count++;
					//	*dst=0;
					//}

					if (*dst!=0&&Thinning3x3Alg.GuoHall(Even, src[-srcStride]!=0, src[-srcStride+1]!=0, src[1]!=0, src[srcStride+1]!=0,
											src[srcStride]!=0, src[srcStride-1]!=0, src[-1]!=0, src[-srcStride-1]!=0)) {
						Count++;
						*dst=0;
					}

				}

				// set edge pixel to black
				*dst=0;
				src+=srcOffset;
				dst+=dstOffset;
			}

			// --- process the last line setting all to black

			// for each pixel
			for (int x = startX-1; x<stopX; x++, src++, dst++) {
				*dst=0;
			}
			*dst=0;
		}
	}

	public static class Thinning3x3Alg
	{
		public static bool GuoHall(bool even, bool p2, bool p3, bool p4, bool p5, bool p6, bool p7, bool p8, bool p9)
		{
			int C = Convert.ToInt32(!p2&(p3|p4))+Convert.ToInt32(!p4&(p5|p6))+Convert.ToInt32(!p6&(p7|p8))+Convert.ToInt32(!p8&(p9|p2));
			int N1 = Convert.ToInt32(p9|p2)+Convert.ToInt32(p3|p4)+Convert.ToInt32(p5|p6)+Convert.ToInt32(p7|p8);
			int N2 = Convert.ToInt32(p2|p3)+Convert.ToInt32(p4|p5)+Convert.ToInt32(p6|p7)+Convert.ToInt32(p8|p9);
			int N = (N1<N2) ? N1 : N2;
			int m = (even) ? Convert.ToInt32((p6|p7|!p9)&p8) : Convert.ToInt32((p2|p3|!p5)&p4);
			if (C==1&&(N>=2&&N<=3)&&m==0) return true;
			return false;
		}
		public static bool ZhangSuen(bool even, bool p2, bool p3, bool p4, bool p5, bool p6, bool p7, bool p8, bool p9)
		{
			int A = Convert.ToInt32((!p2&&p3))+Convert.ToInt32((!p3&&p4))+
					  Convert.ToInt32((!p4&&p5))+Convert.ToInt32((!p5&&p6))+
					  Convert.ToInt32((!p6&&p7))+Convert.ToInt32((!p7&&p8))+
					  Convert.ToInt32((!p8&&p9))+Convert.ToInt32((!p9&&p2)); //NumberOfZeroToOneTransitionFromP9
			int B = Convert.ToInt32(p2)+Convert.ToInt32(p3)+Convert.ToInt32(p4)+Convert.ToInt32(p5)+Convert.ToInt32(p6)+Convert.ToInt32(p7)+Convert.ToInt32(p8)+Convert.ToInt32(p9);
			if (A==1&&B>=2&&B<=6) {
				if (even) {
					if (!(p2&p4&p8)&!(p2&p6&p8)) return true;
				} else {
					if (!(p2&p4&p6)&!(p4&p6&p8)) return true;
				}
			}
			return false;
		}
	}
}