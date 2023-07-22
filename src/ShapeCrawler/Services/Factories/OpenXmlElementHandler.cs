﻿using DocumentFormat.OpenXml;
using OneOf;
using ShapeCrawler.Shapes;
using ShapeCrawler.Texts;

namespace ShapeCrawler.Services.Factories;

internal abstract class OpenXmlElementHandler
{
    internal OpenXmlElementHandler? Successor { get; set; }

    internal abstract SCShape? FromTreeChild(
        OpenXmlCompositeElement pShapeTreeChild,
        OneOf<SCSlide, SCSlideLayout, SCSlideMaster> slideObject,
        OneOf<ShapeCollection, SCGroupShape> shapeCollection,
        ITextFrameContainer textFrameContainer);
}