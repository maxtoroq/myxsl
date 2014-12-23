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
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Xml;
using myxsl.configuration;

namespace myxsl {

   public sealed class XmlDynamicResolver : XmlResolver {

      ICredentials _Credentials;
      readonly IDictionary<string, XmlResolver> resolvers;
      readonly Assembly callingAssembly;

      public Uri DefaultBaseUri { get; set; }

      public override ICredentials Credentials {
         set { _Credentials = value; }
      }

      public XmlDynamicResolver()
         : this(Assembly.GetCallingAssembly()) { }

      public XmlDynamicResolver(Uri defaultBaseUri)
         : this(Assembly.GetCallingAssembly()) {

         this.DefaultBaseUri = defaultBaseUri;
      }

      public XmlDynamicResolver(Assembly callingAssembly) {

         this.resolvers = new Dictionary<string, XmlResolver>();
         this.callingAssembly = callingAssembly;
      }

      public override Uri ResolveUri(Uri baseUri, string relativeUri) {

         if (baseUri == null) {
            baseUri = this.DefaultBaseUri;
         }

         XmlResolver resolver;

         if (baseUri != null && baseUri.IsAbsoluteUri && IsKnownScheme(baseUri.Scheme)) {
            resolver = GetResolver(baseUri.Scheme);

         } else if (IsKnownScheme(Uri.UriSchemeFile)) {
            resolver = GetResolver(Uri.UriSchemeFile);
         
         } else {
            throw new InvalidOperationException("Cannot resolve URI, please register a resolver for the 'file' scheme to act as default.");
         }

         return resolver.ResolveUri(baseUri, relativeUri);
      }

      public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {

         if (absoluteUri == null) throw new ArgumentNullException("absoluteUri");

         string scheme = absoluteUri.Scheme;

         if (!IsKnownScheme(scheme)) {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "There isn't a resolver registered for the '{0}' scheme.", scheme));
         }

         XmlResolver resolver = GetResolver(scheme);

         if (_Credentials != null) {
            resolver.Credentials = _Credentials;
         }

         return resolver.GetEntity(absoluteUri, role, ofObjectToReturn);
      }

      bool IsKnownScheme(string scheme) {
         
         return this.resolvers.ContainsKey(scheme) 
            || LibraryConfigSection.Instance.Resolvers.Get(scheme) != null;
      }

      XmlResolver GetResolver(string scheme) {

         XmlResolver resolver;

         if (!this.resolvers.TryGetValue(scheme, out resolver)) {
            resolver = CreateResolver(scheme);
            this.resolvers[scheme] = resolver;
         }

         return resolver;
      }

      XmlResolver CreateResolver(string scheme) {

         XmlResolver resolver = (XmlResolver)Activator.CreateInstance(LibraryConfigSection.Instance.Resolvers.Get(scheme).TypeInternal);

         if (scheme == XmlEmbeddedResourceResolver.UriSchemeClires) {

            XmlEmbeddedResourceResolver defaultImpl = resolver as XmlEmbeddedResourceResolver;

            if (defaultImpl != null) {
               defaultImpl.DefaultAssembly = this.callingAssembly;
            }
         }

         return resolver;
      }
   }
}
