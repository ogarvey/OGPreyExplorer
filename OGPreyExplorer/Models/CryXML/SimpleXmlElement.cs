using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OGPreyExplorer.Models.CryXML
{
  public class SimpleXmlElement
  {
    public string TagName { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<SimpleXmlElement> Children { get; set; }

    public SimpleXmlElement(string tagName)
    {
      TagName = tagName;
      Attributes = new Dictionary<string, string>();
      Children = new List<SimpleXmlElement>();
    }

    public string ToXmlString()
    {
      var attrs = string.Join(" ", Attributes.Select(kv => $"{kv.Key}=\"{kv.Value}\""));
      var openTag = $"<{TagName}{(attrs.Length > 0 ? " " + attrs : "")}>";
      var closeTag = $"</{TagName}>";
      var childrenString = string.Join("", Children.Select(child => child.ToXmlString()));
      return $"{openTag}{childrenString}{closeTag}";
    }
  }
  public class Element
  {
    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<object> Children { get; set; }

    public override string ToString()
    {
      var element = new XElement(Name, Attributes.Select(attr => new XAttribute(attr.Key, attr.Value)),
          Children.Select(child => child is Element childElement ? childElement.ToString() : child.ToString()));

      return element.ToString();
    }
  }
}
