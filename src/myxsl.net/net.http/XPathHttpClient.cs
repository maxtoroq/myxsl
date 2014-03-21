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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.web;
using myxsl.net.common;

namespace myxsl.net.net.http {

   [XPathModule(Prefix, Namespace)]
   public sealed class XPathHttpClient {
      
      internal const string Namespace = "http://expath.org/ns/http-client";
      internal const string Prefix = "http";

      public static Func<TextReader, IXPathNavigable> HtmlParser { get; set; }

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }
      
      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathFunction("send-request", "item()+", "element(" + Prefix + ":request)?", HasSideEffects = true)]
      public XPathItem[] SendRequest(XPathNavigator request) {
         return SendRequest(request, null);
      }

      [XPathFunction("send-request", "item()+", "element(" + Prefix + ":request)?", "xs:string?", HasSideEffects = true)]
      public XPathItem[] SendRequest(XPathNavigator request, string href) {
         return SendRequest(request, href, null);
      }

      [XPathFunction("send-request", "item()+", "element(" + Prefix + ":request)?", "xs:string?", "item()*", HasSideEffects = true)]
      public XPathItem[] SendRequest(XPathNavigator request, string href, IEnumerable<XPathItem> bodies) {

         XPathItemFactory itemFactory = this.ItemFactory;
         XmlResolver resolver = this.Resolver;

         if (itemFactory == null)
            throw new InvalidOperationException("ItemFactory cannot be null.");
         
         int bodiesLength = bodies != null ? bodies.Count() : 0;

         XPathHttpRequest xpathRequest;

         if (request == null) {

            if (String.IsNullOrEmpty(href))
               throw new ArgumentException("href cannot be null or empty if request is null.", "href");

            xpathRequest = new XPathHttpRequest { 
               Method = WebRequestMethods.Http.Get,
               Href = new Uri(href, UriKind.Absolute),
               Resolver = resolver,
               ItemFactory = itemFactory
            };

            if (bodiesLength > 0)
               throw new ArgumentException("Cannot use the bodies parameter when request is null.", "bodies");

         } else {

            xpathRequest = new XPathHttpRequest {
               Resolver = resolver,
               ItemFactory = itemFactory
            };
            xpathRequest.ReadXml(request);

            if (String.IsNullOrEmpty(href)) {
               if (xpathRequest.Href == null)
                  throw new ArgumentException("href cannot be null or empty if request.Href is null.", "href");

            } else {
               xpathRequest.Href = new Uri(href);
            }

            if (xpathRequest.Body != null) {

               if (bodiesLength > 0) {
                  if (bodiesLength > 1)
                     throw new ArgumentException("bodies must have a single item when request.Body is not null.", "bodies");

                  xpathRequest.Body.Content = bodies.Single();
               }

            } else if (xpathRequest.Multipart != null) {

               if (bodiesLength > 0) {

                  if (bodiesLength != xpathRequest.Multipart.Items.Count)
                     throw new ArgumentException("The number of items in bodies must match the multipart request bodies.", "bodies");

                  for (int i = 0; i < xpathRequest.Multipart.Items.Count; i++) {
                     XPathHttpMultipartItem item = xpathRequest.Multipart.Items[i];

                     if (item.Body != null)
                        item.Body.Content = bodies.Skip(i).First();
                  }
               }

            } else if (bodiesLength > 0) {
               throw new ArgumentException("If bodies is not empty request.Body or request.Multipart cannot be null.", "request");
            }
         }

         XPathHttpResponse xpathResponse = xpathRequest.GetResponse();        
         XPathNavigator responseEl = itemFactory.CreateElement(xpathResponse);

         var result = new List<XPathItem>();
         result.Add(responseEl);

         if (xpathResponse.Body != null) {

            if (xpathResponse.Body.Content != null)
               result.Add(xpathResponse.Body.Content);

         } else if (xpathResponse.Multipart != null) {

            foreach (XPathHttpMultipartItem item in xpathResponse.Multipart.Items) {

               if (item.Body.Content != null)
                  result.Add(item.Body.Content);
            }
         }

         return result.ToArray();
      }
   }
}
