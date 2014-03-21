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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace myxsl.net.web {

   [XPathModule(Prefix, Namespace)]
   public static class RequestModule {

      internal const string Prefix = "request";
      internal const string Namespace = "http://myxsl.net/ns/web/request";

      internal const UriFormat ReturnUriFormat = UriFormat.UriEscaped;

      static HttpContext Context {
         get { return HttpContext.Current; }
      }

      [XPathFunction("application-path", "xs:string")]
      public static string ApplicationPath() {
         return VirtualPathUtility.AppendTrailingSlash(Context.Request.ApplicationPath);
      }

      /// <summary>
      /// The absolute URL of the current HTTP request.
      /// </summary>
      [XPathFunction("url", "xs:string")]
      public static string Url() {
         return Url(null);
      }

      [XPathFunction("url", "xs:string", "xs:string?")]
      public static string Url(string components) {
         return Url(components, null);
      }

      [XPathFunction("url", "xs:string", "xs:string?", "xs:string?")]
      public static string Url(string components, string format) {
         return UriToString(Context.Request.Url, components, format);
      }

      /// <summary>
      /// The pathinfo part of the current HTTP request URL.
      /// </summary>
      [XPathFunction("path-info", "xs:string")]
      public static string PathInfo() {
         return Context.Request.PathInfo;
      }

      [XPathFunction("path", "xs:string")]
      public static string Path() {

         string rawUrl = Context.Request.RawUrl;
         int index = rawUrl.IndexOf('?');

         if (index > -1)
            rawUrl = rawUrl.Substring(0, index);

         return rawUrl;
      }

      [XPathFunction("file-path", "xs:string")]
      public static string FilePath() {
         return Context.Request.FilePath;
      }

      [XPathFunction("resolve-url", "xs:string", "xs:string")]
      public static string ResolveUrl(string relativeUrl) {
         return WebModule.AbsolutePath(WebModule.CombinePath(FilePath(), relativeUrl));
      }

      /// <summary>
      /// The referrer URL of the current HTTP request.
      /// </summary>
      [XPathFunction("referrer-url", "xs:string?")]
      public static string ReferrerUrl() {
         return ReferrerUrl(null);
      }

      [XPathFunction("referrer-url", "xs:string?", "xs:string?")]
      public static string ReferrerUrl(string components) {
         return ReferrerUrl(components, null);
      }

      [XPathFunction("referrer-url", "xs:string?", "xs:string?", "xs:string?")]
      public static string ReferrerUrl(string components, string format) {

         Uri referrerUrl = null;

         try {
            referrerUrl = Context.Request.UrlReferrer;

         } catch (UriFormatException) { }

         if (referrerUrl == null)
            return null;

         return UriToString(referrerUrl, components, format);
      }

      [XPathFunction("query", "xs:string")]
      public static string Query() {
         return Context.Request.QueryString.ToString();
      }

      /// <summary>
      /// The values of the querystring parameters of the current HTTP request, that match the provided name.
      /// </summary>
      [XPathFunction("query", "xs:string*", "xs:string?")]
      public static string[] Query(string name) {
         return Context.Request.QueryString.GetValues(name);
      }

      /// <summary>
      /// The names of the querystring parameters of the current HTTP request.
      /// </summary>
      [XPathFunction("query-names", "xs:string*")]
      public static string[] QueryNames() {
         return Context.Request.QueryString.AllKeys;
      }

      [XPathFunction("form", "xs:string")]
      public static string Form() {
         return Context.Request.Form.ToString();
      }

      /// <summary>
      /// The values of the form parameters of the current HTTP request, that match the provided name.
      /// </summary>
      [XPathFunction("form", "xs:string*", "xs:string")]
      public static string[] Form(string name) {
         return Context.Request.Form.GetValues(name);
      }

      /// <summary>
      /// The names of the form parameters of the current HTTP request.
      /// </summary>
      [XPathFunction("form-names", "xs:string*")]
      public static string[] FormNames() {
         return Context.Request.Form.AllKeys;
      }

      /// <summary>
      /// The HTTP method the current request.
      /// </summary>
      [XPathFunction("http-method", "xs:string")]
      public static string HttpMethod() {
         return Context.Request.HttpMethod;
      }

      [XPathFunction("http-method-override", "xs:string")]
      public static string HttpMethodOverride() {

         HttpRequest request = Context.Request;

         const string XHttpMethodOverrideKey = "X-HTTP-Method-Override";

         string incomingVerb = request.HttpMethod;

         if (!String.Equals(incomingVerb, "POST", StringComparison.OrdinalIgnoreCase)) {
            return incomingVerb;
         }

         string verbOverride = null;
         string headerOverrideValue = request.Headers[XHttpMethodOverrideKey];
         if (!String.IsNullOrEmpty(headerOverrideValue)) {
            verbOverride = headerOverrideValue;
         } else {
            string formOverrideValue = request.Form[XHttpMethodOverrideKey];
            if (!String.IsNullOrEmpty(formOverrideValue)) {
               verbOverride = formOverrideValue;
            } else {
               string queryStringOverrideValue = request.QueryString[XHttpMethodOverrideKey];
               if (!String.IsNullOrEmpty(queryStringOverrideValue)) {
                  verbOverride = queryStringOverrideValue;
               }
            }
         }
         if (verbOverride != null) {
            if (!String.Equals(verbOverride, "GET", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(verbOverride, "POST", StringComparison.OrdinalIgnoreCase)) {
               incomingVerb = verbOverride;
            }
         }
         return incomingVerb;
      }

      /// <summary>
      /// The value of the HTTP header of the current request, that match the provided name.
      /// </summary>
      [XPathFunction("header", "xs:string?", "xs:string")]
      public static string Header(string name) {
         return Context.Request.Headers.Get(name);
      }

      /// <summary>
      /// The value of the Content-Type header of the current HTTP request.
      /// </summary>
      [XPathFunction("content-type", "xs:string?")]
      public static string ContentType() {
         return Context.Request.ContentType;
      }

      [XPathFunction("content-length", "xs:integer")]
      public static int ContentLength() {
         return Context.Request.ContentLength;
      }

      [XPathFunction("is-local", "xs:boolean")]
      public static bool IsLocal() {
         return Context.Request.IsLocal;
      }

      [XPathFunction("cookie", "xs:string?", "xs:string")]
      public static string Cookie(string name) {

         HttpCookie cookie = Context.Request.Cookies.Get(name);

         if (cookie != null)
            return cookie.Value;
         else
            return null;
      }

      /// <summary>
      /// This member supports the myxsl.net infrastructure and is not intended to be used directly from your code.
      /// </summary>
      public static string Cookie(string name, bool remove) {

         string val = Cookie(name);

         if (remove)
            ResponseModule.RemoveCookie(name);

         return val;
      }

      [XPathFunction("user-languages", "xs:string*")]
      public static string[] UserLanguages() {
         return Context.Request.UserLanguages;
      }

      [XPathFunction("user-host-address", "xs:string")]
      public static string UserHostAddress() {
         return Context.Request.UserHostAddress;
      }

      [XPathFunction("user-host-name", "xs:string")]
      public static string UserHostName() {
         return Context.Request.UserHostName;
      }

      [XPathFunction("map-path", "xs:string", "xs:string")]
      public static string MapPath(string virtualPath) {
         return Context.Request.MapPath(virtualPath);
      }

      [XPathFunction("is-ajax-request", "xs:boolean")]
      public static bool IsAjaxRequest() {

         HttpRequest request = Context.Request;

         return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"));
      }

      static string UriToString(Uri uri, string components, string format) {

         if (components != null) {
            UriComponents componentsEnum = (UriComponents)Enum.Parse(typeof(UriComponents), components, ignoreCase: true);
            UriFormat formatEnum = (format == null) ? UriFormat.UriEscaped : (UriFormat)Enum.Parse(typeof(UriFormat), format, ignoreCase: true);

            return uri.GetComponents(componentsEnum, formatEnum);
         }

         return uri.AbsoluteUri;
      }
   }
}
