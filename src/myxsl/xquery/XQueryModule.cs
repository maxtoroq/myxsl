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
using myxsl.common;

namespace myxsl.xquery {

   /// <summary>
   /// Provides functions for compiling and executing XQuery modules.
   /// </summary>
   [XPathModule("xquery", Namespace)]
   public class XQueryModule {

      const string Namespace = "http://myxsl.github.io/ns/xquery";

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathDependency]
      public IXQueryProcessor CurrentXQueryProcessor { get; set; }

      [XPathFunction("compile-from", "xs:string", As = "item()")]
      public XPathItem CompileFrom(string moduleUri) {
         return CompileFrom(moduleUri, null);
      }

      /// <summary>
      /// Compiles the provided <paramref name="moduleUri"/>. Returns an <code>item()</code> to be used
      /// as the first argument of the <see cref="Eval(XPathItem)"/> function.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="moduleUri"/> parameter identifies an XQuery module.
      /// </para>
      /// <para>
      /// The single-argument version of this function has the same effect as the two-argument version called with <paramref name="processor"/> set to an empty sequence.
      /// </para>
      /// <para>
      /// The <paramref name="processor"/> parameter identifies the XQuery processor to use for the compilation.
      /// If ommited, the processor used is the default XQuery processor, specified in (App|Web).config by 
      /// the <code>myxsl/xquery/@defaultProcessor</code> attribute.
      /// </para>
      /// <para>
      /// <strong>It is not necessary to call this function to execute an XQuery module</strong>, you can call the
      /// <see cref="Invoke(string)"/> function directly. 
      /// </para>
      /// <para>
      /// This function is useful if you want to execute a module using a different processor than
      /// the default selected as previously explained. Or, if you simply want to check if the
      /// compilation suceeds.
      /// </para>
      /// <para>If the compilation of <paramref name="moduleUri"/> fails an error is raised.</para>
      /// <para>The return value of this function may vary between versions, so don't take dependencies on it.</para>
      /// </remarks>
      [XPathFunction("compile-from", "xs:string", "xs:string?", As = "item()")]
      public XPathItem CompileFrom(string moduleUri, string processor) { 
         
         IXQueryProcessor proc = (processor != null) ?
            Processors.XQuery[processor]
            : Processors.XQuery.DefaultProcessor;

         Uri uri = this.Resolver.ResolveUri(null, moduleUri);

         XQueryInvoker.With(uri, proc);

         var reference = new CompiledQueryReference { 
            Uri = uri.AbsoluteUri,
            Processor = processor
         };

         return this.ItemFactory.CreateDocument(reference)
            .CreateNavigator();
      }

      [XPathFunction("compile-query", "xs:string", As = "item()")]
      public XPathItem CompileQuery(string query) {
         return CompileQuery(query, null);
      }

      /// <summary>
      /// Compiles the provided <paramref name="query"/>. Returns an <code>item()</code> to be used
      /// as the first argument of the <see cref="Eval(XPathItem)"/> function.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="query"/> parameter represents a literal (or inline) XQuery module.
      /// </para>
      /// <para>
      /// The single-argument version of this function has the same effect as the two-argument version called with <paramref name="processor"/> set to an empty sequence.
      /// </para>
      /// <para>
      /// The <paramref name="processor"/> parameter identifies the XQuery processor to use for the compilation.
      /// If ommited, the processor used is the default XQuery processor, specified in (App|Web).config by 
      /// the <code>myxsl/xquery/@defaultProcessor</code> attribute.
      /// </para>
      /// <para>
      /// <strong>It is not necessary to call this function to execute an XQuery module</strong>, you can call the
      /// <see cref="Eval(XPathItem)"/> function directly. However, it is more efficient to use this function if
      /// you want to execute the query more than once.
      /// </para>
      /// <para>
      /// This function is also useful if you want to execute a module using a different processor than
      /// the default selected as previously explained. Or, if you simply want to check if the
      /// compilation suceeds.
      /// </para>
      /// <para>If the compilation of <paramref name="query"/> fails an error is raised.</para>
      /// <para>The return value of this function may vary between versions, so don't take dependencies on it.</para>
      /// </remarks>
      [XPathFunction("compile-query", "xs:string", "xs:string?", As = "item()")]
      public XPathItem CompileQuery(string query, string processor) {

         IXQueryProcessor proc = (processor != null) ?
            Processors.XQuery[processor]
            : this.CurrentXQueryProcessor ?? Processors.XQuery.DefaultProcessor;

         int hashCode;

         XQueryInvoker.WithQuery(query, proc, null, out hashCode);

         if (processor == null) {
            return this.ItemFactory.CreateAtomicValue(hashCode, XmlTypeCode.Integer);
         }

         var reference = new CompiledQueryReference { 
            HashCode = hashCode,
            Processor = processor
         };

         return this.ItemFactory.CreateDocument(reference)
            .CreateNavigator();
      }

      [XPathFunction("eval", "item()", As = "item()*")]
      public IEnumerable<XPathItem> Eval(XPathItem module) {
         return Eval(module, null);
      }

      [XPathFunction("eval", "item()", "item()?", As = "item()*")]
      public IEnumerable<XPathItem> Eval(XPathItem module, XPathItem input) {
         return Eval(module, input, null);
      }

      /// <summary>
      /// Evaluates the provided XQuery <paramref name="module"/>.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="module"/> parameter identifies an XQuery module, and can be provided in two ways:
      /// 1. A <code>xs:string</code> that represents a literal (or inline) XQuery module.
      /// 2. The result of previously calling the <see cref="CompileFrom(string)"/> or <see cref="CompileQuery(string)"/> functions.
      /// </para>
      /// <para>
      /// The <paramref name="input"/> parameter is used as context item.
      /// </para>
      /// <para>
      /// The <paramref name="parameters"/> parameter is used to provide external variables. The name of the <code>node()</code>
      /// is used as the name of the variable, and its typed value is used as the value of the variable.
      /// </para>
      /// </remarks>
      [XPathFunction("eval", "item()", "item()?", "node()*", As = "item()*")]
      public IEnumerable<XPathItem> Eval(XPathItem module, XPathItem input, IEnumerable<XPathNavigator> parameters) {

         XQueryInvoker invoker;

         IXQueryProcessor currentOrDefaultProc = this.CurrentXQueryProcessor ?? Processors.XQuery.DefaultProcessor;

         if (module.IsNode) {

            XPathNavigator node = ((XPathNavigator)module).Clone();

            if (node.NodeType == XPathNodeType.Root) {
               node.MoveToChild(XPathNodeType.Element);
            }

            if (node.NodeType == XPathNodeType.Element
               && node.NamespaceURI == Namespace) {

               XmlSerializer serializer = XPathItemFactory.GetSerializer(typeof(CompiledQueryReference));

               var reference = (CompiledQueryReference)serializer.Deserialize(node.ReadSubtree());

               IXQueryProcessor specifiedProcessor = (reference.Processor != null) ?
                  Processors.XQuery[reference.Processor]
                  : null;

               invoker = (reference.HashCode > 0) ?
                  XQueryInvoker.WithQuery(reference.HashCode, specifiedProcessor ?? currentOrDefaultProc)
                  : XQueryInvoker.With(reference.Uri, specifiedProcessor);

            } else {
               invoker = XQueryInvoker.WithQuery(module.Value, currentOrDefaultProc);
            }

         } else {

            object value = module.TypedValue;

            if (value.GetType().IsPrimitive) {

               int hashCode = Convert.ToInt32(value, CultureInfo.InvariantCulture);

               invoker = XQueryInvoker.WithQuery(hashCode, currentOrDefaultProc);

            } else {

               Uri moduleUri = ResolveUri(module);

               invoker = XQueryInvoker.WithQuery(module.Value, currentOrDefaultProc);
            }
         }

         XQueryRuntimeOptions options = GetRuntimeOptions(input, parameters);

         return invoker.Query(options).Result();
      }

      [XPathFunction("invoke", "xs:string", As = "item()*")]
      public IEnumerable<XPathItem> Invoke(string moduleUri) {
         return Invoke(moduleUri, null);
      }

      [XPathFunction("invoke", "xs:string", "item()?", As = "item()*")]
      public IEnumerable<XPathItem> Invoke(string moduleUri, XPathItem input) {
         return Invoke(moduleUri, input, null);
      }

      /// <summary>
      /// Evaluates the XQuery module identified by the provided <paramref name="moduleUri"/>.
      /// </summary>
      /// <remarks>
      /// <para>
      /// The <paramref name="moduleUri"/> parameter identifies an XQuery module.
      /// </para>
      /// <para>
      /// The <paramref name="input"/> parameter is used as context item.
      /// </para>
      /// <para>
      /// The <paramref name="parameters"/> parameter is used to provide external variables. The name of the <code>node()</code>
      /// is used as the name of the variable, and its typed value is used as the value of the variable.
      /// </para>
      /// </remarks>
      [XPathFunction("invoke", "xs:string", "item()?", "node()*", As = "item()*")]
      public IEnumerable<XPathItem> Invoke(string moduleUri, XPathItem input, IEnumerable<XPathNavigator> parameters) {

         XQueryRuntimeOptions options = GetRuntimeOptions(input, parameters);

         Uri uri = ResolveUri(moduleUri);

         return XQueryInvoker.With(uri)
            .Query(options)
            .Result();
      }

      XQueryRuntimeOptions GetRuntimeOptions(XPathItem input, IEnumerable<XPathNavigator> parameters) {

         var options = new XQueryRuntimeOptions();

         if (input != null) {
            options.ContextItem = input;
         }

         if (parameters != null) {

            foreach (XPathNavigator n in parameters) {
               options.ExternalVariables.Add(new XmlQualifiedName(n.Name, n.NamespaceURI), n.TypedValue);
            }
         }

         return options;
      }

      Uri ResolveUri(string relativeUri) {

         if (this.Resolver == null) {
            throw new InvalidOperationException("Resolver cannot be null.");
         }

         return this.Resolver.ResolveUri(null, relativeUri);
      }

      Uri ResolveUri(XPathItem relativeUri) {

         Uri uri = relativeUri.TypedValue as Uri;

         if (uri == null) {

            if (this.Resolver == null) {
               throw new InvalidOperationException("Resolver cannot be null.");
            }

            uri = this.Resolver.ResolveUri(null, relativeUri.Value);
         }

         return uri;
      }

      [XmlRoot(Namespace = Namespace)]
      public class CompiledQueryReference {

         [XmlAttribute]
         public string Processor;

         [XmlAttribute]
         public string Uri;

         [XmlAttribute]
         public int HashCode;
      }
   }
}
