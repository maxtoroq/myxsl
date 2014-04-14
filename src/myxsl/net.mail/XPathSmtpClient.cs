// Copyright 2011 Max Toro Q.
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
using System.Net.Mail;
using System.Web;
using System.Xml.XPath;
using myxsl.web;
using myxsl.common;
using System.Xml.Serialization;

namespace myxsl.net.mail {

   [XPathModule(Prefix, Namespace)]
   public sealed class XPathSmtpClient {

      internal const string Namespace = "http://myxsl.github.io/ns/net/mail";
      internal const string Prefix = "mail";

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }

      [XPathFunction("send", "element()", "element(" + Prefix + ":message)", HasSideEffects = true)]
      public IXmlSerializable Send(XPathNavigator message) {

         if (message == null) throw new ArgumentNullException("message");

         XPathItemFactory itemFactory = this.ItemFactory;

         if (itemFactory == null) {
            throw new InvalidOperationException("ItemFactory cannot be null.");
         }

         MailMessage mailMessage = GetMailMessage(message, itemFactory);

         var smtp = new SmtpClient();

         try {
            smtp.Send(mailMessage);

         } catch (SmtpException ex) {
            return GetError(ex);
         }

         return new XPathSmtpSuccess();
      }

      static MailMessage GetMailMessage(XPathNavigator message, XPathItemFactory itemFactory) {

         var xpathMessage = new XPathMailMessage();
         xpathMessage.ReadXml(message);

         return xpathMessage.ToMailMessage(itemFactory);
      }

      static XPathSmtpError GetError(SmtpException exception) {

         return new XPathSmtpError { 
            Status = exception.StatusCode,
            Message = exception.Message
         };
      }
   }
}
