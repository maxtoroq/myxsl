// Copyright 2013 Max Toro Q.
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net {

   /// <summary>
   /// Provides functions for compiling and executing XSLT stylesheets.
   /// </summary>
   [XPathModule("xslt", Namespace)]
   public class XsltModule {

      const string Namespace = "http://myxsl.net/ns/xslt";

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathDependency]
      public IXsltProcessor CurrentXsltProcessor { get; set; }

      [XPathFunction("compile", "item()", "item()")]
      public XPathItem Compile(XPathItem stylesheet) {
         return Compile(stylesheet, null);
      }

      /// <summary>
      /// Compiles the provided XSLT <paramref name="stylesheet"/>. Returns an <code>item()</code> to be used
      /// as the first argument of the <see cref="Transform(XPathItem, XPathNavigator)"/> and 
      /// <see cref="TransformStartingAt(XPathItem, XPathItem, XPathNavigator)"/> functions.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="stylesheet"/> parameter identifies an XSLT stylesheet, and can be provided in two ways:
      /// 1. A URI, as <code>xs:string</code> or <code>xs:anyURI</code>.
      /// 2. A <code>node()</code> that represents a literal (or inline) XSLT stylesheet.
      /// </para>
      /// <para>
      /// The single-argument version of this function has the same effect as the two-argument version called with <paramref name="processor"/> set to an empty sequence.
      /// </para>
      /// <para>
      /// The <paramref name="processor"/> parameter identifies the XSLT processor to use for the compilation.
      /// If ommited, the processor used depends on the <paramref name="stylesheet"/> parameter. If <paramref name="stylesheet"/>
      /// is a <code>node()</code>, and the current executing program is an XSLT processor, then the same processor of the
      /// current executing program is used to compile the <paramref name="stylesheet"/>. In all other cases,
      /// the default XSLT processor, specified in (App|Web).config by the <code>myxsl.net/xslt/@defaultProcessor</code> attribute,
      /// is used.
      /// </para>
      /// <para>
      /// <strong>It is not necessary to call this function to execute an XSLT stylesheet</strong>, you can call the
      /// <see cref="Transform(XPathItem, XPathNavigator)"/> or <see cref="TransformStartingAt(XPathItem, XPathItem, XPathNavigator)"/> functions directly. 
      /// However, it is more efficient to use this function if <paramref name="stylesheet"/> is a <code>node()</code>
      /// and you want to execute the stylesheet more than once.
      /// </para>
      /// <para>
      /// This function is also useful if you want to execute a stylesheet using a different processor than
      /// the default selected as previously explained. Or, if you simply want to check if the
      /// compilation suceeds.
      /// </para>
      /// <para>If the compilation of <paramref name="stylesheet"/> fails an error is raised.</para>
      /// <para>The return value of this function may vary between versions, so don't take dependencies on it.</para>
      /// </remarks>
      [XPathFunction("compile", "item()", "item()", "xs:string?")]
      public XPathItem Compile(XPathItem stylesheet, string processor) {

         CompiledStylesheetReference reference;

         if (stylesheet.IsNode) {

            IXsltProcessor proc = (processor != null) ?
               Processors.Xslt[processor]
               : this.CurrentXsltProcessor ?? Processors.Xslt.DefaultProcessor;
            
            int hashCode;

            XsltInvoker.With((XPathNavigator)stylesheet, proc, null, out hashCode);
            
            if (processor == null)
               return this.ItemFactory.CreateAtomicValue(hashCode, XmlTypeCode.Integer);

            reference = new CompiledStylesheetReference {
               HashCode = hashCode,
               Processor = processor
            };

         } else {

            Uri stylesheetUri = StylesheetAsUri(stylesheet);

            if (processor == null
               || processor == Processors.Xslt.DefaultProcessorName) {

               return this.ItemFactory.CreateAtomicValue(stylesheetUri.ToString(), XmlTypeCode.String);
            }

            XsltInvoker.With(stylesheetUri, processor);

            reference = new CompiledStylesheetReference { 
               Uri = stylesheetUri.AbsoluteUri,
               Processor = processor
            };
         }

         return this.ItemFactory
            .CreateDocument(reference)
            .CreateNavigator();
      }

      [XPathFunction("transform", "document-node()", "item()", "node()")]
      public XPathNavigator Transform(XPathItem stylesheet, XPathNavigator input) {
         return Transform(stylesheet, input, null);
      }

      [XPathFunction("transform", "document-node()", "item()", "node()", "node()*")]
      public XPathNavigator Transform(XPathItem stylesheet, XPathNavigator input, IEnumerable<XPathNavigator> parameters) {
         return Transform(stylesheet, input, parameters, null);
      }

      /// <summary>
      /// Executes the provided XSLT <paramref name="stylesheet"/> using <paramref name="input"/> as initial context node.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="stylesheet"/> parameter identifies an XSLT stylesheet, and can be provided in three ways:
      /// 1. A URI, as <code>xs:string</code> or <code>xs:anyURI</code>.
      /// 2. A <code>node()</code> that represents a literal (or inline) XSLT stylesheet.
      /// 3. The result of previously calling the <see cref="Compile(XPathItem)"/> function.
      /// </para>
      /// <para>
      /// The <paramref name="input"/> parameter is used as initial context node.
      /// </para>
      /// <para>
      /// The <paramref name="parameters"/> parameter is used to provide stylesheet parameters. The name of the <code>node()</code>
      /// is used as the name of the parameter, and its typed value is used as the value of the parameter.
      /// </para>
      /// <para>
      /// The <paramref name="mode"/> parameter is used as the initial mode. It can be provided as a <code>xs:QName</code>,
      /// or as a <code>xs:string</code> which is used as the local part to create a <code>xs:QName</code> in the null namespace.
      /// </para>
      /// </remarks>
      [XPathFunction("transform", "document-node()", "item()", "node()", "node()*", "item()?")]
      public XPathNavigator Transform(XPathItem stylesheet, XPathNavigator input, IEnumerable<XPathNavigator> parameters, XPathItem mode) {

         XsltRuntimeOptions options = GetRuntimeOptions(input, parameters, null, mode);

         return ExecuteStylesheet(stylesheet, options);
      }

      [XPathFunction("transform-starting-at", "document-node()", "item()", "item()")]
      public XPathNavigator TransformStartingAt(XPathItem stylesheet, XPathItem initialTemplate) {
         return TransformStartingAt(stylesheet, initialTemplate, null);
      }

      [XPathFunction("transform-starting-at", "document-node()", "item()", "item()", "node()?")]
      public XPathNavigator TransformStartingAt(XPathItem stylesheet, XPathItem initialTemplate, XPathNavigator input) {
         return TransformStartingAt(stylesheet, initialTemplate, input, null);
      }

      /// <summary>
      /// Executes the provided XSLT <paramref name="stylesheet"/> using <paramref name="initialTemplate"/> as 
      /// the initial named template.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="stylesheet"/> parameter identifies an XSLT stylesheet, and can be provided in three ways:
      /// 1. A URI, as <code>xs:string</code> or <code>xs:anyURI</code>.
      /// 2. A <code>node()</code> that represents a literal (or inline) XSLT stylesheet.
      /// 3. The result of previously calling the <see cref="Compile(XPathItem)"/> function.
      /// </para>
      /// <para>
      /// The <paramref name="initialTemplate"/> parameter is used as the initial named template. It can be provided as a <code>xs:QName</code>,
      /// or as a <code>xs:string</code> which is used as the local part to create a <code>xs:QName</code> in the null namespace.
      /// </para>
      /// <para>
      /// The <paramref name="input"/> parameter is used as initial context node.
      /// </para>
      /// <para>
      /// The <paramref name="parameters"/> parameter is used to provide stylesheet parameters. The name of the <code>node()</code>
      /// is used as the name of the parameter, and its typed value is used as the value of the parameter.
      /// </para>
      /// </remarks>
      [XPathFunction("transform-starting-at", "document-node()", "item()", "item()", "node()?", "node()*")]
      public XPathNavigator TransformStartingAt(XPathItem stylesheet, XPathItem initialTemplate, XPathNavigator input, IEnumerable<XPathNavigator> parameters) {

         XsltRuntimeOptions options = GetRuntimeOptions(input, parameters, initialTemplate, null);

         return ExecuteStylesheet(stylesheet, options);
      }

      XsltRuntimeOptions GetRuntimeOptions(XPathNavigator input, IEnumerable<XPathNavigator> parameters, XPathItem initialTemplate, XPathItem mode) {

         var options = new XsltRuntimeOptions();

         if (input != null)
            options.InitialContextNode = input;

         if (parameters != null) {
            foreach (XPathNavigator n in parameters)
               options.Parameters.Add(new XmlQualifiedName(n.Name, n.NamespaceURI), n.TypedValue);
         }

         if (initialTemplate != null) {

            XmlQualifiedName it = initialTemplate.TypedValue as XmlQualifiedName
               ?? new XmlQualifiedName(initialTemplate.Value);

            options.InitialTemplate = it;
         }

         if (mode != null) {

            XmlQualifiedName m = mode.TypedValue as XmlQualifiedName
               ?? new XmlQualifiedName(mode.Value);

            options.InitialMode = m;
         }

         return options;
      }

      XPathNavigator ExecuteStylesheet(XPathItem stylesheet, XsltRuntimeOptions options) {

         XsltInvoker invoker;

         IXsltProcessor currentOrDefaultProc = this.CurrentXsltProcessor ?? Processors.Xslt.DefaultProcessor;

         if (stylesheet.IsNode) {

            XPathNavigator node = ((XPathNavigator)stylesheet).Clone();

            if (node.NodeType == XPathNodeType.Root) 
               node.MoveToChild(XPathNodeType.Element);

            if (node.NodeType != XPathNodeType.Element)
               throw new ArgumentException("if stylesheet is a node() it must be either a document-node(element()) or an element() node.", "stylesheet");

            if (node.NamespaceURI == Namespace) {

               XmlSerializer serializer = XPathItemFactory.GetSerializer(typeof(CompiledStylesheetReference));

               var reference = (CompiledStylesheetReference)serializer.Deserialize(node.ReadSubtree());

               IXsltProcessor specifiedProcessor = (reference.Processor != null) ?
                  Processors.Xslt[reference.Processor]
                  : null;

               invoker = (reference.HashCode > 0) ? 
                  XsltInvoker.With(reference.HashCode, specifiedProcessor ?? currentOrDefaultProc)
                  : XsltInvoker.With(reference.Uri, specifiedProcessor);

            } else {
               invoker = XsltInvoker.With((XPathNavigator)stylesheet, currentOrDefaultProc);
            }

         } else {

            object value = stylesheet.TypedValue;

            if (value.GetType().IsPrimitive) {

               int hashCode = Convert.ToInt32(value, CultureInfo.InvariantCulture);

               invoker = XsltInvoker.With(hashCode, currentOrDefaultProc);
            
            } else {

               Uri stylesheetUri = StylesheetAsUri(stylesheet);
               
               invoker = XsltInvoker.With(stylesheetUri);
            }
         }

         return invoker.Transform(options)
            .Result()
            .CreateNavigator();
      }

      Uri StylesheetAsUri(XPathItem stylesheet) {

         Uri stylesheetUri = stylesheet.TypedValue as Uri;

         if (stylesheetUri == null){

            if (this.Resolver == null)
               throw new InvalidOperationException("Resolver cannot be null.");

            stylesheetUri = this.Resolver.ResolveUri(null, stylesheet.Value);
         }

         return stylesheetUri;
      }

      [XmlRoot(Namespace = Namespace)]
      public class CompiledStylesheetReference {

         [XmlAttribute]
         public string Processor;

         [XmlAttribute]
         public string Uri;

         [XmlAttribute]
         public int HashCode;
      }
   }
}
