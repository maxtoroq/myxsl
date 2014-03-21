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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using myxsl.net.common;
using Saxon.Api;

namespace myxsl.net.saxon {

   public sealed class SaxonItemFactory : XPathItemFactory {

      internal readonly Processor processor;
      readonly DocumentBuilder defaultDocBuilder;

      [CLSCompliant(false)]
      public SaxonItemFactory(Processor processor) {
         
         this.processor = processor;
         this.defaultDocBuilder = CreateDocumentBuilder(new XmlParsingOptions());
      }

      public override IXPathNavigable CreateNodeReadOnly(Stream input, XmlParsingOptions options) {
         return CreateDocumentBuilder(options).Build(input).ToXPathNavigable();
      }

      public override IXPathNavigable CreateNodeReadOnly(TextReader input, XmlParsingOptions options) {
         return CreateDocumentBuilder(options).Build(input).ToXPathNavigable();
      }

      public override IXPathNavigable CreateNodeReadOnly(XmlReader reader) {
         return CreateXdmNode(reader).ToXPathNavigable();
      }

      public override XPathItem CreateAtomicValue(object value, XmlQualifiedName qualifiedName) {
         
         if (value == null) throw new ArgumentNullException("value");
         if (qualifiedName == null) throw new ArgumentNullException("qualifiedName");

         XdmAtomicValue atomicValue;
         XdmValue xdmValue = value.ToXdmValue(this);

         if (xdmValue.Count != 1)
            throw new ArgumentException("value cannot be empty, or more than one item.", "value");
         
         try {
            XdmItem item = xdmValue.GetXdmEnumerator().AsItems().Single();

            if (item.IsAtomic()) {
               atomicValue = (XdmAtomicValue)item;

               if (!qualifiedName.IsEmpty) {
                  XmlQualifiedName typeName = atomicValue.GetTypeName(this.processor).ToXmlQualifiedName();

                  if (typeName != qualifiedName) 
                     atomicValue = new XdmAtomicValue(value.ToString(), new QName(qualifiedName), this.processor);
               }

            } else {
               atomicValue = (!qualifiedName.IsEmpty) ?
                  new XdmAtomicValue(value.ToString(), new QName(qualifiedName), this.processor) :
                  new XdmAtomicValue(value.ToString())
                  ;
            }
            
         } catch (Exception ex) {
            throw new SaxonException(ex.Message, ex);
         }

         return atomicValue.ToXPathItem();
      }

      DocumentBuilder CreateDocumentBuilder(XmlParsingOptions options) {

         DocumentBuilder docb = this.defaultDocBuilder;

         if (options != null) {

            docb = this.processor.NewDocumentBuilder();

            if (options.BaseUri != null)
               docb.BaseUri = options.BaseUri;

            if (options.XmlResolver != null)
               docb.XmlResolver = options.XmlResolver;

            docb.DtdValidation = options.PerformDtdValidation;
         }

         return docb;
      }

      [CLSCompliant(false)]
      public XdmNode CreateXdmNode(XmlReader reader) {
         return this.defaultDocBuilder.Build(reader);
      }

      public override void Serialize(IEnumerable<XPathItem> items, Stream output, XPathSerializationOptions options) {

         options = options ?? new XPathSerializationOptions();

         Serializer serializer = CreateSerializer(options);
         serializer.SetOutputStream(output);

         Serialize(items, serializer);
      }

      public override void Serialize(IEnumerable<XPathItem> items, TextWriter output, XPathSerializationOptions options) {

         options = options ?? new XPathSerializationOptions();

         Serializer serializer = CreateSerializer(options);
         serializer.SetOutputWriter(output);

         Serialize(items, serializer);
      }

      void Serialize(IEnumerable<XPathItem> items, Serializer serializer) {
         this.processor.WriteXdmValue(items.ToXdmValue(this), serializer);
      }

      [CLSCompliant(false)]
      public Serializer CreateSerializer(XPathSerializationOptions options) {

         var serializer = new Serializer();

         if (options.ByteOrderMark.HasValue)
            serializer.SetOutputProperty(Serializer.BYTE_ORDER_MARK, (options.ByteOrderMark.Value) ? "yes" : "no");

         if (options.ConformanceLevel == ConformanceLevel.Document)
            serializer.SetOutputProperty(Serializer.SAXON_REQUIRE_WELL_FORMED, "yes");

         if (options.DocTypePublic != null)
            serializer.SetOutputProperty(Serializer.DOCTYPE_PUBLIC, options.DocTypePublic);

         if (options.DocTypeSystem != null)
            serializer.SetOutputProperty(Serializer.DOCTYPE_SYSTEM, options.DocTypeSystem);

         if (options.Encoding != null)
            serializer.SetOutputProperty(Serializer.ENCODING, options.Encoding.WebName);

         if (options.Indent.HasValue)
            serializer.SetOutputProperty(Serializer.INDENT, (options.Indent.Value) ? "yes" : "no");

         if (options.MediaType != null)
            serializer.SetOutputProperty(Serializer.MEDIA_TYPE, options.MediaType);

         if (options.Method != null && options.Method.Namespace.Length == 0)
            serializer.SetOutputProperty(Serializer.METHOD, options.Method.Name);

         if (options.OmitXmlDeclaration.HasValue)
            serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, (options.OmitXmlDeclaration.Value) ? "yes" : "no");

         return serializer;
      }
   }
}
