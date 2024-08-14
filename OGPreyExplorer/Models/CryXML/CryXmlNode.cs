using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPreyExplorer.Models.CryXML
{
  public class CryXmlNode
  {
    public int NodeID { get; set; }
    public int NodeNameOffset { get; set; }
    public int ItemType { get; set; }
    public int AttributeCount { get; set; }
    public int ChildCount { get; set; }
    public int ParentNodeID { get; set; }
    public int FirstAttributeIndex { get; set; }
    public int FirstChildIndex { get; set; }
    public int Reserved { get; set; }
  }

}
