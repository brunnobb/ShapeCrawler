﻿using System;
using DocumentFormat.OpenXml.Packaging;
using ShapeCrawler.Drawing;
using ShapeCrawler.Exceptions;
using ShapeCrawler.Extensions;
using ShapeCrawler.Shared;
using ShapeCrawler.Texts;
using A = DocumentFormat.OpenXml.Drawing;

// ReSharper disable CheckNamespace
namespace ShapeCrawler;

/// <summary>
///     Represents a portion of a paragraph.
/// </summary>
public interface IPortion
{
    /// <summary>
    ///     Gets or sets text.
    /// </summary>
    string? Text { get; set; }

    /// <summary>
    ///     Gets font.
    /// </summary>
    IFont? Font { get; }

    /// <summary>
    ///     Gets or sets hypelink.
    /// </summary>
    string? Hyperlink { get; set; }

    /// <summary>
    ///     Gets field.
    /// </summary>
    IField? Field { get; }

    /// <summary>
    ///     Gets or sets Text Highlight Color. 
    /// </summary>
    SCColor? TextHighlightColor { get; set; }

    /// <summary>
    /// 	Removes portion from paragraph.
    /// </summary>
    void Remove();
}

internal sealed class TextPortion : IPortion
{
    private readonly ResettableLazy<SCFont> font;
    private readonly A.Field? aField;
    private readonly A.Run? aRun;
    private readonly SlideStructure slideStructure;

    internal TextPortion(A.Field aField, SlideStructure slideStructure, ITextFrameContainer textFrameContainer, SCParagraph paragraph)
    {
        this.slideStructure = slideStructure;
        this.aField = aField;
        this.AText = aField.GetFirstChild<A.Text>()!;
        this.font = new ResettableLazy<SCFont>(() => new SCFont(this.AText, this, textFrameContainer, paragraph));
    }

    internal TextPortion(A.Run aRun, SlideStructure slideStructure, ITextFrameContainer textFrameContainer, SCParagraph paragraph)
    {
        this.slideStructure = slideStructure;
        this.aRun = aRun;
        this.AText = aRun.Text!;
        this.font = new ResettableLazy<SCFont>(() => new SCFont(this.AText, this, textFrameContainer, paragraph));
    }

    #region Public Properties

    /// <inheritdoc/>
    public string? Text
    {
        get => this.ParseText();
        set => this.SetText(value);
    }

    /// <inheritdoc/>
    public IFont Font => this.font.Value;

    public string? Hyperlink
    {
        get => this.GetHyperlink();
        set => this.SetHyperlink(value);
    }

    public IField? Field => this.GetField();

    public SCColor? TextHighlightColor
    {
        get => this.ParseTextHighlightColor();
        set => this.SetTextHighlightColor(value);
    }

    public void Remove()
    {
        throw new NotImplementedException();
    }

    #endregion Public Properties

    internal A.Text AText { get; }

    internal bool IsRemoved { get; set; }

    private IField? GetField()
    {
        if (this.aField is null)
        {
            return null;
        }

        return new SCField(this.aField);
    }

    private SCColor ParseTextHighlightColor()
    {
        var arPr = this.AText.PreviousSibling<A.RunProperties>();

        // Ensure RgbColorModelHex exists and his value is not null.
        if (arPr?.GetFirstChild<A.Highlight>()?.RgbColorModelHex is not A.RgbColorModelHex aSrgbClr
            || aSrgbClr.Val is null)
        {
            return SCColor.Transparent;
        }

        // Gets node value.
        // TODO: Check if DocumentFormat.OpenXml.StringValue is necessary.
        var hex = aSrgbClr.Val.ToString() !;

        // Check if color value is valid, we are expecting values as "000000".
        var color = SCColor.FromHex(hex);

        // Calculate alpha value if is defined in highlight node.
        var aAlphaValue = aSrgbClr.GetFirstChild<A.Alpha>()?.Val ?? 100000;
        color.Alpha = SCColor.OPACITY / (100000 / aAlphaValue);

        return color;
    }

    private void SetTextHighlightColor(SCColor? color)
    {
        var arPr = this.AText.PreviousSibling<A.RunProperties>() ?? this.AText.Parent!.AddRunProperties();

        arPr.AddAHighlight((SCColor)color);
    }

    private string? ParseText()
    {
        var portionText = this.AText?.Text;
        return portionText;
    }

    private void SetText(string? text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }
        
        this.AText.Text = text;
    }

    private string? GetHyperlink()
    {
        var runProperties = this.AText.PreviousSibling<A.RunProperties>();
        if (runProperties == null)
        {
            return null;
        }

        var hyperlink = runProperties.GetFirstChild<A.HyperlinkOnClick>();
        if (hyperlink == null)
        {
            return null;
        }
        
        var typedOpenXmlPart = this.slideStructure.TypedOpenXmlPart;
        var hyperlinkRelationship = (HyperlinkRelationship)typedOpenXmlPart.GetReferenceRelationship(hyperlink.Id!);

        return hyperlinkRelationship.Uri.ToString();
    }

    private void SetHyperlink(string? url)
    {
        var runProperties = this.AText.PreviousSibling<A.RunProperties>();
        if (runProperties == null)
        {
            runProperties = new A.RunProperties();
        }

        var hyperlink = runProperties.GetFirstChild<A.HyperlinkOnClick>();
        if (hyperlink == null)
        {
            hyperlink = new A.HyperlinkOnClick();
            runProperties.Append(hyperlink);
        }
        
        var slidePart = this.slideStructure.TypedOpenXmlPart;

        var uri = new Uri(url!, UriKind.RelativeOrAbsolute);
        var addedHyperlinkRelationship = slidePart.AddHyperlinkRelationship(uri, true);

        hyperlink.Id = addedHyperlinkRelationship.Id;
    }
}

internal sealed class NewLinePortion : IPortion
{
    private readonly A.Break aBreak;

    internal NewLinePortion(A.Break aBreak)
    {
        this.aBreak = aBreak;
    }

    public string? Text { get; set; } = Environment.NewLine;
    public IFont? Font { get; }

    public string? Hyperlink
    {
        get => null; 
        set => throw new SCException("New Line portion does not support hyperlink.");
    }
    public IField? Field { get; }

    public SCColor? TextHighlightColor
    {
        get => null; 
        set => throw new SCException("New Line portion does not support text highlight color.");
    }

    public void Remove()
    {
        this.aBreak.Remove();
        
    }
}