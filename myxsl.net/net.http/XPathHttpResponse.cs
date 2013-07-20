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
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace myxsl.net.net.http {
   
   sealed class XPathHttpResponse : IXmlSerializable {

      // required
      public HttpStatusCode Status { get; set; }
      public string Message { get; set; }

      // optional
      public NameValueCollection Headers { get; private set; }

      // mutually exclusive
      public XPathHttpBody Body { get; set; }
      public XPathHttpMultipart Multipart { get; set; }

      public XPathHttpResponse() {
         this.Headers = new NameValueCollection();
      }

      public System.Xml.Schema.XmlSchema GetSchema() {
         return null;
      }

      public void ReadXml(XmlReader reader) {
         throw new NotImplementedException();
      }

      public void WriteXml(XmlWriter writer) {

         writer.WriteStartElement(XPathHttpClient.Prefix, "response", XPathHttpClient.Namespace);
         writer.WriteAttributeString("status", this.Status.ToString("d"));
         writer.WriteAttributeString("message", this.Message);

         for (int i = 0; i < this.Headers.Count; i++) {
            writer.WriteStartElement(XPathHttpClient.Prefix, "header", XPathHttpClient.Namespace);
            writer.WriteAttributeString("name", this.Headers.Keys[i]);
            writer.WriteAttributeString("value", this.Headers[i]);
            writer.WriteEndElement();
         }

         if (this.Body != null)
            this.Body.WriteXml(writer);

         else if (this.Multipart != null)
            this.Multipart.WriteXml(writer);

         writer.WriteEndElement();
      }
   }
}
