﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Platform.Maui.UI;
using PlanarSubdivisionExample;

namespace MauiDemoApp
{
    public class PlanarSubdivisionPage : ButtonTextImagePage
    {
       public PlanarSubdivisionPage()
       {
          var button = this.GetButton();
          button.Text = "Calculate Planar Subdivision";
          button.Clicked += (sender, args) =>
          {
             int maxValue = 600, pointCount = 30;

              //DisplayImage.SetImage(DrawSubdivision.Draw(maxValue, pointCount));
              SetImage(DrawSubdivision.Draw(maxValue, pointCount));
              SetMessage(String.Format( "Generated planar subdivision for {0} points", pointCount));
          };
       }
    }
}
