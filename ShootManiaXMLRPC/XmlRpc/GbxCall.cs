
//---------------------------------------------------//
//
//			Credits : Flo
//
//---------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Web;
using System.Net.Sockets;

namespace ShootManiaXMLRPC.XmlRpc
{
    public enum MessageTypes
    {
        None,
        Response,
        Request,
        Callback
    }

    public class GbxCall
    {
        private int m_handle;
        private string m_xml;
        private ArrayList m_params = new ArrayList();
        private bool m_error = false;
        private string m_error_string;
        private int m_error_code;
        private string m_method_name;
        private MessageTypes m_type;

        /// <summary>
        /// Parses an incoming message.
        /// Xml to object.
        /// </summary>
        /// <param name="in_handle"></param>
        /// <param name="in_data"></param>
        public GbxCall(int in_handle, byte[] in_data)
        {
            this.m_type = MessageTypes.None;
            this.m_handle = in_handle;
            this.m_xml = Encoding.UTF8.GetString(in_data);
            this.m_error_code = 0;
            this.m_error_string = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.m_xml);
            XmlElement methodParams = null;

            // message is of type request ...
            if (xmlDoc["methodCall"] != null)
            {
                // check message type ...
                if (in_handle > 0)
                    this.m_type = MessageTypes.Callback;
                else
                    this.m_type = MessageTypes.Request;

                // try to get the method name ...
                if (xmlDoc["methodCall"]["methodName"] != null)
                {
                    this.m_method_name = xmlDoc["methodCall"]["methodName"].InnerText;
                }
                else
                    this.m_error = true;

                // try to get the mehtod's parameters ...
                if (xmlDoc["methodCall"]["params"] != null)
                {
                    this.m_error = false;
                    methodParams = xmlDoc["methodCall"]["params"];
                }
                else
                    this.m_error = true;
            }
            else if (xmlDoc["methodResponse"] != null) // message is of type response ...
            {
                // check message type ...
                this.m_type = MessageTypes.Response;

                if (xmlDoc["methodResponse"]["fault"] != null)
                {
                    Hashtable err_struct = (Hashtable)ParseXml(xmlDoc["methodResponse"]["fault"]);
                    this.m_error_code = (int)err_struct["faultCode"];
                    this.m_error_string = (string)err_struct["faultString"];
                    this.m_error = true;
                }
                else if (xmlDoc["methodResponse"]["params"] != null)
                {
                    this.m_error = false;
                    methodParams = xmlDoc["methodResponse"]["params"];
                }
                else
                {
                    this.m_error = true;
                }
            }
            else
            {
                this.m_error = true;
            }

            // parse each parameter of the message, if there are any ...
            if (methodParams != null)
            {
                foreach (XmlElement param in methodParams)
                {
                    this.m_params.Add(ParseXml(param));
                }
            }
        }

        /// <summary>
        /// Parses a response message.
        /// Object to xml.
        /// </summary>
        /// <param name="in_params"></param>
        public GbxCall(object[] in_params)
        {
            this.m_xml = "<?xml version=\"1.0\" ?>\n";
            this.m_xml += "<methodResponse>\n";
            this.m_xml += "<params>\n";
            foreach (object param in in_params)
            {
                this.m_xml += "<param>" + ParseObject(param) + "</param>\n";
            }
            this.m_xml += "</params>";
            this.m_xml += "</methodResponse>";
        }

        /// <summary>
        /// Parses a request message.
        /// Object to xml.
        /// </summary>
        /// <param name="in_method_name"></param>
        /// <param name="in_params"></param>
        public GbxCall(string in_method_name, object[] in_params)
        {
            this.m_xml = "<?xml version=\"1.0\" ?>\n";
            this.m_xml += "<methodCall>\n";
            this.m_xml += "<methodName>" + in_method_name + "</methodName>\n";
            this.m_xml += "<params>\n";
            foreach (object param in in_params)
            {
                this.m_xml += "<param>" + ParseObject(param) + "</param>\n";
            }
            this.m_xml += "</params>";
            this.m_xml += "</methodCall>";
        }

        private string ParseObject(object inParam)
        {
            // open parameter ...
            string xml = "<value>";

            if (inParam.GetType() == typeof(string)) // parse type string ...
            {
                xml += "<string>" + HttpUtility.HtmlEncode((string)inParam) + "</string>";
            }
            else if (inParam.GetType() == typeof(int)) // parse type int32 ...
            {
                xml += "<int>" + (int)inParam + "</int>";
            }
            else if (inParam.GetType() == typeof(double)) // parse type double ...
            {
                xml += "<double>" + (double)inParam + "</double>";
            }
            else if (inParam.GetType() == typeof(bool))  // parse type bool ...
            {
                if ((bool)inParam)
                    xml += "<boolean>1</boolean>";
                else
                    xml += "<boolean>0</boolean>";
            }
            else if (inParam.GetType() == typeof(ArrayList)) // parse type array ...
            {
                xml += "<array><data>";
                foreach (object element in ((ArrayList)inParam))
                {
                    xml += ParseObject(element);
                }
                xml += "</data></array>";
            }
            else if (inParam.GetType() == typeof(Hashtable)) // parse type struct ...
            {
                xml += "<struct>";
                foreach (object key in ((Hashtable)inParam).Keys)
                {
                    xml += "<member>";
                    xml += "<name>" + key.ToString() + "</name>";
                    xml += ParseObject(((Hashtable)inParam)[key]);
                    xml += "</member>";
                }
                xml += "</struct>";
            }
            else if (inParam.GetType() == typeof(byte[])) // parse type of byte[] into base64
            {
                xml += "<base64>";
                xml += Convert.ToBase64String((byte[])inParam);
                xml += "</base64>";
            }

            // close parameter ...
            return xml + "</value>\n";
        }

        private object ParseXml(XmlElement inParam)
        {
            XmlElement val;
            if (inParam["value"] == null)
            {
                val = inParam;
            }
            else
            {
                val = inParam["value"];
            }

            if (val["string"] != null) // param of type string ...
            {
                return val["string"].InnerText;
            }
            else if (val["int"] != null) // param of type int32 ...
            {
                return Int32.Parse(val["int"].InnerText);
            }
            else if (val["i4"] != null) // param of type int32 (alternative) ...
            {
                return Int32.Parse(val["i4"].InnerText);
            }
            else if (val["double"] != null) // param of type double ...
            {
                return double.Parse(val["double"].InnerText);
            }
            else if (val["boolean"] != null) // param of type boolean ...
            {
                if (val["boolean"].InnerText == "1")
                    return true;
                else
                    return false;
            }
            else if (val["struct"] != null) // param of type struct ...
            {
                Hashtable structure = new Hashtable();
                foreach (XmlElement member in val["struct"])
                {
                    // parse each member ...
                    structure.Add(member["name"].InnerText, ParseXml(member));
                }
                return structure;
            }
            else if (val["array"] != null) // param of type array ...
            {
                ArrayList array = new ArrayList();
                foreach (XmlElement data in val["array"]["data"])
                {
                    // parse each data field ...
                    array.Add(ParseXml(data));
                }
                return array;
            }
            else if (val["base64"] != null) // param of type base64 ...
            {
                byte[] data = Convert.FromBase64String(val["base64"].InnerText);
                return data;
            }

            return null;
        }

        public string MethodName
        {
            get
            {
                return this.m_method_name;
            }
        }

        public MessageTypes Type
        {
            get
            {
                return this.m_type;
            }
        }

        public string Xml
        {
            get
            {
                return this.m_xml;
            }
        }

        public ArrayList Params
        {
            get
            {
                return this.m_params;
            }
        }

        public int Size
        {
            get
            {
                return this.m_xml.Length;
            }
        }

        public int Handle
        {
            get
            {
                return this.m_handle;
            }
            set
            {
                this.m_handle = value;
            }
        }

        public bool Error
        {
            get
            {
                return this.m_error;
            }
        }

        public string ErrorString
        {
            get
            {
                return m_error_string;
            }
        }

        public int ErrorCode
        {
            get
            {
                return m_error_code;
            }
        }
    }
}
