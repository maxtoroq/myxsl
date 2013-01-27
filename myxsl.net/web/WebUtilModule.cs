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
using System.Configuration;
using System.Web;

namespace myxsl.net.web {

   [XPathModule("web", "http://myxsl.net/ns/web")]
   public static class WebModule {

      [XPathFunction("absolute-path", "xs:string", "xs:string")]
      public static string AbsolutePath(string appRelativePath) {
         return VirtualPathUtility.ToAbsolute(appRelativePath);
      }

      [XPathFunction("app-relative-path", "xs:string", "xs:string")]
      public static string AppRelativePath(string absolutePath) {
         return VirtualPathUtility.ToAppRelative(absolutePath);
      }

      [XPathFunction("combine-path", "xs:string", "xs:string?", "xs:string")]
      public static string CombinePath(string basePath, string relativePath) {
         return VirtualPathUtility.Combine(basePath, relativePath);
      }

      [XPathFunction("encode-url", "xs:string", "xs:string")]
      public static string EncodeUrl(string str) {
         return HttpUtility.UrlEncode(str);
      }
   }
}
