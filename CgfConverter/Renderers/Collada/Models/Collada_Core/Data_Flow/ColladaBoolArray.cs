using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaBoolArray : ColladaBoolArrayString
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("count")]
    public int Count;

}

