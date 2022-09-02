﻿using System;
using System.Linq;
using ShapeCrawler.AutoShapes;
using ShapeCrawler.Drawing;
using ShapeCrawler.Shapes;
using ShapeCrawler.SlideMasters;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

// ReSharper disable CheckNamespace
// ReSharper disable PossibleMultipleEnumeration
namespace ShapeCrawler
{
    /// <summary>
    ///     Represents AutoShape located on Slide.
    /// </summary>
    internal class SlideAutoShape : SlideShape, IAutoShape, ITextBoxContainer
    {
        private readonly Lazy<ShapeFill> shapeFill;
        private readonly Lazy<SCTextBox?> textBox;
        private readonly P.Shape pShape;

        internal SlideAutoShape(P.Shape pShape, SCSlide slideInternal, SlideGroupShape groupShape)
            : base(pShape, slideInternal, groupShape)
        {
            this.textBox = new Lazy<SCTextBox?>(this.GetTextBox);
            this.shapeFill = new Lazy<ShapeFill>(this.GetFill);
            this.pShape = pShape;
        }

        #region Public Properties

        public ITextBox? TextBox => this.textBox.Value;

        public IShapeFill Fill => this.shapeFill.Value;

        public IShape Shape => this; // TODO: should be internal?
        
        public ShapeType ShapeType => ShapeType.AutoShape;

        #endregion Public Properties

        private SCTextBox? GetTextBox()
        {
            var pTextBody = this.PShapeTreesChild.GetFirstChild<P.TextBody>();
            if (pTextBody == null)
            {
                return null;
            }

            var aTexts = pTextBody.Descendants<A.Text>();
            if (aTexts.Sum(t => t.Text.Length) > 0)
            {
                return new SCTextBox(pTextBody, this);
            }

            return null;
        }

        private ShapeFill GetFill() // TODO: duplicate of LayoutAutoShape.TryGetFill()
        {
            var slide = this.Slide;
            var image = SCImage.ForAutoShapeFill(this, slide.SDKSlidePart);

            if (image != null)
            {
                return ShapeFill.FromImage(image);
            }

            var aSolidFill = this.pShape.ShapeProperties.GetFirstChild<A.SolidFill>(); // <a:solidFill>
            if (aSolidFill == null)
            {
                return ShapeFill.CreateNoFill();
            }

            var aRgbColorModelHex = aSolidFill.RgbColorModelHex;
            if (aRgbColorModelHex != null)
            {
                return ShapeFill.FromXmlSolidFill(aRgbColorModelHex);
            }

            return ShapeFill.FromASchemeClr(aSolidFill.SchemeColor);
        }
    }
}