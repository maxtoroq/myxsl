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
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.web.ui;

namespace myxsl.net.web.compilation {
   
   public sealed class RequestExpressionBuilder : BindingExpressionBuilder {

      internal const string Namespace = RequestModule.Namespace;

      static readonly CodeTypeReference RequestModuleTypeReference;

      static RequestExpressionBuilder() {
         RequestModuleTypeReference = new CodeTypeReference(typeof(RequestModule));
      }

      public override BindingExpressionInfo ParseExpression(string expression, BindingExpressionContext context) {

         var uri = new Uri(expression, UriKind.RelativeOrAbsolute);

         if (!uri.IsAbsoluteUri)
            uri = new Uri(String.Concat(RequestModule.Prefix, ":", uri.OriginalString), UriKind.Absolute);

         var validValues = new List<string>() { 
            bind.query, bind.cookie, bind.form, 
            bind.header, bind.http_method
         };

         string path = uri.AbsolutePath;
         string nodeName = context.NodeName ?? context.BoundNode.Name;

         if (!validValues.Contains(path))
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The value of the '{0}' attribute must be one of these values: {1}.", nodeName, String.Join(", ", validValues.ToArray())));

         BasePageParser pageParser = context.Parser as BasePageParser;

         NameValueCollection query = (uri.Query.Length > 1) ?
            HttpUtility.ParseQueryString(uri.Query.Replace(';', '&')) :
            new NameValueCollection();

         var exprInfo = new BindingExpressionInfo(expression) { 
            ParsedObject = uri
         };

         if (query["name"] != null) {

            exprInfo.ParsedValues["name"] = query["name"];
            query.Remove("name");

         } else {

            XPathNavigator nav = context.BoundNode.Clone();
            nav.MoveToParent();

            if (nav.NodeType == XPathNodeType.Element && nav.LocalName == "param" && nav.NamespaceURI == WellKnownNamespaces.XSLT)
               exprInfo.ParsedValues["name"] = nav.GetAttribute("name", "");
         }

         if (query["accept"] != null) {

            switch (path) {
               case bind.http_method:
                  throw new ArgumentException(
                     String.Format(CultureInfo.InvariantCulture, "When '{0}' is set to '{1}' use the '{2}' attribute instead of the 'accept' option.", XsltPageParser.page.bind_initial_template, bind.http_method, XsltPageParser.page.accept_verbs)
                  );
            }

            exprInfo.ParsedValues["accept"] = query["accept"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            query.Remove("accept");
         
         } else {

            if (context.AffectsXsltInitiation) {

               if (path != bind.http_method) {
                  
                  throw new ArgumentException(
                     String.Format(CultureInfo.InvariantCulture, "The 'accept' option is required for '{0}'. Try this: {1}?accept=[comma-separated template names]", XsltPageParser.page.bind_initial_template, path)
                  );
               
               } else if (pageParser != null && pageParser.AcceptVerbs.Count == 0) {

                  throw new ArgumentException(
                     String.Format(CultureInfo.InvariantCulture, "The '{0}' attribute is required when '{1}' is set to '{2}'.",XsltPageParser.page.accept_verbs, XsltPageParser.page.bind_initial_template, path)
                  );
               }
            }
         }

         switch (path) {
            case bind.cookie:
               exprInfo.ParsedValues["remove"] = GetBooleanOrDefault(query["remove"]);
               query.Remove("remove");
               break;

            default:
               if (query["remove"] != null) {
                  throw new ArgumentException(
                     String.Format(CultureInfo.InvariantCulture, "The remove option is not valid for {0}.", path)
                  );
               }
               break;
         }

         foreach (string key in query.AllKeys) 
            exprInfo.ParsedValues.Add(key, query[key]);

         return exprInfo;
      }

      public override CodeExpression GetCodeExpression(BindingExpressionInfo exprInfo) {

         IDictionary<string, object> options = exprInfo.ParsedValues;
         Uri uri = (Uri)exprInfo.ParsedObject;

         string inputName = options.ContainsKey("name") ? options["name"].ToString() : null;
         string path = uri.AbsolutePath;

         switch (path) {
            case bind.query:
               return GetQueryExpression(inputName);

            case bind.form:
               return GetFormExpression(inputName);

            case bind.cookie:
               return GetCookieExpression(inputName, (bool)options["remove"]);

            case bind.header:
               return GetHeaderExpression(inputName);

            case bind.http_method:
               return GetHttpMethodExpression();

            default:
               throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Invalid path '{0}'.", path), "exprInfo");
         }
      }

      CodeExpression GetQueryExpression(string name) {

         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "Query",
               TargetObject = new CodeTypeReferenceExpression(RequestModuleTypeReference)
            },
            Parameters = { 
               new CodePrimitiveExpression(name)
            }
         };
      }

      CodeExpression GetFormExpression(string name) {

         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "Form",
               TargetObject = new CodeTypeReferenceExpression(RequestModuleTypeReference)
            },
            Parameters = { 
               new CodePrimitiveExpression(name)
            }
         };
      }

      CodeExpression GetCookieExpression(string name, bool remove) {

         var methodInvoke = new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "Cookie",
               TargetObject = new CodeTypeReferenceExpression(RequestModuleTypeReference)
            },
            Parameters = { 
               new CodePrimitiveExpression(name)
            }
         };

         if (remove)
            methodInvoke.Parameters.Add(new CodePrimitiveExpression(true));

         return methodInvoke;
      }

      CodeExpression GetHeaderExpression(string name) {
         
         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "Header",
               TargetObject = new CodeTypeReferenceExpression(RequestModuleTypeReference)
            },
            Parameters = { 
               new CodePrimitiveExpression(name)
            }
         };
      }

      CodeExpression GetHttpMethodExpression() {
         
         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "HttpMethod",
               TargetObject = new CodeTypeReferenceExpression(RequestModuleTypeReference)
            }
         };
      }

      bool GetBooleanOrDefault(string value) {
         return value != null ? XmlConvert.ToBoolean(value) : false;
      }

      struct bind {
         public const string query = "query";
         public const string form = "form";
         public const string cookie = "cookie";
         public const string header = "header";
         public const string http_method = "http-method";
      }
   }
}
