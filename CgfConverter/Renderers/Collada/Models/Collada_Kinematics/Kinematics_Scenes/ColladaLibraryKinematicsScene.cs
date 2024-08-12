using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Scenes
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "library_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaLibraryKinematicsScene
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;


        [XmlElement(ElementName = "kinematics_scene")]
        public ColladaKinematicsScene[] Kinematics_Scene;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

