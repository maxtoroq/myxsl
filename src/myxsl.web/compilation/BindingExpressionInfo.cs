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
using System.CodeDom;
using System.Collections.Generic;

namespace myxsl.web.compilation {
   
   public class BindingExpressionInfo {

      public string Expression { get; private set; }
      public IDictionary<string, object> ParsedValues { get; private set; }
      public object ParsedObject { get; set; }
      public int LineNumber { get; set; }

      internal BindingExpressionBuilder ExpressionBuilder { get; set; }

      public BindingExpressionInfo(string expression) {

         if (expression == null) throw new ArgumentNullException("expression");

         this.Expression = expression;
         this.ParsedValues = new Dictionary<string, object>();
      }

      public CodeExpression GetCodeExpression() {

         if (this.ExpressionBuilder == null) {
            throw new InvalidOperationException("ExpressionBuilder cannot be null.");
         }

         return this.ExpressionBuilder.GetCodeExpression(this);
      }
   }
}
