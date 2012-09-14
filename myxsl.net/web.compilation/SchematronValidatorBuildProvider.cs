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
using System.Globalization;
using System.IO;
using System.Web.Compilation;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;
using myxsl.net.validation;
using myxsl.net.web.ui;

namespace myxsl.net.web.compilation {

   [BuildProviderAppliesTo(BuildProviderAppliesTo.All)]
   public class SchematronValidatorBuildProvider : BaseBuildProvider {

      Uri _ValidatorUri;

      protected Uri ValidatorUri {
         get {
            if (_ValidatorUri == null) {
               _ValidatorUri = new UriBuilder {
                  Scheme = XmlEmbeddedResourceResolver.UriSchemeClires,
                  Host = null,
                  Path = GeneratedTypeFullName + ".xsl"
               }.Uri;  
            }
            return _ValidatorUri;
         }
      }

      public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
         
         base.GenerateCode(assemblyBuilder);

         SchematronParser schParser = (SchematronParser)this.Parser;
         
         IXsltProcessor proc = Processors.Xslt[schParser.ProcessorName];
         
         IXPathNavigable schemaDoc;

         using (TextReader textReader = OpenReader()) 
            schemaDoc = proc.ItemFactory.CreateNodeReadOnly(textReader, new XmlParsingOptions { BaseUri = this.PhysicalPath }); 
         
         Stream resourceStream;

         try {
            resourceStream = assemblyBuilder.CreateEmbeddedResource(this, this.ValidatorUri.AbsolutePath);
         } catch (ArgumentException) {
            throw CreateCompileException(String.Format(CultureInfo.InvariantCulture, "There is already another type generated with the name '{0}'.", this.GeneratedTypeFullName));
         }

         using (resourceStream) {  
            using (var copyStream = new MemoryStream()) {
            
               try {
                  XmlWriter writer = XmlWriter.Create(copyStream);
                  proc.BuildSchematronValidatorStylesheet(schemaDoc, writer);

                  writer.Close();
               
               } catch (ProcessorException ex) {
                  throw CreateCompileException(ex);
               }
               
               copyStream.Position = 0;
               copyStream.WriteTo(resourceStream);

               // test compilation

               copyStream.Position = 0;
               
               try {
                  proc.Compile(copyStream, new XsltCompileOptions { BaseUri = this.PhysicalPath });
               } catch (ProcessorException ex) {
                  throw CreateCompileException(ex);
               }
            }
         }
      }

      protected override BaseParser CreateParser() {
         return new SchematronParser();
      }

      protected override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(BaseParser parser) {
         
         var codeDomGen = new SchematronValidatorCodeDomTreeGenerator((SchematronParser)parser);
         codeDomGen.ValidatorUri = this.ValidatorUri;

         return codeDomGen;
      }
   } 
}
