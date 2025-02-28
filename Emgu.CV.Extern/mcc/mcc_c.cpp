//----------------------------------------------------------------------------
//
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.
//
//----------------------------------------------------------------------------

#include "mcc_c.h"

cv::mcc::CChecker* cveCCheckerCreate(cv::Ptr<cv::mcc::CChecker>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	cv::Ptr<cv::mcc::CChecker> checker = cv::mcc::CChecker::create();
	*sharedPtr = new cv::Ptr<cv::mcc::CChecker>(checker);
	return (*sharedPtr)->get();
#else
	throw_no_mcc();
#endif
}

void cveCCheckerGetBox(cv::mcc::CChecker* checker, std::vector< cv::Point2f >* box)
{
#ifdef HAVE_OPENCV_MCC
	std::vector<cv::Point2f> pts = checker->getBox();
	*box = pts;
#else
	throw_no_mcc();
#endif
}
void cveCCheckerSetBox(cv::mcc::CChecker* checker, std::vector< cv::Point2f >* box)
{
#ifdef HAVE_OPENCV_MCC
	checker->setBox(*box);
#else
	throw_no_mcc();
#endif
}

void cveCCheckerGetCenter(cv::mcc::CChecker* checker, CvPoint2D32f* center)
{
#ifdef HAVE_OPENCV_MCC
	cv::Point2f p = checker->getCenter();
	center->x = p.x;
	center->y = p.y;
#else
	throw_no_mcc();
#endif
}
void cveCCheckerSetCenter(cv::mcc::CChecker* checker, CvPoint2D32f* center)
{
#ifdef HAVE_OPENCV_MCC
	cv::Point2f p = *center;
	checker->setCenter(p);
#else
	throw_no_mcc();
#endif
}

void cveCCheckerRelease(cv::Ptr<cv::mcc::CChecker>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	delete* sharedPtr;
	*sharedPtr = 0;
#else
	throw_no_mcc();
#endif
}

void cveCCheckerGetChartsRGB(cv::mcc::CChecker* checker, cv::_OutputArray* chartsRgb)
{
#ifdef HAVE_OPENCV_MCC
	cv::Mat m = checker->getChartsRGB();
	m.copyTo(*chartsRgb);
#else
	throw_no_mcc();
#endif
}

void cveCCheckerSetChartsRGB(cv::mcc::CChecker* checker, cv::Mat* chartsRgb)
{
#ifdef HAVE_OPENCV_MCC
	checker->setChartsRGB(*chartsRgb);
#else
	throw_no_mcc();
#endif
}

cv::mcc::CCheckerDraw* cveCCheckerDrawCreate(
	cv::mcc::CChecker* pChecker,
	CvScalar* color,
	int thickness,
	cv::Ptr<cv::mcc::CCheckerDraw>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	cv::Ptr<cv::mcc::CChecker> cCheckerPtr(pChecker, [](cv::mcc::CChecker* p) {});
	cv::Ptr<cv::mcc::CCheckerDraw> checkerDraw = cv::mcc::CCheckerDraw::create(
		cCheckerPtr,
		*color,
		thickness);
	*sharedPtr = new cv::Ptr<cv::mcc::CCheckerDraw>(checkerDraw);
	return (*sharedPtr)->get();
#else
	throw_no_mcc();
#endif
}

void cveCCheckerDrawDraw(cv::mcc::CCheckerDraw* ccheckerDraw, cv::_InputOutputArray* img)
{
#ifdef HAVE_OPENCV_MCC
	ccheckerDraw->draw(*img);
#else
	throw_no_mcc();
#endif
}
void cveCCheckerDrawRelease(cv::Ptr<cv::mcc::CCheckerDraw>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	delete* sharedPtr;
	*sharedPtr = 0;
#else
	throw_no_mcc();
#endif  
}


cv::mcc::CCheckerDetector* cveCCheckerDetectorCreate(cv::Algorithm** algorithm, cv::Ptr<cv::mcc::CCheckerDetector>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	cv::Ptr<cv::mcc::CCheckerDetector> checkerDetector = cv::mcc::CCheckerDetector::create();
	*sharedPtr = new cv::Ptr<cv::mcc::CCheckerDetector>(checkerDetector);
	*algorithm = dynamic_cast<cv::Algorithm*>((*sharedPtr)->get());
	return (*sharedPtr)->get();
#else
	throw_no_mcc();
#endif
}

bool cveCCheckerDetectorProcess(
	cv::mcc::CCheckerDetector* detector,
	cv::_InputArray* image,
	const cv::mcc::TYPECHART chartType,
	const int nc,
	bool useNet,
	cv::mcc::DetectorParameters* param)
{
#ifdef HAVE_OPENCV_MCC
	if (param) {
		cv::Ptr<cv::mcc::DetectorParameters> paramPtr(param, [](cv::mcc::DetectorParameters* p) {});
		return detector->process(*image, chartType, nc, useNet, paramPtr);
	}
	else
	{
		return detector->process(*image, chartType, nc, useNet);
	}
#else
	throw_no_mcc();
#endif
}

cv::mcc::CChecker* cveCCheckerDetectorGetBestColorChecker(cv::mcc::CCheckerDetector* detector)
{
#ifdef HAVE_OPENCV_MCC
	cv::Ptr<cv::mcc::CChecker> ptr = detector->getBestColorChecker();
	return ptr.get();
#else
	throw_no_mcc();
#endif
}
void cveCCheckerDetectorRelease(cv::Ptr<cv::mcc::CCheckerDetector>** sharedPtr)
{
#ifdef HAVE_OPENCV_MCC
	delete* sharedPtr;
	*sharedPtr = 0;
#else
	throw_no_mcc();
#endif
}

cv::mcc::DetectorParameters* cveCCheckerDetectorParametersCreate()
{
#ifdef HAVE_OPENCV_MCC
	return new cv::mcc::DetectorParameters();
#else
	throw_no_mcc();
#endif
}
void cveCCheckerDetectorParametersRelease(cv::mcc::DetectorParameters** parameters)
{
#ifdef HAVE_OPENCV_MCC
	delete* parameters;
	*parameters = 0;
#else
	throw_no_mcc();
#endif	
}


cv::ccm::ColorCorrectionModel* cveColorCorrectionModelCreate1(cv::Mat* src, int constColor)
{
#ifdef HAVE_OPENCV_MCC
	return new cv::ccm::ColorCorrectionModel(*src, static_cast<cv::ccm::CONST_COLOR>(constColor));
#else
	throw_no_mcc();
#endif	
}

cv::ccm::ColorCorrectionModel* cveColorCorrectionModelCreate2(cv::Mat* src, cv::Mat* colors, int refCs)
{
#ifdef HAVE_OPENCV_MCC
	return new cv::ccm::ColorCorrectionModel(*src, *colors, static_cast<cv::ccm::COLOR_SPACE>(refCs));
#else
	throw_no_mcc();
#endif	
}

cv::ccm::ColorCorrectionModel* cveColorCorrectionModelCreate3(cv::Mat* src, cv::Mat* colors, int refCs, cv::Mat* colored)
{
#ifdef HAVE_OPENCV_MCC
	return new cv::ccm::ColorCorrectionModel(*src, *colors, static_cast<cv::ccm::COLOR_SPACE>(refCs), *colored);
#else
	throw_no_mcc();
#endif	
}

void cveColorCorrectionModelRelease(cv::ccm::ColorCorrectionModel** ccm)
{
#ifdef HAVE_OPENCV_MCC
	delete* ccm;
	*ccm = 0;
#else
	throw_no_mcc();
#endif	
}
void cveColorCorrectionModelRun(cv::ccm::ColorCorrectionModel* ccm)
{
#ifdef HAVE_OPENCV_MCC
	ccm->run();
#else
	throw_no_mcc();
#endif	
}

void cveColorCorrectionModelGetCCM(cv::ccm::ColorCorrectionModel* ccm, cv::_OutputArray* result)
{
#ifdef HAVE_OPENCV_MCC
	cv::Mat m = ccm->getCCM();
	m.copyTo(*result);
#else
	throw_no_mcc();
#endif	
}

void cveColorCorrectionModelInfer(cv::ccm::ColorCorrectionModel* ccm, cv::Mat* img, cv::_OutputArray* result, bool islinear)
{
#ifdef HAVE_OPENCV_MCC
	cv::Mat m = ccm->infer(*img, islinear);
	m.copyTo(*result);
#else
	throw_no_mcc();
#endif		
}