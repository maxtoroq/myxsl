﻿// Copyright 2011 Max Toro Q.
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
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

namespace myxsl.net.net.mail {
   
   sealed class XPathSmtpError : IXmlSerializable {

      public SmtpStatusCode Status { get; set; }
      public string Message { get; set; }

      public System.Xml.Schema.XmlSchema GetSchema() {
         return null;
      }

      public void ReadXml(XmlReader reader) {
         throw new NotImplementedException();
      }

      public void WriteXml(XmlWriter writer) {

         writer.WriteStartElement(XPathSmtpClient.Prefix, "error", XPathSmtpClient.Namespace);
         writer.WriteAttributeString("status", this.Status.ToString("d"));
         writer.WriteString(this.Message);
         writer.WriteEndElement();
      }
   }
}
