// Copyright 2009 Max Toro Q.
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net.net.http {

   sealed class XPathHttpBody {

      static readonly Func<string, byte[]> fromBinHexString;

      Stream contentStream;
      XmlQualifiedName _Method;

      // required
      public string MediaType { get; set; }

      // optional
      public Encoding Encoding { get; set; }
      
      // mutually exclusive
      public XPathItem Content { get; set; }
      public Uri Src { get; set; }

      // serialization options
      public string DocTypePublic { get; set; }
      public string DocTypeSystem { get; set; }
      public bool Indent { get; set; }
      public bool ByteOrderMark { get; set; }
      public bool OmitXmlDeclaration { get; set; }

      public XmlQualifiedName Method {
         get {
            if (_Method != null)
               return _Method;
            return XPathSerializationMethods.Xml;
         }
         set { _Method = value; }
      }

      static XPathHttpBody() {
         fromBinHexString = (Func<string, byte[]>)Delegate.CreateDelegate(typeof(Func<string, byte[]>), typeof(XmlConvert).GetMethod("FromBinHexString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(String) }, null));
      }

      public void ReadXml(XPathNavigator node, XmlResolver resolver) {

         if (node.NodeType == XPathNodeType.Element) {

            XPathNavigator el = node.Clone();

            if (node.MoveToFirstAttribute()) {

               do {
                  switch (node.LocalName) {
                     case "byte-order-mark":
                        this.ByteOrderMark = node.ValueAsBoolean;
                        break;

                     case "doctype-public":
                        this.DocTypePublic = node.Value;
                        break;

                     case "doctype-system":
                        this.DocTypeSystem = node.Value;
                        break;

                     case "encoding":
                        this.Encoding = Encoding.GetEncoding(node.Value);
                        break;

                     case "indent":
                        this.Indent = node.ValueAsBoolean;
                        break;
                     
                     case "media-type":
                        this.MediaType = node.Value;
                        break;

                     case "method":

                        if (node.Value.Contains(":")) {

                           string name = XmlConvert.VerifyName(node.Value);
                           string[] parts = name.Split(':');

                           string local = parts[1];
                           string ns = node.LookupNamespace(parts[0]) ?? "";

                           XmlQualifiedName qname = new XmlQualifiedName(local, ns);

                           if (ns != null
                              && (qname == ExtensionMethods.Base64Binary
                                 || qname == ExtensionMethods.HexBinary)) {
                              this.Method = qname;
                           } else {
                              throw new ArgumentException("The value of the method attribute must be one of: xml, html, xhtml, text, http:base64Binary or http:hexBinary.", "node");                           
                           }

                        } else {

                           switch (node.Value) {
                              case "xml":
                                 this.Method = XPathSerializationMethods.Xml;
                                 break;

                              case "html":
                                 this.Method = XPathSerializationMethods.Html;
                                 break;

                              case "xhtml":
                                 this.Method = XPathSerializationMethods.XHtml;
                                 break;

                              case "text":
                                 this.Method = XPathSerializationMethods.Text;
                                 break;

                              default:
                                 throw new ArgumentException("The value of the method attribute must be one of: xml, html, xhtml, text, http:base64Binary or http:hexBinary.", "node");
                           }
                        }
                        break;

                     case "omit-xml-declaration":
                        this.OmitXmlDeclaration = node.ValueAsBoolean;
                        break;

                     case "src":

                        Uri elBaseUri = !String.IsNullOrEmpty(el.BaseURI) ?
                           new Uri(el.BaseURI) :
                           null;

                        if (resolver != null) {
                           this.Src = resolver.ResolveUri(elBaseUri, node.Value);
                        } else {
                           this.Src = (elBaseUri != null) ?
                              new Uri(elBaseUri, node.Value) :
                              new Uri(node.Value);
                        }

                        break;

                     default:
                        break;
                  }
               
               } while (node.MoveToNextAttribute());

               node.MoveToParent();
            }
            
            if (node.MoveToFirstChild()) {

               do {
                  if (node.NodeType == XPathNodeType.Element || node.NodeType == XPathNodeType.Text) {
                     this.Content = node.Clone();
                     break;
                  }

               } while (node.MoveToNext());

               node.MoveToParent();
            }
         }
      }

      public void WriteXml(XmlWriter writer) {

         writer.WriteStartElement(XPathHttpClient.Prefix, "body", XPathHttpClient.Namespace);
         writer.WriteAttributeString("media-type", this.MediaType);

         if (this.Encoding != null)
            writer.WriteAttributeString("encoding", this.Encoding.WebName);

         writer.WriteEndElement();
      }

      public long PrepareContent(XPathItemFactory itemFactory, XmlResolver resolver) {

         XPathItem item = this.Content;

         if (item != null) {

            XmlQualifiedName method = _Method ?? GetMethodFromMediaType(this.MediaType, this.Method);

            this.contentStream = new MemoryStream();
            Serialize(this.contentStream, itemFactory, method);
            this.contentStream.Position = 0;

         } else if (this.Src != null) {

            if (resolver == null)
               throw new ArgumentNullException("resolver");

            Stream source = resolver.GetEntity(this.Src, null, typeof(Stream)) as Stream;
               
            if (source != null) {

               if (source.CanSeek) {
                  this.contentStream = source;
               } else {
                  this.contentStream = new MemoryStream();

                  source.CopyTo(this.contentStream);
                  this.contentStream.Position = 0;

                  source.Dispose();
               }
               
            } else {
               throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Could not resolve {0}.", this.Src.AbsoluteUri));
            }
         }

         return (this.contentStream != null) ? this.contentStream.Length : 0;
      }

      public Stream GetContentStream() {
         return this.contentStream;
      }

      public void Serialize(Stream output, XPathItemFactory itemFactory, XmlQualifiedName method) {

         if (this.Content == null) throw new InvalidOperationException("Content cannot be null.");

         XPathItem item = this.Content;

         if (method == ExtensionMethods.Base64Binary) {
            
            byte[] buffer = (!item.IsNode && item.XmlType.TypeCode == XmlTypeCode.Base64Binary) ? 
               (byte[])item.TypedValue :
               Convert.FromBase64String(item.Value);
            
            output.Write(buffer, 0, buffer.Length);

         } else if (method == ExtensionMethods.HexBinary) {

            byte[] buffer = (!item.IsNode && item.XmlType.TypeCode == XmlTypeCode.HexBinary) ?
               (byte[])item.TypedValue :
               fromBinHexString(item.Value);

            output.Write(buffer, 0, buffer.Length);

         } else {

            var serialization = new XPathSerializationOptions { 
               Indent = this.Indent,
               OmitXmlDeclaration = this.OmitXmlDeclaration,
               MediaType = this.MediaType,
               Method = method,
               DocTypePublic = this.DocTypePublic,
               DocTypeSystem = this.DocTypeSystem,
               Encoding = this.Encoding,
               ByteOrderMark = this.ByteOrderMark
            };

            itemFactory.Serialize(item, output, serialization);
         }
      }

      public void Deserialize(Stream source, Uri sourceUri, XPathItemFactory itemFactory, string overrideMediaType) {

         string mediaType = !String.IsNullOrEmpty(overrideMediaType) ? overrideMediaType : this.MediaType;
         XmlQualifiedName method = GetMethodFromMediaType(mediaType, ExtensionMethods.Base64Binary);

         Deserialize(source, sourceUri, itemFactory, method);
      }

      public void Deserialize(Stream source, Uri sourceUri, XPathItemFactory itemFactory, XmlQualifiedName method) {

         if (source == null) throw new ArgumentNullException("source");
         if (itemFactory == null) throw new ArgumentNullException("itemFactory");

         XPathItem content;

         TextReader textReader = (this.Encoding != null) ? new StreamReader(source, this.Encoding) : new StreamReader(source);

         if (method == XPathSerializationMethods.Xml || method == XPathSerializationMethods.XHtml) {
            content = itemFactory.CreateNodeReadOnly(textReader, new XmlParsingOptions { BaseUri = sourceUri }).CreateNavigator();

         } else if (method == XPathSerializationMethods.Html) {

            var htmlParser = XPathHttpClient.HtmlParser;

            if (htmlParser != null) 
               content = htmlParser(textReader).CreateNavigator();
            else 
               content = itemFactory.CreateAtomicValue(textReader.ReadToEnd(), XmlTypeCode.String);

         } else if (method == XPathSerializationMethods.Text) {
            content = itemFactory.CreateAtomicValue(textReader.ReadToEnd(), XmlTypeCode.String);

         } else {

            byte[] buffer = StreamUtil.ReadFully(source);
            byte[] base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(buffer));

            content = itemFactory.CreateAtomicValue(base64, XmlTypeCode.Base64Binary);
         }

         this.Content = content;
      }

      static XmlQualifiedName GetMethodFromMediaType(string mediaType, XmlQualifiedName defaultValue) {

         if (MediaTypes.Equals(mediaType, MediaTypes.XHtml))
            return XPathSerializationMethods.XHtml;
         
         if (MediaTypes.IsXml(mediaType))
            return XPathSerializationMethods.Xml;
         
         if (MediaTypes.Equals(mediaType, MediaTypes.Html))
            return XPathSerializationMethods.Html;
         
         if (MediaTypes.IsText(mediaType))
            return XPathSerializationMethods.Text;
         
         return defaultValue;
      }

      #region Nested Types

      static class ExtensionMethods {

         static readonly XmlQualifiedName _Base64Binary = new XmlQualifiedName("base64Binary", XPathHttpClient.Namespace);
         static readonly XmlQualifiedName _HexBinary = new XmlQualifiedName("hexBinary", XPathHttpClient.Namespace);

         public static XmlQualifiedName Base64Binary { get { return _Base64Binary; } }
         public static XmlQualifiedName HexBinary { get { return _HexBinary; } }
      } 

      #endregion
   }
}
