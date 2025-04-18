﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Models;
using Emgu.Util;
using System.Diagnostics;

namespace Emgu.CV.Models
{
    /// <summary>
    /// Tesseract Ocr model.
    /// </summary>
    public class TesseractModel : DisposableObject, IProcessAndRenderModel
    {
        /// <summary>
        /// The rendering method
        /// </summary>
        public RenderType RenderMethod
        {
            get
            {
                return RenderType.Update;
            }
        }

        private Tesseract _ocr;

        private String _lang;
        private OcrEngineMode _mode;
        private String _modelFolderName = Path.Combine("emgu", "tessdata");
        private String _tessDataDirectory;

        /// <summary>
        /// Get the ocr model
        /// </summary>
        public Tesseract Model
        {
            get { return _ocr; }
        }

        /// <summary>
        /// Return true if the model is initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _ocr != null;
            }
        }

        /// <summary>
        /// Return the directly where the Tesseract data is downloaded into.
        /// If the model has not been completed downloaded. The folder is null.
        /// </summary>
        public String TessDataDirectory
        {
            get
            {
                return _tessDataDirectory; 
            }
        }

        /// <summary>
        /// Create a tesseract model with the specific language and mode.
        /// </summary>
        /// <param name="lang">The language model</param>
        /// <param name="mode">The ocr engine mode</param>
		/// <param name="modelFolderName">The subfolder to store the tesseract model data. It is appended to the data download folder.</param>
        public TesseractModel(String lang = "eng", OcrEngineMode mode = OcrEngineMode.TesseractLstmCombined, String modelFolderName = null)
        {
            _lang = lang;
            _mode = mode;
            if (modelFolderName != null)
                _modelFolderName = modelFolderName;
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        private IEnumerator
#else
        private async Task 
#endif
            InitTesseract(String lang, OcrEngineMode mode, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
        {
            if (_ocr == null)
            {
                FileDownloadManager manager = new FileDownloadManager();
                manager.AddFile(Emgu.CV.OCR.Tesseract.GetLangFileUrl(lang), _modelFolderName);
                manager.AddFile(Emgu.CV.OCR.Tesseract.GetLangFileUrl("osd"), _modelFolderName); //script orientation detection
                manager.AddFile("https://github.com/tesseract-ocr/tessconfigs/blob/3decf1c8252ba6dbeef0bf908f4b0aab7f18d113/pdf.ttf?raw=true", _modelFolderName); //PDF fonts for PDFRenderer

                if (onDownloadProgressChanged != null)
                    manager.OnDownloadProgressChanged += onDownloadProgressChanged;
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return manager.Download();
#else
                await manager.Download();
#endif

                if (manager.AllFilesDownloaded)
                {
                    //_lang = lang;
                    //_mode = mode;
                    FileInfo fi = new FileInfo(manager.Files[0].LocalFile);
                    _tessDataDirectory = fi.DirectoryName;
                    var rawData = System.IO.File.ReadAllBytes(Path.Combine(_tessDataDirectory, String.Format("{0}.traineddata", _lang)));
                    _ocr = new Tesseract();
                    _ocr.Init(rawData, _lang, _mode);
                    //_ocr = new Tesseract(_tessDataDirectory, _lang, _mode);
                }
            }
        }

        /// <summary>
        /// Clear and reset the model. Required Init function to be called again before calling ProcessAndRender.
        /// </summary>
        public void Clear()
        {
            if (_ocr != null)
            {
                _ocr.Dispose();
                _ocr = null;
            }

            _tessDataDirectory = null;
        }

        /// <summary>
        /// Release all the unmanaged memory associated to this tesseract OCR model.
        /// </summary>
        protected override void DisposeObject()
        {
            Clear();
        }

        private MCvScalar _renderColor = new MCvScalar(255, 0, 0);

        /// <summary>
        /// Get or Set the color used in rendering.
        /// </summary>
        public MCvScalar RenderColor
        {
            get
            {
                return _renderColor;
            }
            set
            {
                _renderColor = value;
            }
        }

        /// <summary>
        /// Process the input image and render into the output image
        /// </summary>
        /// <param name="imageIn">The input image</param>
        /// <param name="imageOut">
        /// The output image, can be the same as imageIn, in which case we will render directly into the input image.
        /// Note that if no text is detected, the output image will remain unchanged. 
        /// If text are detected, we will render the text region on top of the existing output image.
        /// If the output image is not the same object as the input image, it is a good idea to copy the pixels over from the input image before passing it to this function.
        /// </param>
        /// <returns>The messages that we want to display.</returns>
        public string ProcessAndRender(IInputArray imageIn, IInputOutputArray imageOut)
        {
            Stopwatch watch = Stopwatch.StartNew();
            _ocr.SetImage(imageIn);
            if (_ocr.Recognize() != 0)
                throw new Exception("Failed to recognize image");
            String ocrResult = _ocr.GetUTF8Text();
            watch.Stop();

            Tesseract.Word[] words = _ocr.GetWords();
            foreach (Tesseract.Word w in words)
            {
                CvInvoke.Rectangle(imageOut, w.Region, RenderColor);
            }

            return String.Format(
                "Tesseract version {2}; lang: {0}; mode: {1}{3}Text Detected:{3}{4}",
                _lang,
                _mode.ToString(),
                Emgu.CV.OCR.Tesseract.VersionString,
                System.Environment.NewLine, ocrResult);

        }

        /// <summary>
        /// Initialize the tesseract ocr model
        /// </summary>
        /// <param name="onDownloadProgressChanged">Call back method during download</param>
        /// <param name="initOptions">Initialization options. None supported at the moment, any value passed will be ignored.</param>
        /// <returns>Asyn task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator
#else
        public async Task 
#endif
            Init(FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null, Object initOptions = null)
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
            yield return
#else
            await 
#endif
                InitTesseract(_lang, _mode, onDownloadProgressChanged);
        }

    }
}
