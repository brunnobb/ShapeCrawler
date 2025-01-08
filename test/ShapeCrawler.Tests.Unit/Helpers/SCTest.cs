﻿using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;

namespace ShapeCrawler.Tests.Unit.Helpers;

public abstract class SCTest
{
    protected static T GetShape<T>(string presentation, int slideNumber, int shapeId)
    {
        var scPresentation = GetPresentationFromAssembly(presentation);
        var slide = scPresentation.Slides[slideNumber - 1];
        var shape = slide.Shapes.First(sp => sp.Id == shapeId);

        return (T)shape;
    }

    protected static T GetWorksheetCellValue<T>(byte[] workbookByteArray, string cellAddress)
    {
        var stream = new MemoryStream(workbookByteArray);
        var xlWorkbook = new XLWorkbook(stream);
        var cellValue = xlWorkbook.Worksheets.First().Cell(cellAddress).Value;

        return (T)cellValue;
    }

    protected static byte[] GetTestBytes(string fileName)
    {
        return StreamOf(fileName).ToArray();
    }

    public static MemoryStream StreamOf(string file)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetResourceStream(file);
        var mStream = new MemoryStream();
        stream!.CopyTo(mStream);

        return mStream;
    }

    protected static string StringOf(string fileName)
    {
        var stream = StreamOf(fileName);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
    
    protected string GetTestPath(string fileName)
    {
        var stream = StreamOf(fileName);
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, stream.ToArray());

        return path;
    }

    protected static Presentation SaveAndOpenPresentation(IPresentation presentation)
    {
        var stream = new MemoryStream();
        presentation.SaveAs(stream);

        return new Presentation(stream);
    }

    protected static PresentationDocument SaveAndOpenPresentationAsSdk(IPresentation presentation)
    {
        var stream = new MemoryStream();
        presentation.SaveAs(stream);
        stream.Position = 0;

        return PresentationDocument.Open(stream, true);
    }

#if DEBUG

    protected void SaveResult(IPresentation pres)
    {

        var testFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "TestResults",
            TestContext.CurrentContext.Test.MethodName);
        Directory.CreateDirectory(testFolder);

        pres.SaveAs(Path.Combine(testFolder, "result.pptx"));
    }

#endif

    private static IPresentation GetPresentationFromAssembly(string fileName)
    {
        var stream = StreamOf(fileName);

        return new Presentation(stream);
    }
}