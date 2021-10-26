using SvgToXaml.Properties;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SvgToXaml
{
    class SvgToXaml
    {
        public static XElement defs;
        public static string Convert(string svg, string name)
        {



            Regex regex = new Regex("p-id=\".*?\"");
            var svgstr = regex.Replace(svg, "");
            regex = new Regex("<\\?xml.+?dtd\">", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "");
            regex = new Regex("<defs.*?>.*</defs>", RegexOptions.Singleline);
            var defsxml1 = regex.Match(svg);
            if (defsxml1.Success)
            {
                defs = XElement.Parse(defsxml1.Value);
            }
            else
            {
                defs = null;
            }
            svgstr = regex.Replace(svgstr, "");

            regex = new Regex("<title>.*</title>", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "");
            regex = new Regex("<clipPath.*?>", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "");
            svgstr = svgstr.Replace("</clipPath>", "");
            regex = new Regex("<metadata.*?>.*</metadata>", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "");
            regex = new Regex("<svg.*?>", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "<Canvas>");
            svgstr = svgstr.Replace("</svg>", "</Canvas>");
            svgstr = svgstr.Replace("path", "Path");
            svgstr = svgstr.Replace("rect", "Rectangle");
            svgstr = svgstr.Replace("polygon", "Polygon");
            svgstr = svgstr.Replace("points", "Points");
            svgstr = svgstr.Replace("width", "Width");
            svgstr = svgstr.Replace("height", "Height");
            svgstr = svgstr.Replace("text", "Label");
            svgstr = svgstr.Replace("circle", "Ellipse");
            svgstr = svgstr.Replace("ellipse", "Ellipse");
            XDocument svgxml = XDocument.Parse(svgstr);


            var gs = svgxml.Root.Elements("g").ToList();
            while (true)
            {
                foreach (var g in gs)
                {
                    RemoveButKeepChildren(g);
                }
                gs = svgxml.Root.Elements("g").ToList();
                if (gs.Count == 0)
                {
                    break;
                }
            }

            var polygons = svgxml.Root.Elements("Polygon").ToList();
            foreach (var polygon in polygons)
            {
                HandleStyle(polygon);
            }


            var paths = svgxml.Root.Elements("Path").ToList();
            foreach (var path in paths)
            {
                var dAttr = path.Attribute("d");
                path.SetAttributeValue("Data", dAttr.Value);
                dAttr.Remove();

                dAttr = path.Attribute("fill");
                if (dAttr != null)
                {
                    path.SetAttributeValue("Fill", dAttr.Value);
                    dAttr.Remove();
                }

                dAttr = path.Attribute("id");
                dAttr?.Remove();
                //style
                HandleStyle(path);

            }

            var rects = svgxml.Root.Elements("Rectangle").ToList();
            foreach (var rect in rects)
            {
                var dAttr = rect.Attribute("fill");
                if (dAttr != null)
                {
                    rect.SetAttributeValue("Fill", dAttr.Value);
                    dAttr.Remove();
                }
                var xAttr = rect.Attribute("x");
                if (xAttr != null)
                {
                    rect.SetAttributeValue("Canvas.Left", xAttr.Value);
                    xAttr.Remove();
                }
                var yAttr = rect.Attribute("y");
                if (yAttr != null)
                {
                    rect.SetAttributeValue("Canvas.Top", yAttr.Value);
                    yAttr.Remove();
                }
                yAttr = rect.Attribute("rx");
                if (yAttr != null)
                {
                    rect.SetAttributeValue("RadiusX", yAttr.Value);
                    rect.SetAttributeValue("RadiusY", yAttr.Value);
                    yAttr.Remove();
                }
                dAttr = rect.Attribute("id");
                dAttr?.Remove();
                dAttr = rect.Attribute("data-name");
                dAttr?.Remove();
                //style
                HandleStyle(rect);

            }

            var circles = svgxml.Root.Elements("Ellipse").ToList();
            foreach (var circle in circles)
            {

                var xAttr = circle.Attribute("cx");
                if (xAttr != null)
                {
                    circle.SetAttributeValue("Canvas.Left", xAttr.Value);
                    xAttr.Remove();
                }
                var yAttr = circle.Attribute("cy");
                if (yAttr != null)
                {
                    circle.SetAttributeValue("Canvas.Top", yAttr.Value);
                    yAttr.Remove();
                }
                yAttr = circle.Attribute("r");
                if (yAttr != null)
                {
                    var r = float.Parse(yAttr.Value) * 2;
                    circle.SetAttributeValue("Width", r);
                    circle.SetAttributeValue("Height", r);
                    yAttr.Remove();
                }

                var aAttr = circle.Attribute("rx");
                if (aAttr != null)
                {
                    var r = float.Parse(aAttr.Value) * 2;
                    circle.SetAttributeValue("Width", r);
                    aAttr.Remove();
                }

                aAttr = circle.Attribute("ry");
                if (aAttr != null)
                {
                    var r = float.Parse(aAttr.Value) * 2;
                    circle.SetAttributeValue("Height", r);
                    aAttr.Remove();
                }

                var dAttr = circle.Attribute("id");
                dAttr?.Remove();
                dAttr = circle.Attribute("data-name");
                dAttr?.Remove();
                //style
                HandleStyle(circle);

            }

            var texts = svgxml.Root.Elements("Label").ToList();
            foreach (var text in texts)
            {
                var xAttr = text.Attribute("x");
                if (xAttr != null)
                {
                    text.SetAttributeValue("Canvas.Left", xAttr.Value);
                    xAttr.Remove();
                }
                var yAttr = text.Attribute("y");
                if (yAttr != null)
                {
                    text.SetAttributeValue("Canvas.Top", yAttr.Value);
                    yAttr.Remove();
                }
                HandleStyle(text);
                var dAttr = text.Attribute("Fill");
                if (dAttr != null)
                {
                    text.SetAttributeValue("Foreground", dAttr.Value);
                    dAttr.Remove();
                }
            }

            var xaml = $"{string.Format(Resources.Start, name)}{svgxml.ToString()}{Resources.End}";
            return xaml.ToString();
        }

        private static XElement GetFillDefs(string name, string value)
        {
            foreach (var item in defs.Elements())
            {
                if (item.Attribute("id")?.Value == value)
                {
                    if (item.Name == "linearGradient")
                    {
                        var startPoint = $"{item.Attribute("x1").Value},{item.Attribute("y1").Value}";
                        var endPoint = $"{item.Attribute("x2").Value},{item.Attribute("y2").Value}";

                        string rtnxml = $@"<{name}.Fill>
                        <LinearGradientBrush StartPoint={startPoint.Maohao()} EndPoint={endPoint.Maohao() } MappingMode={"Absolute".Maohao()} SpreadMethod={"Pad".Maohao()}>
                         
                        </LinearGradientBrush> </{name}.Fill>";
                        XElement rtnxmle = XElement.Parse(rtnxml);
                        var brush = rtnxmle.Element("LinearGradientBrush");
                        foreach (var stop in item.Elements("stop"))
                        {
                            var str = stop.ToString().Replace("stop-color", "Color").Replace("stop", "GradientStop");
                            var stopx = XElement.Parse(str);
                            if (stopx.Attribute("Color") == null)
                            {
                                stopx.SetAttributeValue("Color", "Black");
                            }
                            brush.Add(stopx);
                        }
                        return rtnxmle;
                    }
                    else
                    {
                        if (item.Name == "radialGradient")
                        {
                            var center = $"{item.Attribute("cx").Value},{item.Attribute("cy").Value}";
                            var gradientOrigin = $"{item.Attribute("fx").Value},{item.Attribute("fy").Value}";
                            var radiusX = $"{item.Attribute("r").Value}";

                            string rtnxml = $@"<{name}.Fill>
                        <RadialGradientBrush  Center={center.Maohao()} GradientOrigin={gradientOrigin.Maohao()} RadiusX={radiusX.Maohao() } RadiusY={radiusX.Maohao() } MappingMode={"Absolute".Maohao()}>
                         
                        </RadialGradientBrush > </{name}.Fill>";
                            XElement rtnxmle = XElement.Parse(rtnxml);
                            var brush = rtnxmle.Element("RadialGradientBrush");
                            foreach (var stop in item.Elements("stop"))
                            {
                                var str = stop.ToString().Replace("stop-color", "Color").Replace("stop", "GradientStop");
                                var stopx = XElement.Parse(str);
                                if (stopx.Attribute("Color") == null)
                                {
                                    stopx.SetAttributeValue("Color", "Black");
                                }
                                brush.Add(stopx);
                            }
                            return rtnxmle;
                        }
                    }
                }
            }
            return null;
        }

        static void RemoveButKeepChildren(XElement path)
        {
            var parent = path.Parent;
            path.AddAfterSelf(path.Elements());
            path.Remove();
        }

        private static void HandleStyle(XElement path)
        {
            var styleAttr = path.Attribute("style");
            if (styleAttr != null)
            {
                var style = styleAttr.Value;
                styleAttr.Remove();
                var styles = style.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in styles)
                {
                    var temp = s.Split(':');
                    if (temp.Length != 2)
                    {
                        return;
                    }
                    var name = temp[0];
                    var value = temp[1];

                    if (name == "fill" && value != "none")
                    {
                        if (styleAttr.Value.Contains("url"))
                        {
                            var regex = new Regex(@"url\(#(.+)\)", RegexOptions.Singleline);
                            var match = regex.Match(styleAttr.Value);
                            var group = match.Groups[1];

                            XElement fillxml = GetFillDefs(path.Name.ToString(), group.Value);
                            path.Add(fillxml);
                        }
                        else
                        {
                            path.SetAttributeValue("Fill", value);
                        }
                    }
                    else if (name == "stroke" && value != "none")
                    {
                        path.SetAttributeValue("Stroke", value);
                    }
                    else if (name == "stroke-width")
                    {
                        path.SetAttributeValue("StrokeThickness", value.Replace("px", ""));
                    }
                    else if (name == "opacity")
                    {
                        path.SetAttributeValue("Opacity", value);
                    }
                    else if (name == "font-size")
                    {
                        path.SetAttributeValue("FontSize", value.Replace("px", ""));
                    }
                }
            }


            var transformAttr = path.Attribute("transform");
            if (transformAttr != null)
            {
                var style = transformAttr.Value;
                transformAttr.Remove();
                style = style.Replace("translate(", "");
                style = style.Replace(")", "");
                if (style != "0")
                {
                    var translates = style.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var x = translates[0];
                    var y = translates[1];

                    path.SetAttributeValue("Canvas.Left", x);
                    path.SetAttributeValue("Canvas.Top", y);
                }
            }
        }
    }
}
