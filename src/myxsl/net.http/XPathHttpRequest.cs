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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Net.Mime;
using myxsl.common;

namespace myxsl.net.http {
   
   sealed class XPathHttpRequest {

      // required
      public string Method { get; set; }

      // optional
      public Uri Href { get; set; }
      public bool StatusOnly { get; set; }
      public string Username { get; set; }
      public string Password { get; set; }
      public string AuthMethod { get; set; }
      public bool SendAuthorization { get; set; }
      public string OverrideMediaType { get; set; }
      public bool FollowRedirect { get; set; }
      public int Timeout { get; set; }
      public NameValueCollection Headers { get; private set; }

      // mutually exclusive
      public XPathHttpBody Body { get; set; }
      public XPathHttpMultipart Multipart { get; set; }

      public XmlResolver Resolver { get; set; }
      public XPathItemFactory ItemFactory { get; set; }

      public XPathHttpRequest() {

         this.FollowRedirect = true;
         this.Headers = new NameValueCollection();
      }

      public void ReadXml(XPathNavigator node) {

         bool movedToDocEl = false;

         if (node.NodeType == XPathNodeType.Root) {
            movedToDocEl = node.MoveToChild(XPathNodeType.Element);
         }

         if (node.NamespaceURI == XPathHttpClient.Namespace
            && node.LocalName == "request") {

            if (node.NodeType == XPathNodeType.Element) {

               if (node.MoveToFirstAttribute()) {

                  do {
                     switch (node.LocalName) {
                        case "method":
                           this.Method = node.Value;
                           break;

                        case "href":
                           this.Href = new Uri(node.Value, UriKind.Absolute);
                           break;

                        case "status-only":
                           this.StatusOnly = node.ValueAsBoolean;
                           break;

                        case "username":
                           this.Username = node.Value;
                           break;

                        case "password":
                           this.Password = node.Value;
                           break;

                        case "auth-method":
                           this.AuthMethod = node.Value;
                           break;

                        case "send-authorization":
                           this.SendAuthorization = node.ValueAsBoolean;
                           break;

                        case "override-media-type":
                           this.OverrideMediaType = node.Value;
                           break;

                        case "follow-redirect":
                           this.FollowRedirect = node.ValueAsBoolean;
                           break;

                        case "timeout":
                           this.Timeout = node.ValueAsInt;
                           break;

                        default:
                           break;
                     }

                  } while (node.MoveToNextAttribute());

                  node.MoveToParent();
               }

               if (node.MoveToChild(XPathNodeType.Element)) {
                  do {
                     if (node.NamespaceURI == XPathHttpClient.Namespace) {
                        switch (node.LocalName) {
                           case "header":
                              this.Headers.Add(node.GetAttribute("name", ""), node.GetAttribute("value", ""));
                              break;

                           case "body":
                              this.Body = new XPathHttpBody();
                              this.Body.ReadXml(node, this.Resolver);
                              break;

                           case "multipart":
                              this.Multipart = new XPathHttpMultipart();
                              this.Multipart.ReadXml(node, this.Resolver);
                              break;

                           default:
                              break;
                        }
                     }
                  } while (node.MoveToNext(XPathNodeType.Element));

                  node.MoveToParent();
               }
            }
         }

         if (movedToDocEl) {
            node.MoveToParent();
         }
      }

      public XPathHttpResponse GetResponse() {

         EnsureValidForHttpRequest();

         HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(this.Href);
         httpRequest.Method = this.Method;
         httpRequest.AllowAutoRedirect = this.FollowRedirect;
         httpRequest.Headers.Add(this.Headers);

         if (this.Timeout > 0) {
            httpRequest.Timeout = this.Timeout;
         }

         if (this.SendAuthorization) {

            httpRequest.Credentials = new NetworkCredential {
               UserName = this.Username,
               Password = this.Password
            }.GetCredential(httpRequest.RequestUri, this.AuthMethod);
         }

         if (this.Multipart != null) {
            LoadContent(httpRequest, this.Multipart);

         } else if (this.Body != null) {
            LoadContent(httpRequest, this.Body);
         }

         HttpWebResponse httpResponse;

         try {
            httpResponse = (HttpWebResponse)httpRequest.GetResponse();

         } catch (WebException ex) {

            if (ex.Response != null) {
               httpResponse = (HttpWebResponse)ex.Response;
            } else {
               throw;
            }
         }

         return GetResponse(httpResponse);
      }

      XPathHttpResponse GetResponse(HttpWebResponse httpResponse) {

         var response = new XPathHttpResponse {
            Status = httpResponse.StatusCode,
            Message = httpResponse.StatusDescription,
            Headers = { httpResponse.Headers }
         };

         if (httpResponse.ContentLength > 0) {

            string mediaType = "";

            try {
               var contentType = new ContentType(httpResponse.ContentType);
               mediaType = contentType.MediaType;

            } catch (FormatException) { 
            } catch (ArgumentException) { 
            }

            if (MediaTypes.IsMultipart(mediaType)) {

               var multipart = new XPathHttpMultipart {
                  MediaType = mediaType
               };

               using (Stream responseBody = httpResponse.GetResponseStream()) {
                  multipart.Deserialize(responseBody, this.StatusOnly);
               }

               response.Multipart = multipart;

            } else {

               var body = new XPathHttpBody {
                  MediaType = mediaType
               };

               if (!String.IsNullOrEmpty(httpResponse.CharacterSet)) {
                  try {
                     body.Encoding = Encoding.GetEncoding(httpResponse.CharacterSet);
                  } catch (ArgumentException) { }
               }

               if (!this.StatusOnly) {

                  using (Stream responseBody = httpResponse.GetResponseStream())
                     body.Deserialize(responseBody, httpResponse.ResponseUri, this.ItemFactory, this.OverrideMediaType);
               }

               response.Body = body;
            }
         }

         return response;
      }

      void EnsureValidForHttpRequest() {

         if (this.Href == null) {
            throw new InvalidOperationException("Href cannot be null.");
         }

         if (String.IsNullOrEmpty(this.Method)) {
            throw new InvalidOperationException("Method cannot be null or empty.");
         }

         if (this.Multipart != null 
            && this.Body != null) {

            throw new InvalidOperationException("Multipart and Body properties are mutually exclusive.");
         }

         if (!String.IsNullOrEmpty(this.Username)) {

            if (String.IsNullOrEmpty(this.Password)) {
               throw new InvalidOperationException("Password cannot be null or empty if Username isn't.");
            }

            if (String.IsNullOrEmpty(this.AuthMethod)) {
               throw new InvalidOperationException("AuthMethod cannot be null or empty if Username isn't.");
            }

         } else if (this.SendAuthorization) {
            throw new InvalidOperationException("SendAuthorization cannot be true if Username is null or empty.");
         }

         if (this.Body != null) {

            if (String.IsNullOrEmpty(this.Body.MediaType)) {
               throw new InvalidOperationException("Body.MediaType cannot be null or empty.");
            }

         } else if (this.Multipart != null) {

            if (this.Multipart.Items.Count == 0) {
               throw new InvalidOperationException("A multipart request must have at least one part.");
            }

            if (String.IsNullOrEmpty(this.Multipart.MediaType)) {
               throw new InvalidOperationException("Multipart.MediaType cannot be null or empty.");
            }

            if (!MediaTypes.IsMultipart(this.Multipart.MediaType)) {
               throw new InvalidOperationException("Multipart.MediaType must be set to a multipart media type.");
            }

            for (int i = 0; i < this.Multipart.Items.Count; i++) {

               XPathHttpMultipartItem item = this.Multipart.Items[i];

               if (item.Body == null) {
                  throw new InvalidOperationException("Multipart.Items[" + i + "].Body cannot be null.");
               }

               if (String.IsNullOrEmpty(item.Body.MediaType)) {
                  throw new InvalidOperationException("Multipart.Items[" + i + "].MediaType cannot be null or empty.");
               }

               if (item.Body.Content == null 
                  && item.Body.Src == null) {

                  throw new InvalidOperationException("All multipart bodies must have content.");
               }
            }
         }
      }

      void LoadContent(HttpWebRequest httpRequest, XPathHttpBody body) {

         long contentLength = body.PrepareContent(this.ItemFactory, this.Resolver);

         if (contentLength > 0) {

            httpRequest.ContentLength = contentLength;
            httpRequest.ContentType = body.MediaType;

            using (Stream contentStream = body.GetContentStream(),
               requestStream = httpRequest.GetRequestStream()) {

               contentStream.CopyTo(requestStream);
            }
         }
      }

      void LoadContent(HttpWebRequest httpRequest, XPathHttpMultipart multipart) {

         string boundary = multipart.Boundary;

         if (String.IsNullOrEmpty(boundary)) {
            boundary = "----------" + DateTime.Now.Ticks.ToStringInvariant();
         }

         Encoding encoding = Encoding.UTF8;

         string newLine = Environment.NewLine;
         byte[] newLineBytes = encoding.GetBytes(newLine);

         var headers = new List<byte[]>();
         byte[] footer = encoding.GetBytes(String.Concat("--", boundary, "--", newLine));

         long contentLength = 0;

         for (int i = 0; i < multipart.Items.Count; i++) {
            
            XPathHttpMultipartItem item = multipart.Items[i];

            var sb = new StringBuilder()
               .Append("--")
               .Append(boundary)
               .Append(newLine);

            foreach (string key in item.Headers) {

               sb.Append(key)
                  .Append(": ")
                  .Append(item.Headers[key])
                  .Append(newLine);
            }

            sb.Append(newLine);

            byte[] header = encoding.GetBytes(sb.ToString());

            headers.Add(header);

            contentLength += header.LongLength;
            contentLength += item.Body.PrepareContent(this.ItemFactory, this.Resolver);
            contentLength += newLineBytes.LongLength;
         }

         contentLength += footer.LongLength;

         httpRequest.ContentLength = contentLength;
         httpRequest.ContentType = String.Concat(multipart.MediaType, "; boundary=", boundary);

         using (Stream requestStream = httpRequest.GetRequestStream()) {

            for (int i = 0; i < multipart.Items.Count; i++) {

               byte[] header = headers[i];

               requestStream.Write(header, 0, header.Length);

               using (Stream contentStream = multipart.Items[i].Body.GetContentStream()) {

                  contentStream.CopyTo(requestStream);
                  requestStream.Write(newLineBytes, 0, newLineBytes.Length);
               }
            }

            requestStream.Write(footer, 0, footer.Length);
         }
      }
   }
}
