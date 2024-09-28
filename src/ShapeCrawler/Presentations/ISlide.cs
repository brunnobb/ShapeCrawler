using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
#if DEBUG
using System.Threading.Tasks;
#endif

namespace ShapeCrawler;

/// <summary>
///     Represents a slide.
/// </summary>
public interface ISlide
{
    /// <summary>
    ///     Gets background image.
    /// </summary>
    IImage? Background { get; }

    /// <summary>
    ///     Gets or sets custom data. It returns <see langword="null"/> if custom data is not presented.
    /// </summary>
    string? CustomData { get; set; }
    
    /// <summary>
    ///     Gets referenced Slide Layout.
    /// </summary>
    ISlideLayout SlideLayout { get; }
    
    /// <summary>
    ///     Gets or sets slide number.
    /// </summary>
    int Number { get; set; }
    
    /// <summary>
    ///     Gets underlying instance of <see cref="DocumentFormat.OpenXml.Packaging.SlidePart"/>.
    /// </summary>
    SlidePart SDKSlidePart { get; }
    
    /// <summary>
    ///     Gets the shape collection.
    /// </summary>
    ISlideShapes Shapes { get; }

    /// <summary>
    ///     Gets slide notes as a single text frame.
    /// </summary>
    ITextBox? Notes { get; }

    /// <summary>
    ///     List of all text frames on that slide.
    /// </summary>
    public IList<ITextBox> TextFrames();

    /// <summary>
    ///     Hides slide.
    /// </summary>
    void Hide();
    
    /// <summary>
    ///     Gets a value indicating whether the slide is hidden.
    /// </summary>
    bool Hidden();
    
    /// <summary>
    ///     Gets table by name.
    /// </summary>
    ITable Table(string name);
    
    /// <summary>
    ///     Adds specified lines to the slide notes.
    /// </summary>
    void AddNotes(IEnumerable<string> lines);
    
    /// <summary>
    ///     Returns shape with specified name.
    /// </summary>
    /// <param name="name">Shape name.</param>
    /// <returns> The IShape requested. </returns>
    IShape Shape(string name);
    
#if DEBUG
    
    /// <summary>
    ///     Saves slide as PNG image.
    /// </summary>
    void SaveAsPng(System.IO.Stream stream);
#endif
}