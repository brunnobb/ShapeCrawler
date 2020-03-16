﻿using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using SlideDotNet.Collections;
using SlideDotNet.Enums;
using SlideDotNet.Exceptions;
using P = DocumentFormat.OpenXml.Presentation;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using A = DocumentFormat.OpenXml.Drawing;

namespace SlideDotNet.Models.SlideComponents.Chart
{
    /// <summary>
    /// <inheritdoc cref="IChart"/>
    /// </summary>
    public class ChartEx : IChart
    {
        #region Fields

        // Contains chart elements, e.g. <c:pieChart>. If the chart type is not a combination,
        // then collection contains only single item.
        private List<OpenXmlElement> _sdkCharts;

        private readonly SlidePart _sldPart;
        private readonly P.GraphicFrame _grFrame;
        private readonly Lazy<ChartType> _chartType;
        private C.Chart _cChart;
        private Lazy<SeriesCollection> _seriesCollection;
        private Lazy<CategoryCollection> _categories;
        private string _chartTitle;
        private ChartPart _sdkChartPart;

        #endregion Fields

        #region Properties

        /// <summary>
        /// <inheritdoc cref="IChart.Type"/>
        /// </summary>
        public ChartType Type => _chartType.Value;

        /// <summary>
        /// <inheritdoc cref="IChart.Title"/>
        /// </summary>
        public string Title
        {
            get
            {
                if (_chartTitle == null)
                {
                    _chartTitle = TryGetTitle();
                }

                return _chartTitle ?? throw new SlideDotNetException(ExceptionMessages.NotTitle);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IChart.HasTitle"/>
        /// </summary>
        public bool HasTitle
        {
            get
            {
                if (_chartTitle == null)
                {
                    _chartTitle = TryGetTitle();
                }

                return _chartTitle != null;
            }
        }

        /// <summary>
        /// <inheritdoc cref="IChart.HasCategories"/>
        /// </summary>
        public bool HasCategories => _categories.Value != null;

        /// <summary>
        /// <inheritdoc cref="IChart.SeriesCollection"/>
        /// </summary>
        public SeriesCollection SeriesCollection => _seriesCollection.Value;

        /// <summary>
        /// <inheritdoc cref="IChart.Categories"/>
        /// </summary>
        public CategoryCollection Categories
        {
            get
            {
                if (_categories.Value == null)
                {
                    var msg = ExceptionMessages.ChartCanNotHaveCategory.Replace("#0", Type.ToString());
                    throw new RuntimeDefinedPropertyException(msg);
                }

                return _categories.Value;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartEx"/> class.
        /// </summary>
        public ChartEx(P.GraphicFrame grFrame, SlidePart sldPart)
        {
            _grFrame = grFrame ?? throw new ArgumentNullException(nameof(grFrame));
            _sldPart = sldPart ?? throw new ArgumentNullException(nameof(sldPart));
            _chartType = new Lazy<ChartType>(GetChartType);
            Init(); //TODO: convert to lazy loading
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            var chartPartRef = _grFrame.GetFirstChild<A.Graphic>().GetFirstChild<A.GraphicData>().GetFirstChild<C.ChartReference>().Id;
            _sdkChartPart = (ChartPart)_sldPart.GetPartById(chartPartRef);

            _cChart = _sdkChartPart.ChartSpace.GetFirstChild<C.Chart>();
            _sdkCharts = _cChart.PlotArea.Where(e => e.LocalName.EndsWith("Chart")).ToList();  // example: <c:barChart>, <c:lineChart>
            _seriesCollection = new Lazy<SeriesCollection>(GetSeriesCollection);
            _categories = new Lazy<CategoryCollection>(TryGetCategories);
        }

        private ChartType GetChartType()
        {
            if (_sdkCharts.Count > 1)
            {
                return ChartType.Combination;
            }

            var chartName = _sdkCharts.Single().LocalName;
            Enum.TryParse(chartName, true, out ChartType chartType);
            return chartType;
        }

        private string TryGetTitle()
        {
            var title = _cChart.Title;
            if (title == null) // chart has not title
            {
                return null;
            }

            var xmlChartText = title.ChartText;
            var staticAvailable = TryGetStatic(xmlChartText, out var staticTitle);
            if (staticAvailable)
            {
                return staticTitle;
            }

            // Dynamic title
            if (xmlChartText != null)
            {
                return xmlChartText.Descendants<C.StringPoint>().Single().InnerText;
            }

            if (Type == ChartType.PieChart)
            {
                // Parses PieChart dynamic title
                return _sdkCharts.Single().GetFirstChild<C.PieChartSeries>().GetFirstChild<C.SeriesText>().Descendants<C.StringPoint>().Single().InnerText;
            }

            return null;
        }

        private bool TryGetStatic(C.ChartText chartText, out string staticTitle)
        {
            staticTitle = null;
            if (Type == ChartType.Combination)
            {
                staticTitle = chartText.RichText.Descendants<A.Text>().Select(t => t.Text).Aggregate((t1, t2) => t1 + t2);
                return true;
            }

            var rRich = chartText?.RichText;
            if (rRich != null)
            {
                staticTitle = rRich.Descendants<A.Text>().Select(t => t.Text).Aggregate((t1, t2) => t1 + t2);
                return true;
            }

            return false;
        }

        private SeriesCollection GetSeriesCollection()
        {
            return new SeriesCollection(_sdkCharts, _sdkChartPart);
        }

        private CategoryCollection TryGetCategories()
        {
            if (Type == ChartType.BubbleChart || Type == ChartType.ScatterChart)
            {
                return null;
            }
            var sdkChartSeries = _sdkCharts.First().ChildElements.First(e => e.LocalName.Equals("ser"));
            return new CategoryCollection(sdkChartSeries);
        }

        #endregion
    }
}


