//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;

namespace Emgu.CV.CvEnum
{

    /// <summary>
    /// cvLoadImage type
    /// </summary>
    [Flags]
    public enum ImreadModes
    {
        /// <summary>
        /// If set, return the loaded image as is (with alpha channel, otherwise it gets cropped).
        /// </summary>
        Unchanged = -1,

        /// <summary>
        /// If set, always convert image to the single channel grayscale image.
        /// </summary>
        Grayscale = 0,

        /// <summary>
        /// If set, always convert image to the 3 channel BGR color image.
        /// </summary>
        ColorBgr = 1,

        /// <summary>
        /// If set, return 16-bit/32-bit image when the input has the corresponding depth, otherwise convert it to 8-bit.
        /// </summary>
        AnyDepth = 2,

        /// <summary>
        /// If set, the image is read in any possible color format.
        /// </summary>
        AnyColor = 4,

        /// <summary>
        /// If set, use the gdal driver for loading the image.
        /// </summary>
        LoadGdal = 8,

        /// <summary>
        /// If set, always convert image to the single channel grayscale image and the image size reduced 1/2.
        /// </summary>
        ReducedGrayscale2 = 16,

        /// <summary>
        /// If set, always convert image to the 3 channel BGR color image and the image size reduced 1/2.
        /// </summary>
        ReducedColor2 = 17,

        /// <summary>
        /// If set, always convert image to the single channel grayscale image and the image size reduced 1/4.
        /// </summary>
        ReducedGrayscale4 = 32,

        /// <summary>
        /// If set, always convert image to the 3 channel BGR color image and the image size reduced 1/4.
        /// </summary>
        ReducedColor4 = 33,

        /// <summary>
        /// If set, always convert image to the single channel grayscale image and the image size reduced 1/8.
        /// </summary>
        ReducedGrayscale8 = 64,

        /// <summary>
        /// If set, always convert image to the 3 channel BGR color image and the image size reduced 1/8.
        /// </summary>
        ReducedColor8 = 65,

        /// <summary>
        /// If set, do not rotate the image according to EXIF's orientation flag.
        /// </summary>
        IgnoreOrientation = 128,

        /// <summary>
        /// If set, always convert image to the 3 channel RGB color image.
        /// </summary>
        ColorRgb = 256,
    }

}
