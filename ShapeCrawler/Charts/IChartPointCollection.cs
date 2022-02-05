﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using X = DocumentFormat.OpenXml.Spreadsheet;

namespace ShapeCrawler.Charts
{
    /// <summary>
    ///     Represents collection of chart points.
    /// </summary>
    public interface IChartPointCollection : IReadOnlyList<IChartPoint>
    {
    }

    internal class ChartPointCollection : IChartPointCollection
    {
        private readonly List<ChartPoint> chartPoints;

        private ChartPointCollection(List<ChartPoint> points)
        {
            this.chartPoints = points;
        }

        public int Count => this.chartPoints.Count;

        public IChartPoint this[int index] => this.chartPoints[index];

        public IEnumerator<IChartPoint> GetEnumerator()
        {
            return this.chartPoints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal static ChartPointCollection Create(SCChart chart, OpenXmlElement cSerXmlElement)
        {
            var cVal = cSerXmlElement.GetFirstChild<Values>();
            var cNumberReference = cVal != null ? cVal.NumberReference! : cSerXmlElement.GetFirstChild<YValues>() !.NumberReference!;

            // Get addresses
            var cFormula = cNumberReference.Formula!;
            var normalizedFormula = cFormula.Text.Replace("$", string.Empty).Replace("'", string.Empty);
            var dataSheetName = Regex.Match(normalizedFormula, @"(?<=\(*)[\p{L} 0-9]+?(?=!)").Value; // eg: Sheet1!A2:A5 -> Sheet1
            var addressMatches = Regex.Matches(normalizedFormula, @"[A-Z]\d+(:[A-Z]\d+)*"); // eg: Sheet1!A2:A5 -> A2:A5
            var pointAddresses = new List<string>();
            foreach (Match match in addressMatches)
            {
                if (match.Value.Contains(':'))
                {
                    var rangePointAddresses = new CellsRangeParser(match.Value).GetCellAddresses();
                    pointAddresses.AddRange(rangePointAddresses);
                }
                else
                {
                    pointAddresses.Add(match.Value);
                }
            }

            // Get cached values
            List<C.NumericValue>? cNumericValues = null;
            if (cNumberReference.NumberingCache != null)
            {
                cNumericValues = cNumberReference.NumberingCache.Descendants<C.NumericValue>().ToList();
            }

            // Generate points
            var chartPoints = new List<ChartPoint>(pointAddresses.Count);
            for (int i = 0; i < pointAddresses.Count; i++)
            {
                var address = pointAddresses[i];
                NumericValue? cNumericValue = null;

                // Empty cells of range don't have the corresponding C.NumericValue.
                if (i < cNumericValues?.Count)
                {
                    cNumericValue = cNumericValues[i];
                }

                chartPoints.Add(new ChartPoint(chart,  dataSheetName, address, cNumericValue));
            }

            return new ChartPointCollection(chartPoints);
        }
    }
}