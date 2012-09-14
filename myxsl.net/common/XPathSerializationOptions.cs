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
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace myxsl.net.common {
   
   public class XPathSerializationOptions {

      static readonly Action<XmlWriterSettings, XmlOutputMethod> setOutputMethod;
      static readonly Action<XmlWriterSettings, string> setDocTypePublic;
      static readonly Action<XmlWriterSettings, string> setDocTypeSystem;
      static readonly Action<XmlWriterSettings, string> setMediaType;
      static readonly Action<XmlWriterSettings, bool> setReadOnly;

      public bool? ByteOrderMark { get; set; }
      public string DocTypePublic { get; set; }
      public string DocTypeSystem { get; set; }
      public Encoding Encoding { get; set; }
      public bool? Indent { get; set; }
      public string MediaType { get; set; }
      public XmlQualifiedName Method { get; set; }
      public bool? OmitXmlDeclaration { get; set; }
      
      public ConformanceLevel ConformanceLevel { get; set; }

      static XPathSerializationOptions() {

         Type settingsType = typeof(XmlWriterSettings);

         setOutputMethod = (Action<XmlWriterSettings, XmlOutputMethod>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, XmlOutputMethod>), settingsType.GetProperty("OutputMethod", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true));
         setDocTypePublic = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("DocTypePublic", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setDocTypeSystem = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("DocTypeSystem", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setMediaType = (Action<XmlWriterSettings, string>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, string>), settingsType.GetProperty("MediaType", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
         setReadOnly = (Action<XmlWriterSettings, bool>)Delegate.CreateDelegate(typeof(Action<XmlWriterSettings, bool>), settingsType.GetProperty("ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true));
      }

      public static explicit operator XmlWriterSettings(XPathSerializationOptions options) {

         var settings = new XmlWriterSettings();

         CopyTo(options, settings);

         return settings;
      }

      static void CopyTo(XPathSerializationOptions options, XmlWriterSettings settings) {

         if (options == null) throw new ArgumentNullException("options");
         if (settings == null) throw new ArgumentNullException("settings");

         if (options.Method != null 
            && !options.Method.IsEmpty
            && options.Method != XPathSerializationMethods.Xml) {

            if (options.Method == XPathSerializationMethods.Html) {
               setOutputMethod(settings, XmlOutputMethod.Html);

            } else if (options.Method == XPathSerializationMethods.Text) {
               setOutputMethod(settings, XmlOutputMethod.Text);
            }
         }

         if (options.DocTypePublic != null)
            setDocTypePublic(settings, options.DocTypePublic);

         if (options.DocTypeSystem != null)
            setDocTypeSystem(settings, options.DocTypeSystem);

         if (options.Encoding != null)
            settings.Encoding = options.Encoding;

         if (options.Indent.HasValue)
            settings.Indent = options.Indent.Value;

         if (options.MediaType != null)
            setMediaType(settings, options.MediaType);

         if (options.OmitXmlDeclaration.HasValue)
            settings.OmitXmlDeclaration = options.OmitXmlDeclaration.Value;

         Encoding enc = settings.Encoding;

         if (options.ByteOrderMark.HasValue 
            && !options.ByteOrderMark.Value) {
            
            if (enc is UTF8Encoding)
               settings.Encoding = new UTF8Encoding(false);
         }

         settings.ConformanceLevel = options.ConformanceLevel;
      }

      public XPathSerializationOptions() {
         this.ConformanceLevel = ConformanceLevel.Auto;
      }

      public void CopyTo(XmlWriterSettings settings) {

         if (settings == null) throw new ArgumentNullException("settings");

         setReadOnly(settings, false);
         
         CopyTo(this, settings); 
      }
   }
}
