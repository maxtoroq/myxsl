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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml;
using System.Xml.XPath;
using myxsl.common;
using myxsl.web.compilation;

namespace myxsl.web.ui {
   
   public class XsltPageParser : BasePageParser {

      string _XsltVirtualPath;
      Uri _XsltPhysicalUri;
      List<string> visitedDocs = new List<string>();

      public string XsltVirtualPath {
         get { return _XsltVirtualPath; }
         set {
            _XsltVirtualPath = value;
            _XsltPhysicalUri = null;
         }
      }

      public Uri XsltPhysicalUri {
         get {
            if (_XsltPhysicalUri == null) {
               
               if (XsltVirtualPath == null) {
                  throw new InvalidOperationException("XsltVirtualPath cannot be null");
               }

               _XsltPhysicalUri = new Uri(HostingEnvironment.MapPath(XsltVirtualPath), UriKind.Absolute);
            }
            return _XsltPhysicalUri;
         }
      }

      public XsltPageType PageType { get; set; }
      public XmlQualifiedName DocumentName { get; set; }

      public XmlQualifiedName InitialTemplate { get; set; }
      public BindingExpressionInfo InitialTemplateBinding { get; set; }
      public string ProcessorName { get; set; }

      protected XPathNavigator Navigator { get; private set; }

      public override void Parse(TextReader source) {

         var readerSettings = new XmlReaderSettings { 
            CloseInput = false,
            IgnoreComments = true,
            IgnoreWhitespace = true
         };
         XmlReader reader = XmlReader.Create(source, readerSettings);
         
         this.Navigator = new XPathDocument(reader).CreateNavigator();

         XPathNavigator nav = this.Navigator;

         // determine PageType
         nav.MoveToChild(XPathNodeType.Element);

         this.DocumentName = new XmlQualifiedName(nav.Name, nav.NamespaceURI);

         string associatedStylesheetVirtualPath;
         this.PageType = GetPageType(out associatedStylesheetVirtualPath);

         nav.MoveToRoot();

         if (Processors.Xslt.Count == 0) {
            throw CreateParseException("There are no XSLT processors registered to render this page.");
         }

         this.ProcessorName = Processors.Xslt.Default;
         this.XsltVirtualPath = this.VirtualPath;

         switch (PageType) {
            case XsltPageType.StandardStylesheet:
               // default named template
               this.InitialTemplate = new XmlQualifiedName("main");
               break;

            case XsltPageType.AssociatedStylesheet:
               this.XsltVirtualPath = associatedStylesheetVirtualPath;
               this.SourceDependencies.Add(this.XsltVirtualPath);
               break;

            default:
               break;
         }

         this.visitedDocs.Add(this.XsltVirtualPath);

         bool pageDone, outputCacheDone;
         pageDone = outputCacheDone = false;

         for (bool moved = nav.MoveToFirstChild(); moved; moved = nav.MoveToNext()) {

            if (nav.NodeType == XPathNodeType.Element) {

               if (this.PageType == XsltPageType.StandardStylesheet) {
                  ParseDeclarations();
               }

            } else if (nav.NodeType == XPathNodeType.ProcessingInstruction) {

               switch (nav.LocalName) {
                  case page.it:
                     if (pageDone) { 
                        goto non_unique; 
                     }
                     ParsePagePI();
                     pageDone = true;
                     break;

                  case output_cache.it:
                     if (outputCacheDone) {
                        goto non_unique;
                     }
                     this.OutputCache = ParseOutputCachePI();
                     outputCacheDone = true;
                     break;

                  case reference.it:
                     ParseReferencePI();
                     break;

                  case import.it:
                     ParseImportPI();
                     break;

                  default:
                     continue;
               }

               continue;

            non_unique:
               throw CreateParseException("There can be only one '{0}' processing instruction.", nav.LocalName);
            }
         }
      }

      IXPathNavigable OpenDocument(string virtualPath) {

         var readerSettings = new XmlReaderSettings {
            IgnoreComments = true,
            IgnoreWhitespace = true
         };

         var vppFile = this.VirtualPathProvider.GetFile(virtualPath);

         if (vppFile == null) {
            throw CreateParseException("Could not find file '{0}'.", virtualPath);
         }

         Stream source;

         try {
            source = vppFile.Open();
         } catch (Exception ex) {

            throw CreateParseException(ex.Message);
         }

         using (source) {
            return new XPathDocument(XmlReader.Create(source, readerSettings));
         }
      }

      protected XsltPageType GetPageType(out string associatedStylesheetVirtualPath) {

         associatedStylesheetVirtualPath = null;

         XPathNavigator nav = this.Navigator;

         nav.MoveToRoot();
         
         bool xmlStyleDone = false;

         for (bool moved = nav.MoveToChild(XPathNodeType.ProcessingInstruction); moved; moved = nav.MoveToNext()) {

            switch (nav.LocalName) {
               case xml_stylesheet.it:
                  IDictionary<string, string> styleAttr = GetAttributes(nav.Value);
                  string assocVirtualPath = GetAssociatedStylesheetVirtualPath(styleAttr);

                  if (assocVirtualPath != null) {
                     
                     if (xmlStyleDone) {
                        throw CreateParseException("There can be only one '{0}' processing instruction.", nav.LocalName);
                     }

                     associatedStylesheetVirtualPath = assocVirtualPath;
                     return XsltPageType.AssociatedStylesheet;
                  }

                  break;
               default:
                  continue;
            }
         }
         
         nav.MoveToRoot();
         nav.MoveToChild(XPathNodeType.Element);

         if (nav.NamespaceURI == WellKnownNamespaces.XSLT) {
            return XsltPageType.StandardStylesheet;
         }

         if (nav.HasAttributes 
            && !String.IsNullOrEmpty(nav.GetAttribute("version", WellKnownNamespaces.XSLT))) {

            return XsltPageType.SimplifiedStylesheet;
         }

         throw CreateParseException("xsl:version attribute is missing (xsl prefix bound to '{0}').", WellKnownNamespaces.XSLT);
      }

      protected string GetAssociatedStylesheetVirtualPath(IDictionary<string, string> attribs) {

         string href = GetVirtualPathAttribute(attribs, xml_stylesheet.href, true);
         EnsureNonNull(href, xml_stylesheet.it, xml_stylesheet.href);

         return href;
      }

      protected void ParsePagePI() {

         XPathNavigator nav = this.Navigator;

         IDictionary<string, string> attribs = GetAttributes(nav.Value);

         // language
         string language = GetNonEmptyNoWhitespaceAttribute(attribs, page.language);

         if (language != null) {
            this.Language = language;
         }

         // class-name
         string className = GetFullClassNameAttribute(attribs, page.class_name);

         if (!String.IsNullOrEmpty(className)) {
            this.GeneratedTypeFullName = className;
         }

         // content-type
         string contentType = GetNonEmptyAttribute(attribs, page.content_type);

         if (contentType != null) {
            this.ContentType = contentType;
         }

         // enable-session-state
         object enableSs = GetEnumAttribute(attribs, page.enable_session_state, typeof(PagesEnableSessionState));

         if (enableSs != null) {
            this.EnableSessionState = (PagesEnableSessionState)enableSs;
         }

         // validate-request
         bool valReq = default(bool);

         if (GetBooleanAttribute(attribs, page.validate_request, ref valReq)) {
            this.ValidateRequest = valReq;
         }

         // accept-verbs
         string acceptVerbs = GetNonEmptyAttribute(attribs, page.accept_verbs);

         if (acceptVerbs != null) { 

            string[] verbs = acceptVerbs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < verbs.Length; i++) {
               this.AcceptVerbs.Add(verbs[i].Trim());
            }
         }

         // XSLT related attributes

         XPathNavigator nav2 = nav.CreateNavigator();
         nav2.MoveToRoot();
         nav2.MoveToChild(XPathNodeType.Element);
         
         IDictionary<string, string> namespacesInScope = nav2.GetNamespacesInScope(XmlNamespaceScope.All);

         // initial-template
         string initialTempl = GetNonEmptyAttribute(attribs, page.initial_template);

         if (initialTempl != null) {

            if (this.PageType != XsltPageType.StandardStylesheet) {
               throw CreateParseException("The '{0}' attribute can only be used on standard XSLT pages.", page.initial_template);
            }

            string itLocal = initialTempl;
            string itNamespace = "";

            if (initialTempl.Contains(":")) {
               string[] itParts = initialTempl.Split(':');
               itLocal = itParts[1];
               string itPrefix = itParts[0];

               if (namespacesInScope.ContainsKey(itPrefix)) {
                  itNamespace = namespacesInScope[itPrefix];
               }
            }

            this.InitialTemplate = new XmlQualifiedName(itLocal, itNamespace);
         }

         // initial-template-binding

         string initialTemplBind = GetNonEmptyAttribute(attribs, page.bind_initial_template);

         if (initialTemplBind != null) {

            if (this.PageType != XsltPageType.StandardStylesheet) {
               throw CreateParseException("The '{0}' attribute can only be used on standard XSLT pages.", page.bind_initial_template);
            }

            var exprBuilderContext = new BindingExpressionContext(this, nav.Clone(), namespacesInScope) {
               NodeName = page.bind_initial_template,
               AffectsXsltInitiation = true
            };

            try {
               this.InitialTemplateBinding = BindingExpressionBuilder.ParseExpr(initialTemplBind, exprBuilderContext);

            } catch (Exception ex) {
               throw CreateParseException(ex.Message);
            }

            if (this.InitialTemplateBinding != null) {
               this.InitialTemplateBinding.LineNumber = ((IXmlLineInfo)nav).LineNumber;
            }
         }

         // processor
         string processor = GetNonEmptyAttribute(attribs, page.processor);

         if (processor != null) {

            if (!Processors.Xslt.Exists(processor)) {
               throw CreateParseException("The processor '{0}' is not registered.", processor);
            }

            this.ProcessorName = processor; 
         }
      }

      protected IDictionary<string, object> ParseOutputCachePI() {

         XPathNavigator nav = this.Navigator;

         IDictionary<string, string> attribs = GetAttributes(nav.Value);

         var parameters = new Dictionary<string, object>();

         // location
         object location = GetEnumAttribute(attribs, output_cache.location, typeof(OutputCacheLocation));
         OutputCacheLocation loc = default(OutputCacheLocation);

         if (location != null) {
            loc = (OutputCacheLocation)location;
            parameters["Location"] = loc;
         }

         // cache-profile
         string cacheProfile = GetNonEmptyAttribute(attribs, output_cache.cache_profile);

         if (cacheProfile != null) {
            parameters["CacheProfile"] = cacheProfile;
         }

         bool otherParamsRequired = loc != OutputCacheLocation.None
            && cacheProfile == null;

         // duration
         string durationStr = GetNonEmptyAttribute(attribs, output_cache.duration);

         if (otherParamsRequired) {
            EnsureNonNull(durationStr, output_cache.it, output_cache.duration);
         }

         if (durationStr != null) {

            int duration;

            if (!int.TryParse(durationStr, out duration) 
               || duration <= 0) {

               throw CreateParseException("The '{0}' attribute must be set to a positive integer value.", output_cache.duration);
            }
            
            parameters["Duration"] = duration; 
         }

         // vary-by-param
         string varyByParam = GetNonEmptyAttribute(attribs, output_cache.vary_by_param);

         if (otherParamsRequired) {
            EnsureNonNull(varyByParam, output_cache.it, output_cache.vary_by_param);
         }

         if (varyByParam != null) {
            parameters["VaryByParam"] = String.Equals(varyByParam, "none", StringComparison.OrdinalIgnoreCase) ? null 
               : varyByParam; 
         }

         // no-store
         bool noStore = default(bool);

         if (GetBooleanAttribute(attribs, output_cache.no_store, ref noStore)) {
            parameters["NoStore"] = noStore;
         }

         // vary-by-header
         string varyByHeader = GetNonEmptyAttribute(attribs, output_cache.vary_by_header);

         if (varyByHeader != null) {
            parameters["VaryByHeader"] = varyByHeader;
         }

         // vary-by-custom
         string varyByCustom = GetNonEmptyAttribute(attribs, output_cache.vary_by_custom);

         if (varyByCustom != null) {
            parameters["VaryByCustom"] = varyByCustom;
         }

         // vary-by-content-encodings
         string varyByContentEncoding = GetNonEmptyAttribute(attribs, output_cache.vary_by_content_encodings);

         if (varyByContentEncoding != null) {
            parameters["VaryByContentEncoding"] = varyByContentEncoding;
         }

         return parameters;
      }

      protected void ParseReferencePI() {

         IDictionary<string, string> attribs = GetAttributes(this.Navigator.Value);

         string href = GetVirtualPathAttribute(attribs, reference.href, true);
         EnsureNonNull(href, reference.it, reference.href);

         if (!this.SourceDependencies.Contains(href)) {
            this.SourceDependencies.Add(href);
         }
      }

      protected void ParseImportPI() {

         XPathNavigator nav = this.Navigator;
         
         IDictionary<string, string> attribs = GetAttributes(nav.Value);

         string ns = GetNonEmptyAttribute(attribs, import.@namespace);
         EnsureNonNull(ns, import.it, import.@namespace);

         IXmlLineInfo lineInfo = (IXmlLineInfo)nav;

         this.Namespaces.Add(new ParsedValue<string>(ns, this.PhysicalPath.LocalPath, lineInfo.LineNumber));
      }

      protected void ParseDeclarations() {

         XPathNavigator nav = this.Navigator;
         
         bool movedToChildren = nav.MoveToFirstChild();

         for (bool moved = movedToChildren; moved; moved = nav.MoveToNext()) {

            if (nav.LocalName == "param" && nav.NamespaceURI == WellKnownNamespaces.XSLT) {
               ParseParameter();
            
            } else if ((nav.LocalName == "import" || nav.LocalName == "include") && nav.NamespaceURI == WellKnownNamespaces.XSLT) {

               ParseImportDeclaration(this.VirtualPath);
            }
         }

         if (movedToChildren) {
            nav.MoveToParent();
         }
      }

      protected void ParseImportDeclaration(string importingVirtualPath) {

         XPathNavigator nav = this.Navigator;

         string href = nav.GetAttribute("href", "");

         Uri uri = null;

         try {
            uri = new Uri(href, UriKind.RelativeOrAbsolute);
         } catch (UriFormatException) { }

         if (uri != null && !uri.IsAbsoluteUri) {
            string virtualPath = null;

            try {
               virtualPath = VirtualPathUtility.Combine(importingVirtualPath, href);
            } catch (HttpException) { }

            if (virtualPath != null) {

               string appRelVirtualPath = VirtualPathUtility.ToAppRelative(virtualPath);

               bool isInCodeDir = appRelVirtualPath.Remove(0, 2)
                  .Split('/')[0]
                  .Equals("App_Code", StringComparison.OrdinalIgnoreCase);

               if (!isInCodeDir 
                  && !this.SourceDependencies.Contains(virtualPath)) {

                  this.SourceDependencies.Add(virtualPath);
               }

               if (!visitedDocs.Contains(virtualPath)) {
                  visitedDocs.Add(virtualPath);
                  this.Navigator = OpenDocument(virtualPath).CreateNavigator();
                  ParseNestedImportDeclarations(virtualPath);
                  this.Navigator = nav;
               }
            }
         }
      }

      protected void ParseNestedImportDeclarations(string importingVirtualPath) {

         XPathNavigator nav = this.Navigator;

         if (nav.MoveToChild(XPathNodeType.Element)) {

            for (bool moved = nav.MoveToChild(XPathNodeType.Element); moved; moved = nav.MoveToNext(XPathNodeType.Element)) {

               if (nav.LocalName == "param" && nav.NamespaceURI == WellKnownNamespaces.XSLT) {
                  ParseParameter();

               } else if ((nav.LocalName == "import" || nav.LocalName == "include") && nav.NamespaceURI == WellKnownNamespaces.XSLT) {
                  ParseImportDeclaration(importingVirtualPath);
               }
            }
         }
      }

      protected void ParseParameter() {

         XPathNavigator nav = this.Navigator;

         string nameValue = nav.GetAttribute("name", "");

         if (this.Parameters.Contains(nameValue)) {
            return;
         }

         PageParameterInfo paramInfo = null;

         if (nav.MoveToAttribute("bind", WebModule.Namespace)) {

            var exprBuilderContext = new BindingExpressionContext(this, nav.Clone());

            BindingExpressionInfo exprInfo = null;

            try {
               exprInfo = BindingExpressionBuilder.ParseExpr(nav.Value, exprBuilderContext);

            } catch (Exception ex) {
               throw CreateParseException(ex.Message);
            }

            if (exprInfo != null) {
               exprInfo.LineNumber = ((IXmlLineInfo)nav).LineNumber;
            }

            nav.MoveToParent();

            IDictionary<string, string> namespacesInScope = nav.GetNamespacesInScope(XmlNamespaceScope.All);

            bool hasDefaultValue = !String.IsNullOrEmpty(nav.GetAttribute("select", ""));
            string asValue = nav.GetAttribute("as", "");
            bool required = nav.GetAttribute("required", "") == "yes";

            paramInfo = PageParameterInfo.FromSequenceType(nameValue, asValue, namespacesInScope);

            if (hasDefaultValue) {
               if (paramInfo.MinLength > 0) {
                  paramInfo.MinLength = 0;
               }

            } else if (required) {

               if (paramInfo.MinLength == 0) {
                  paramInfo.MinLength = 1;
               }
            }

            paramInfo.Binding = exprInfo;
         }

         if (paramInfo != null) {
            this.Parameters.Add(paramInfo);
         }
      }

      protected override Exception CreateParseException(string format, params object[] args) {
         return CreateParseException(this.Navigator as IXmlLineInfo, format, args);
      }

      protected void EnsureNonNull(object value, string procInstName, string name) {

         if (value == null) {
            throw CreateParseException("The '{0}' processing instruction is missing a '{1}' attribute.", procInstName, name);
         }
      }

      internal struct page {
         public const string it = "page";

         public const string language = "language";
         public const string class_name = "class-name";
         public const string validate_request = "validate-request";
         public const string enable_session_state = "enable-session-state";
         public const string content_type = "content-type";
         public const string accept_verbs = "accept-verbs";

         public const string processor = "processor";
         public const string initial_template = "initial-template";
         public const string bind_initial_template = "bind-initial-template";
      }

      struct output_cache {
         public const string it = "output-cache";

         public const string duration = "duration";
         public const string vary_by_param = "vary-by-param";
         public const string location = "location";
         public const string no_store = "no-store";
         public const string cache_profile = "cache-profile";
         public const string vary_by_header = "vary-by-header";
         public const string vary_by_custom = "vary-by-custom";
         public const string vary_by_content_encodings = "vary-by-content-encodings";
      }

      struct reference {
         public const string it = "reference";
         public const string href = "href";
      }

      struct import {
         public const string it = "import";
         public const string @namespace = "namespace";
      }

      struct xml_stylesheet {
         public const string it = "xml-stylesheet";

         public const string href = "href";
      }
   }
}
