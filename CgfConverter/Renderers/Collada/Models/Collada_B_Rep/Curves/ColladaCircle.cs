using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_B_Rep.Curves;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaCircle
{
    [XmlElement(ElementName = "radius")]
    public float Radius;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

