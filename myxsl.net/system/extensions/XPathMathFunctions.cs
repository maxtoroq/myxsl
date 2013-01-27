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

namespace myxsl.net.system {

   public class XPathMathFunctions {

      internal const string Namespace = "http://www.w3.org/2005/xpath-functions/math";

      public double acos(double arg) {
         return Math.Acos(arg);
      }

      public double asin(double arg) {
         return Math.Asin(arg);
      }

      public double atan(double arg) {
         return Math.Atan(arg);
      }

      public double cos(double arg) {
         return Math.Cos(arg);
      }

      public double exp(double arg) {
         return Math.Exp(arg);
      }

      public double exp10(double arg) {
         return Math.Pow(10, arg);
      }

      public double log(double arg) {
         return Math.Log(arg);
      }

      public double log10(double arg) {
         return Math.Log10(arg);
      }

      public double pi() {
         return Math.PI;
      }

      public double pow(double arg1, double arg2) {
         return Math.Pow(arg1, arg2);
      }

      public double sin(double arg) {
         return Math.Sin(arg);
      }

      public double sqrt(double arg) {
         return Math.Sqrt(arg);
      }

      public double tan(double arg) {
         return Math.Tan(arg);
      }
   }
}
