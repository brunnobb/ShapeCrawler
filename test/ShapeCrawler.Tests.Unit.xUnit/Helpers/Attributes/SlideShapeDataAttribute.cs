﻿using System.Collections.Generic;
using System.Reflection;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ShapeCrawler.Shapes;
using ShapeCrawler.Tests.Shared;
using Xunit.Sdk;

namespace ShapeCrawler.Tests.Unit.Helpers.Attributes;

public class SlideShapeDataAttribute : DataAttribute
{
    private readonly string pptxFile;
    private readonly int slideNumber;
    private readonly string shapeName;
    private readonly int shapeId;
    private readonly object expectedResult;
    private readonly string displayName;

    public SlideShapeDataAttribute(string pptxFile, int slideNumber, string shapeName, object expectedResult)
    : this(pptxFile, slideNumber, shapeName)
    {
        this.expectedResult = expectedResult;
    }
    
    public SlideShapeDataAttribute(string pptxFile, int slideNumber, string shapeName)
    {
        this.pptxFile = pptxFile;
        this.slideNumber = slideNumber;
        this.shapeName = shapeName;
    }
    
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var pptxStream = SCTest.StreamOf(this.pptxFile);
        var pres = new Presentation(pptxStream);
        var slide = pres.Slides[this.slideNumber - 1];
        var shape = this.shapeName != null
            ? slide.Shapes.GetByName<IShape>(this.shapeName)
            : slide.Shapes.GetById<IShape>(this.shapeId);

        var input = new List<object>();

        if (this.displayName != null)
        {
            input.Add(this.displayName);
        }
        
        input.Add(shape);
        
        if (this.expectedResult != null)
        {
            input.Add(this.expectedResult);
        }

        yield return input.ToArray();
    }
}