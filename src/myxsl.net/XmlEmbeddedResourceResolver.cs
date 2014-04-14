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
using System.Net;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Globalization;

namespace myxsl {
   
   public class XmlEmbeddedResourceResolver : XmlResolver {
      
      public static readonly string UriSchemeClires = "clires";

      public Assembly DefaultAssembly { get; set; }

      public override ICredentials Credentials {
         set { }
      }

      public XmlEmbeddedResourceResolver() {
         this.DefaultAssembly = Assembly.GetCallingAssembly();
      }

      public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
         
         if (absoluteUri == null) throw new ArgumentNullException("absoluteUri");
         if (absoluteUri.AbsolutePath.Length <= 1) throw new ArgumentException("The embedded resource name must be specified in the AbsolutePath portion of the supplied Uri.");

         string host = absoluteUri.Host;

         if (String.IsNullOrEmpty(host)) {
            host = null;
         }

         string resourceName = ((host != null) ? 
            absoluteUri.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped)
            : absoluteUri.AbsolutePath)
            .Replace("/", ".");

         Assembly assembly = null;

         NameValueCollection query = HttpUtility.ParseQueryString(
            (absoluteUri.Query ?? "").Replace(';', '&')
         );

         string asm = host ?? query["asm"];
         string ver = query["ver"];
         string loc = query["loc"];
         string sn = query["sn"];
         string from = query["from"];
         
         if (!String.IsNullOrEmpty(from)) {
            assembly = Assembly.ReflectionOnlyLoadFrom(from);
         
         } else if (!String.IsNullOrEmpty(asm)) {

            var longNameBuilder = new StringBuilder(asm);

            if (!String.IsNullOrEmpty(ver)) {
               longNameBuilder.Append(", Version=").Append(ver);
            }

            if (!String.IsNullOrEmpty(loc)) {
               longNameBuilder.Append(", Culture=").Append(loc);
            }

            if (!String.IsNullOrEmpty(sn)) {
               longNameBuilder.Append(", PublicKeyToken=").Append(sn);
            }

            assembly = Assembly.ReflectionOnlyLoad(longNameBuilder.ToString());

         } else if (this.DefaultAssembly != null) {
            assembly = this.DefaultAssembly;
         }

         if (assembly != null) {
            return assembly.GetManifestResourceStream(resourceName);
         }

         throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Could not determine the assembly of the resource identified by \"{0}\".", absoluteUri));
      }
   }
}
