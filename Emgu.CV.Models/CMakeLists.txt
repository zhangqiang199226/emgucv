# --------------------------------------------------------
#  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(EMGU_CV_BUILD OR EMGU_CV_EXAMPLE_BUILD)

  INSTALL(
	  DIRECTORY
	  ${CMAKE_CURRENT_SOURCE_DIR}
	  DESTINATION .
		COMPONENT emgucv_source
	  FILES_MATCHING 
	  PATTERN "*.cs"
	  PATTERN "*.shproj"
	  PATTERN "*.csproj"
	  PATTERN "*.projitems"
	  PATTERN "*.resx"
	  PATTERN "Directory.Build.props"
	  PATTERN "README.txt"
	  PATTERN ".svn" EXCLUDE
	  PATTERN "obj" EXCLUDE
	  PATTERN "CMakeFiles" EXCLUDE
	  PATTERN "${PROJECT_NAME}.dir" EXCLUDE
	  PATTERN "*Unity*" EXCLUDE
	)

ENDIF()

ADD_SUBDIRECTORY(NetStandard)

