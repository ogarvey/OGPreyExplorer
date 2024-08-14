using OGPreyExplorer.Models.CryXML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xml2Js = System.Xml.Serialization; // Alias to avoid conflicts

// Assuming you have corresponding C# classes for CryXmlNode, CryXmlReference, CryXmlValue, CryXmlHeaderInfo, and SimpleXmlElement

public class CryXmlSerializer
{
  private static readonly byte[] PbxmlMagic = Encoding.UTF8.GetBytes("pbxml\0");
  private static readonly byte[] CryXmlMagic = Encoding.UTF8.GetBytes("CryXmlB\0");

  public static async Task<string> ReadFileAsync(string filePath)
  {
    try
    {
      byte[] fileBuffer = await File.ReadAllBytesAsync(filePath);
      string xmlContent = await ProcessDataAsync(fileBuffer);

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(xmlContent);

      using (StringWriter sw = new StringWriter())
      {
        using (XmlTextWriter xw = new XmlTextWriter(sw))
        {
          xw.Formatting = Formatting.Indented;
          doc.WriteContentTo(xw);
        }
        return sw.ToString();
      }
    }
    catch (Exception e)
    {
      throw new Exception($"Error reading file: {e.Message}");
    }
  }

  public static bool IsBinaryXml(byte[] buffer)
  {
    return buffer.Take(CryXmlMagic.Length).SequenceEqual(CryXmlMagic) ||
           buffer.Take(PbxmlMagic.Length).SequenceEqual(PbxmlMagic);
  }

  private static async Task<string> ProcessDataAsync(byte[] data)
  {
    int maxMagicLength = Math.Max(PbxmlMagic.Length, CryXmlMagic.Length);
    byte[] peek = data.Take(maxMagicLength).ToArray();

    if (peek.SequenceEqual(PbxmlMagic))
    {
      return await LoadPbxmlFileAsync(data);
    }
    else if (peek.SequenceEqual(CryXmlMagic))
    {
      return await LoadCryXmlBFileAsync(data);
    }
    else
    {
      return Encoding.UTF8.GetString(data); // Assuming the file is UTF-8 encoded
    }
  }

  private static async Task<string> LoadPbxmlFileAsync(byte[] data)
  {
    throw new NotImplementedException();
  }

  public static async Task<string?> LoadCryXmlBFileAsync(byte[] buffer) {
    var headerInfo = await ReadCryXmlHeaderInfoAsync(buffer);
    var nodeTable = await BuildCryXmlNodeTableAsync(buffer, headerInfo);
    var attributeTable = await BuildCryXmlAttributeTableAsync(buffer, headerInfo);
    var orderTable = await BuildCryXmlOrderTableAsync(buffer, headerInfo);
    var dataTable = await BuildCryXmlDataTableAsync(buffer, headerInfo);

    var xmlMap = new Dictionary<int, SimpleXmlElement>();
    int attributeIndex = 0;
    bool bugged = false;

    foreach (var node in nodeTable)
    {
      var nodeName = dataTable.TryGetValue(node.NodeNameOffset, out var name) ? name : "unknown";
      var element = new SimpleXmlElement(nodeName);

      for (int i = 0; i < node.AttributeCount; i++)
      {
        var attribute = attributeTable[attributeIndex++];
        var attrName = dataTable.TryGetValue(attribute.NameOffset, out var aname) ? aname : "";
        var attrValue = dataTable.TryGetValue(attribute.ValueOffset, out var avalue) ? avalue : "BUGGED";
        bugged = bugged || attrValue == "BUGGED";
        element.Attributes[attrName] = attrValue;
      }

      xmlMap[node.NodeID] = element;
      if (node.ParentNodeID >= 0 && xmlMap.TryGetValue(node.ParentNodeID, out var parent))
      {
        parent.Children.Add(element);
      }
    }

    var xmlRoot = xmlMap[0];
    Console.WriteLine(xmlRoot?.ToXmlString());
    return xmlRoot?.ToXmlString() ?? string.Empty;
  }

  private static async Task<CryXmlHeaderInfo> ReadCryXmlHeaderInfoAsync(byte[] buffer)
  {
    int offset = CryXmlMagic.Length;
    var headerInfo = new CryXmlHeaderInfo
    {
      FileLength = BitConverter.ToInt32(buffer, offset),
      NodeTableOffset = BitConverter.ToInt32(buffer, offset += 4),
      NodeTableCount = BitConverter.ToInt32(buffer, offset += 4),
      ReferenceTableOffset = BitConverter.ToInt32(buffer, offset += 4),
      ReferenceTableCount = BitConverter.ToInt32(buffer, offset += 4),
      OrderTableOffset = BitConverter.ToInt32(buffer, offset += 4),
      OrderTableCount = BitConverter.ToInt32(buffer, offset += 4),
      ContentOffset = BitConverter.ToInt32(buffer, offset += 4),
      ContentLength = BitConverter.ToInt32(buffer, offset += 4)
    };

    return await Task.FromResult(headerInfo);
  }

  private static async Task<List<CryXmlNode>> BuildCryXmlNodeTableAsync(byte[] buffer, CryXmlHeaderInfo headerInfo)
  {
    int offset = headerInfo.NodeTableOffset;
    var nodeTable = new List<CryXmlNode>();
    int nodeID = 0;

    while (offset < headerInfo.NodeTableOffset + headerInfo.NodeTableCount * headerInfo.NodeTableSize)
    {
      var node = new CryXmlNode
      {
        NodeID = nodeID++,
        NodeNameOffset = BitConverter.ToInt32(buffer, offset),
        ItemType = BitConverter.ToInt32(buffer, offset += 4),
        AttributeCount = BitConverter.ToInt16(buffer, offset += 4),
        ChildCount = BitConverter.ToInt16(buffer, offset += 2),
        ParentNodeID = BitConverter.ToInt32(buffer, offset += 2),
        FirstAttributeIndex = BitConverter.ToInt32(buffer, offset += 4),
        FirstChildIndex = BitConverter.ToInt32(buffer, offset += 4),
        Reserved = BitConverter.ToInt32(buffer, offset += 4)
      };

      nodeTable.Add(node);
      offset += 4;
    }

    return await Task.FromResult(nodeTable);
  }

  private static async Task<List<CryXmlReference>> BuildCryXmlAttributeTableAsync(byte[] buffer, CryXmlHeaderInfo headerInfo)
  {
    int offset = headerInfo.ReferenceTableOffset;
    var attributeTable = new List<CryXmlReference>();

    while (offset < headerInfo.ReferenceTableOffset + headerInfo.ReferenceTableCount * headerInfo.ReferenceTableSize)
    {
      var attribute = new CryXmlReference
      {
        NameOffset = BitConverter.ToInt32(buffer, offset),
        ValueOffset = BitConverter.ToInt32(buffer, offset += 4)
      };

      attributeTable.Add(attribute);
      offset += 4;
    }

    return await Task.FromResult(attributeTable);
  }

  private static async Task<List<int>> BuildCryXmlOrderTableAsync(byte[] buffer, CryXmlHeaderInfo headerInfo)
  {
    int offset = headerInfo.OrderTableOffset;
    var orderTable = new List<int>();

    while (offset < headerInfo.OrderTableOffset + headerInfo.OrderTableCount * headerInfo.OrderTableSize)
    {
      int value = BitConverter.ToInt32(buffer, offset);
      offset += 4;
      orderTable.Add(value);
    }

    return await Task.FromResult(orderTable);
  }

  private static async Task<Dictionary<long, string>> BuildCryXmlDataTableAsync(byte[] buffer, CryXmlHeaderInfo headerInfo)
  {
    int offset = headerInfo.ContentOffset;
    var dataTable = new List<CryXmlValue>();

    while (offset < buffer.Length)
    {
      int position = offset;
      var cryXmlValue = new CryXmlValue
      {
        Offset = position - headerInfo.ContentOffset
      };

      var (value, valueLength) = ReadCString(buffer, offset);
      offset += valueLength;

      cryXmlValue.Value = value;
      dataTable.Add(cryXmlValue);
    }

    var dataMap = new Dictionary<long, string>();
    foreach (var item in dataTable)
    {
      dataMap[item.Offset] = item.Value;
    }

    return await Task.FromResult(dataMap);
  }

  private static async Task<(Element, int)> CreateNewElementAsync(byte[] buffer, int offset)
  {
    int numberOfChildren = ReadCryInt(buffer, offset);
    offset += 4;

    int numberOfAttributes = ReadCryInt(buffer, offset);
    offset += 4;

    (string nodeName, int nodeNameLength) = ReadCString(buffer, offset);
    offset += nodeNameLength;

    var element = new Element { Name = nodeName, Attributes = new Dictionary<string, string>(), Children = new List<object>() };

    for (int i = 0; i < numberOfAttributes; i++)
    {
      (string key, int keyLength) = ReadCString(buffer, offset);
      offset += keyLength;
      (string value, int valueLength) = ReadCString(buffer, offset);
      offset += valueLength;

      element.Attributes[key] = value;
    }

    (string nodeText, int nodeTextLength) = ReadCString(buffer, offset);
    offset += nodeTextLength;
    if (!string.IsNullOrEmpty(nodeText))
    {
      element.Children.Add(nodeText);
    }

    for (int i = 0; i < numberOfChildren; i++)
    {
      int expectedLength = ReadCryInt(buffer, offset);
      offset += 4;
      int expectedPosition = offset + expectedLength;

      (Element childElement, int newOffset) = await CreateNewElementAsync(buffer, offset);
      element.Children.Add(childElement);
      offset = newOffset;

      if (offset != expectedPosition)
      {
        throw new Exception("Expected length does not match.");
      }
    }

    return (element, offset);
  }

  static int ReadCryInt(byte[] buffer, int offset)
  {
    // Check if there's enough space in the buffer to read an integer
    if (offset + 4 > buffer.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(offset), "Not enough bytes in buffer to read an integer.");
    }

    // Use BitConverter to convert 4 bytes from the buffer into an integer
    // Little-endian is assumed based on the original TypeScript comment
    return BitConverter.ToInt32(buffer, offset);
  }

  static (string, int) ReadCString(byte[] buffer, int offset)
  {
    // Check if there's enough space in the buffer to read the string length
    if (offset + 4 > buffer.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(offset), "Not enough bytes in buffer to read the string length.");
    }

    // Read the length of the string
    int length = BitConverter.ToInt32(buffer, offset);

    // Check if there's enough space in the buffer to read the string data
    if (offset + 4 + length > buffer.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(offset), "Not enough bytes in buffer to read the string data.");
    }

    // Decode the string data using UTF-8 encoding
    string data = Encoding.UTF8.GetString(buffer, offset + 4, length);

    // Return the decoded string and the new offset
    return (data, 4 + length);
  }
  // ... (Rest of the methods: loadCryXmlBFileAsync, loadPbxmlFileAsync, 
  //      readCryXmlHeaderInfo, buildCryXmlNodeTable, buildCryXmlAttributeTable, 
  //      buildCryXmlOrderTable, buildCryXmlDataTable, readCryInt, readCString)
}
