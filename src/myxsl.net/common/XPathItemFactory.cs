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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace myxsl.net.common {
   
   public abstract class XPathItemFactory {

      static readonly ConcurrentDictionary<Type, XmlSerializer> serializerCache = new ConcurrentDictionary<Type, XmlSerializer>();
      static readonly ConcurrentDictionary<Type, XmlRootAttribute> xmlRootCache = new ConcurrentDictionary<Type, XmlRootAttribute>();

      // Conversion: CLR type to XPathItem

      public XPathItem CreateAtomicValue(object value, XmlTypeCode typeCode) {
         return CreateAtomicValue(value, XmlSchemaType.GetBuiltInSimpleType(typeCode).QualifiedName);
      }

      public abstract XPathItem CreateAtomicValue(object value, XmlQualifiedName qualifiedName);

      public IEnumerable<XPathItem> CreateAtomicValueSequence(object value, XmlQualifiedName qualifiedName) {

         if (value == null)
            yield break;

         yield return CreateAtomicValue(value, qualifiedName);
      }

      public IEnumerable<XPathItem> CreateAtomicValueSequence(string value, XmlQualifiedName qualifiedName) {
         return CreateAtomicValueSequence(value as object, qualifiedName);
      }

      public IEnumerable<XPathItem> CreateAtomicValueSequence(IEnumerable value, XmlQualifiedName qualifiedName) {

         if (value == null)
            yield break;

         foreach (object item in value) {
            if (item != null)
               yield return CreateAtomicValue(item, qualifiedName);
         }
      }

      public IXPathNavigable CreateDocument(object value) {

         if (value == null)
            return null;

         IXPathNavigable inputNode = value as IXPathNavigable;

         if (inputNode != null)
            return inputNode;

         XNode xNode = value as XNode;

         if (xNode != null)
            return xNode.CreateNavigator();

         IXmlSerializable xmlSer = value as IXmlSerializable;

         if (xmlSer != null)
            return CreateDocument(xmlSer);

         inputNode = CreateNodeEditable();

         XmlSerializer serializer = GetSerializer(value.GetType());

         XmlWriter xmlWriter = inputNode.CreateNavigator().AppendChild();

         serializer.Serialize(xmlWriter, value);

         xmlWriter.Close();

         return inputNode;
      }

      public IXPathNavigable CreateDocument(IXmlSerializable value) {

         if (value == null)
            return null;

         IXPathNavigable doc = CreateNodeEditable();
         XmlWriter writer = doc.CreateNavigator().AppendChild();

         Serialize(value, writer);

         writer.Close();

         return doc;
      }

      public XPathNavigator CreateElement(object value) {

         IXPathNavigable doc = CreateDocument(value);

         if (doc == null)
            return null;

         return MoveToDocumentElement(doc);
      }

      public XPathNavigator CreateElement(IXmlSerializable value) {

         IXPathNavigable doc = CreateDocument(value);

         if (doc == null)
            return null;

         return MoveToDocumentElement(doc);
      }

      static XPathNavigator MoveToDocumentElement(IXPathNavigable doc) {

         XPathNavigator nav = doc.CreateNavigator();
         nav.MoveToChild(XPathNodeType.Element);

         return nav;
      }

      internal static XmlSerializer GetSerializer(Type type) {
         return serializerCache.GetOrAdd(type, t => new XmlSerializer(t));
      }

      static XmlRootAttribute GetXmlRootAttribute(Type type) {

         return xmlRootCache.GetOrAdd(type, t => {

            XmlRootAttribute xmlRootAttr = t.GetCustomAttributes(typeof(XmlRootAttribute), inherit: false)
               .Cast<XmlRootAttribute>()
               .SingleOrDefault();

            if (xmlRootAttr != null)
               return xmlRootAttr;

            return new XmlRootAttribute(XmlConvert.EncodeLocalName(type.Name));
         });
      }

      // Parsing

      public virtual IXPathNavigable CreateNodeReadOnly() {
         return CreateNodeReadOnly(XmlReader.Create(new StringReader(""), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }));      
      }

      public IXPathNavigable CreateNodeReadOnly(Stream input) {
         return CreateNodeReadOnly(input, null);
      }
      
      public abstract IXPathNavigable CreateNodeReadOnly(Stream input, XmlParsingOptions options);

      public IXPathNavigable CreateNodeReadOnly(TextReader input) {
         return CreateNodeReadOnly(input, null);
      }

      public abstract IXPathNavigable CreateNodeReadOnly(TextReader input, XmlParsingOptions options);

      public abstract IXPathNavigable CreateNodeReadOnly(XmlReader reader);

      public virtual IXPathNavigable CreateNodeEditable() {
         return new XmlDocument();
      }
      
      public IXPathNavigable CreateNodeEditable(Stream input) {
         return CreateNodeEditable(input, null);
      }

      public virtual IXPathNavigable CreateNodeEditable(Stream input, XmlParsingOptions options) {

         options = options ?? new XmlParsingOptions();

         XmlReaderSettings settings = (XmlReaderSettings)options;

         XmlReader reader;

         if (options.BaseUri != null)
            reader = XmlReader.Create(input, settings, options.BaseUri.AbsoluteUri);
         else
            reader = XmlReader.Create(input, settings);

         using (reader) 
            return CreateNodeEditable(reader); 
      }

      public IXPathNavigable CreateNodeEditable(TextReader input) {
         return CreateNodeEditable(input, null);
      }

      public virtual IXPathNavigable CreateNodeEditable(TextReader input, XmlParsingOptions options) {
         
         options = options ?? new XmlParsingOptions();

         XmlReaderSettings settings = (XmlReaderSettings)options;

         XmlReader reader;

         if (options.BaseUri != null)
            reader = XmlReader.Create(input, settings, options.BaseUri.AbsoluteUri);
         else
            reader = XmlReader.Create(input, settings);

         using (reader) 
            return CreateNodeEditable(reader); 
      }

      public virtual IXPathNavigable CreateNodeEditable(XmlReader reader) {
         
         var document = new XmlDocument();
         document.Load(reader);

         return document;
      }
      
      // Serialization

      public void Serialize(XPathItem item, Stream output) {
         Serialize(item, output, null);
      }

      public void Serialize(XPathItem item, Stream output, XPathSerializationOptions options) {
         Serialize(new XPathItem[1] { item }, output, options);
      }

      public void Serialize(XPathItem item, TextWriter output) {
         Serialize(item, output, null);
      }

      public void Serialize(XPathItem item, TextWriter output, XPathSerializationOptions options) {
         Serialize(new XPathItem[1] { item }, output, options);
      }

      public void Serialize(XPathItem item, XmlWriter output) {
         Serialize(new XPathItem[1] { item }, output);
      }

      public void Serialize(IEnumerable<XPathItem> items, Stream output) {
         Serialize(items, output, null);
      }

      public virtual void Serialize(IEnumerable<XPathItem> items, Stream output, XPathSerializationOptions options) {

         options = options ?? new XPathSerializationOptions();

         XmlWriter writer = XmlWriter.Create(output, (XmlWriterSettings)options);

         if (options.Method == XPathSerializationMethods.XHtml)
            writer = new XHtmlWriter(writer);

         Serialize(items, writer);

         // NOTE: don't close writer if Serialize fails
         writer.Close();
      }

      public void Serialize(IEnumerable<XPathItem> items, TextWriter output) {
         Serialize(items, output, null);
      }

      public virtual void Serialize(IEnumerable<XPathItem> items, TextWriter output, XPathSerializationOptions options) {

         options = options ?? new XPathSerializationOptions();

         XmlWriter writer = XmlWriter.Create(output, (XmlWriterSettings)options);

         if (options.Method == XPathSerializationMethods.XHtml)
            writer = new XHtmlWriter(writer);

         Serialize(items, writer);

         // NOTE: don't close writer if Serialize fails
         writer.Close();
      }

      public void Serialize(IEnumerable<XPathItem> items, XmlWriter output) {

         items = items ?? Enumerable.Empty<XPathItem>();

         IEnumerator<XPathItem> enumerator = items.GetEnumerator();

         XPathItem lastItem = null;
         StringBuilder textBuffer = null;

         while (enumerator.MoveNext()) {

            XPathItem item = enumerator.Current;

            if (item == null) continue;

            if (item.IsNode) {

               XPathNavigator node = (XPathNavigator)item;

               switch (node.NodeType) {
                  case XPathNodeType.SignificantWhitespace:
                  case XPathNodeType.Text:
                  case XPathNodeType.Whitespace:

                     if (textBuffer == null)
                        textBuffer = new StringBuilder();

                     textBuffer.Append(node.Value);
                     break;

                  default:

                     if (textBuffer != null) {
                        output.WriteString(textBuffer.ToString());
                        
                        textBuffer = null;
                     }

                     node.WriteSubtree(output);
                     break;
               }

            } else {

               if (textBuffer == null)
                  textBuffer = new StringBuilder();

               if (!lastItem.IsNode)
                  textBuffer.Append(" ");

               textBuffer.Append(item.Value);
            }

            lastItem = item;
         }

         if (lastItem == null) {
            // Empty sequence, write empty string
            output.WriteString("");
         
         } else if (textBuffer != null) {
            output.WriteString(textBuffer.ToString());            
         }
      }

      public void Serialize(IXmlSerializable item, XmlWriter output) {

         if (item == null) throw new ArgumentNullException("value");
         if (output == null) throw new ArgumentNullException("output");

         XmlRootAttribute xmlRootAttr = GetXmlRootAttribute(item.GetType());
         XmlRootPrefixedAttribute xmlRootPrefixAttr = xmlRootAttr as XmlRootPrefixedAttribute;

         if (xmlRootPrefixAttr != null
            && xmlRootPrefixAttr.Prefix.HasValue()) {

            output.WriteStartElement(xmlRootPrefixAttr.Prefix, xmlRootAttr.ElementName, xmlRootAttr.Namespace);

         } else {
            output.WriteStartElement(xmlRootAttr.ElementName, xmlRootAttr.Namespace);
         }

         item.WriteXml(output);
         output.WriteEndElement();
      }
   }
}
