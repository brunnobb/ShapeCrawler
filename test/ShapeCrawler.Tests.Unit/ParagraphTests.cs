using A = DocumentFormat.OpenXml.Drawing;
using FluentAssertions;
using NUnit.Framework;
using ShapeCrawler.Tests.Unit.Helpers;

namespace ShapeCrawler.Tests.Unit;

public class ParagraphTests : SCTest
{
    [Test]
    public void IndentLevel_Setter_sets_indent_level()
    {
        // Act
        var pres = new Presentation();
        pres.SlideCollection[0].ShapeCollection.AddShape(100,100, 500, 100);
        var addedShape = (IShape)pres.SlideCollection[0].ShapeCollection.Last();
        addedShape.TextBox!.Paragraphs.Add();
        var paragraph = addedShape.TextBox.Paragraphs.Last();
        paragraph.Text = "Test";
        
        // Act
        paragraph.IndentLevel = 2;

        // Assert
        paragraph.IndentLevel.Should().Be(2);
    }
    
    [Test]
    public void Bullet_FontName_Getter_returns_font_name()
    {
        // Arrange
        var pptx = TestAsset("002.pptx");
        var pres = new Presentation(pptx);
        var shapes = pres.SlideCollection[1].ShapeCollection;
        var shape3Pr1Bullet = ((IShape)shapes.First(x => x.Id == 3)).TextBox.Paragraphs[0].Bullet;
        var shape4Pr2Bullet = ((IShape)shapes.First(x => x.Id == 4)).TextBox.Paragraphs[1].Bullet;

        // Act
        var shape3BulletFontName = shape3Pr1Bullet.FontName;
        var shape4BulletFontName = shape4Pr2Bullet.FontName;

        // Assert
        shape3BulletFontName.Should().BeNull();
        shape4BulletFontName.Should().Be("Calibri");
    }

    [Test]
    public void Bullet_Type_Getter_returns_bullet_type()
    {
        // Arrange
        var pptx = TestAsset("002.pptx");
        var pres = new Presentation(pptx);
        var shapeList = pres.SlideCollection[1].ShapeCollection;
        var shape4 = shapeList.First(x => x.Id == 4);
        var shape5 = shapeList.First(x => x.Id == 5);
        var shape4Pr2Bullet = ((IShape)shape4).TextBox.Paragraphs[1].Bullet;
        var shape5Pr1Bullet = ((IShape)shape5).TextBox.Paragraphs[0].Bullet;
        var shape5Pr2Bullet = ((IShape)shape5).TextBox.Paragraphs[1].Bullet;

        // Act
        var shape5Pr1BulletType = shape5Pr1Bullet.Type;
        var shape5Pr2BulletType = shape5Pr2Bullet.Type;
        var shape4Pr2BulletType = shape4Pr2Bullet.Type;

        // Assert
        shape5Pr1BulletType.Should().Be(BulletType.Numbered);
        shape5Pr2BulletType.Should().Be(BulletType.Picture);
        shape4Pr2BulletType.Should().Be(BulletType.Character);
    }

    [Test]
    public void HorizontalAlignment_Setter_sets_horizontal_alignment()
    {
        // Arrange
        var pres = new Presentation();
        pres.Slide(1).ShapeCollection.AddTable(10, 10, 2, 2);
        var table = pres.Slide(1).ShapeCollection.Last<ITable>();
        var textFrame = table.Rows[0].Cells[0].TextBox;
        textFrame.Text = "some-text";
        var paragraph = textFrame.Paragraphs[0];
        
        // Act 
        paragraph.HorizontalAlignment = TextHorizontalAlignment.Center;

        // Assert 
        paragraph.HorizontalAlignment.Should().Be(TextHorizontalAlignment.Center);
        pres.Validate();
    }

    [Test]
    public void Paragraph_Bullet_Type_Getter_returns_None_value_When_paragraph_doesnt_have_bullet()
    {
        // Arrange
        var pptx = TestAsset("001.pptx");
        var pres = new Presentation(pptx);
        var autoShape = pres.SlideCollection[0].ShapeCollection.GetById<IShape>(2);
        var bullet = autoShape.TextBox.Paragraphs[0].Bullet;

        // Act
        var bulletType = bullet.Type;

        // Assert
        bulletType.Should().Be(BulletType.None);
    }

    [Test]
    public void Paragraph_BulletColorHexAndCharAndSizeProperties_ReturnCorrectValues()
    {
        // Arrange
        var pres2 = new Presentation(TestAsset("002.pptx"));
        var shapeList = pres2.SlideCollection[1].ShapeCollection;
        var shape4 = shapeList.First(x => x.Id == 4);
        var shape4Pr2Bullet = ((IShape)shape4).TextBox.Paragraphs[1].Bullet;

        // Act
        var bulletColorHex = shape4Pr2Bullet.ColorHex;
        var bulletChar = shape4Pr2Bullet.Character;
        var bulletSize = shape4Pr2Bullet.Size;

        // Assert
        bulletColorHex.Should().Be("C00000");
        bulletChar.Should().Be("'");
        bulletSize.Should().Be(120);
    }
        
    [Test]
    [Platform(Exclude = "Linux", Reason = "Test fails on ubuntu-latest")]
    public void Paragraph_Text_Setter_updates_paragraph_text_and_resize_shape()
    {
        // Arrange
        var pres = new Presentation(TestAsset("autoshape-case003.pptx"));
        var shape = pres.Slide(1).Shape("AutoShape 4");
        var paragraph = shape.TextBox.Paragraphs[0];
            
        // Act
        paragraph.Text = "AutoShape 4 some text";

        // Assert
        shape.Height.Should().BeApproximately(51.48m,0.01m);
        shape.Y.Should().Be(145m);
    }

    [Test]
    public void Text_Setter_sets_paragraph_text_in_New_Presentation()
    {
        // Arrange
        var pres = new Presentation();
        var slide = pres.SlideCollection[0];
        slide.ShapeCollection.AddShape(10, 10, 10, 10);
        var addedShape = (IShape)slide.ShapeCollection.Last();
        var paragraph = addedShape.TextBox!.Paragraphs[0];

        // Act
        paragraph.Text = "test";

        // Assert
        paragraph.Text.Should().Be("test");
        pres.Validate();
    }
    
    [Test]
    public void Text_Setter_sets_paragraph_text_for_grouped_shape()
    {
        // Arrange
        var pres = new Presentation(TestAsset("autoshape-case003.pptx"));
        var shape = pres.SlideCollection[0].ShapeCollection.GetByName<IGroupShape>("Group 1").Shapes.GetByName<IShape>("Shape 1");
        var paragraph = shape.TextBox.Paragraphs[0];
        
        // Act
        paragraph.Text = $"Safety{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";
        
        // Assert
        paragraph.Text.Should().BeEquivalentTo($"Safety{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
        pres.Validate();
    }
    
    [Test]
    public void Paragraph_Text_Getter_returns_paragraph_text()
    {
        // Arrange
        var textBox1 = ((IShape)new Presentation(TestAsset("008.pptx")).SlideCollection[0].ShapeCollection.First(sp => sp.Id == 37)).TextBox;
        var textBox2 = ((ITable)new Presentation(TestAsset("009_table.pptx")).SlideCollection[2].ShapeCollection.First(sp => sp.Id == 3)).Rows[0].Cells[0]
            .TextBox;

        // Act
        string paragraphTextCase1 = textBox1.Paragraphs[0].Text;
        string paragraphTextCase2 = textBox1.Paragraphs[1].Text;
        string paragraphTextCase3 = textBox2.Paragraphs[0].Text;

        // Assert
        paragraphTextCase1.Should().BeEquivalentTo("P1t1 P1t2");
        paragraphTextCase2.Should().BeEquivalentTo("p2");
        paragraphTextCase3.Should().BeEquivalentTo("0:0_p1_lvl1");
    }

    [Test]
    public void ReplaceText_finds_and_replaces_text()
    {
        // Arrange
        var pptxStream = TestAsset("autoshape-case003.pptx");
        var pres = new Presentation(pptxStream);
        var paragraph = pres.SlideCollection[0].ShapeCollection.GetByName<IShape>("AutoShape 3").TextBox!.Paragraphs[0];
            
        // Act
        paragraph.ReplaceText("Some text", "Some text2");

        // Assert
        paragraph.Text.Should().BeEquivalentTo("Some text2");
        pres.Validate();
    }

    [Test]
    public void Paragraph_Portions_counter_returns_number_of_text_portions_in_the_paragraph()
    {
        // Arrange
        var textFrame = new Presentation(TestAsset("009_table.pptx")).SlideCollection[2].ShapeCollection.GetById<IShape>(2).TextBox;

        // Act
        var portions = textFrame.Paragraphs[0].Portions;

        // Assert
        portions.Should().HaveCount(2);
    }
    
    [Test]
    public void Portions_Add()
    {
        // Arrange
        var pres = new Presentation(TestAsset("autoshape-case001.pptx"));
        var shape = pres.SlideMasters[0].Shapes.GetByName<IShape>("AutoShape 1");
        shape.TextBox!.Paragraphs.Add();
        var paragraph = shape.TextBox.Paragraphs.Last();
        var expectedPortionCount = paragraph.Portions.Count + 1;
        
        // Act
        paragraph.Portions.AddText(" ");
        
        // Assert
        paragraph.Portions.Count.Should().Be(expectedPortionCount);
    }
    
    [Test]
    [SlideParagraph("Case #1","autoshape-case003.pptx", 1, "AutoShape 5", 1, 1)]
    [SlideParagraph("Case #2","autoshape-case003.pptx", 1, "AutoShape 5", 2, 2)]
    public void IndentLevel_Getter_returns_indent_level(IParagraph paragraph, int expectedLevel)
    {
        // Act
        var indentLevel = paragraph.IndentLevel;

        // Assert
        indentLevel.Should().Be(expectedLevel);
    }
    
    [Test]
    [SlideShape("001.pptx", 1, "TextBox 3", TextHorizontalAlignment.Center)]
    [SlideShape("001.pptx", 1, "Head 1", TextHorizontalAlignment.Center)]
    public void HorizontalAlignment_Getter_returns_text_horizontal_alignment(IShape autoShape, TextHorizontalAlignment expectedAlignment)
    {
        // Arrange
        var paragraph = autoShape.TextBox.Paragraphs[0];

        // Act-Assert
        paragraph.HorizontalAlignment.Should().Be(expectedAlignment);
    }

    [Test]
    [TestCase("001.pptx", 1, "TextBox 4")]
    public void HorizontalAlignment_Setter_updates_text_horizontal_alignment(string presName, int slideNumber, string shapeName)
    {
        // Arrange
        var pres = new Presentation(TestAsset(presName));
        var paragraph = pres.SlideCollection[slideNumber - 1].ShapeCollection.GetByName<IShape>(shapeName).TextBox.Paragraphs[0];
        var mStream = new MemoryStream();
        
        // Act
        paragraph.HorizontalAlignment = TextHorizontalAlignment.Right;

        // Assert
        paragraph.HorizontalAlignment.Should().Be(TextHorizontalAlignment.Right);

        pres.SaveAs(mStream);
        pres = new Presentation(mStream);
        paragraph = pres.SlideCollection[slideNumber - 1].ShapeCollection.GetByName<IShape>(shapeName).TextBox.Paragraphs[0];
        paragraph.HorizontalAlignment.Should().Be(TextHorizontalAlignment.Right);
    }

    [Test]
    [TestCase("002.pptx", 2, 4, 3, "Text", 1)]
    [TestCase("002.pptx", 2, 4, 3, "Text{NewLine}", 2)]
    [TestCase("002.pptx", 2, 4, 3, "Text{NewLine}Text2", 3)]
    [TestCase("002.pptx", 2, 4, 3, "Text{NewLine}Text2{NewLine}", 4)]
    [TestCase("023.pptx", 1, 2, 2, "Text", 1)]
    public void Text_Setter_sets_paragraph_text(string presName, int slideNumber, int shapeId, int paraNumber, string paraText, int expectedPortionsCount)
    {
        // Arrange
        var pres = new Presentation(TestAsset(presName));
        var paragraph = pres.SlideCollection[slideNumber - 1].ShapeCollection.GetById<IShape>(shapeId).TextBox.Paragraphs[paraNumber - 1];
        var mStream = new MemoryStream();
        paraText = paraText.Replace("{NewLine}", Environment.NewLine);

        // Act
        paragraph.Text = paraText;

        // Assert
        paragraph.Text.Should().BeEquivalentTo(paraText);
        paragraph.Portions.Count.Should().Be(expectedPortionsCount);

        pres.SaveAs(mStream);
        pres = new Presentation(mStream);
        paragraph = pres.SlideCollection[slideNumber - 1].ShapeCollection.GetById<IShape>(shapeId).TextBox.Paragraphs[paraNumber - 1];
        paragraph.Text.Should().BeEquivalentTo(paraText);
        paragraph.Portions.Count.Should().Be(expectedPortionsCount);
    }
    
    [Test]
    [SlideShape("autoshape-grouping.pptx", 1, "TextBox 5", 1.0)]
    [SlideShape("autoshape-grouping.pptx", 1, "TextBox 4", 1.5)]
    [SlideShape("autoshape-grouping.pptx", 1, "TextBox 3", 2.0)]
    public void Paragraph_Spacing_LineSpacingLines_returns_line_spacing_in_Lines(IShape shape, double expectedLines)
    {
        // Arrange
        var paragraph = shape.TextBox!.Paragraphs[0];
            
        // Act
        var spacingLines = paragraph.Spacing.LineSpacingLines;
            
        // Assert
        spacingLines.Should().Be(expectedLines);
        paragraph.Spacing.LineSpacingPoints.Should().BeNull();
    }
    
    [Test]
    [SlideShape("autoshape-grouping.pptx", 1, "TextBox 6", 21.6)]
    public void Paragraph_Spacing_LineSpacingPoints_returns_line_spacing_in_Points(IShape shape, double expectedPoints)
    {
        // Arrange
        var paragraph = shape.TextBox!.Paragraphs[0];
            
        // Act
        var spacingPoints = paragraph.Spacing.LineSpacingPoints;
            
        // Assert
        spacingPoints.Should().Be(expectedPoints);
        paragraph.Spacing.LineSpacingLines.Should().BeNull();
    }
    
    [Test]
    [SlideShape("062_text_spacing.pptx", 1, "TextBox 4", 50)]
    public void Spacing_BeforeSpacingPoints_returns_before_spacing_in_points(IShape shape, double expectedPoints)
    {
        // Arrange
        var paragraph = shape.TextBox!.Paragraphs[0];
            
        // Act-Assert
        paragraph.Spacing.BeforeSpacingPoints.Should().Be(expectedPoints);
    }
    
    [Test]
    [SlideShape("062_text_spacing.pptx", 1, "TextBox 4", 50)]
    public void Spacing_AfterSpacingPoints_returns_after_spacing_in_points(IShape shape, double expectedPoints)
    {
        // Arrange
        var paragraph = shape.TextBox!.Paragraphs[0];
            
        // Act-Assert
        paragraph.Spacing.AfterSpacingPoints.Should().Be(expectedPoints);
    }
    
    [Test]
    public void Spacing_BeforeSpacingPoints_Setter_sets_before_spacing()
    {
        // Arrange
        var pres = new Presentation();
        var slide = pres.SlideCollection[0];
        slide.ShapeCollection.AddShape(10, 10, 10, 10);
        var addedShape = slide.ShapeCollection[0];
        var paragraph = addedShape.TextBox.Paragraphs[0];
        paragraph.Text = "test";
        
        // Act
        paragraph.Spacing.BeforeSpacingPoints = 50;
        
        // Assert
        paragraph.Spacing.BeforeSpacingPoints.Should().Be(50);

        using var mStream = new MemoryStream();
        pres.SaveAs(mStream);
        pres = new Presentation(mStream);
        paragraph = pres.SlideCollection[0].ShapeCollection.Last().TextBox.Paragraphs[0];
        paragraph.Spacing.BeforeSpacingPoints.Should().Be(50);
    }
    
    [Test]
    public void Spacing_AfterSpacingPoints_Setter_sets_after_spacing()
    {
        // Arrange
        var pres = new Presentation();
        var slide = pres.SlideCollection[0];
        slide.ShapeCollection.AddShape(10, 10, 10, 10);
        var addedShape = slide.ShapeCollection[0];
        var paragraph = addedShape.TextBox.Paragraphs[0];
        paragraph.Text = "test";
        
        // Act
        paragraph.Spacing.AfterSpacingPoints = 50;
        
        // Assert
        paragraph.Spacing.AfterSpacingPoints.Should().Be(50);
        var mStream = new MemoryStream();
        pres.SaveAs(mStream);
        pres = new Presentation(mStream);
        paragraph = pres.SlideCollection[0].ShapeCollection.Last().TextBox.Paragraphs[0];
        paragraph.Spacing.AfterSpacingPoints.Should().Be(50);
    }
}
