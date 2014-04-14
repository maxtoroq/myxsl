// BSD License
// 
// Copyright (c) 2005, XMLMVP Project
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright 
// notice, this list of conditions and the following disclaimer. 
// * Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the 
// distribution. 
// * Neither the name of the XMLMVP Project nor the names of its 
// contributors may be used to endorse or promote products derived
// from this software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
// COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

//-----------------------------------------------------------------//
// This code has been slightly modified from it's original version //
//-----------------------------------------------------------------//

//namespace Mvp.Xml.Common {
namespace myxsl {

   abstract class XmlWrappingWriter : XmlWriter {
      
      readonly XmlWriter baseWriter;

      protected XmlWrappingWriter(XmlWriter baseWriter) {
         
         if (baseWriter == null) throw new ArgumentNullException("baseWriter");

         this.baseWriter = baseWriter;
      }

      public override XmlWriterSettings Settings {
         get { return this.baseWriter.Settings; }
      }

      public override WriteState WriteState {
         get { return this.baseWriter.WriteState; }
      }

      public override string XmlLang {
         get { return this.baseWriter.XmlLang; }
      }

      public override XmlSpace XmlSpace {
         get { return this.baseWriter.XmlSpace; }
      }

      public override void Close() { 
         this.baseWriter.Close(); 
      }

      protected override void Dispose(bool disposing) {

         if (this.WriteState != WriteState.Closed) {
            Close();
         }

         ((IDisposable)this.baseWriter).Dispose();
      }

      public override void Flush() { 
         this.baseWriter.Flush(); 
      }

      public override string LookupPrefix(string ns) { 
         return this.baseWriter.LookupPrefix(ns); 
      }

      public override void WriteBase64(byte[] buffer, int index, int count) { 
         this.baseWriter.WriteBase64(buffer, index, count); 
      }

      public override void WriteCData(string text) { 
         this.baseWriter.WriteCData(text); 
      }

      public override void WriteCharEntity(char ch) { 
         this.baseWriter.WriteCharEntity(ch); 
      }

      public override void WriteChars(char[] buffer, int index, int count) { 
         this.baseWriter.WriteChars(buffer, index, count); 
      }

      public override void WriteComment(string text) { 
         this.baseWriter.WriteComment(text); 
      }

      public override void WriteDocType(string name, string pubid, string sysid, string subset) { 
         this.baseWriter.WriteDocType(name, pubid, sysid, subset); 
      }

      public override void WriteEndAttribute() { 
         this.baseWriter.WriteEndAttribute(); 
      }

      public override void WriteEndDocument() { 
         this.baseWriter.WriteEndDocument(); 
      }

      public override void WriteEndElement() { 
         this.baseWriter.WriteEndElement(); 
      }

      public override void WriteEntityRef(string name) { 
         this.baseWriter.WriteEntityRef(name); 
      }

      public override void WriteFullEndElement() { 
         this.baseWriter.WriteFullEndElement(); 
      }

      public override void WriteProcessingInstruction(string name, string text) { 
         this.baseWriter.WriteProcessingInstruction(name, text); 
      }

      public override void WriteRaw(string data) {
         this.baseWriter.WriteRaw(data); 
      }

      public override void WriteRaw(char[] buffer, int index, int count) { 
         this.baseWriter.WriteRaw(buffer, index, count); 
      }

      public override void WriteStartAttribute(string prefix, string localName, string ns) { 
         this.baseWriter.WriteStartAttribute(prefix, localName, ns); 
      }

      public override void WriteStartDocument() { 
         this.baseWriter.WriteStartDocument(); 
      }

      public override void WriteStartDocument(bool standalone) { 
         this.baseWriter.WriteStartDocument(standalone); 
      }

      public override void WriteStartElement(string prefix, string localName, string ns) { 
         this.baseWriter.WriteStartElement(prefix, localName, ns); 
      }

      public override void WriteString(string text) { 
         this.baseWriter.WriteString(text); 
      }

      public override void WriteSurrogateCharEntity(char lowChar, char highChar) { 
         this.baseWriter.WriteSurrogateCharEntity(lowChar, highChar); 
      }

      public override void WriteValue(bool value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(DateTime value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(decimal value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(double value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(int value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(long value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(object value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(float value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteValue(string value) { 
         this.baseWriter.WriteValue(value); 
      }

      public override void WriteWhitespace(string ws) { 
         this.baseWriter.WriteWhitespace(ws); 
      }
   }
}