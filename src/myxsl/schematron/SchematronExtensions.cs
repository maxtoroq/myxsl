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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using myxsl.common;

namespace myxsl.schematron {

   public static class SchematronExtensions {

      public static void BuildSchematronValidatorStylesheet(this IXsltProcessor processor, IXPathNavigable schemaDoc, XmlWriter output) {

         if (processor == null) throw new ArgumentNullException("processor");
         if (schemaDoc == null) throw new ArgumentNullException("schemaDoc");
         if (output == null) throw new ArgumentNullException("output");

         XPathNavigator nav = schemaDoc.CreateNavigator();

         if (nav.NodeType != XPathNodeType.Root) {
            throw new ArgumentException("The schema must be a document node.", "schemaDoc");
         }

         string queryBinding = nav.GetAttribute("queryBinding", "");
         decimal procXsltVersion = processor.GetXsltVersion();
         
         string xsltVersion;

         if (String.IsNullOrEmpty(queryBinding)) {

            int maxMajorVersion = (procXsltVersion >= 3m) ? 2 
               : (int)Decimal.Floor(procXsltVersion);

            xsltVersion = "xslt" + maxMajorVersion.ToStringInvariant();

         } else {

            string qbLower = queryBinding.ToLowerInvariant();

            switch (qbLower) {
               case "xslt":
               case "xslt1":
               case "xpath":
               case "xpath1":
                  xsltVersion = "xslt1";
                  break;

               case "xslt2":
               case "xpath2":

                  if (procXsltVersion < 2) {
                     throw new ArgumentException(
                        "The queryBinding '{0}' is not supported by this processor. Lower the language version or use a different processor.".FormatInvariant(queryBinding),
                        "schemaDoc"
                     );
                  }

                  xsltVersion = "xslt2";
                  break;
               
               default:
                  throw new ArgumentException(
                     "The queryBinding '{0}' is not supported. Valid values are: {1}.".FormatInvariant(queryBinding, String.Join(", ", GetQueryBindings())), 
                     "schemaDoc"
                  );
            }
         }

         Assembly assembly = Assembly.GetExecutingAssembly();
         
         Uri baseUri = new UriBuilder {
            Scheme = XmlEmbeddedResourceResolver.UriSchemeClires,
            Host = null,
            Path = String.Concat(assembly.GetName().Name, "/schematron/", xsltVersion, "/")
         }.Uri;

         var compileOptions = new XsltCompileOptions(baseUri);

         string[] stages = { "iso_dsdl_include.xsl", "iso_abstract_expand.xsl", String.Concat("iso_svrl_for_", xsltVersion, ".xsl") };

         IXPathNavigable input = schemaDoc;

         for (int i = 0; i < stages.Length; i++) {

            var stageUri = new Uri(baseUri, stages[i]);

            using (var stageDoc = (Stream)compileOptions.XmlResolver.GetEntity(stageUri, null, typeof(Stream))) {

               XsltExecutable executable = processor.Compile(stageDoc, compileOptions);

               var runtimeOptions = new XsltRuntimeOptions { 
                  InitialContextNode = input,
                  InputXmlResolver = compileOptions.XmlResolver
               };

               if (i < stages.Length - 1) {
                  // output becomes the input for the next stage
                  input = executable.Run(runtimeOptions);
               } else {
                  // last stage is output to writer
                  executable.Run(output, runtimeOptions);
               }
            }
         }
      }

      public static SchematronValidator CreateSchematronValidator(this IXsltProcessor processor, IXPathNavigable schemaDoc) {

         if (processor == null) throw new ArgumentNullException("processor");
         if (schemaDoc == null) throw new ArgumentNullException("schemaDoc");

         IXPathNavigable stylesheetDoc = processor.ItemFactory.BuildNode();

         XmlWriter builder = stylesheetDoc.CreateNavigator().AppendChild();
         
         processor.BuildSchematronValidatorStylesheet(schemaDoc, builder);

         builder.Close();

         var compileOptions = new XsltCompileOptions();

         XPathNavigator schemaNav = schemaDoc.CreateNavigator();

         if (!String.IsNullOrEmpty(schemaNav.BaseURI)) {
            compileOptions.BaseUri = new Uri(schemaNav.BaseURI);
         }

         return new XsltSchematronValidator(processor.Compile(stylesheetDoc, compileOptions));
      }

      internal static string[] GetQueryBindings() { 
         return new[] { "xslt", "xslt1", "xslt2", "xpath", "xpath1", "xpath2" };
      }
   }
}
