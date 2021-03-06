// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Media;
using Avalonia.Visuals.Platform;
using Xunit;

namespace Avalonia.Visuals.UnitTests.Media
{
    public class PathMarkupParserTests
    {
        [Fact]
        public void Parses_Move()
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {               
                parser.Parse("M10 10");

                var figure = pathGeometry.Figures[0];

                Assert.Equal(new Point(10, 10), figure.StartPoint);
            }
        }

        [Fact]
        public void Parses_Line()
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse("M0 0L10 10");

                var figure = pathGeometry.Figures[0];

                var segment = figure.Segments[0];

                Assert.IsType<LineSegment>(segment);

                var lineSegment = (LineSegment)segment;

                Assert.Equal(new Point(10, 10), lineSegment.Point);
            }
        }

        [Fact]
        public void Parses_Close()
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse("M0 0L10 10z");

                var figure = pathGeometry.Figures[0];

                Assert.True(figure.IsClosed);
            }
        }

        [Fact]
        public void Parses_FillMode_Before_Move()
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse("F 1M0,0");             

                Assert.Equal(FillRule.NonZero, pathGeometry.FillRule);
            }
        }

        [Theory]
        [InlineData("M0 0 10 10 20 20")]
        [InlineData("M0,0 10,10 20,20")]
        [InlineData("M0,0,10,10,20,20")]
        public void Parses_Implicit_Line_Command_After_Move(string pathData)
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse(pathData);

                var figure = pathGeometry.Figures[0];

                var segment = figure.Segments[0];

                Assert.IsType<LineSegment>(segment);

                var lineSegment = (LineSegment)segment;

                Assert.Equal(new Point(10, 10), lineSegment.Point);

                figure = pathGeometry.Figures[1];

                segment = figure.Segments[0];

                Assert.IsType<LineSegment>(segment);

                lineSegment = (LineSegment)segment;

                Assert.Equal(new Point(20, 20), lineSegment.Point);
            }
        }

        [Theory]
        [InlineData("m0 0 10 10 20 20")]
        [InlineData("m0,0 10,10 20,20")]
        [InlineData("m0,0,10,10,20,20")]
        public void Parses_Implicit_Line_Command_After_Relative_Move(string pathData)
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse(pathData);

                var figure = pathGeometry.Figures[0];

                var segment = figure.Segments[0];

                Assert.IsType<LineSegment>(segment);

                var lineSegment = (LineSegment)segment;

                Assert.Equal(new Point(10, 10), lineSegment.Point);

                segment = figure.Segments[1];

                Assert.IsType<LineSegment>(segment);

                lineSegment = (LineSegment)segment;

                Assert.Equal(new Point(30, 30), lineSegment.Point);
            }
        }       

        [Theory]
        [InlineData("F1 M24,14 A2,2,0,1,1,20,14 A2,2,0,1,1,24,14 z")] // issue #1107
        [InlineData("M0 0L10 10z")]
        [InlineData("M50 50 L100 100 L150 50")]
        [InlineData("M50 50L100 100L150 50")]
        [InlineData("M50,50 L100,100 L150,50")]
        [InlineData("M50 50 L-10 -10 L10 50")]
        [InlineData("M50 50L-10-10L10 50")]
        [InlineData("M50 50 L100 100 L150 50zM50 50 L70 70 L120 50z")]
        [InlineData("M 50 50 L 100 100 L 150 50")]
        [InlineData("M50 50 L100 100 L150 50 H200 V100Z")]
        [InlineData("M 80 200 A 100 50 45 1 0 100 50")]
        [InlineData(
            "F1 M 16.6309 18.6563C 17.1309 8.15625 29.8809 14.1563 29.8809 14.1563C 30.8809 11.1563 34.1308 11.4063" +
            " 34.1308 11.4063C 33.5 12 34.6309 13.1563 34.6309 13.1563C 32.1309 13.1562 31.1309 14.9062 31.1309 14.9" +
            "062C 41.1309 23.9062 32.6309 27.9063 32.6309 27.9062C 24.6309 24.9063 21.1309 22.1562 16.6309 18.6563 Z" +
            " M 16.6309 19.9063C 21.6309 24.1563 25.1309 26.1562 31.6309 28.6562C 31.6309 28.6562 26.3809 39.1562 18" +
            ".3809 36.1563C 18.3809 36.1563 18 38 16.3809 36.9063C 15 36 16.3809 34.9063 16.3809 34.9063C 16.3809 34" +
            ".9063 10.1309 30.9062 16.6309 19.9063 Z ")]
        [InlineData(
            "F1M16,12C16,14.209 14.209,16 12,16 9.791,16 8,14.209 8,12 8,11.817 8.03,11.644 8.054,11.467L6.585,10 4,10 " +
            "4,6.414 2.5,7.914 0,5.414 0,3.586 3.586,0 4.414,0 7.414,3 7.586,3 9,1.586 11.914,4.5 10.414,6 " +
            "12.461,8.046C14.45,8.278,16,9.949,16,12")]
        public void Should_Parse(string pathData)
        {
            var pathGeometry = new PathGeometry();
            using (var context = new PathGeometryContext(pathGeometry))
            using (var parser = new PathMarkupParser(context))
            {
                parser.Parse(pathData);

                Assert.True(true);
            }
        }
    }
}