// Copyright 2010 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace myxsl.net.common {
   
   public class XPathSerializationOptions : IXmlSerializable {

      const string W3CSerializationNamespace = "http://www.w3.org/2010/xslt-xquery-serialization";
      const string W3CSerializationPrefix = "output";

      static readonly Action<XmlWriterSettings, XmlOutputMethod> setOutputMethod;
      static readonly Action<XmlWriterSettings, string> setDocTypePublic;
      static readonly Action<XmlWriterSettings, string> setDocTypeSystem;
      static readonly Action<XmlWriterSettings, string> setMediaType;
      static readonly Action<XmlWriterSettings, bool> setReadOnly;

      public bool? ByteOrderMark { get; set; }
      public string DocTypePublic { get; set; }
      public string DocTypeSystem { get; set; }
      public Encoding Encoding { get; set; }
      public bool? Indent { get; set; }
      public string MediaType { get; set; }
      public XmlQualifiedName Method { get; set; }
      public bool? OmitXmlDeclaration { get; set; }
      
      public ConformanceLevel ConformanceLevel { get; set; }

      static XPathSerializationOptions() {

         Type settingsType = typeof(XmlWriterSettings);

         setOutputMethod = (Action<XmlWriterSettings, XmlOutputMethod>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, XmlOutputMethod>), settingsType.GetProperty("OutputMethod", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true));
         setDocTypePublic = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("DocTypePublic", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setDocTypeSystem = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("DocTypeSystem", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setMediaType = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("MediaType", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setReadOnly = (Action<XmlWriterSettings, bool>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, bool>), settingsType.GetProperty("ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
      }

      public static explicit operator XmlWriterSettings(XPathSerializationOptions options) {

         var settings = new XmlWriterSettings();

         CopyTo(options, settings);

         return settings;
      }

      static void CopyTo(XPathSerializationOptions options, XmlWriterSettings settings) {

         if (options == null) throw new ArgumentNullException("options");
         if (settings == null) throw new ArgumentNullException("settings");

         if (options.Method != null 
            && !options.Method.IsEmpty
            && options.Method != XPathSerializationMethods.Xml) {

            if (options.Method == XPathSerializationMethods.Html) {
               setOutputMethod(settings, XmlOutputMethod.Html);

            } else if (options.Method == XPathSerializationMethods.Text) {
               setOutputMethod(settings, XmlOutputMethod.Text);
            }
         }

         if (options.DocTypePublic != null)
            setDocTypePublic(settings, options.DocTypePublic);

         if (options.DocTypeSystem != null)
            setDocTypeSystem(settings, options.DocTypeSystem);

         if (options.Encoding != null)
            settings.Encoding = options.Encoding;

         if (options.Indent.HasValue)
            settings.Indent = options.Indent.Value;

         if (options.MediaType != null)
            setMediaType(settings, options.MediaType);

         if (options.OmitXmlDeclaration.HasValue)
            settings.OmitXmlDeclaration = options.OmitXmlDeclaration.Value;

         Encoding enc = settings.Encoding;

         if (options.ByteOrderMark.HasValue 
            && !options.ByteOrderMark.Value) {
            
            if (enc is UTF8Encoding)
               settings.Encoding = new UTF8Encoding(false);
         }

         settings.ConformanceLevel = options.ConformanceLevel;
      }

      public XPathSerializationOptions() {
         this.ConformanceLevel = ConformanceLevel.Auto;
      }

      public void CopyTo(XmlWriterSettings settings) {

         if (settings == null) throw new ArgumentNullException("settings");

         setReadOnly(settings, false);
         
         CopyTo(this, settings); 
      }

      #region IXmlSerializable Members

      System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() {
         throw new NotImplementedException();
      }

      void IXmlSerializable.ReadXml(XmlReader reader) {

         int initialDepth = reader.Depth;

         while (reader.Read()) {

            if (reader.NodeType != XmlNodeType.Element
               || reader.Depth == initialDepth)
               continue;

            if (reader.Depth > initialDepth + 1) {
               reader.Skip();
               continue;
            }

            ReadOption(reader);
         }
      }

      void ReadOption(XmlReader reader) {

         switch (reader.NamespaceURI) {
            case W3CSerializationNamespace:
               
               switch (reader.LocalName) {
                  case "byte-order-mark":
                     this.ByteOrderMark = ParseYesOrNo(reader.GetAttribute("value"), reader.LocalName);
                     break;

                  case "doctype-public":
                     this.DocTypePublic = reader.GetAttribute("value");
                     break;

                  case "doctype-system":
                     this.DocTypeSystem = reader.GetAttribute("value");
                     break;
                  
                  case "encoding":
                     this.Encoding = Encoding.GetEncoding(reader.GetAttribute("value"));
                     break;

                  case "indent":
                     this.Indent = ParseYesOrNo(reader.GetAttribute("value"), reader.LocalName);
                     break;

                  case "media-type":
                     this.MediaType = reader.GetAttribute("value");
                     break;

                  case "method":
                     this.Method = ParseQName(reader.GetAttribute("value"), reader);
                     break;

                  case "omit-xml-declaration":
                     this.OmitXmlDeclaration = ParseYesOrNo(reader.GetAttribute("value"), reader.LocalName);
                     break;

                  default:
                     break;
               }

               break;

            default:
               break;
         }
      }

      static XmlQualifiedName ParseQName(string lexicalQName, XmlReader reader) {

         string[] parts = lexicalQName.Split(':');

         if (parts.Length == 1)
            return new XmlQualifiedName(parts[0]);

         string prefix = parts[0];
         string ns = reader.LookupNamespace(prefix);

         return new XmlQualifiedName(parts[1], ns);
      }

      static bool ParseYesOrNo(string value, string name) {

         switch ((value ?? "").Trim()) {
            case "yes":
               return true;
            case "no":
               return false;
            default:
               throw new ArgumentException("{0} must be one of the following values: 'yes' or 'no'.".FormatInvariant(name));
         }
      }

      void IXmlSerializable.WriteXml(XmlWriter writer) {
         
         writer.WriteStartElement(W3CSerializationPrefix, "serialization-parameters", W3CSerializationNamespace);

         if (this.ByteOrderMark.HasValue) 
            WriteOption("byte-order-mark", SerializeYesOrNo(this.ByteOrderMark.Value), writer);

         if (this.DocTypePublic != null)
            WriteOption("doctype-public", this.DocTypePublic, writer);

         if (this.DocTypeSystem != null)
            WriteOption("doctype-system", this.DocTypeSystem, writer);

         if (this.Encoding != null)
            WriteOption("encoding", this.Encoding.WebName, writer);

         if (this.Indent.HasValue) 
            WriteOption("indent", SerializeYesOrNo(this.Indent.Value), writer);

         if (this.MediaType != null)
            WriteOption("media-type", this.MediaType, writer);

         if (this.Method != null) {
            
            writer.WriteStartElement(W3CSerializationPrefix, "method", W3CSerializationNamespace);

            string value;

            if (this.Method.Namespace.HasValue()) {

               string prefix = "method";

               writer.WriteAttributeString("xmlns", prefix, null, this.Method.Namespace);

               value = String.Concat(prefix, ":", this.Method.Name);
            
            } else {

               value = this.Method.Name;
            }

            writer.WriteElementString("value", value);
            
            writer.WriteEndElement();
         }

         if (this.OmitXmlDeclaration.HasValue) 
            WriteOption("omit-xml-declaration", SerializeYesOrNo(this.OmitXmlDeclaration.Value), writer);

         writer.WriteEndElement();
      }

      static void WriteOption(string name, string value, XmlWriter writer) {

         writer.WriteStartElement(W3CSerializationPrefix, name, W3CSerializationNamespace);
         writer.WriteElementString("value", value);
         writer.WriteEndElement();
      }

      static string SerializeYesOrNo(bool value) {

         if (value)
            return "yes";

         return "no";
      }

      #endregion
   }
}
