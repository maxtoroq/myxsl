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
using System.Globalization;
using System.Linq;
using myxsl.web.configuration;

namespace myxsl.web.compilation {
   
   public abstract class BindingExpressionBuilder {

      public static BindingExpressionInfo ParseExpr(string expression, BindingExpressionContext context) {

         if (expression == null) throw new ArgumentNullException("expression");
         if (context == null) throw new ArgumentNullException("context");

         int colonIndex = expression.IndexOf(':');

         if (colonIndex == -1) {
		      throw new ArgumentException("The expression must contain a colon.", "expression");
	      }

         string prefix = expression.Substring(0, colonIndex);
         string ns;

         if (!context.InScopeNamespaces.TryGetValue(prefix, out ns)) {
            throw new ArgumentException("The are no namespaces defined for prefix '{0}'.".FormatInvariant(prefix), "expression");
         }

         string value = expression.Substring(colonIndex + 1);
         
         BindingExpressionBuilder exprBuilder;

         ExpressionBuilderElement el = WebSection.Instance.Compilation.ExpressionBuilders.Get(ns);

         if (el == null) {
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "There are no expression builders registered for namespace '{0}'.", ns));
         }

         exprBuilder = (BindingExpressionBuilder)Activator.CreateInstance(el.TypeInternal);

         BindingExpressionInfo exprInfo = exprBuilder.ParseExpression(value, context);

         if (exprInfo == null) {
            exprInfo = new BindingExpressionInfo(expression);
         }

         exprInfo.ExpressionBuilder = exprBuilder;

         return exprInfo;
      }

      public virtual BindingExpressionInfo ParseExpression(string expression, BindingExpressionContext context) {
         return null;
      }

      public abstract CodeExpression GetCodeExpression(BindingExpressionInfo exprInfo);
   }
}
