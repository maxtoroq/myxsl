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

namespace myxsl.net.web.ui {
   
   public class ParsedValue<T> {

      public T Value { get; private set; }
      public string FileName { get; private set; }
      public int LineNumber { get; private set; }

      public ParsedValue(T value, string fileName, int lineNumber) {
         
         this.Value = value;
         this.FileName = fileName;
         this.LineNumber = lineNumber;
      }
   }
}
