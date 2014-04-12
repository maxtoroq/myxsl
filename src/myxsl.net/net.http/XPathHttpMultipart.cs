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
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace myxsl.net.net.http {
   
   sealed class XPathHttpMultipart {

      // required
      public string MediaType { get; set; }
      
      // one or more
      public IList<XPathHttpMultipartItem> Items { get; private set; }

      // optional
      public string Boundary { get; set; }

      public XPathHttpMultipart() {
         this.Items = new List<XPathHttpMultipartItem>();
      }

      public void ReadXml(XPathNavigator node, XmlResolver resolver) {

         if (node.NodeType == XPathNodeType.Element) {

            if (node.MoveToFirstAttribute()) {
               do {
                  switch (node.LocalName) {
                     case "media-type":
                        this.MediaType = node.Value;
                        break;

                     case "boundary":
                        this.Boundary = node.Value;
                        break;
                  }
               } while (node.MoveToNextAttribute());

               node.MoveToParent();
            }

            if (node.MoveToChild(XPathNodeType.Element)) {

               XPathHttpMultipartItem currentItem = null;

               do {
                  if (node.NamespaceURI == XPathHttpClient.Namespace) {

                     switch (node.LocalName) {
                        case "header":
                           if (currentItem == null) {
                              currentItem = new XPathHttpMultipartItem();
                           }

                           currentItem.Headers.Add(node.GetAttribute("name", ""), node.GetAttribute("value", ""));
                           break;

                        case "body":
                           if (currentItem == null) {
                              currentItem = new XPathHttpMultipartItem();
                           }

                           currentItem.Body = new XPathHttpBody();
                           currentItem.Body.ReadXml(node, resolver);

                           this.Items.Add(currentItem);
                           currentItem = null;
                           break;
                     }
                  }

               } while (node.MoveToNext(XPathNodeType.Element));
               
               node.MoveToParent();
            }
         }
      }

      // TODO: Multipart responses

      public void WriteXml(XmlWriter writer) {
         throw new NotImplementedException();
      }

      public void Deserialize(Stream source, bool statusOnly) {
         throw new NotImplementedException("Multipart responses are not implemented.");         
      }
   }
}
