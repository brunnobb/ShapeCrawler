﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using ShapeCrawler.Shapes;
using ShapeCrawler.Tests.Shared;
using ShapeCrawler.Tests.Unit.Helpers;
using ShapeCrawler.Tests.Unit.Helpers.Attributes;
using Xunit;

// ReSharper disable TooManyDeclarations
// ReSharper disable InconsistentNaming
// ReSharper disable TooManyChainedReferences

namespace ShapeCrawler.Tests.Unit;

public class ShapeFillTests : SCTest
{
    [Theory]
    [MemberData(nameof(TestCasesFillType))]
    public void Type_returns_Gradient_fill_type(IShape shape, FillType expectedFill)
    {
        // Act
        var fillType = shape.Fill.Type;

        // Assert   
        fillType.Should().Be(expectedFill);
    }

    public static IEnumerable<object[]> TestCasesFillType()
    {
        var pptxStream = StreamOf("009_table.pptx");
        var pres = new Presentation(pptxStream);
        var withGradient = pres.Slides[1].Shapes.GetByName<IShape>("AutoShape 1");
        yield return new object[] { withGradient, FillType.Gradient };
    }
}