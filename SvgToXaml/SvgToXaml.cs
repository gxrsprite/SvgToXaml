using SvgToXaml.Properties;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SvgToXaml
{
    class SvgToXaml
    {
        public static string Convert(string svg, string name)
        {
            Regex regex = new Regex("p-id=\".*?\"");
            var svgstr = regex.Replace(svg, "");
            regex = new Regex("<\\?xml.+?dtd\">", RegexOptions.Singleline);
            svgstr = regex.Replace(svgstr, "");
            regex = new Regex("<defs.*?>.*</defs>", RegexOptions.Singleline);
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

            var xaml = $"{string.Format(Resources.Start, name)}{svgxml.ToString()}{Resources.End}";
            return xaml.ToString();
        }

        static void RemoveButKeepChildren(XElement path)
        {
            var parent = path.Parent;
            path.Remove();
            parent.Add(path.Elements());
        }

        private static void HandleStyle(XElement path)
        {
            var styleAttr = path.Attribute("style");
            if (styleAttr == null)
            {
                return;
            }
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
                    path.SetAttributeValue("Fill", value);
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
            }
        }
    }
}
