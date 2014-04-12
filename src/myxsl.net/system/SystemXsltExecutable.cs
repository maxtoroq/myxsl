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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using myxsl.net.common;

namespace myxsl.net.system {

   using MonoTransform = Action<XslCompiledTransform, XPathNavigator, XsltArgumentList, XmlWriter, XmlResolver>;
   using Net20Transform = Action<object, IXPathNavigable, XmlResolver, XsltArgumentList, XmlWriter>;
   
   public sealed class SystemXsltExecutable : XsltExecutable {

      static readonly MonoTransform monoTransform;
      static readonly Net20Transform net20Transform;
      static readonly FieldInfo commandField;
      static readonly object padlock = new object();
      static IDictionary<string, Tuple<XPathModuleInfo, Type>> _ExtensionObjects;
      readonly object command;

      readonly XslCompiledTransform transform;
      readonly SystemXsltProcessor _Processor;
      readonly Uri _StaticBaseUri;
      readonly bool possiblyXhtmlMethod;

      static IDictionary<string, Tuple<XPathModuleInfo, Type>> ExtensionObjects {
         get {
            if (_ExtensionObjects == null) {
               lock (padlock) {
                  if (_ExtensionObjects == null) {

                     _ExtensionObjects = new Dictionary<string, Tuple<XPathModuleInfo, Type>>();

                     string[] xpathNs = {
                        extensions.XPathFunctions.Namespace,
                        extensions.XPathMathFunctions.Namespace,
                        extensions.XmlSchemaConstructorFunctions.Namespace
                     };

                     Type[] xpathExtObj = ExtensionObjectGenerator.RenameMethodsIfNecessary(new[] {
                        typeof(extensions.XPathFunctions),
                        typeof(extensions.XPathMathFunctions),
                        typeof(extensions.XmlSchemaConstructorFunctions)
                     }).ToArray();
                     
                     for (int i = 0; i < xpathNs.Length; i++) 
                        _ExtensionObjects.Add(xpathNs[i], Tuple.Create((XPathModuleInfo)null, xpathExtObj[i]));

                     XPathModuleInfo[] modules = XPathModules.Modules.ToArray();
                     Type[] modulesExtObj = ExtensionObjectGenerator.Generate(modules);

                     for (int i = 0; i < modules.Length; i++)
                        _ExtensionObjects.Add(modules[i].Namespace, Tuple.Create(modules[i], modulesExtObj[i]));
                  }
               }
            }

            return _ExtensionObjects;
         }
      }

      public new SystemXsltProcessor Processor { get { return _Processor; } }
      protected override IXsltProcessor XsltProcessor { get { return this.Processor; } }

      public override Uri StaticBaseUri { get { return _StaticBaseUri; } }

      static SystemXsltExecutable() {

         if (CLR.IsMono) {
            monoTransform = (MonoTransform)Delegate.CreateDelegate(typeof(MonoTransform), typeof(XslCompiledTransform).GetMethod("Transform", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(XPathNavigator), typeof(XsltArgumentList), typeof(XmlWriter), typeof(XmlResolver) }, null));
         } else {

            commandField = typeof(XslCompiledTransform).GetField("command", BindingFlags.Instance | BindingFlags.NonPublic);

            ParameterExpression commandParam = Expression.Parameter(typeof(object), "instance");
            ParameterExpression inputParam = Expression.Parameter(typeof(IXPathNavigable), "input");
            ParameterExpression resolverParam = Expression.Parameter(typeof(XmlResolver), "resolver");
            ParameterExpression argumentsParam = Expression.Parameter(typeof(XsltArgumentList), "arguments");
            ParameterExpression writerParam = Expression.Parameter(typeof(XmlWriter), "writer");

            MethodInfo executeMethod = commandField.FieldType.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(IXPathNavigable), typeof(XmlResolver), typeof(XsltArgumentList), typeof(XmlWriter) }, null);
            MethodCallExpression executeMethodExpr = Expression.Call(Expression.Convert(commandParam, commandField.FieldType), executeMethod, inputParam, resolverParam, argumentsParam, writerParam);
            net20Transform = Expression.Lambda<Net20Transform>(executeMethodExpr, commandParam, inputParam, resolverParam, argumentsParam, writerParam).Compile();
         }
      }

      internal SystemXsltExecutable(XslCompiledTransform transform, SystemXsltProcessor processor, Uri staticBaseUri) {

         this.transform = transform;
         this._Processor = processor;
         this._StaticBaseUri = staticBaseUri;

         this.possiblyXhtmlMethod = this.transform.OutputSettings != null &&
            this.transform.OutputSettings.OutputMethod == XmlOutputMethod.AutoDetect;

         if (!CLR.IsMono) {
            this.command = commandField.GetValue(this.transform);
         }
      }

      public override void Run(Stream output, XsltRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         XmlWriterSettings settings = this.transform.OutputSettings;
         options.Serialization.CopyTo(settings);

         XmlWriter writer = XmlWriter.Create(output, settings);
         
         Run(writer, options);

         // NOTE: don't close writer if Run fails
         writer.Close();
      }

      public override void Run(TextWriter output, XsltRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         XmlWriterSettings settings = this.transform.OutputSettings;
         options.Serialization.CopyTo(settings);

         XmlWriter writer = XmlWriter.Create(output, settings);
         
         Run(writer, options);

         // NOTE: don't close writer if Run fails
         writer.Close();
      }

      public override void Run(XmlWriter output, XsltRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         if (this.possiblyXhtmlMethod
            || options.Serialization.Method == XPathSerializationMethods.XHtml) {

            output = new XHtmlWriter(output);
         }

         IXPathNavigable input;

         if (options.InitialContextNode != null) {
            input = options.InitialContextNode;

         } else {
            // this processor doesn't support initial template,
            // a node must be provided
            input = this.Processor.ItemFactory.CreateNodeReadOnly();
         }

         XsltArgumentList args = GetArguments(options);
         
         XmlResolver resolver = options.InputXmlResolver;
         XmlDynamicResolver dynamicResolver = resolver as XmlDynamicResolver;

         if (dynamicResolver != null
            && dynamicResolver.DefaultBaseUri == null) {
            
            dynamicResolver.DefaultBaseUri = this.StaticBaseUri;
         }

         try {
            
            if (CLR.IsMono) {
               monoTransform(this.transform, ((input != null) ? input.CreateNavigator() : null), args, output, resolver);
            } else {
               net20Transform(this.command, input, resolver, args, output);
            }

         } catch (XsltException ex) {
            throw new SystemXsltException(ex);
         }
      }

      public override IXPathNavigable Run(XsltRuntimeOptions options) {

         IXPathNavigable doc = this.Processor.ItemFactory.CreateNodeEditable();

         XmlWriter writer = doc.CreateNavigator().AppendChild();
         
         Run(writer, options);

         writer.Close();

         return doc;
      }

      XsltArgumentList GetArguments(XsltRuntimeOptions options) {

         var list = new XsltArgumentList();

         foreach (var item in options.Parameters) {

            object val = ExtensionObjectConvert.ToInputOrEmpty(item.Value);

            if (!ExtensionObjectConvert.IsEmpty(val)) {
               list.AddParam(item.Key.Name, item.Key.Namespace, val);
            }
         }

         foreach (var pair in ExtensionObjects) {
            list.AddExtensionObject(pair.Key, InitializeExtensionObject(pair.Value, options.InputXmlResolver));
         }

         list.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(args_XsltMessageEncountered);

         return list;
      }

      object InitializeExtensionObject(Tuple<XPathModuleInfo, Type> info, XmlResolver resolver) {

         object instance = Activator.CreateInstance(info.Item2);

         XPathModuleInfo moduleInfo = info.Item1;

         if (moduleInfo == null) {

            var xpathFn = instance as extensions.XPathFunctions;

            if (xpathFn != null) {
               xpathFn.resolver = resolver;
            }
         
         } else if (moduleInfo.Dependencies.Count > 0) {

            object[] args = new object[info.Item1.Dependencies.Count];

            for (int i = 0; i < info.Item1.Dependencies.Count; i++) {
               XPathDependencyInfo dependency = info.Item1.Dependencies[i];

               if (dependency.Type == typeof(XPathItemFactory)) {
                  args[i] = this.Processor.ItemFactory;
                  continue;
               }

               if (dependency.Type == typeof(XmlResolver)) {
                  args[i] = resolver;
                  continue;
               }

               if (dependency.Type == typeof(IXsltProcessor)) {
                  args[i] = this.Processor;
                  continue;
               }

               args[i] = dependency.Type.IsValueType ?
                  Activator.CreateInstance(dependency.Type)
                  : null;
            }

            info.Item2.GetMethod("Initialize").Invoke(instance, args);
         }

         return instance;
      }

      void args_XsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e) {
         Trace.WriteLine(e.Message);
      }
   }
}
