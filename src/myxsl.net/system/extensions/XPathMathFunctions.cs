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
using System.Linq;
using System.Text;

namespace myxsl.xml.xsl.extensions {

   public class XPathMathFunctions {

      internal const string Namespace = "http://www.w3.org/2005/xpath-functions/math";

      public object acos(object arg) {
         // math:acos($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Acos(value.Value);
      }

      public object asin(object arg) {
         // math:asin($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Asin(value.Value);
      }

      public object atan(object arg) {
         // math:atan($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Atan(value.Value);
      }

      public object cos(object arg) {
         // math:cos($θ as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Cos(value.Value);
      }

      public object exp(object arg) {
         // math:exp($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Exp(value.Value);
      }

      public object exp10(object arg) {
         // math:exp10($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Pow(10, value.Value);
      }

      public object log(object arg) {
         // math:log($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Log(value.Value);
      }

      public object log10(object arg) {
         // math:log10($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Log10(value.Value);
      }

      public double pi() {
         // math:pi() as xs:double
         return Math.PI;
      }

      public object pow(object x, double y) {
         // math:pow($x as xs:double?, $y as numeric) as xs:double?

         double? xVal = ExtensionObjectConvert.ToNullableDouble(x);

         if (xVal == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Pow(xVal.Value, y);
      }

      public object sin(object arg) {
         // math:sin($θ as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Sin(value.Value);
      }

      public object sqrt(object arg) {
         // math:sqrt($arg as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Sqrt(value.Value);
      }

      public object tan(object arg) {
         // math:tan($θ as xs:double?) as xs:double?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return Math.Tan(value.Value);
      }
   }
}
