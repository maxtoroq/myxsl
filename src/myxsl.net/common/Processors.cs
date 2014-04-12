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
using myxsl.net.configuration;

namespace myxsl.net.common {
   
   public static class Processors {

      static readonly Dictionary<string, object> instances = new Dictionary<string, object>();
      static readonly object padlock = new object();

      static Processors<object> _All;
      static Processors<IXsltProcessor> _Xslt;
      static Processors<IXQueryProcessor> _XQuery;

      public static Processors<object> All {
         get {
            if (_All == null) {
               lock (padlock) {
                  if (_All == null) {
                     _All = new Processors<object>(LibraryConfigSection.Instance.Processors, null);
                  }
               }
            }
            return _All;
         }
      }

      public static Processors<IXsltProcessor> Xslt {
         get {
            if (_Xslt == null) {
               lock (padlock) {
                  if (_Xslt == null) {
                     LibraryConfigSection config = LibraryConfigSection.Instance;
                     _Xslt = new Processors<IXsltProcessor>(config.Processors, config.Xslt.DefaultProcessor);
                  }
               }
            }
            return _Xslt;
         }
      }

      public static Processors<IXQueryProcessor> XQuery {
         get {
            if (_XQuery == null) {
               lock (padlock) {
                  if (_XQuery == null) {
                     LibraryConfigSection config = LibraryConfigSection.Instance;
                     _XQuery = new Processors<IXQueryProcessor>(config.Processors, config.XQuery.DefaultProcessor);
                  }
               }
            }
            return _XQuery;
         }
      }

      internal static object GetInstance(string name) {

         if (!instances.ContainsKey(name)) {

            lock (padlock) {

               if (!instances.ContainsKey(name)) {

                  ProcessorElementCollection config = LibraryConfigSection.Instance.Processors;
                  Type type = config.Get(name).TypeInternal;

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

      readonly ReadOnlyCollection<string> names;
      TProc _DefaultProcessor;
      string _DefaultProcessorName;

      public TProc DefaultProcessor {
         get {
            return _DefaultProcessor
               ?? (_DefaultProcessor = (TProc)Processors.GetInstance(DefaultProcessorName));
         }
      }

      public string DefaultProcessorName {
         get { return _DefaultProcessorName; }
      }

      public ReadOnlyCollection<string> Names {
         get { return this.names; }
      }

      public int Count {
         get { return this.names.Count; }
      }

      public TProc this[string name] {
         get {
            if (!Exists(name)) {
               throw new ArgumentException("The processor '{0}' is not registered.".FormatInvariant(name), "name");
            }
            
            return (TProc)Processors.GetInstance(name);
         }
      }

      internal Processors(ProcessorElementCollection config, string @default) {

         this.names = new ReadOnlyCollection<string>(
            (from p in config.Cast<ProcessorElement>()
             where typeof(TProc).IsAssignableFrom(p.TypeInternal)
             select p.Name).ToList()
         );

         this._DefaultProcessorName = @default;
      }

      public bool Exists(string name) {
         return this.Names.Contains(name);
      }
   }
}
