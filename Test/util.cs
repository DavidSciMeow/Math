using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

namespace Test
{

    public class MyXmlSerializable : IXmlSerializable
    {
        public XmlSchema? GetSchema() => null;

        public virtual void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("xml");
            for (int i = 0; i < this.GetType().GetProperties().Length; i++)
            {
                PropertyInfo? property = GetType().GetProperty(reader.Name);
                property?.SetValue(this, reader.ReadElementContentAsString(property.Name, ""));
            }
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                writer.WriteStartElement(property.Name);
                writer.WriteCData((property.GetValue(this, null) ?? "").ToString());
                writer.WriteEndElement();
            }
        }
    }


    [XmlRoot("xml")]
    public class SandboxSignResult : MyXmlSerializable
    {
        public string retmsg { get; set; }
        public string retcode { get; set; }
        public string return_code { get; set; }
        public string return_msg { get; set; }
        public string sandbox_signkey { get; set; }
    }


}
