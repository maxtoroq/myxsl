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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace myxsl.net.web.ui {
   
   public abstract class BaseParser {

      string _VirtualPath;
      string _AppRelativeVirtualPath;

      // input state

      public string VirtualPath {
         get { return _VirtualPath; }
         set {
            _VirtualPath = value;
            _AppRelativeVirtualPath = null;
         }
      }

      public string AppRelativeVirtualPath {
         get {
            if (_AppRelativeVirtualPath == null) {
               if (VirtualPath == null)
                  throw new InvalidOperationException("VirtualPath cannot be null");
               _AppRelativeVirtualPath = VirtualPathUtility.ToAppRelative(VirtualPath);
            }
            return _AppRelativeVirtualPath;
         }
      }

      public Uri PhysicalPath { get; set; }

      // output state

      public string GeneratedTypeFullName { get; set; }
      public string Language { get; set; }

      public abstract void Parse(TextReader source);

      protected virtual Exception CreateParseException(string format, params object[] args) {
         throw new HttpParseException(String.Format(CultureInfo.InvariantCulture, format, args));
      }

      protected virtual Exception CreateParseException(IXmlLineInfo lineInfo, string format, params object[] args) {

         if (lineInfo != null && lineInfo.HasLineInfo())
            return new HttpParseException(String.Format(CultureInfo.InvariantCulture, format, args), null, this.VirtualPath, null, lineInfo.LineNumber);
         else
            return new HttpParseException(String.Format(CultureInfo.InvariantCulture, format, args));
      }
      
      protected string GetVirtualPathAttribute(IDictionary<string, string> attribs, string name, bool checkFileExists) {

         string str = GetNonEmptyAttribute(attribs, name);
         
         if (str != null) {

            string combined;

            try {
               combined = HostingEnvironment.VirtualPathProvider.CombineVirtualPaths(this.VirtualPath, str);

            } catch (Exception ex) {
               throw CreateParseException(ex.Message);
            }

            if (checkFileExists && !HostingEnvironment.VirtualPathProvider.FileExists(combined))
               throw CreateParseException("The file '{0}' does not exist.", str);

            return combined;
         } else {
            return null;
         }
      }

      protected string GetFullClassNameAttribute(IDictionary<string, string> attribs, string name) {

         string value = GetNonEmptyNoWhitespaceAttribute(attribs, name);

         if (value != null) {
            string[] nsArray = value.Split(new char[] { '.' });

            foreach (string str in nsArray) {
               if (!CodeGenerator.IsValidLanguageIndependentIdentifier(str))
                  throw CreateParseException("'{0}' is not a valid value for attribute '{1}'.", value, name);
            }
         }

         return value;
      }

      protected object GetEnumAttribute(IDictionary<string, string> attribs, string name, Type enumType) {

         string str = GetNonEmptyAttribute(attribs, name);

         if (str != null) {
            try {
               return Enum.Parse(enumType, str, true);
            } catch (ArgumentException) {
               string enumNames = String.Join(", ", Enum.GetNames(enumType));
               throw CreateParseException("The '{0}' attribute must be one of the following values: {1}.", name, enumNames);
            }
         }

         return null;
      }
   
      protected bool GetBooleanAttribute(IDictionary<string, string> attribs, string name, ref bool val) {

         string str = GetNonEmptyAttribute(attribs, name);

         if (str != null) {
            try {
               val = bool.Parse(str);
            } catch {
               throw CreateParseException("The '{0}' attribute must be set to 'true' or 'false'.", name);
            }
            return true;
         }
         return false;
      }

      protected string GetNonEmptyNoWhitespaceAttribute(IDictionary<string, string> attribs, string name) {

         string str = GetNonEmptyAttribute(attribs, name);

         if (str != null && ContainsWhiteSpace(str))
            throw CreateParseException("The '{0}' attribute cannot contain any whitespace characters.", name);

         return str;
      }

      protected string GetNonEmptyAttribute(IDictionary<string, string> attribs, string name) {

         string val;

         if (attribs.TryGetValue(name, out val)) {
            val = val.Trim();

            if (val.Length == 0)
               throw CreateParseException("The '{0}' attribute cannot be an empty string.", name);
         }

         return val;
      }

      protected bool ContainsWhiteSpace(string s) {

         for (int i = s.Length - 1; i >= 0; i--) {
            if (Char.IsWhiteSpace(s[i]))
               return true;
         }

         return false;
      }

      protected IDictionary<string, string> GetAttributes(string content) {
         return PseudoAttributeParser.GetAttributes(content);
      }
   }
}
