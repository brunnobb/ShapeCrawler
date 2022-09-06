﻿#if DEBUG

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using ShapeCrawler.Extensions;
using ShapeCrawler.Media;
using ShapeCrawler.Shapes;
using ShapeCrawler.Tests.Helpers;
using ShapeCrawler.Tests.Properties;
using Xunit;

// ReSharper disable TooManyDeclarations
// ReSharper disable InconsistentNaming
// ReSharper disable TooManyChainedReferences

namespace ShapeCrawler.Tests
{
    public class ShapeTests : ShapeCrawlerTest, IClassFixture<PresentationFixture>
    {
        private readonly PresentationFixture _fixture;

        public ShapeTests(PresentationFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(TestCasesPlaceholderType))]
        public void PlaceholderType_GetterReturnsPlaceholderTypeOfTheShape(IShape shape, PlaceholderType expectedType)
        {
            // Act
            PlaceholderType actualType = shape.Placeholder.Type;

            // Assert
            actualType.Should().Be(expectedType);
        }

        public static IEnumerable<object[]> TestCasesPlaceholderType()
        {
            IShape shape = SCPresentation.Open(Resources._021, false).Slides[3].Shapes.First(sp => sp.Id == 2);
            yield return new object[] { shape, PlaceholderType.Footer };

            shape = SCPresentation.Open(Resources._008, false).Slides[0].Shapes.First(sp => sp.Id == 3);
            yield return new object[] { shape, PlaceholderType.DateAndTime };

            shape = SCPresentation.Open(Resources._019, false).Slides[0].Shapes.First(sp => sp.Id == 2);
            yield return new object[] { shape, PlaceholderType.SlideNumber };

            shape = SCPresentation.Open(Resources._013, false).Slides[0].Shapes.First(sp => sp.Id == 281);
            yield return new object[] { shape, PlaceholderType.Custom };
        }

        [Fact]
        public void AudioShape_BinaryData_returns_audio_bytes()
        {
            // Arrange
            var pptxStream = GetTestFileStream("audio-case001.pptx");
            var pres = SCPresentation.Open(pptxStream, false);
            var audioShape = pres.Slides[0].Shapes.GetByName<IAudioShape>("Audio 1");

            // Act
            var bytes = audioShape.BinaryData;

            // Assert
            bytes.Should().NotBeEmpty();
        }
        
        [Fact]
        public void AudioShape_MIME_returns_MIME_type_of_audio_content()
        {
            // Arrange
            var pptxStream = GetTestFileStream("audio-case001.pptx");
            var pres = SCPresentation.Open(pptxStream, false);
            var audioShape = pres.Slides[0].Shapes.GetByName<IAudioShape>("Audio 1");

            // Act
            var mime = audioShape.MIME;

            // Assert
            mime.Should().Be("audio/mpeg");
        }

        [Fact]
        public void AutoShapeFill_IsNotNull_WhenAutoShapeIsFilled()
        {
            // Arrange
            IAutoShape autoShape = (IAutoShape)_fixture.Pre021.Slides[0].Shapes.First(sp => sp.Id == 108);

            // Act-Assert
            autoShape.Fill.Should().NotBeNull();
        }

        [Fact(Skip = "https://github.com/ShapeCrawler/ShapeCrawler/issues/279")]
        public void AutoShape_Fill_SetPicture_sets_specified_picture_as_a_shape_fill_When_shape_is_Not_filled()
        {
            // Arrange
            var pptxStream = GetTestFileStream("008.pptx");
            var imageStream = GetTestFileStream("test-image-1.png");
            var pres = SCPresentation.Open(pptxStream, true);
            var shape = pres.Slides[0].Shapes.GetByName<IAutoShape>("AutoShape 1");

            // Act
            shape.Fill.SetPicture(imageStream);

            // Assert
            var pictureBytes = shape.Fill.Picture!.GetBytes().Result;
            imageStream.Position = 0;
            var imageBytes = imageStream.ToArray();
            pictureBytes.SequenceEqual(imageBytes).Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(TestCasesFillType))]
        public void AutoShape_Fill_Type_returns_fill_type(IAutoShape shape, FillType expectedFill)
        {
            // Act
            var fillType = shape.Fill.Type;

            // Assert
            fillType.Should().Be(expectedFill);
        }

        public static IEnumerable<object[]> TestCasesFillType()
        {
            var pptxStream = GetTestFileStream("009_table.pptx");
            var pres = SCPresentation.Open(pptxStream, false);
            var autoShape = pres.Slides[2].Shapes.GetById<IAutoShape>(4);
            yield return new object[] { autoShape, FillType.Picture };

            autoShape = pres.Slides[1].Shapes.GetById<IAutoShape>(2);
            yield return new object[] { autoShape, FillType.Solid };

            autoShape = pres.Slides[1].Shapes.GetById<IAutoShape>(6);
            yield return new object[] { autoShape, FillType.NoFill };
        }

        [Fact]
        public void AutoShape_Fill_Type_returns_NoFill_When_shape_is_Not_filled()
        {
            // Arrange
            var autoShape = (IAutoShape)_fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 6);

            // Act
            var fillType = autoShape.Fill.Type;

            // Assert
            fillType.Should().Be(FillType.NoFill);
        }

        [Fact]
        public void AutoShapeFillSolidColorName_GetterReturnsSolidColorNameByWhichTheAutoShapeIsFilled()
        {
            // Arrange
            IAutoShape autoShape = (IAutoShape)_fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 2);

            // Act
            var shapeSolidColorName = autoShape.Fill.SolidColor.Name;

            // Assert
            shapeSolidColorName.Should().BeEquivalentTo("ff0000");
        }

        [Fact]
        public async void AutoShapeFillPictureGetImageBytes_ReturnsImageByWhichTheAutoShapeIsFilled()
        {
            // Arrange
            IAutoShape shape = (IAutoShape)_fixture.Pre009.Slides[2].Shapes.First(sp => sp.Id == 4);

            // Act
            byte[] imageBytes = await shape.Fill.Picture.GetBytes().ConfigureAwait(false);

            // Assert
            imageBytes.Length.Should().BePositive();
        }

        [Fact]
        public void AutoShapeFillPictureSetImage_ChangesPicture()
        {
            // Arrange
            IPresentation presentation = SCPresentation.Open(TestFiles.Presentations.pre009, true);
            IAutoShape autoShape = (IAutoShape)presentation.Slides[2].Shapes.First(sp => sp.Id == 4);
            MemoryStream newImage = TestFiles.Images.img02_stream;
            int imageSizeBefore = autoShape.Fill.Picture.GetBytes().GetAwaiter().GetResult().Length;

            // Act
            autoShape.Fill.Picture.SetImage(newImage);

            // Assert
            int imageSizeAfter = autoShape.Fill.Picture.GetBytes().GetAwaiter().GetResult().Length;
            imageSizeAfter.Should().NotBe(imageSizeBefore, "because image has been changed");
        }

        [Fact]
        public void VideoShape_BinaryData_returns_video_bytes()
        {
            // Arrange
            var pptxStream = GetTestFileStream("video-case001.pptx");
            var pres = SCPresentation.Open(pptxStream, false);
            var videoShape = pres.Slides[0].Shapes.GetByName<IVideoShape>("Video 1");

            // Act
            var bytes = videoShape.BinaryData;

            // Assert
            bytes.Should().NotBeEmpty();
        }
        
        [Fact]
        public void AudioShape_MIME_returns_MIME_type_of_video_content()
        {
            // Arrange
            var pptxStream = GetTestFileStream("video-case001.pptx");
            var pres = SCPresentation.Open(pptxStream, false);
            var videoShape = pres.Slides[0].Shapes.GetByName<IVideoShape>("Video 1");

            // Act
            var mime = videoShape.MIME;

            // Assert
            mime.Should().Be("video/mp4");
        }

        [Fact]
        public void PictureSetImage_ShouldNotImpactOtherPictureImage_WhenItsOriginImageIsShared()
        {
            // Arrange
            IPresentation presentation = SCPresentation.Open(TestFiles.Presentations.pre009, true);
            IPicture picture5 = (IPicture)presentation.Slides[3].Shapes.First(sp => sp.Id == 5);
            IPicture picture6 = (IPicture)presentation.Slides[3].Shapes.First(sp => sp.Id == 6);
            int pic6LengthBefore = picture6.Image.GetBytes().GetAwaiter().GetResult().Length;
            MemoryStream modifiedPresentation = new();

            // Act
            picture5.Image.SetImage(TestFiles.Images.imageByteArray02);

            // Assert
            int pic6LengthAfter = picture6.Image.GetBytes().GetAwaiter().GetResult().Length;
            pic6LengthAfter.Should().Be(pic6LengthBefore);

            presentation.SaveAs(modifiedPresentation);
            presentation = SCPresentation.Open(modifiedPresentation, false);
            picture6 = (IPicture)presentation.Slides[3].Shapes.First(sp => sp.Id == 6);
            pic6LengthBefore = picture6.Image.GetBytes().GetAwaiter().GetResult().Length;
            pic6LengthAfter.Should().Be(pic6LengthBefore);
        }

        [Fact]
        public void X_Getter_returns_x_coordinate_in_pixels()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre021.Slides[3].Shapes.First(sp => sp.Id == 2);
            IShape shapeCase2 = _fixture.Pre008.Slides[0].Shapes.First(sp => sp.Id == 3);
            IShape shapeCase3 = _fixture.Pre006.Slides[0].Shapes.First(sp => sp.Id == 2);
            IShape shapeCase4 = _fixture.Pre018.Slides[0].Shapes.First(sp => sp.Id == 7);
            IShape shapeCase5 = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 9);
            IShape shapeCase6 = _fixture.Pre025.Slides[2].Shapes.First(sp => sp.Id == 7);
            float horizontalResolution = TestHelper.HorizontalResolution;

            // Act
            int xCoordinateCase1 = shapeCase1.X;
            int xCoordinateCase2 = shapeCase2.X;
            int xCoordinateCase3 = shapeCase3.X;
            int xCoordinateCase6 = shapeCase5.X;
            int xCoordinateCase7 = shapeCase6.X;

            // Assert
            xCoordinateCase1.Should().Be((int)(3653579 * horizontalResolution / 914400));
            xCoordinateCase2.Should().Be((int)(628650 * horizontalResolution / 914400));
            xCoordinateCase3.Should().Be((int)(1524000 * horizontalResolution / 914400));
            xCoordinateCase6.Should().Be((int)(699323 * horizontalResolution / 914400));
            xCoordinateCase7.Should().Be((int)(757383 * horizontalResolution / 914400));
        }

        [Fact]
        public void X_Getter_returns_x_coordinate_in_pixels_of_Grouped_shape()
        {
            // Arrange
            IGroupShape groupShape = (IGroupShape)_fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 7);
            IShape groupedShape = groupShape.Shapes.First(sp => sp.Id == 5);
            float horizontalResolution = TestHelper.HorizontalResolution;

            // Act
            int xCoordinate = groupedShape.X;

            // Assert
            xCoordinate.Should().Be((int)(1581846 * horizontalResolution / 914400));
        }

        [Fact]
        public void Y_Getter_returns_y_coordinate_in_pixels()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre006.Slides[0].Shapes.First(sp => sp.Id == 2);
            IShape shapeCase2 = _fixture.Pre018.Slides[0].Shapes.First(sp => sp.Id == 7);
            IShape shapeCase3 = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 9);
            float verticalResoulution = TestHelper.VerticalResolution;

            // Act
            int yCoordinate1 = shapeCase1.Y;
            int yCoordinate2 = shapeCase2.Y;
            int yCoordinate3 = shapeCase3.Y;

            // Assert
            yCoordinate1.Should().Be((int)(1122363 * verticalResoulution / 914400));
            yCoordinate2.Should().Be((int)(4 * verticalResoulution / 914400));
            yCoordinate3.Should().Be((int)(3463288 * verticalResoulution / 914400));
        }

        [Fact]
        public void Y_Setter_updates_y_coordinate()
        {
            // Arrange
            var autoShape = GetAutoShape("001.pptx", 1, 4);

            // Act
            autoShape.Y = 100;

            // Assert
            autoShape.Y.Should().Be(100);
        }

        [Fact]
        public void XAndWidth_SetterSetXAndWidthOfTheShape()
        {
            // Arrange
            IPresentation presentation = SCPresentation.Open(Resources._006_1_slides, true);
            IShape shape = presentation.Slides.First().Shapes.First(sp => sp.Id == 3);
            Stream stream = new MemoryStream();
            const int xPixels = 400;
            const int widthPixels = 600;

            // Act
            shape.X = xPixels;
            shape.Width = widthPixels;

            // Assert
            presentation.SaveAs(stream);
            presentation = SCPresentation.Open(stream, false);
            shape = presentation.Slides.First().Shapes.First(sp => sp.Id == 3);

            shape.X.Should().Be(xPixels);
            shape.Width.Should().Be(widthPixels);
        }

        [Fact]
        public void Width_returns_shape_width_in_pixels()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre006.Slides[0].Shapes.First(sp => sp.Id == 2);
            IGroupShape groupShape = (IGroupShape)_fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 7);
            IShape shapeCase2 = groupShape.Shapes.First(sp => sp.Id == 5);
            IShape shapeCase3 = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 9);

            // Act
            int width1 = shapeCase1.Width;
            int width2 = shapeCase2.Width;
            int width3 = shapeCase3.Width;

            // Assert
            (width1 * 914400 / TestHelper.HorizontalResolution).Should().Be(9144000);
            (width2 * 914400 / TestHelper.HorizontalResolution).Should().Be(1181100);
            (width3 * 914400 / TestHelper.HorizontalResolution).Should().Be(485775);
        }

        [Theory]
        [InlineData("050_title-placeholder.pptx", 1, 2, 777)]
        [InlineData("051_title-placeholder.pptx", 1, 3074, 864)]
        public void Width_returns_width_of_Title_placeholder(string filename, int slideNumber, int shapeId,
            int expectedWidth)
        {
            // Arrange
            var autoShape = GetAutoShape(filename, slideNumber, shapeId);

            // Act
            var shapeWidth = autoShape.Width;

            // Assert
            shapeWidth.Should().Be(expectedWidth);
        }

        [Fact]
        public void Height_ReturnsHeightInPixels()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre006.Slides[0].Shapes.First(sp => sp.Id == 2);
            IGroupShape groupShape = (IGroupShape)_fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 7);
            IShape shapeCase2 = groupShape.Shapes.First(sp => sp.Id == 5);
            IShape shapeCase3 = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 9);
            float verticalResulution = TestHelper.VerticalResolution;

            // Act
            int height1 = shapeCase1.Height;
            int height2 = shapeCase2.Height;
            int height3 = shapeCase3.Height;

            // Assert
            (height1 * 914400 / verticalResulution).Should().Be(1419225);
            (height2 * 914400 / verticalResulution).Should().Be(647700);
            (height3 * 914400 / verticalResulution).Should().Be(371475);
        }

        [Theory]
        [MemberData(nameof(GeometryTypeTestCases))]
        public void GeometryType_returns_shape_geometry_type(IShape shape, GeometryType expectedGeometryType)
        {
            // Assert
            shape.GeometryType.Should().Be(expectedGeometryType);
        }

        public static IEnumerable<object[]> GeometryTypeTestCases()
        {
            var pptxStream = GetTestFileStream("021.pptx");
            var presentation = SCPresentation.Open(pptxStream, false);
            var shapeCase1 = presentation.Slides[3].Shapes.First(sp => sp.Id == 2);
            var shapeCase2 = presentation.Slides[3].Shapes.First(sp => sp.Id == 3);

            yield return new object[] { shapeCase1, GeometryType.Rectangle };
            yield return new object[] { shapeCase2, GeometryType.Ellipse };
        }

        [Fact]
        public void Shape_IsOLEObject()
        {
            // Arrange
            IOLEObject oleObject = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 8) as IOLEObject;

            // Act-Assert
            oleObject.Should().NotBeNull();
        }

        [Fact]
        public void Shape_IsNotGroupShape()
        {
            // Arrange
            IShape shape = _fixture.Pre006.Slides[0].Shapes.First(x => x.Id == 3);

            // Act-Assert
            shape.Should().NotBeOfType<IGroupShape>();
        }

        [Fact]
        public void Shape_IsNotAutoShape()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre009.Slides[4].Shapes.First(sp => sp.Id == 5);
            IShape shapeCase2 = _fixture.Pre011.Slides[0].Shapes.First(sp => sp.Id == 4);

            // Act-Assert
            shapeCase1.Should().NotBeOfType<IAutoShape>();
            shapeCase2.Should().NotBeOfType<IAutoShape>();
        }

        [Fact]
        public void CustomData_ReturnsNull_WhenShapeHasNotCustomData()
        {
            // Arrange
            var shape = _fixture.Pre009.Slides.First().Shapes.First();

            // Act
            var shapeCustomData = shape.CustomData;

            // Assert
            shapeCustomData.Should().BeNull();
        }

        [Fact]
        public void CustomData_ReturnsCustomDataOfTheShape_WhenShapeWasAssignedSomeCustomData()
        {
            // Arrange
            const string customDataString = "Test custom data";
            var savedPreStream = new MemoryStream();
            var presentation = SCPresentation.Open(Resources._009, true);
            var shape = presentation.Slides.First().Shapes.First();

            // Act
            shape.CustomData = customDataString;
            presentation.SaveAs(savedPreStream);

            // Assert
            presentation = SCPresentation.Open(savedPreStream, false);
            shape = presentation.Slides.First().Shapes.First();
            shape.CustomData.Should().Be(customDataString);
        }

        [Fact]
        public void Name_ReturnsShapeNameString()
        {
            // Arrange
            IShape shape = _fixture.Pre009.Slides[1].Shapes.First(sp => sp.Id == 8);

            // Act
            string shapeName = shape.Name;

            // Assert
            shapeName.Should().BeEquivalentTo("Object 2");
        }

        [Fact]
        public void Hidden_ReturnsValueIndicatingWhetherShapeIsHiddenFromTheSlide()
        {
            // Arrange
            IShape shapeCase1 = _fixture.Pre004.Slides[0].Shapes[0];
            IShape shapeCase2 = _fixture.Pre004.Slides[0].Shapes[1];

            // Act-Assert
            shapeCase1.Hidden.Should().BeTrue();
            shapeCase2.Hidden.Should().BeFalse();
        }
    }
}

#endif