// Copyright 2014 Max Toro Q.
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
using System.Xml;
using Saxon.Api;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using net.sf.saxon.type;

namespace myxsl.saxon {

   class XdmWriter : XmlWriter {

      readonly Builder builder;

      NodeName attrName;
      string attrValue;
      WriteState currentState;

      XdmNode document;

      public XdmNode Document {
         get {
            if (currentState != WriteState.Closed) {
               throw new InvalidOperationException();
            }
            return document;
         }
      }

      public override WriteState WriteState {
         get { return currentState; }
      }

      public XdmWriter(Builder builder) {

         if (builder == null) throw new ArgumentNullException("builder");

         this.builder = builder;

         this.builder.open();
         this.builder.startDocument(0);
      }

      public override void Close() {

         if (this.currentState == WriteState.Closed) {
            return;
         }

         try {
            
            this.builder.endDocument();
            this.builder.close();
         
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }

         this.currentState = WriteState.Closed;
         this.document = (XdmNode)XdmValue.Wrap(this.builder.getCurrentRoot());
      }

      public override void Flush() { }

      public override string LookupPrefix(string ns) {
         throw new NotSupportedException();
      }

      public override void WriteBase64(byte[] buffer, int index, int count) {
         throw new NotSupportedException();
      }

      public override void WriteCData(string text) {
         WriteString(text);
      }

      public override void WriteCharEntity(char ch) {
         WriteString(new string(ch, 1));
      }

      public override void WriteChars(char[] buffer, int index, int count) {

         try {
            this.builder.characters(new string(buffer, index, count), 0, 0);
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteComment(string text) {

         try {
            this.builder.comment(text, 0, 0);
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteDocType(string name, string pubid, string sysid, string subset) { }

      public override void WriteEndAttribute() {

         try {
            
            if (this.attrName != null) {

               string prefix = this.attrName.getPrefix();
               string localName = this.attrName.getLocalPart();

               bool prefixIsEmpty = String.IsNullOrEmpty(prefix);
               string prefixOrLocal = !prefixIsEmpty ? prefix : localName;

               if (prefixOrLocal == "xmlns") {
                  this.builder.@namespace(new NamespaceBinding(prefixIsEmpty ? "" : localName, this.attrValue), 0);
               } else {
                  this.builder.attribute(this.attrName, AnySimpleType.getInstance(), this.attrValue, 0, 0);
               }

               this.attrName = null;
               this.attrValue = null;
            }

         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteEndDocument() { }

      public override void WriteEndElement() {

         try {
            this.builder.endElement();
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteEntityRef(string name) {

         switch (name) {
            case "amp":
               WriteString("&");
               break;

            case "apos":
               WriteString("'");
               break;

            case "gt":
               WriteString(">");
               break;

            case "lt":
               WriteString("<");
               break;

            case "quot":
               WriteString("\"");
               break;

            default:
               this.currentState = WriteState.Error;
               throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Entity '{0}' not supported.", name), "name");
         }
      }

      public override void WriteFullEndElement() {
         WriteEndElement();
      }

      public override void WriteProcessingInstruction(string name, string text) {

         try {
            this.builder.processingInstruction(name, text, 0, 0);
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteRaw(string data) {
         WriteString(data);
      }

      public override void WriteRaw(char[] buffer, int index, int count) {
         WriteString(new string(buffer, index, count));
      }

      public override void WriteStartAttribute(string prefix, string localName, string ns) {
         this.attrName = CreateNodeName(prefix, localName, ns);
      }

      public override void WriteStartDocument(bool standalone) { }

      public override void WriteStartDocument() { }

      public override void WriteStartElement(string prefix, string localName, string ns) {

         try {
            this.builder.startElement(CreateNodeName(prefix, localName, ns), Untyped.getInstance(), 0, 0);
         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteString(string text) {

         try {

            if (this.attrName != null) {
               this.attrValue = text;
               return;
            }

            this.builder.characters(text, 0, 0);

         } catch {

            this.currentState = WriteState.Error;
            throw;
         }
      }

      public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
         WriteString(new string(new char[] { highChar, lowChar }));
      }

      public override void WriteWhitespace(string ws) {
         WriteString(ws);
      }

      static NodeName CreateNodeName(string prefix, string localName, string ns) {
         return new FingerprintedQName(prefix ?? "", ns ?? "", localName);
      }
   }
}