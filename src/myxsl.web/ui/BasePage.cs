// Copyright 2009 Max Toro Q.
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
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Text;
using System.Linq;
using System.Globalization;
using myxsl.common;

namespace myxsl.web.ui {
   
   public abstract class BasePage : IHttpHandler {

      public HttpRequest Request { get; private set; }
      public HttpResponse Response { get; private set; }
      public HttpContext Context { get; private set; }
      public virtual bool IsReusable { get { return false; } }

      public virtual void ProcessRequest(HttpContext context) {

         SetIntrinsics(context);
         FrameworkInitialize();
         ProcessRequest();
      }

      public void SetIntrinsics(HttpContext context) {
         
         this.Context = context;
         this.Request = context.Request;
         this.Response = context.Response;
      }

      protected virtual void FrameworkInitialize() {
         AddFileDependencies();
      }

      protected void CheckHttpMethod(string[] acceptVerbs) {

         if (!Array.Exists(acceptVerbs, s => String.Equals(s, this.Request.HttpMethod, StringComparison.OrdinalIgnoreCase))) {
            throw new HttpException((int)HttpStatusCode.MethodNotAllowed, "Method Not Allowed");
         }
      }

      public virtual void AddFileDependencies() { }

      protected void AddFileDependencies(string[] fileDependencies) {

         if (fileDependencies != null) {
            this.Response.AddFileDependencies(fileDependencies);
         }
      }

      protected virtual void InitOutputCache(OutputCacheParameters parameters) {
         new OutputCachedPage(parameters).ProcessRequest(this.Context);
      }

      protected virtual void ProcessRequest() {

         try {
            Render(this.Response.Output);

         } catch (ProcessorException ex) {
            throw CreateRenderException(ex);
         }
      }
      
      public abstract void Render(TextWriter writer);

      protected HttpException CreateRenderException(ProcessorException ex) {

         var sb = new StringBuilder();

         if (ex.ErrorCode != null && !ex.ErrorCode.IsEmpty) {
            sb.Append("(")
               .Append(ex.ErrorCode.ToString())
               .Append(") ");
         }

         sb.Append(ex.Message);

         if (ex.ModuleUri != null) {
            sb.AppendLine()
               .Append("URI: ")
               .Append(ex.ModuleUri.ToString())
               .AppendLine()
               .Append("Line number: ")
               .Append(ex.LineNumber.ToString(CultureInfo.InvariantCulture));
         }

         return new HttpException((int)HttpStatusCode.InternalServerError, sb.ToString(), ex);
      }
      
      protected virtual void SetBoundParameters(IDictionary<XmlQualifiedName, object> parameters) { }

      protected T CheckParamLength<T>(string name, T value, int minLength, int maxLength) {

         int length = (value == null) ? 0 : 1;

         CheckParamLengthValidate(name, length, minLength, maxLength);

         return value;
      }

      protected T[] CheckParamLength<T>(string name, T[] values, int minLength, int maxLength) {

         int length = (values == null) ? 0 
            : values.Length;

         CheckParamLengthValidate(name, length, minLength, maxLength);

         return values;
      }

      protected IEnumerable<T> CheckParamLength<T>(string name, IEnumerable<T> values, int minLength, int maxLength) {

         int length = (values == null) ? 0 
            : values.Count();

         CheckParamLengthValidate(name, length, minLength, maxLength);

         return values;
      }

      void CheckParamLengthValidate(string name, int length, int minLength, int maxLength) {

         if (length < minLength)
            throw new HttpException((int)HttpStatusCode.BadRequest, String.Format(CultureInfo.InvariantCulture, "At least {0} value(s) is expected for parameter '{1}'.", minLength, name));

         else if (maxLength != -1 && length > maxLength)
            throw new HttpException((int)HttpStatusCode.BadRequest, String.Format(CultureInfo.InvariantCulture, "At most {0} value(s) is expected for parameter '{1}'.", maxLength, name));
      }

      protected string CheckParamValues(string name, string value, string[] accept) {

         if (value != null
            && !accept.Contains(value)) {

            throw CheckParamValuesException(name);
         }

         return value;
      }

      protected string[] CheckParamValues(string name, string[] values, string[] accept) {

         if (values != null
            && !values.All(s => accept.Contains(s))) {

            throw CheckParamValuesException(name);
         }

         return values;
      }

      protected IEnumerable<string> CheckParamValues(string name, IEnumerable<string> values, string[] accept) {

         if (values != null
            && !values.All(s => accept.Contains(s))) {
            
            throw CheckParamValuesException(name);
         }

         return values;
      }

      static Exception CheckParamValuesException(string name) {
         return new HttpException((int)HttpStatusCode.BadRequest, String.Format(CultureInfo.InvariantCulture, "Parameter '{0}' has an invalid value.", name));
      }
   }
}
