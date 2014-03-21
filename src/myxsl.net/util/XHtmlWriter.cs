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
namespace myxsl.net {

   class XHtmlWriter : XmlWrappingWriter {
      
      readonly Stack<XmlQualifiedName> elementStack;

      public XHtmlWriter(XmlWriter baseWriter)
         : base(baseWriter) {
         
         this.elementStack = new Stack<XmlQualifiedName>();
      }

      public override void WriteStartElement(string prefix, string localName, string ns) {
         
         this.elementStack.Push(new XmlQualifiedName(localName, ns));
         base.WriteStartElement(prefix, localName, ns);
      }

      public override void WriteEndElement() {
         WriteXHMLEndElement(fullEndTag: false);
      }

      public override void WriteFullEndElement() {
         WriteXHMLEndElement(fullEndTag: true);
      }

      void WriteXHMLEndElement(bool fullEndTag) {
         
         bool writeFullEndTag = fullEndTag;
         XmlQualifiedName elementName = elementStack.Pop();
         
         if (elementName.Namespace == WellKnownNamespaces.XHTML) {
            
            switch (elementName.Name.ToLowerInvariant()) {
               case "area":
               case "base":
               case "basefont":
               case "br":
               case "col":
               case "frame":
               case "hr":
               case "img":
               case "input":
               case "isindex":
               case "link":
               case "meta":
               case "param":
                  writeFullEndTag = false;
                  break;

               default:
                  writeFullEndTag = true;
                  break;
            }
         }
         
         if (writeFullEndTag) 
            base.WriteFullEndElement();
         else 
            base.WriteEndElement();
      }
   }
}