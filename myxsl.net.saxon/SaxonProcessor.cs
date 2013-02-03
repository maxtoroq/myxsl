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
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;
using Saxon.Api;
using XsltExecutable = myxsl.net.common.XsltExecutable;
using XQueryExecutable = myxsl.net.common.XQueryExecutable;
using SaxonApiXsltExecutable = Saxon.Api.XsltExecutable;
using SaxonApiXQueryExecutable = Saxon.Api.XQueryExecutable;
using System.Globalization;

namespace myxsl.net.saxon {

   public sealed class SaxonProcessor : IXsltProcessor, IXQueryProcessor {

      readonly Processor processor;
      readonly SaxonItemFactory _ItemFactory;

      public SaxonItemFactory ItemFactory { get { return _ItemFactory; } }
      XPathItemFactory IXsltProcessor.ItemFactory { get { return this.ItemFactory; } }
      XPathItemFactory IXQueryProcessor.ItemFactory { get { return this.ItemFactory; } }

      public SaxonProcessor() {

         processor = new Processor();
         _ItemFactory = new SaxonItemFactory(processor);
         RegisterExtensionFunctions(processor, _ItemFactory);
      }

      void RegisterExtensionFunctions(Processor processor, SaxonItemFactory itemFactory) {

         ExtensionFunctionDefinition[] precompiledFunctions = 
            extensions.exslt.common.Index.GetFunctions()
            .Concat(extensions.w3c.xpath.math.Index.GetFunctions())
            .Concat(extensions.saxon.Index.GetFunctions(itemFactory))
            .ToArray();

         bool[] funcAvailable = FunctionsAvailable(precompiledFunctions.Select(d => d.FunctionName).ToArray(), processor, itemFactory);
         
         for (int i = 0; i < precompiledFunctions.Length; i++) {
            if (!funcAvailable[i]) 
               processor.RegisterExtensionFunction(precompiledFunctions[i]);
         }
         
         Type itemFactoryType = itemFactory.GetType();

         var fnGen = new IntegratedExtensionFunctionGenerator();

         foreach (Type t in fnGen.Generate(XPathModules.Modules)) {

            ConstructorInfo ctor = t.GetConstructors().First();
            
            object[] args = ctor.GetParameters().Select(p => 
               p.ParameterType.IsAssignableFrom(typeof(SaxonProcessor)) ? (object)this 
               : p.ParameterType.IsAssignableFrom(itemFactoryType) ? (object)itemFactory 
               : null
            ).ToArray();

            var def = (ExtensionFunctionDefinition)ctor.Invoke(args);

            processor.RegisterExtensionFunction(def);
         }
      }

      static bool[] FunctionsAvailable(QName[] names, Processor processor, XPathItemFactory itemFactory) {

         const string xsltNs = "http://www.w3.org/1999/XSL/Transform";

         IXPathNavigable stylesheetDoc = itemFactory.CreateNodeEditable();
         XmlWriter builder = stylesheetDoc.CreateNavigator().AppendChild();

         builder.WriteStartElement("stylesheet", xsltNs);
         builder.WriteAttributeString("version", "2.0");

         for (int i = 0; i < names.Length; i++) {
            QName item = names[i];

            builder.WriteAttributeString("xmlns", "p" + i.ToStringInvariant(), null, item.Uri);
         }

         builder.WriteStartElement("output", xsltNs);
         builder.WriteAttributeString("method", "text");
         builder.WriteEndElement();

         builder.WriteStartElement("template", xsltNs);
         builder.WriteAttributeString("name", "main");

         for (int i = 0; i < names.Length; i++) {
            QName item = names[i];

            if (i > 0)
               builder.WriteElementString("text", xsltNs, "|");

            builder.WriteStartElement("value-of", xsltNs);
            builder.WriteAttributeString("select", "function-available('{0}:{1}')".FormatInvariant("p" + i.ToStringInvariant(), item.LocalName));
            builder.WriteEndElement();
         }

         builder.WriteEndElement(); // template
         builder.WriteEndElement(); // stylesheet

         builder.Close();

         XsltCompiler compiler = processor.NewXsltCompiler();
         compiler.BaseUri = new Uri("foo:bar");
         compiler.XmlResolver = null;

         XsltTransformer transform = compiler.Compile(stylesheetDoc.CreateNavigator().ReadSubtree()).Load();

         transform.InitialTemplate = new QName("main");

         using (var output = new StringWriter(CultureInfo.InvariantCulture)) {
            
            var serializer = new Serializer();
            serializer.SetOutputWriter(output);

            transform.Run(serializer);

            return output.ToString().Trim().Split('|').Select(s => XmlConvert.ToBoolean(s)).ToArray();
         }
      }

      XsltCompiler CreateCompiler(XsltCompileOptions options) {

         XsltCompiler compiler = this.processor.NewXsltCompiler();
         compiler.ErrorList = new ArrayList();
         compiler.BaseUri = options.BaseUri;

         if (options.XmlResolver != null)
            compiler.XmlResolver = options.XmlResolver;

         return compiler;
      }

      public XsltExecutable Compile(Stream module, XsltCompileOptions options) {

         XsltCompiler compiler = CreateCompiler(options);

         try {
            return WrapExecutable(compiler.Compile(module), options, default(Uri));
         } catch (Exception ex) {
            throw WrapCompileException(ex, compiler);
         }
      }

      public XsltExecutable Compile(TextReader module, XsltCompileOptions options) {

         XsltCompiler compiler = CreateCompiler(options);

         try {
            return WrapExecutable(compiler.Compile(module), options, default(Uri));
         } catch (Exception ex) {
            throw WrapCompileException(ex, compiler);
         }
      }

      public XsltExecutable Compile(XmlReader module, XsltCompileOptions options) {

         XsltCompiler compiler = CreateCompiler(options);

         string baseUri = module.BaseURI;

         try {
            return WrapExecutable(compiler.Compile(module), options, baseUri);
         } catch (Exception ex) {
            throw WrapCompileException(ex, compiler);
         }
      }

      public XsltExecutable Compile(IXPathNavigable module, XsltCompileOptions options) {

         XsltCompiler compiler = CreateCompiler(options);

         XPathNavigator nav = module.CreateNavigator();
         XdmNode node;

         if (SaxonExtensions.TryGetXdmNode(nav, out node)) {
            Uri baseUri = node.BaseUri;
            
            try {
               return WrapExecutable(compiler.Compile(node), options, baseUri);
            } catch (Exception ex) {
               throw WrapCompileException(ex, compiler);
            }
         } else {
            return Compile(nav.ReadSubtree(), options);
         }
      }

      static ProcessorException WrapCompileException(Exception ex, XsltCompiler compiler) {
         return WrapCompileException(ex, compiler.ErrorList);
      }

      XsltExecutable WrapExecutable(SaxonApiXsltExecutable xsltExecutable, XsltCompileOptions options, string baseUri) {

         Uri parsedBaseUri = null;

         if (!String.IsNullOrEmpty(baseUri)) {
            try {
               parsedBaseUri = new Uri(baseUri);
            } catch (UriFormatException) { }
         }

         return WrapExecutable(xsltExecutable, options, parsedBaseUri);
      }

      XsltExecutable WrapExecutable(SaxonApiXsltExecutable xsltExecutable, XsltCompileOptions options, Uri baseUri) {
         return new SaxonXsltExecutable(xsltExecutable, this, baseUri ?? options.BaseUri);
      }

      XQueryCompiler CreateCompiler(XQueryCompileOptions options) {

         XQueryCompiler compiler = this.processor.NewXQueryCompiler();
         compiler.ErrorList = new ArrayList();
         compiler.BaseUri = options.BaseUri.AbsoluteUri;

         foreach (XPathModuleInfo item in XPathModules.Modules.Where(m => m.Predeclare)) 
            compiler.DeclareNamespace(item.PredeclarePrefix, item.Namespace);

         return compiler;
      }

      public XQueryExecutable Compile(Stream module, XQueryCompileOptions options) {

         XQueryCompiler compiler = CreateCompiler(options);

         try {
            return WrapExecutable(compiler.Compile(module), options);
         } catch (Exception ex) {
            throw WrapCompileException(ex, compiler);
         }
      }

      public XQueryExecutable Compile(TextReader module, XQueryCompileOptions options) {

         XQueryCompiler compiler = CreateCompiler(options);

         try {
            return WrapExecutable(compiler.Compile(module.ReadToEnd()), options);
         } catch (Exception ex) {
            throw WrapCompileException(ex, compiler);
         }
      }

      XQueryExecutable WrapExecutable(SaxonApiXQueryExecutable xqueryExecutable, XQueryCompileOptions options) {
         return new SaxonXQueryExecutable(xqueryExecutable, this, options.BaseUri);
      }

      static ProcessorException WrapCompileException(Exception ex, XQueryCompiler compiler) {
         return WrapCompileException(ex, compiler.ErrorList);
      }

      static ProcessorException WrapCompileException(Exception ex, IList errors) {

         StaticError err =
            errors.OfType<StaticError>().FirstOrDefault(s => !s.IsWarning)
            ?? ex as StaticError;
         
         if (err != null)
            return new SaxonException(err);

         return new SaxonException(ex.Message, ex);
      }
   }
}
