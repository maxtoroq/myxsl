// Copyright 2012 Max Toro Q.
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
using System.Xml.XPath;
using System.Xml.Xsl.Runtime;

namespace myxsl.net.system.extensions {
   
   public class XmlSchemaConstructorFunctions {

      internal const string Namespace = WellKnownNamespaces.XMLSchema;
      internal const string DateFormat = "yyyy-MM-ddzzzzzz";
      internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.ssssssszzzzzz";
      internal const string TimeFormat = "HH:mm:ss.ssssssszzzzzz";

      public object boolean(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToBoolean(arg.Current.Value);
      }

      public object date(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToString(XmlConvert.ToDateTimeOffset(arg.Current.Value), DateFormat);
      }

      public object dateTime(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToString(XmlConvert.ToDateTimeOffset(arg.Current.Value), DateTimeFormat);
      }

      public object @decimal(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToDecimal(arg.Current.Value);
      }

      public object @double(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToDouble(arg.Current.Value);
      }

      public object @duration(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();
         
         return XmlConvert.ToString(XmlConvert.ToTimeSpan(arg.Current.Value));
      }

      public object @float(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();
         
         return XmlConvert.ToSingle(arg.Current.Value);
      }

      public object @integer(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return Decimal.Parse(arg.Current.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
      }

      public object @string(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return arg.Current.Value;
      }

      public object time(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         return XmlConvert.ToString(XmlConvert.ToDateTimeOffset(arg.Current.Value), TimeFormat);
      }
   }
}
