namespace OGPreyExplorer.Models.CryXML
{
  public class CryXmlHeaderInfo
  {
    public int FileLength { get; set; }
    public int NodeTableOffset { get; set; }
    public int NodeTableCount { get; set; }
    public int NodeTableSize { get; set; } = 28;
    public int ReferenceTableOffset { get; set; }
    public int ReferenceTableCount { get; set; }
    public int ReferenceTableSize { get; set; } = 8;
    public int OrderTableOffset { get; set; }
    public int OrderTableCount { get; set; }
    public int OrderTableSize { get; set; } = 4;
    public int ContentOffset { get; set; }
    public int ContentLength { get; set; }
  }

}
