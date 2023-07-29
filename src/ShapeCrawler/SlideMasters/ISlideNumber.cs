﻿using System.Collections.Generic;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

// ReSharper disable once CheckNamespace
namespace ShapeCrawler;

/// <summary>
///     Represents a slide number.
/// </summary>
public interface ISlideNumber
{
    /// <summary>
    ///     Gets font.
    /// </summary>
    ISlideNumberFont Font { get; }
}

internal class SCSlideNumber : ISlideNumber
{
    internal SCSlideNumber(P.Shape pSldNum, List<ITextPortionFont> portionFonts)
    {
        var aDefaultRunProperties = pSldNum.TextBody!.ListStyle!.Level1ParagraphProperties?.GetFirstChild<A.DefaultRunProperties>() !; 
        this.Font = new SCSlideNumberFont(aDefaultRunProperties, portionFonts);
    }

    public ISlideNumberFont Font { get; }
}