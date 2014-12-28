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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace myxsl.common {
   
   public static class Processors {

      internal static readonly Dictionary<string, Type> types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
      static readonly Dictionary<string, object> instances = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
      static readonly object padlock = new object();

      static readonly Processors<object> _All;
      static readonly Processors<IXsltProcessor> _Xslt;
      static readonly Processors<IXQueryProcessor> _XQuery;

      public static Processors<object> All {
         get { return _All; }
      }

      public static Processors<IXsltProcessor> Xslt {
         get { return _Xslt; }
      }

      public static Processors<IXQueryProcessor> XQuery {
         get { return _XQuery; }
      }

      static Processors() {

         Type sysProcType = Type.GetType("myxsl.xml.xsl.SystemXsltProcessor, myxsl.xml.xsl", throwOnError: false, ignoreCase: false);
         Type saxonProcType = Type.GetType("myxsl.saxon.SaxonProcessor, myxsl.saxon", throwOnError: false, ignoreCase: false);

         if (sysProcType != null) {
            RegisterProcessor("system", sysProcType);
         }

         if (saxonProcType != null) {
            RegisterProcessor("saxon", saxonProcType);
         }

         _All = new Processors<object>();
         _Xslt = new Processors<IXsltProcessor>();
         _XQuery = new Processors<IXQueryProcessor>();

         if (sysProcType != null) {
            _Xslt.Default = "system";
         }

         if (saxonProcType != null) {
            _Xslt.Default = "saxon";
            _XQuery.Default = "saxon";
         }
      }

      public static void RegisterProcessor(string name, Type type) {

         if (name == null) throw new ArgumentNullException("name");
         if (type == null) throw new ArgumentNullException("type");

         if (!typeof(IXsltProcessor).IsAssignableFrom(type)
            && !typeof(IXQueryProcessor).IsAssignableFrom(type)) {

            throw new ArgumentException(
               "The processor must implement {0} or {1}."
                  .FormatInvariant(typeof(IXsltProcessor).FullName, typeof(IXQueryProcessor).FullName)
               , "type");
         }

         lock (padlock) {

            types[name] = type;
            instances.Remove(name);

            if (_Xslt != null) {
               _Xslt.Reset();
            }

            if (_XQuery != null) {
               _XQuery.Reset();
            }
         }
      }

      internal static object GetOrCreateInstance(string name) {

         if (!instances.ContainsKey(name)) {

            lock (padlock) {

               if (!instances.ContainsKey(name)) {

                  Type type = types[name];

                  instances[name] = Expression.Lambda<Func<object>>(
                     Expression.Convert(Expression.New(type), typeof(object))
                  ).Compile()();
               }
            }
         }

         return instances[name];
      }
   }

   public sealed class Processors<TProc> where TProc : class {

      ReadOnlyCollection<string> names;
      TProc _DefaultProcessor;
      string _Default;

      public TProc DefaultProcessor {
         get {

            if (!Default.HasValue()) {
               throw new InvalidOperationException("Set DefaultProcessorName first.");
            }

            return _DefaultProcessor
               ?? (_DefaultProcessor = (TProc)Processors.GetOrCreateInstance(Default));
         }
      }

      public string Default {
         get { return _Default; }
         set {
            _Default = value;
            _DefaultProcessor = null;
         }
      }

      public ReadOnlyCollection<string> Names {
         get { return names; }
         private set {
            names = value;
            Default = null;
         }
      }

      public int Count {
         get { return Names.Count; }
      }

      public TProc this[string name] {
         get {

            if (!Exists(name)) {
               throw new ArgumentException("The processor '{0}' is not registered.".FormatInvariant(name), "name");
            }
            
            return (TProc)Processors.GetOrCreateInstance(name);
         }
      }

      internal Processors() {
         Reset();
      }

      internal void Reset() {

         this.Names = new ReadOnlyCollection<string>(
            Processors.types
               .Where(p => typeof(TProc).IsAssignableFrom(p.Value))
               .Select(p => p.Key)
               .ToArray());
      }

      public bool Exists(string name) {
         return this.Names.Contains(name, Processors.types.Comparer);
      }
   }
}
