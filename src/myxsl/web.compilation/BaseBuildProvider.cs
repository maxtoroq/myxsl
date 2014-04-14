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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using myxsl.common;
using myxsl.web.ui;

namespace myxsl.web.compilation {

   public abstract class BaseBuildProvider : BuildProvider {

      static readonly Action<ParserError> reportParseError;

      string _GeneratedTypeName, _GeneratedTypeNamespace, _GeneratedTypeFullName;

      Uri _PhysicalPath, _ApplicationPhysicalPath;
      string _AppRelativeVirtualPath;
      bool? _IsFileInCodeDir;

      protected string AppRelativeVirtualPath {
         get {
            return _AppRelativeVirtualPath
               ?? (_AppRelativeVirtualPath = VirtualPathUtility.ToAppRelative(this.VirtualPath));
         }
      }

      protected Uri PhysicalPath { 
         get {
            return _PhysicalPath
               ?? (_PhysicalPath = new Uri(HostingEnvironment.MapPath(VirtualPath), UriKind.Absolute));
         } 
      }

      protected Uri ApplicationPhysicalPath {
         get {
            return _ApplicationPhysicalPath
               ?? (_ApplicationPhysicalPath = new Uri(HostingEnvironment.ApplicationPhysicalPath, UriKind.Absolute));
         }
      }

      protected bool IsFileInCodeDir {
         get {
            return _IsFileInCodeDir
               ?? (_IsFileInCodeDir = AppRelativeVirtualPath
                  .Remove(0, 2)
                  .Split('/')[0]
                  .Equals("App_Code", StringComparison.OrdinalIgnoreCase)).Value;
         }
      }

      protected string GeneratedTypeName {
         get {
            if (_GeneratedTypeName == null) {
               _GeneratedTypeNamespace = GetNamespaceAndTypeNameFromVirtualPath((IsFileInCodeDir) ? 1 : 0, out _GeneratedTypeName);
            }
            return _GeneratedTypeName;
         }
      }

      protected string GeneratedTypeNamespace {
         get {
            if (_GeneratedTypeNamespace == null) {
               // getting GeneratedTypeName will initialize _GeneratedTypeNamespace
               string s = GeneratedTypeName;
            }
            return _GeneratedTypeNamespace;
         }
      }

      protected string GeneratedTypeFullName {
         get {
            return _GeneratedTypeFullName
               ?? (_GeneratedTypeFullName = (GeneratedTypeNamespace.Length == 0) ? GeneratedTypeName 
                  : String.Concat(GeneratedTypeNamespace, ".", GeneratedTypeName));
         }
         set {
            if (String.IsNullOrEmpty(value)) {
               throw new ArgumentException("value cannot be null or empty", "value");
            }

            _GeneratedTypeName = _GeneratedTypeNamespace = _GeneratedTypeFullName = null;

            if (value.Contains(".")) {
               string[] segments = value.Split('.');
               _GeneratedTypeName = segments[segments.Length - 1];
               _GeneratedTypeNamespace = String.Join(".", segments, 0, segments.Length - 1);
            } else {
               _GeneratedTypeName = value;
               _GeneratedTypeNamespace = "";
            }
         }
      }

      protected BaseParser Parser { get; private set; }

      public override CompilerType CodeCompilerType {
         get {
            Parse();

            return (!String.IsNullOrEmpty(Parser.Language)) ? 
               GetDefaultCompilerTypeForLanguage(Parser.Language)
               : base.CodeCompilerType;
         }
      }

      static BaseBuildProvider() {
         
         MethodInfo reportParseErrorMethod = typeof(BuildManager).GetMethod("ReportParseError", BindingFlags.Static | BindingFlags.NonPublic);

         if (reportParseErrorMethod != null) {
            reportParseError = (Action<ParserError>)Delegate.CreateDelegate(typeof(Action<ParserError>), reportParseErrorMethod);
         }
      }

      protected abstract BaseParser CreateParser();
      protected abstract BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(BaseParser parser);

      void Parse() {

         BaseParser parser = CreateParser();
         parser.VirtualPath = this.VirtualPath;
         parser.PhysicalPath = this.PhysicalPath;

         this.Parser = parser;

         try {
            using (TextReader reader = OpenReader()) {
               parser.Parse(reader);
            }
         } catch (TypeInitializationException ex) {

            ReportParseError(new ParserError(ex.InnerException.Message, this.VirtualPath, 1));
            throw new HttpParseException(ex.InnerException.Message, ex.InnerException);

         } catch (HttpParseException ex) {

            ReportParseError(new ParserError(ex.Message, this.VirtualPath, ex.Line));
            throw;
         }

         if (parser.GeneratedTypeFullName != null) {
            this.GeneratedTypeFullName = parser.GeneratedTypeFullName;
         }
      }

      void ReportParseError(ParserError err) {

         if (reportParseError != null) {
            reportParseError(err);
         }
      }

      public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
         
         BaseCodeDomTreeGenerator codeDomGen = CreateCodeDomTreeGenerator(this.Parser);
         codeDomGen.GeneratedTypeName = this.GeneratedTypeName;
         codeDomGen.GeneratedTypeNamespace = this.GeneratedTypeNamespace;
         codeDomGen.GeneratedTypeFullName = this.GeneratedTypeFullName;

         var compileUnit = new CodeCompileUnit();
         codeDomGen.BuildCodeDomTree(compileUnit);

         assemblyBuilder.AddCodeCompileUnit(this, compileUnit);
      }

      public override Type GetGeneratedType(CompilerResults results) {
         return results.CompiledAssembly.GetType(this.GeneratedTypeFullName);
      }

      protected Exception CreateCompileException(ProcessorException exception) {

         string localPath = this.PhysicalPath.LocalPath;
         string virtualPath = this.VirtualPath;

         int lineNumber = exception.LineNumber;
         string errorCode = exception.GetErrorCodeAsClarkName();
         string message = exception.Message;

         if (exception.ModuleUri != null && exception.ModuleUri.IsFile && exception.ModuleUri != this.PhysicalPath) {
            localPath = exception.ModuleUri.LocalPath;

            Uri diff = this.ApplicationPhysicalPath.MakeRelativeUri(exception.ModuleUri);

            string diffString = diff.ToString();

            if (!diffString.StartsWith("..", StringComparison.Ordinal)) {
               virtualPath = "/" + diffString;
            }
         }

         return CreateCompileException(message, errorCode, lineNumber, localPath, virtualPath);
      }

      protected Exception CreateCompileException(string errorMessage) {
         return CreateCompileException(errorMessage, null, 1, this.PhysicalPath.LocalPath, this.VirtualPath);
      }

      Exception CreateCompileException(string errorMessage, string errorCode, int lineNumber, string localPath, string virtualPath) {
         
         var r = new CompilerResults(null);
         r.Errors.Add(new CompilerError(localPath, lineNumber, 1, errorCode, errorMessage));

         ReportParseError(new ParserError(errorMessage, virtualPath, lineNumber));

         return new HttpCompileException(r, this.PhysicalPath.LocalPath);
      }

      string GetNamespaceAndTypeNameFromVirtualPath(int chunksToIgnore, out string typeName) {

         string fileName = (this.IsFileInCodeDir) ?
            VirtualPathUtility.GetFileName(this.VirtualPath) :
            this.AppRelativeVirtualPath.Remove(0, 2);
         
         string[] strArray = fileName.Split(new char[] { '.', '/', '\\' });
         int num = strArray.Length - chunksToIgnore;

         if (strArray[num - 1].Trim().Length == 0)
            throw new HttpException(String.Format(CultureInfo.InvariantCulture, "The file name '{0}' is not supported.", fileName));

         typeName = MakeValidTypeNameFromString(
            (this.IsFileInCodeDir) ? strArray[num - 1] 
               : String.Join("_", strArray, 0, num).ToLowerInvariant()
         );

         if (!this.IsFileInCodeDir) {
            return "ASP";
         }

         for (int i = 0; i < (num - 1); i++) {
            if (strArray[i].Trim().Length == 0)
               throw new HttpException(String.Format(CultureInfo.InvariantCulture, "The file name '{0}' is not supported.", fileName));

            strArray[i] = MakeValidTypeNameFromString(strArray[i]);
         }

         return String.Join(".", strArray, 0, num - 1);
      }

      string MakeValidTypeNameFromString(string s) {

         var builder = new StringBuilder();

         for (int i = 0; i < s.Length; i++) {

            if ((i == 0) && char.IsDigit(s[0])) {
               builder.Append('_');
            }

            builder.Append(char.IsLetterOrDigit(s[i]) ? s[i] : '_');
         }
         return builder.ToString();
      }
   }
}