﻿using System.IO;

#pragma warning disable IDE0130
namespace ShapeCrawler;
#pragma warning restore IDE0130

/// <summary>
///     Represents a presentation.
/// </summary>
public interface IPresentation
{
    /// <summary>
    ///     Gets the presentation slides.
    /// </summary>
    ISlideCollection Slides { get; }

    /// <summary>
    ///     Gets or sets presentation slides width in pixels.
    /// </summary>
    decimal SlideWidth { get; set; }

    /// <summary>
    ///     Gets or sets the presentation slides height.
    /// </summary>
    decimal SlideHeight { get; set; }

    /// <summary>
    ///     Gets the collection of the slide masters.
    /// </summary>
    ISlideMasterCollection SlideMasters { get; }
    
    /// <summary>
    ///     Gets the collection of sections.
    /// </summary>
    ISectionCollection Sections { get; }
    
    /// <summary>
    ///     Gets footer.
    /// </summary>
    IFooter Footer { get; }

    /// <summary>
    ///     Gets presentation properties.
    /// </summary>
    IPresentationProperties Properties { get; }
    
    /// <summary>
    ///     Returns slide with specified order number.
    /// </summary>
    ISlide Slide(int number);

    /// <summary>
    ///     Saves presentation.
    /// </summary>
    void Save();

    /// <summary>
    ///     Copies presentation to the specified stream.
    /// </summary>
    void Copy(Stream stream);
}