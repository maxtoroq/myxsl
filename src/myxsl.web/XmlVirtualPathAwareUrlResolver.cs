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
using System.Web.Hosting;
using System.Text.RegularExpressions;
using System.Web;

namespace myxsl.web {
   
   public class XmlVirtualPathAwareUrlResolver : XmlUrlResolver {

      static readonly Uri applicationBaseUri = (HostingEnvironment.IsHosted) ? 
         new Uri(HostingEnvironment.ApplicationPhysicalPath, UriKind.Absolute)
         : null;

      public override Uri ResolveUri(Uri baseUri, string relativeUri) {

         if (applicationBaseUri != null) {

            bool baseUriIsInApp;

            if (baseUri == null || !baseUri.IsAbsoluteUri) {
               baseUri = applicationBaseUri;
               baseUriIsInApp = true;
            } else {
               Uri baseDiff = applicationBaseUri.MakeRelativeUri(baseUri);
               baseUriIsInApp = !baseDiff.IsAbsoluteUri;
            }

            Uri relUri = (!String.IsNullOrEmpty(relativeUri)) ?
               new Uri(relativeUri, UriKind.RelativeOrAbsolute) :
               null;

            if (relUri != null && !relUri.IsAbsoluteUri && baseUriIsInApp) {

               if (VirtualPathUtility.IsAbsolute(relUri.OriginalString) 
                  || VirtualPathUtility.IsAppRelative(relUri.OriginalString)) {

                  return new Uri(HostingEnvironment.MapPath(relUri.OriginalString), UriKind.Absolute);
               }
            }
         }

         return base.ResolveUri(baseUri, relativeUri);
      }

      public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {

         if (applicationBaseUri != null) {

            Uri diff = applicationBaseUri.MakeRelativeUri(absoluteUri);

            bool uriIsInApp = !diff.IsAbsoluteUri;

            if (uriIsInApp) {

               string virtualPath = VirtualPathUtility.ToAbsolute("~/" + diff.OriginalString);

               var vppFile = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath);

               if (vppFile == null) {
                  return null;
               }

               return vppFile.Open();
            }
         }

         return base.GetEntity(absoluteUri, role, ofObjectToReturn);
      }
   }
}
