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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace myxsl.web {

   [XPathModule("response", "http://myxsl.github.io/ns/web/response", Predeclare = true)]
   public static class ResponseModule {
      
      static HttpContext Context {
         get { return HttpContext.Current; }
      }

      [XPathFunction("redirect", "xs:string", As = "empty-sequence()")]
      public static void Redirect(string url) {
         Context.Response.Redirect(url, endResponse: false);
      }

      [XPathFunction("redirect", "xs:string", "xs:integer", As = "empty-sequence()")]
      public static void Redirect(string url, int statusCode) {

         HttpResponse response = Context.Response;

         response.RedirectLocation = url;
         response.StatusCode = statusCode;
      }

      [XPathFunction("set-header", "xs:string", "xs:string", As = "empty-sequence()")]
      public static void SetHeader(string name, string value) {
         Context.Response.Headers.Set(name, value);
      }

      [XPathFunction("set-content-type", "xs:string", As = "empty-sequence()")]
      public static void SetContentType(string value) {
         Context.Response.ContentType = value;
      }

      [XPathFunction("set-status", "xs:integer", As = "empty-sequence()")]
      public static void SetStatus(int code) {
         Context.Response.StatusCode = code;
      }

      [XPathFunction("set-status", "xs:integer", "xs:string", As = "empty-sequence()")]
      public static void SetStatus(int code, string description) {

         HttpResponse response = Context.Response;

         response.StatusCode = code;
         response.StatusDescription = description;
      }

      [XPathFunction("set-cookie", "xs:string", "xs:string", As = "empty-sequence()")]
      public static void SetCookie(string name, string value) {
         Context.Response.Cookies.Set(new HttpCookie(name, value));
      }

      [XPathFunction("remove-cookie", "xs:string", As = "empty-sequence()")]
      public static void RemoveCookie(string name) {
         Context.Response.Cookies.Remove(name);
      }
   }
}
