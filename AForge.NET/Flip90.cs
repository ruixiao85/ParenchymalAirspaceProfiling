namespace AForge.Imaging.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    
    public class Flip90 : BaseTransformationFilter
    {
        /// <summary>
        /// New image width.
        /// </summary>
        protected int newWidth;

        /// <summary>
        /// New image height.
        /// </summary>
        protected int newHeight;
        // format translation dictionary
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        /// <summary>
        /// Format translations dictionary.
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizeBilinear"/> class.
        /// </summary>
        /// 
        /// <param name="newWidth">Width of the new image.</param>
        /// <param name="newHeight">Height of the new image.</param>
        /// 
		public Flip90() 
		{            
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
        }
        /// <summary>
        /// Calculates new image size.
        /// </summary>
        /// 
        /// <param name="sourceData">Source image data.</param>
        /// 
        /// <returns>New image size - size of the destination image.</returns>
        /// 
        protected override System.Drawing.Size CalculateNewImageSize(UnmanagedImage sourceData)
        {
            this.newWidth = sourceData.Height;
            this.newHeight = sourceData.Width;
            return new Size(newWidth, newHeight);
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
            // get source image size
            int width = sourceData.Width;
            int height = sourceData.Height;

            int pixelSize = Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
            int srcStride = sourceData.Stride;
            int dstStride = destinationData.Stride;
            //int srcOffset = sourceData.Stride - pixelSize * width;
            //int dstOffset = destinationData.Stride - pixelSize * newWidth;

            // do the job
            byte* src = (byte*)sourceData.ImageData.ToPointer();
            byte* dst = (byte*)destinationData.ImageData.ToPointer();

            // for each line
            for (int y = 0; y < newHeight; y++) {
                // for each pixel
                for (int x = 0; x < newWidth; x++) {
                    for (int p = 0; p < pixelSize; ++p) {
                        dst[y * dstStride + x * pixelSize + p] = src[x * srcStride + y * pixelSize + p];
                    }
                }
            }
        }
    }
}