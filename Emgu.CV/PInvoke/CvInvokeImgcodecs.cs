﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.Util.TypeEnum;

namespace Emgu.CV
{
    public partial class CvInvoke
    {
        /// <summary>
        /// Loads an image from the specified file and returns the pointer to the loaded image. Currently the following file formats are supported: 
        /// Windows bitmaps - BMP, DIB; 
        /// JPEG files - JPEG, JPG, JPE; 
        /// Portable Network Graphics - PNG; 
        /// Portable image format - PBM, PGM, PPM; 
        /// Sun rasters - SR, RAS; 
        /// TIFF files - TIFF, TIF; 
        /// OpenEXR HDR images - EXR; 
        /// JPEG 2000 images - jp2. 
        /// </summary>
        /// <param name="filename">The name of the file to be loaded</param>
        /// <param name="loadType">The image loading type</param>
        /// <returns>The loaded image</returns>
        public static Mat Imread(String filename, CvEnum.ImreadModes loadType = ImreadModes.ColorBgr)
        {
            return new Mat(filename, loadType);
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        internal static extern IntPtr cveImread(
           IntPtr filename,
           CvEnum.ImreadModes loadType,
           IntPtr result);

        /// <summary>
        /// Returns true if the specified image can be decoded by OpenCV.
        /// </summary>
        /// <param name="fileName">File name of the image</param>
        /// <returns>True if the specified image can be decoded by OpenCV.</returns>
        public static bool HaveImageReader(String fileName)
        {
            using (CvString csFileName = new CvString(fileName))
                return cveHaveImageReader(csFileName);
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        internal static extern bool cveHaveImageReader(IntPtr filename);

        /// <summary>
        /// Returns true if an image with the specified filename can be encoded by OpenCV.
        /// </summary>
        /// <param name="fileName">File name of the image</param>
        /// <returns>True if an image with the specified filename can be encoded by OpenCV.</returns>
        public static bool HaveImageWriter(String fileName)
        {
            using (CvString csFileName = new CvString(fileName))
                return cveHaveImageWriter(csFileName);
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        internal static extern bool cveHaveImageWriter(IntPtr filename);

        /// <summary>
        /// Save multiple images to a specified file (e.g. ".tiff" that support multiple images).
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <param name="images">Images to be saved.</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>true if success</returns>
        public static bool Imwritemulti(String filename, IInputArrayOfArrays images, params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (CvString strFilename = new CvString(filename))
            using (Util.VectorOfInt vec = new Util.VectorOfInt())
            using (InputArray iaImages = images.GetInputArray())
            {
                PushParameters(vec, parameters);
                return cveImwritemulti(strFilename, iaImages, vec);
            }
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        internal static extern bool cveImwritemulti(IntPtr filename, IntPtr mats, IntPtr flags);


        /// <summary>
        /// The function imreadmulti loads a multi-page image from the specified file into a vector of Mat objects.
        /// </summary>
        /// <param name="filename">Name of file to be loaded.</param>
        /// <param name="flags">Read flags</param>
        /// <returns>Null if the reading fails, otherwise, an array of Mat from the file</returns>
        public static Mat[] Imreadmulti(String filename, CvEnum.ImreadModes flags = ImreadModes.AnyColor)
        {
            using (VectorOfMat vm = new VectorOfMat())
            using (CvString strFilename = new CvString(filename))
            {
                if (!cveImreadmulti(strFilename, vm, flags))
                    return null;
                Mat[] result = new Mat[vm.Size];

                for (int i = 0; i < result.Length; i++)
                {
                    Mat m = new Mat();
                    CvInvoke.Swap(m, vm[i]);
                    result[i] = m;
                }
                return result;
            }
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        internal static extern bool cveImreadmulti(IntPtr filename, IntPtr mats, CvEnum.ImreadModes flags);

        private static void PushParameters(VectorOfInt vec, KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return;
            foreach (KeyValuePair<CvEnum.ImwriteFlags, int> p in parameters)
            {
                vec.Push(new int[] { (int)p.Key, p.Value });
            }
        }

        /// <summary>
        /// Saves the image to the specified file. The function imwrite saves the image to the specified file. The image format is chosen based on the filename extension (see cv::imread for the list of extensions).
        /// </summary>
        /// <param name="filename">The name of the file to be saved to</param>
        /// <param name="image">The image to be saved</param>
        /// <param name="parameters">The parameters</param>
        /// <remarks>In general, only 8-bit single-channel or 3-channel (with 'BGR' channel order) images can be saved using this function, with these exceptions:
        /// 16-bit unsigned(CV_16U) images can be saved in the case of PNG, JPEG 2000, and TIFF formats
        /// 32-bit float (CV_32F) images can be saved in PFM, TIFF, OpenEXR, and Radiance HDR formats; 3-channel(CV_32FC3) TIFF images will be saved using the LogLuv high dynamic range encoding(4 bytes per pixel)
        /// PNG images with an alpha channel can be saved using this function.To do this, create 8-bit (or 16-bit) 4-channel image BGRA, where the alpha channel goes last. Fully transparent pixels should have alpha set to 0, fully opaque pixels should have alpha set to 255 / 65535(see the code sample below).
        /// Multiple images(vector of Mat) can be saved in TIFF format(see the code sample below).
        /// If the image format is not supported, the image will be converted to 8 - bit unsigned(CV_8U) and saved that way.
        /// If the format, depth or channel order is different, use Mat::convertTo and cv::cvtColor to convert it before saving. Or, use the universal FileStorage I / O functions to save the image to XML or YAML format.</remarks>
        /// <returns>true if success</returns>
        public static bool Imwrite(String filename, IInputArray image, params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (Util.VectorOfInt vec = new Util.VectorOfInt())
            {
                PushParameters(vec, parameters);

                using (CvString s = new CvString(filename))
                {
                    bool containsUnicode = (s.Length != filename.Length);
                    if (containsUnicode &&
                        (Emgu.Util.Platform.OperationSystem != Emgu.Util.Platform.OS.MacOS) &&
                        (Emgu.Util.Platform.OperationSystem != Emgu.Util.Platform.OS.Linux))
                    {
                        //Handle unicode in Windows platform
                        //Work around for Open CV ticket:
                        //https://github.com/Itseez/opencv/issues/4292
                        //https://github.com/Itseez/opencv/issues/4866     
                        System.IO.FileInfo fi = new System.IO.FileInfo(filename);

                        using (VectorOfByte vb = new VectorOfByte())
                        {
                            CvInvoke.Imencode(fi.Extension, image, vb, parameters);
                            byte[] arr = vb.ToArray();
                            System.IO.File.WriteAllBytes(filename, arr);
                            return true;
                        }
                    }
                    else
                        using (InputArray iaImage = image.GetInputArray())
                            return cveImwrite(s, iaImage, vec);
                }
            }
        }
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImwrite(IntPtr filename, IntPtr image, IntPtr parameters);

        /// <summary>
        /// Decode image stored in the buffer
        /// </summary>
        /// <param name="buf">The buffer</param>
        /// <param name="loadType">The image loading type</param>
        /// <param name="dst">The output placeholder for the decoded matrix.</param>
        public static void Imdecode(byte[] buf, CvEnum.ImreadModes loadType, Mat dst)
        {
            using (VectorOfByte vb = new VectorOfByte(buf))
            {
                Imdecode((IInputArray)vb, loadType, dst);
            }
        }

        /// <summary>
        /// Decode image stored in the buffer
        /// </summary>
        /// <param name="buf">The buffer</param>
        /// <param name="loadType">The image loading type</param>
        /// <param name="dst">The output placeholder for the decoded matrix.</param>
        public static void Imdecode(Stream buf, CvEnum.ImreadModes loadType, Mat dst)
        {
            using (VectorOfByte vb = new VectorOfByte(buf))
            {
                Imdecode((IInputArray)vb, loadType, dst);
            }
        }

        /// <summary>
        /// Decode image stored in the buffer
        /// </summary>
        /// <param name="buf">The buffer</param>
        /// <param name="loadType">The image loading type</param>
        /// <param name="dst">The output placeholder for the decoded matrix.</param>
        public static void Imdecode(IInputArray buf, CvEnum.ImreadModes loadType, Mat dst)
        {
            using (InputArray iaBuffer = buf.GetInputArray())
                cveImdecode(iaBuffer, loadType, dst);
        }
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        private static extern void cveImdecode(IntPtr buf, CvEnum.ImreadModes loadType, IntPtr dst);

        /// <summary>
        /// Reads a multi-page image from a buffer in memory.
        /// </summary>
        /// <param name="buf">Input array of bytes.</param>
        /// <param name="loadType">The same flags as in Imread</param>
        /// <param name="mats">A vector of Mat objects holding each page, if more than one.</param>
        /// <param name="range">A continuous selection of pages.</param>
        /// <returns>The function imdecodemulti reads a multi-page image from the specified buffer in the memory. If the buffer is too short or contains invalid data, the function returns false.</returns>
        public static bool Imdecodemulti(
            byte[] buf,
            CvEnum.ImreadModes loadType,
            VectorOfMat mats,
            Emgu.CV.Structure.Range range = new Emgu.CV.Structure.Range())
        {
            using (VectorOfByte vBuffer = new VectorOfByte(buf))
            {
                return Imdecodemulti(vBuffer, loadType, mats, range);
            }
        }

        /// <summary>
        /// Reads a multi-page image from a buffer in memory.
        /// </summary>
        /// <param name="buf">Input array or vector of bytes.</param>
        /// <param name="loadType">The same flags as in Imread</param>
        /// <param name="mats">A vector of Mat objects holding each page, if more than one.</param>
        /// <param name="range">A continuous selection of pages.</param>
        /// <returns>The function imdecodemulti reads a multi-page image from the specified buffer in the memory. If the buffer is too short or contains invalid data, the function returns false.</returns>
        public static bool Imdecodemulti(
            IInputArray buf, 
            CvEnum.ImreadModes loadType, 
            VectorOfMat mats, 
            Emgu.CV.Structure.Range range = new Emgu.CV.Structure.Range())
        {
            using (var iaBuf = buf.GetInputArray())
            {
                return cveImdecodemulti(iaBuf, (int) loadType, mats, ref range);
            }
        }
        
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImdecodemulti(IntPtr buf, int flags, IntPtr mats, ref Emgu.CV.Structure.Range range);

        /// <summary>
        /// Encode image and return the result as a byte vector.
        /// </summary>
        /// <param name="ext">The image format</param>
        /// <param name="image">The image</param>
        /// <param name="parameters">The pointer to the array of integers, which contains the parameter for encoding, use IntPtr.Zero for default</param>
        /// <returns>Byte array that contains the image in the specific image format. If failed to encode, return null</returns>
        public static byte[] Imencode(String ext, IInputArray image, params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (VectorOfByte vb = new VectorOfByte())
            {
                if (!Imencode(ext, image, vb, parameters))
                    return null;
                return vb.ToArray();
            }
        }

        /// <summary>
        /// Encode image and store the result as a byte vector.
        /// </summary>
        /// <param name="ext">The image format</param>
        /// <param name="image">The image</param>
        /// <param name="buf">Output buffer resized to fit the compressed image.</param>
        /// <param name="parameters">The pointer to the array of integers, which contains the parameter for encoding, use IntPtr.Zero for default</param>
        /// <returns>True if successfully encoded the image into the buffer.</returns>
        public static bool Imencode(String ext, IInputArray image, VectorOfByte buf, params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (CvString extStr = new CvString(ext))
            using (VectorOfInt p = new VectorOfInt())
            {
                PushParameters(p, parameters);
                using (InputArray iaImage = image.GetInputArray())
                    return cveImencode(extStr, iaImage, buf, p);
            }
        }

        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImencode(IntPtr ext, IntPtr image, IntPtr buffer, IntPtr parameters);

        /// <summary>
        /// Encodes array of images into a memory buffer.
        /// </summary>
        /// <param name="ext">File extension that defines the output format. Must include a leading period.</param>
        /// <param name="imgs">Vector of images to be written.</param>
        /// <param name="parameters">Format-specific parameters.</param>
        /// <returns>Output compressed data</returns>
        public static byte[] Imencodemulti(
            string ext, 
            IInputArrayOfArrays imgs,
            params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (var buffer = new Util.VectorOfByte())
            {
                if (!Imencodemulti(ext, imgs, buffer, parameters))
                    return null;
                return buffer.ToArray();
            }
        }

        /// <summary>
        /// Encodes array of images into a memory buffer.
        /// </summary>
        /// <param name="ext">File extension that defines the output format. Must include a leading period.</param>
        /// <param name="imgs">Vector of images to be written.</param>
        /// <param name="buf">Output buffer resized to fit the compressed data.</param>
        /// <param name="parameters">Format-specific parameters.</param>
        /// <returns>True if encoding is successful</returns>
        public static bool Imencodemulti(
            string ext, 
            IInputArrayOfArrays imgs, 
            VectorOfByte buf,
            params KeyValuePair<CvEnum.ImwriteFlags, int>[] parameters)
        {
            using (var extPtr = new CvString(ext))
            using (var imgsPtr = imgs.GetInputArray())
            using (var paramVec = new VectorOfInt())
            {
                PushParameters(paramVec, parameters);
                return cveImencodemulti(extPtr, imgsPtr, buf, paramVec);
            }
        }
        
        [DllImport(ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImencodemulti(IntPtr ext, IntPtr imgs, IntPtr buffer, IntPtr parameters);

        /// <summary>
        /// Loads frames from an animated image file into an Animation structure.
        /// </summary>
        /// <param name="fileName">A string containing the path to the file.</param>
        /// <param name="animation">A reference to an Animation structure where the loaded frames will be stored. It should be initialized before the function is called.</param>
        /// <param name="start">The index of the first frame to load. </param>
        /// <param name="count">The number of frames to load.</param>
        /// <returns>Returns true if the file was successfully loaded and frames were extracted; returns false otherwise.</returns>
        public static bool ImreadAnimation(String fileName, Animation animation, int start = 0, int count = Int16.MaxValue)
        {
            using (CvString csFileName = new CvString(fileName))
                return cveImreadAnimation(csFileName, animation, start, count);
        }

        [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImreadAnimation(IntPtr filename, IntPtr animation, int start, int count);

        /// <summary>
        /// Saves an Animation to a specified file.
        /// </summary>
        /// <param name="fileName">The name of the file where the animation will be saved. The file extension determines the format.</param>
        /// <param name="animation">A constant reference to an Animation struct containing the frames and metadata to be saved.</param>
        /// <param name="parameters">Optional format-specific parameters encoded as pairs (paramId_1, paramValue_1, paramId_2, paramValue_2, ...).</param>
        /// <returns>Returns true if the animation was successfully saved; returns false otherwise.</returns>
        public static bool ImwriteAnimation(String fileName, Animation animation, VectorOfInt parameters = null)
        {

            using (CvString csFileName = new CvString(fileName))
            {
                if (parameters == null)
                    return cveImwriteAnimation(csFileName, animation, IntPtr.Zero);
                else
                    return cveImwriteAnimation(csFileName, animation, parameters);
            }
        }

        [DllImport(CvInvoke.ExternLibrary, CallingConvention = CvInvoke.CvCallingConvention)]
        [return: MarshalAs(CvInvoke.BoolMarshalType)]
        private static extern bool cveImwriteAnimation(IntPtr filename, IntPtr animation, IntPtr parameters);

    }
}
