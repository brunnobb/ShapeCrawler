﻿using FluentAssertions;
using NUnit.Framework;
using ShapeCrawler.Tests.Unit.Helpers;

namespace ShapeCrawler.Tests.Unit;

public class LineShapeTests : SCTest
{
    [Test]
    public void StartPoint_returns_start_point_coordinates()
    {
        // Arrange
        var pres = new Presentation();
        var shapes = pres.SlideCollection[0].ShapeCollection;
        shapes.AddLine(startPointX: 50, startPointY: 60, endPointX: 100, endPointY: 60);
        var line = (ILine)shapes[0];

        // Act
        var startPoint = line.StartPoint;

        // Assert
        startPoint.X.Should().Be(50);
        startPoint.Y.Should().Be(60);
    }
}