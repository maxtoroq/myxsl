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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;

namespace myxsl.common {

   static class TypeResolver {

      public static readonly string UriSchemeClitype = "clitype";

      public static Type ResolveUri(Uri typeUri) {

         if (typeUri == null) throw new ArgumentNullException("typeUri");
         if (!typeUri.IsAbsoluteUri) throw new ArgumentException("typeUri must be absolute", "typeUri");
         if (typeUri.Scheme != UriSchemeClitype) throw new ArgumentException("", "typeUri");

         NameValueCollection query = HttpUtility.ParseQueryString(
            (typeUri.Query ?? "").Replace(';', '&')
         );

         string asm = query["asm"];
         string ver = query["ver"];
         string loc = query["loc"];
         string sn = query["sn"];
         string from = query["from"];

         string className = typeUri.AbsolutePath;

         Type type;
         bool throwOnError = false, ignoreCase = false;

         if (!String.IsNullOrEmpty(from)) {
            
            Assembly assembly = Assembly.LoadFrom(from);
            type = assembly.GetType(className, throwOnError, ignoreCase);

         } else {

            string typeName;
            
            if (!String.IsNullOrEmpty(asm)) {

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

               typeName = String.Concat(className, ", ", longNameBuilder.ToString());

            } else {
               typeName = className;
            }

            type = BuildManager.GetType(typeName, throwOnError, ignoreCase);
         }

         return type;
      }
   }
}
