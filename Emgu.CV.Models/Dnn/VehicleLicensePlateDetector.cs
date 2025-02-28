﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;

namespace Emgu.CV.Models
{
    /// <summary>
    /// DNN Vehicle license plate detector using OpenVino
    /// </summary>
    public class VehicleLicensePlateDetector : DisposableObject, IProcessAndRenderModel
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

        /// <summary>
        /// License plate
        /// </summary>
        public struct LicensePlate
        {
            /// <summary>
            /// The region of the license plate
            /// </summary>
            public Rectangle Region;
            /// <summary>
            /// The text on the license plate
            /// </summary>
            public String Text;
        }

        /// <summary>
        /// Vehicle
        /// </summary>
        public class Vehicle
        {
            /// <summary>
            /// The vehicle region
            /// </summary>
            public Rectangle Region;
            /// <summary>
            /// The color of the vehicle
            /// </summary>
            public String Color;
            /// <summary>
            /// The vehicle type
            /// </summary>
            public String Type;
            /// <summary>
            /// The license plate. If null, there is no license plate detected.
            /// </summary>
            public LicensePlate? LicensePlate;

            /// <summary>
            /// If the license plate region is located within the vehicle region
            /// </summary>
            /// <param name="p">The license plate</param>
            /// <param name="plateOverlapRatio">A license plate is overlapped with the vehicle if the specific ratio of the license plate area is overlapped.</param>
            /// <returns>True if the license plate overlap with the vehicle.</returns>
            public bool ContainsPlate(LicensePlate p, double plateOverlapRatio = 0.8)
            {
                if (Region.IsEmpty || p.Region.IsEmpty)
                    return false;
                double plateSize = p.Region.Width * p.Region.Height;
                Rectangle overlap = p.Region;
                overlap.Intersect(Region);
                double overlapSize = overlap.Width * overlap.Height;
                if (overlapSize / plateSize < plateOverlapRatio)
                    return false;
                return true;
            }
        }

        private String _modelFolderName = "vehicle-license-plate-detection-barrier-0106-openvino-2023_1";

        private DetectionModel _vehicleLicensePlateDetectionModel = null;
        
        private Model _vehicleAttrRecognizerModel = null;
        private Net _ocr = null;

        /// <summary>
        /// Return true if the model is initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                if (_vehicleLicensePlateDetectionModel == null)
                    return false;
                if (_ocr == null)
                    return false;
                if (_vehicleAttrRecognizerModel == null)
                    return false;
                return true;
            }
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        private IEnumerator InitOCR(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#else
        private async Task InitOCR(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#endif
        {
            if (_ocr == null)
            {
                FileDownloadManager manager = new FileDownloadManager();

                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/license-plate-recognition-barrier-0001/FP32/license-plate-recognition-barrier-0001.xml",
                    _modelFolderName,
                    "35D77DE2F71AC340B5B065B67EB2B791550B4BC395F16AAC2E742496F35DD9D8");
                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/license-plate-recognition-barrier-0001/FP32/license-plate-recognition-barrier-0001.bin",
                    _modelFolderName,
                    "055CEA6236129BEBBC64852C69FDAEFE07E074800251147DDE0005A824BCFEBA");

                if (onDownloadProgressChanged != null)
                {
                    manager.OnDownloadProgressChanged += onDownloadProgressChanged;
                }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return manager.Download();
#else
                await manager.Download();
#endif

                _ocr =
                    DnnInvoke.ReadNetFromModelOptimizer(manager.Files[0].LocalFile, manager.Files[1].LocalFile);

                using (Mat seqInd = new Mat(
                    new Size(1, 88),
                    DepthType.Cv32F,
                    1))
                {
                    if (seqInd.Depth == DepthType.Cv32F)
                    {
                        float[] seqIndValues = new float[seqInd.Width * seqInd.Height];
                        for (int j = 1; j < seqIndValues.Length; j++)
                        {
                            seqIndValues[j] = 1.0f;
                        }

                        seqIndValues[0] = 0.0f;
                        seqInd.SetTo(seqIndValues);
                    }

                    _ocr.SetInput(seqInd, "seq_ind");
                }

                _ocr.SetPreferableBackend(preferredBackend);
                _ocr.SetPreferableTarget(preferredTarget);
                
            }
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        private IEnumerator InitVehicleAttributesRecognizer(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#else
        private async Task InitVehicleAttributesRecognizer(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#endif
        {
            if (_vehicleAttrRecognizerModel == null)
            {
                FileDownloadManager manager = new FileDownloadManager();

                
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.2/models_bin/3/vehicle-attributes-recognition-barrier-0042/FP32/vehicle-attributes-recognition-barrier-0042.xml",
                //    _modelFolderName,
                //    "9D1E877B153699CAF4547D08BFF7FE268F65B663441A42B929924B8D95DACDBB");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.2/models_bin/3/vehicle-attributes-recognition-barrier-0042/FP32/vehicle-attributes-recognition-barrier-0042.bin",
                //    _modelFolderName,
                //    "492520E55F452223E767D54227D6EF6B60B0C1752DD7B9D747BE65D57B685A0E");
                
                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-attributes-recognition-barrier-0042/FP32/vehicle-attributes-recognition-barrier-0042.xml",
                    _modelFolderName,
                    "B669E6F98C66525F1849DB4D16DF8C92876164BB41D6420F84A8D3AE9B4DC699");
                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-attributes-recognition-barrier-0042/FP32/vehicle-attributes-recognition-barrier-0042.bin",
                    _modelFolderName,
                    "717BFD7413FFD5EE6D08C5E087CAA0A48CC69646BD68B7791C62C825399620B4");

                manager.OnDownloadProgressChanged += onDownloadProgressChanged;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return manager.Download();
#else
                await manager.Download();
#endif
                //_vehicleAttrRecognizer =
                //    DnnInvoke.ReadNetFromModelOptimizer(manager.Files[0].LocalFile, manager.Files[1].LocalFile);
                _vehicleAttrRecognizerModel = new Model(manager.Files[1].LocalFile, manager.Files[0].LocalFile);
                _vehicleAttrRecognizerModel.SetInputSize(new Size(72, 72));
                _vehicleAttrRecognizerModel.SetInputMean(new MCvScalar());
                _vehicleAttrRecognizerModel.SetInputCrop(false);
                _vehicleAttrRecognizerModel.SetInputSwapRB(false);
                _vehicleAttrRecognizerModel.SetInputScale(1.0);

                _vehicleAttrRecognizerModel.SetPreferableBackend(preferredBackend);
                _vehicleAttrRecognizerModel.SetPreferableTarget(preferredTarget);
            }
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        private IEnumerator InitLicensePlateDetector(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#else
        private async Task InitLicensePlateDetector(Dnn.Backend preferredBackend, Dnn.Target preferredTarget, FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null)
#endif
        {
            if (_vehicleLicensePlateDetectionModel == null)
            {
                FileDownloadManager manager = new FileDownloadManager();

                
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.2/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.xml",
                //    _modelFolderName,
                //    "E305DEA89C08501B66774FDCD31EEE279C3531CBAB04127C62AEAC34682C0330");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.2/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.bin",
                //    _modelFolderName,
                //    "DEF0179E5896B732D54654C369F1C0D896A457D7D3A50627DD9020E45FD6D6EB");
                

                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.xml",
                //    _modelFolderName,
                //    "45D82B40F48CE43E61188ED16A140C8A77C4340CF367FC18C491B1F1A1218DD7");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.bin",
                //    _modelFolderName,
                //    "F84139F97805D343273E840D47B81C89A7C96C6263226A5622670E66424FB2A1");
                

                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP16/vehicle-license-plate-detection-barrier-0106.xml",
                //    _modelFolderName,
                //    "69ED174CEE70E19B098215F53D1FE6F27EDC5B417790BBC44CDE7F37CC082FAE");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2023.0/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP16/vehicle-license-plate-detection-barrier-0106.bin",
                //    _modelFolderName,
                //    "7F96BFAA0AC3AF65D136F6A185D1D78D68D03BAB2E1F2FAAFE1D27CCD94B0DC1");
                

                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2022.3/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.xml",
                //    _modelFolderName,
                //    "3F162572ECA8E4F27A7345FD4FC2FB0DC2D36ABCF4EB31494433FDBF29D534CE");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2022.3/models_bin/1/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.bin",
                //    _modelFolderName,
                //    "F84139F97805D343273E840D47B81C89A7C96C6263226A5622670E66424FB2A1");
                

                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2022.1/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.xml",
                    _modelFolderName,
                    "F750216ED67856CEEC5F1C39186B5BD0BC8220954D169EA1EE7EC1E51929707C");
                manager.AddFile(
                    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2022.1/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.bin",
                    _modelFolderName,
                    "81E0B494FE469A157EDAAFF1E2325C69E66D835F94B22258B774E9228E43BCE2");
                
                
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.4/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.xml",
                //    _modelFolderName,
                //    "74FECD516D990371613330E4386D66879C7FBB445E45A6ABEB043F022F323A45");
                //manager.AddFile(
                //    "https://storage.openvinotoolkit.org/repositories/open_model_zoo/2021.4/models_bin/3/vehicle-license-plate-detection-barrier-0106/FP32/vehicle-license-plate-detection-barrier-0106.bin",
                //    _modelFolderName,
                //    "7B19141E631ACD08B16D5940144343D4AFEAE66C13F9DAEB2CD3215ECE6CFBA0");
                
                manager.OnDownloadProgressChanged += onDownloadProgressChanged;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
                yield return manager.Download();
#else
                await manager.Download();
#endif

                
                _vehicleLicensePlateDetectionModel =
                    new DetectionModel(manager.Files[1].LocalFile, manager.Files[0].LocalFile);
                
                
                //_vehicleLicensePlateDetectionModel = new DetectionModel(
                //    "D:\\tmp\\public\\vehicle-license-plate-detection-barrier-0123\\FP32\\vehicle-license-plate-detection-barrier-0123.bin",
                //    "D:\\tmp\\public\\vehicle-license-plate-detection-barrier-0123\\FP32\\vehicle-license-plate-detection-barrier-0123.xml"
                //    );
                _vehicleLicensePlateDetectionModel.SetInputSize(new Size(300, 300));
                _vehicleLicensePlateDetectionModel.SetInputMean(new MCvScalar());
                _vehicleLicensePlateDetectionModel.SetInputScale(1.0);
                _vehicleLicensePlateDetectionModel.SetInputSwapRB(false);
                _vehicleLicensePlateDetectionModel.SetInputCrop(false);


                //_vehicleLicensePlateDetectionModel.SetPreferableBackend(preferredBackend);
                //_vehicleLicensePlateDetectionModel.SetPreferableTarget(preferredTarget);
                
            }
        }

        private String[] _colorName = new String[] { "white", "gray", "yellow", "red", "green", "blue", "black" };
        private String[] _vehicleType = new String[] { "car", "van", "truck", "bus" };
        private String[] _plateText = new string[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "<Anhui>", "<Beijing>", "<Chongqing>", "<Fujian>",
            "<Gansu>", "<Guangdong>", "<Guangxi>", "<Guizhou>",
            "<Hainan>", "<Hebei>", "<Heilongjiang>", "<Henan>",
            "<HongKong>", "<Hubei>", "<Hunan>", "<InnerMongolia>",
            "<Jiangsu>", "<Jiangxi>", "<Jilin>", "<Liaoning>",
            "<Macau>", "<Ningxia>", "<Qinghai>", "<Shaanxi>",
            "<Shandong>", "<Shanghai>", "<Shanxi>", "<Sichuan>",
            "<Tianjin>", "<Tibet>", "<Xinjiang>", "<Yunnan>",
            "<Zhejiang>", "<police>",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        };


        /// <summary>
        /// Download and initialize the vehicle detector, the license plate detector and OCR.
        /// </summary>
        /// <param name="onDownloadProgressChanged">Callback when download progress has been changed</param>
        /// <param name="initOptions">Initialization options. A string in the format of "backend;target" that represent the DNN backend and target.</param>
        /// <returns>Async task</returns>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
        public IEnumerator Init(
            FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            Object initOptions = null)
#else
        public async Task Init(
            FileDownloadManager.DownloadProgressChangedEventHandler onDownloadProgressChanged = null, 
            Object initOptions = null)
#endif
        {
            Dnn.Backend backend = Dnn.Backend.InferenceEngine;
            Dnn.Target target = Target.Cpu;
            if (initOptions != null && ((initOptions as String) != null))
            {
                String[] backendOptions = (initOptions as String).Split(';');
                if (backendOptions.Length == 2)
                {
                    String backendStr = backendOptions[0];
                    String targetStr = backendOptions[1];

                    if (!(Enum.TryParse(backendStr, true, out backend) &&
                        Enum.TryParse(targetStr, true, out target)))
                    {
                        //If failed to part either backend or target, use the following default
                        backend = Dnn.Backend.InferenceEngine;
                        target = Target.Cpu;
                    }
                }
            }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL
            yield return InitLicensePlateDetector(backend, target, onDownloadProgressChanged);
            yield return InitVehicleAttributesRecognizer(backend, target, onDownloadProgressChanged);
            yield return InitOCR(backend, target, onDownloadProgressChanged);
#else
            await InitLicensePlateDetector(backend, target, onDownloadProgressChanged);
            await InitVehicleAttributesRecognizer(backend, target, onDownloadProgressChanged);
            await InitOCR(backend, target, onDownloadProgressChanged);
#endif
        }

        /// <summary>
        /// Process the input image and render into the output image
        /// </summary>
        /// <param name="imageIn">The input image</param>
        /// <param name="imageOut">
        /// The output image, can be the same as <paramref name="imageIn"/>, in which case we will render directly into the input image.
        /// Note that if no vehicle is detected, <paramref name="imageOut"/> will remain unchanged.
        /// If vehicle / license plate are detected, we will draw the text and (rectangle) region on top of the existing pixels of <paramref name="imageOut"/>.
        /// If the <paramref name="imageOut"/> is not the same object as <paramref name="imageIn"/>, it is a good idea to copy the pixels over from the input image before passing it to this function.
        /// </param>
        /// <returns>The messages that we want to display.</returns>
        public string ProcessAndRender(IInputArray imageIn, IInputOutputArray imageOut)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Vehicle[] detectionResult = Detect(imageIn);
            watch.Stop();

            Render(imageOut, detectionResult);
            return String.Format("Detected in {0} milliseconds.", watch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Detect vehicle from the given image
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>The detected vehicles.</returns>
        public Vehicle[] Detect(IInputArray image)
        {
            float vehicleConfidenceThreshold = 0.5f;
            float licensePlateConfidenceThreshold = 0.5f;

            
            double scale = 1.0;
            MCvScalar meanVal = new MCvScalar();

            List<Vehicle> vehicles = new List<Vehicle>();
            List<LicensePlate> plates = new List<LicensePlate>();
            using (InputArray iaImage = image.GetInputArray())
            using (Mat iaImageMat = iaImage.GetMat())
                foreach (DetectedObject vehicleOrPlate in _vehicleLicensePlateDetectionModel.Detect(image, Math.Min(vehicleConfidenceThreshold, licensePlateConfidenceThreshold), 0.0f))
                {
                    Rectangle region = vehicleOrPlate.Region;

                    if (vehicleOrPlate.ClassId == 1 && vehicleOrPlate.Confident > vehicleConfidenceThreshold)
                    {
                        //this is a vehicle
                        Vehicle v = new Vehicle();
                        v.Region = region;

#region find out the type and color of the vehicle

                        using (Mat vehicle = new Mat(iaImageMat, region))
                        using (VectorOfMat vm = new VectorOfMat(2))
                        {
                            _vehicleAttrRecognizerModel.Predict(vehicle, vm);
                            //_vehicleAttrRecognizer.Forward(vm, new string[] { "color", "type" });
                            using (Mat vehicleColorMat = vm[0])
                            using (Mat vehicleTypeMat = vm[1])
                            {
                                float[] vehicleColorData = vehicleColorMat.GetData(false) as float[];
                                float maxProbColor = vehicleColorData.Max();
                                int maxIdxColor = Array.IndexOf(vehicleColorData, maxProbColor);
                                v.Color = _colorName[maxIdxColor];
                                float[] vehicleTypeData = vehicleTypeMat.GetData(false) as float[];
                                float maxProbType = vehicleTypeData.Max();
                                int maxIdxType = Array.IndexOf(vehicleTypeData, maxProbType);
                                v.Type = _vehicleType[maxIdxType];
                            }
                        }
#endregion

                        vehicles.Add(v);
                    }
                    else if (vehicleOrPlate.ClassId == 2 && vehicleOrPlate.Confident > licensePlateConfidenceThreshold)
                    {
                        //this is a license plate
                        LicensePlate p = new LicensePlate();
                        p.Region = region;

#region OCR on license plate
                        using (Mat plate = new Mat(iaImageMat, region))
                        {
                            using (Mat inputBlob = DnnInvoke.BlobFromImage(
                                plate,
                                scale,
                                new Size(94, 24),
                                meanVal,
                                false,
                                false,
                                DepthType.Cv32F))
                            {
                                _ocr.SetInput(inputBlob, "data");
                                using (Mat output = _ocr.Forward("decode"))
                                {
                                    float[] plateValue = output.GetData(false) as float[];
                                    StringBuilder licensePlateStringBuilder = new StringBuilder();
                                    foreach (int j in plateValue)
                                    {
                                        if (j >= 0)
                                        {
                                            licensePlateStringBuilder.Append(_plateText[j]);
                                        }
                                    }

                                    p.Text = licensePlateStringBuilder.ToString();
                                }
                            }
                        }
#endregion

                        plates.Add(p);
                    }
                }

            foreach (LicensePlate p in plates)
            {
                foreach (Vehicle v in vehicles)
                {
                    if (v.ContainsPlate(p))
                    {
                        v.LicensePlate = p;
                        break;
                    }
                }
            }

            return vehicles.ToArray();
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
        /// Draw the vehicles to the image.
        /// </summary>
        /// <param name="image">The image to be drawn to.</param>
        /// <param name="vehicles">The vehicles.</param>
        public void Render(IInputOutputArray image, Vehicle[] vehicles)
        {
            foreach (Vehicle v in vehicles)
            {
                CvInvoke.Rectangle(image, v.Region, RenderColor, 2);
                String label = String.Format("{0} {1} {2}",
                    v.Color, v.Type, v.LicensePlate == null ? String.Empty : v.LicensePlate.Value.Text);
                CvInvoke.PutText(
                    image,
                    label,
                    new Point(v.Region.Location.X, v.Region.Location.Y + 20),
                    FontFace.HersheyComplex,
                    1.0,
                    RenderColor,
                    2);
            }
        }

        /// <summary>
        /// Clear and reset the model. Required Init function to be called again before calling ProcessAndRender.
        /// </summary>
        public void Clear()
        {
            if (_vehicleLicensePlateDetectionModel != null)
            {
                _vehicleLicensePlateDetectionModel.Dispose();
                _vehicleLicensePlateDetectionModel = null;
            }

            if (_vehicleAttrRecognizerModel != null)
            {
                _vehicleAttrRecognizerModel.Dispose();
                _vehicleAttrRecognizerModel = null;
            }

            if (_ocr != null)
            {
                _ocr.Dispose();
                _ocr = null;
            }
        }

        /// <summary>
        /// Release the memory associated with this vehicle license plate detector
        /// </summary>
        protected override void DisposeObject()
        {
            Clear();
        }
    }
}
*/