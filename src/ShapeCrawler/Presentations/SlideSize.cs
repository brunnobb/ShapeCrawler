﻿using DocumentFormat.OpenXml;
using ShapeCrawler.Units;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Presentations;

internal sealed class SlideSize(P.SlideSize pSlideSize)
{
    internal decimal Width
    {
        get
        {
            return new Emus(pSlideSize.Cx!.Value).AsHorizontalPixels();
        }
        set
        {
            var emus = new Pixels(value).AsHorizontalEmus();
            pSlideSize.Cx = new Int32Value((int)emus);
        }
    }

    internal decimal Height
    {
        get
        {
            return new Emus(pSlideSize.Cy!.Value).AsVerticalPixels();
        }
        set
        {
            var emus = new Pixels(value).AsVerticalEmus();
            pSlideSize.Cy = new Int32Value((int)emus);
        }
    }
}