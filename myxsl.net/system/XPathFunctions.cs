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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using myxsl.net.common;

namespace myxsl.net.system {

   public class XPathFunctions {

      internal const string Namespace = "http://www.w3.org/2005/xpath-functions";

      public double abs(double arg) {
         return Math.Abs(arg);
      }

      public double avg(XPathNavigator[] args) {
         return args.Average(n => n.ValueAsDouble);
      }

      protected string base_uri(XPathNavigator arg) {
         return arg.BaseURI;
      }

      public int compare(string arg1, string arg2) {
         return String.Compare(arg1, arg2);
      }

      protected string current_date() {
         return XmlConvert.ToString(DateTime.Today, XmlSchemaConstructorFunctions.DateFormat);
      }

      protected string current_dateTime() {
         return XmlConvert.ToString(DateTime.Now, XmlSchemaConstructorFunctions.DateTimeFormat);
      }

      protected string current_time() {
         return XmlConvert.ToString(DateTime.Now, XmlSchemaConstructorFunctions.TimeFormat);
      }

      protected bool deep_equals(object arg1, object arg2) {

         if (arg1 == null)
            return arg2 == null;

         if (arg2 == null)
            return false;

         var iter1 = arg1 as XPathNodeIterator;
         var iter2 = arg2 as XPathNodeIterator;

         if (iter1 != null) {
            
            if (iter2 == null)
               return false;

            if (iter1.Count != iter2.Count)
               return false;

            while (iter1.MoveNext() && iter2.MoveNext()) {

               if (!XPathNavigatorEqualityComparer.Instance.Equals(iter1.Current, iter2.Current)) 
                  return false;
            }

            return true;
         }

         if (iter2 != null)
            return false;

         return arg1.Equals(arg2);
      }

      protected XPathNavigator[] distinct_values(XPathNodeIterator iter) {

         XPathNavigator[] nodes = iter.Cast<XPathNavigator>().ToArray();
         var distinct = new Dictionary<object, XPathNavigator>();

         foreach (object item in nodes.Select(n => n.TypedValue).Distinct()) {
            if (!distinct.ContainsKey(item))
               distinct.Add(item, nodes.First(n => n.TypedValue == item));
         }

         return distinct.Values.ToArray();
      }

      public bool empty(XPathNodeIterator iter) {
         return iter.Count == 0;
      }

      protected string encode_for_uri(string arg) {
         return Uri.EscapeDataString(arg);
      }

      protected bool ends_with(string arg1, string arg2) {
         return arg1.EndsWith(arg2);
      }

      public void error() {
         throw new XsltException("Error signalled by application call on fn:error().");
      }

      public void error(string code) {
         throw new XsltException("Error signalled by application call on fn:error(). Code: '{0}'.".FormatInvariant(code));
      }

      public void error(string code, string description) {
         throw new XsltException("Error signalled by application call on fn:error(). Code: '{0}'. Description: '{1}'.".FormatInvariant(code, description));
      }

      protected XPathNavigator exactly_one(XPathNodeIterator iter) {

         if (iter.Count == 0 || iter.Count > 1)
            throw new XsltException("fn:exactly-one called with a sequence containing zero or more than one item.");

         return iter.Cast<XPathNavigator>().Single();
      }

      public bool exists(XPathNodeIterator iter) {
         return iter.Count > 0;
      }

      protected bool has_children(XPathNavigator arg) {
         return arg.HasChildren;
      }

      public XPathNavigator head(XPathNodeIterator iter) { 
         return iter.Cast<XPathNavigator>().First();
      }

      protected object in_scope_prefixes(XPathNavigator element) {
         return ExtensionObjectConvert.ToInputOrEmpty(element.GetNamespacesInScope(XmlNamespaceScope.All).Keys);
      }

      protected string lower_case(string arg) {
         return arg.ToLower();
      }

      public bool matches(string input, string pattern) {
         return Regex.IsMatch(input, pattern);
      }

      public bool matches(string input, string pattern, string flags) {
         return Regex.IsMatch(input, pattern, ParseFlags(flags));
      }

      public object max(XPathNodeIterator iter) {
         return iter.Cast<XPathNavigator>().Max(n => n.TypedValue);
      }

      public object min(XPathNodeIterator iter) {
         return iter.Cast<XPathNavigator>().Min(n => n.TypedValue);
      }

      protected object namespace_uri_for_prefix(XPathNodeIterator prefix, XPathNavigator element) {

         IDictionary<string, string> namespaces = element.GetNamespacesInScope(XmlNamespaceScope.All);

         string p = (prefix.MoveNext()) ? prefix.Current.Value : "";

         if (!namespaces.ContainsKey(p))
            return ExtensionObjectConvert.EmptyIterator;

         return namespaces[p];
      }

      protected XPathNodeIterator one_or_more(XPathNodeIterator iter) {
         
         if (iter.Count == 0)
            throw new XsltException("fn:one-or-more called with a sequence containing no items.");

         return iter;
      }

      protected object parse_xml(XPathNodeIterator arg) {
         return parse_xml_impl(arg, fragment: false);
      }

      protected object parse_xml_fragment(XPathNodeIterator arg) {
         return parse_xml_impl(arg, fragment: true);
      }

      static object parse_xml_impl(XPathNodeIterator arg, bool fragment) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         var parseOptions = new XmlParsingOptions { 
            ConformanceLevel = (fragment) ? 
               ConformanceLevel.Fragment
               : ConformanceLevel.Document
         };

         var itemFactory = new SystemItemFactory();

         using (var reader = new StringReader(arg.Current.Value)) 
            return ExtensionObjectConvert.ToInput(itemFactory.CreateNodeReadOnly(reader, parseOptions));
      }

      public object path(XPathNodeIterator arg) {

         if (ExtensionObjectConvert.IsEmpty(arg))
            return ExtensionObjectConvert.EmptyIterator;

         arg.MoveNext();

         var reverseBuffer = new List<string>();

         XPathNavigator clone = arg.Current.Clone();

         path_impl(clone, reverseBuffer);

         if (reverseBuffer.Count == 1)
            return reverseBuffer[0];

         // replace document-node path expr with empty string
         // so join operation doesn't create //
         if (reverseBuffer[reverseBuffer.Count - 1] == "/")
            reverseBuffer[reverseBuffer.Count - 1] = "";

         return String.Join("/", Enumerable.Reverse(reverseBuffer));
      }

      static void path_impl(XPathNavigator node, List<string> reverseBuffer) {

         switch (node.NodeType) {
            case XPathNodeType.Attribute:

               if (node.NamespaceURI.HasValue()) {
                  reverseBuffer.Add(String.Concat("@\"", node.NamespaceURI, "\":", node.LocalName));

               } else {
                  reverseBuffer.Add("@" + node.LocalName);
               }

               break;

            case XPathNodeType.Comment: {

                  int position = 1;

                  while (node.MoveToPrevious()) {
                     if (node.NodeType == XPathNodeType.Comment) 
                        position++;
                  }

                  reverseBuffer.Add(String.Concat("comment()[", position.ToStringInvariant(), "]"));
               }
               break;

            case XPathNodeType.Element: {

                  string currentLocalName = node.LocalName;
                  string currentNamespace = node.NamespaceURI;
                  int position = 1;

                  while (node.MoveToPrevious()) {
                     if (node.NodeType == XPathNodeType.Element
                        && node.LocalName == currentLocalName
                        && node.NamespaceURI == currentNamespace) {

                        position++;
                     }
                  }

                  reverseBuffer.Add(String.Concat("\"", currentNamespace, "\":", currentLocalName, "[", position.ToStringInvariant(), "]"));
               }
               break;

            case XPathNodeType.Namespace:

               if (node.LocalName.HasValue()) {
                  reverseBuffer.Add("namespace::" + node.Prefix);
               
               } else {
                  reverseBuffer.Add("namespace::*[\"http://www.w3.org/2005/xpath-functions\":local-name()=\"\"]");
               }

               break;
            
            case XPathNodeType.ProcessingInstruction: {

                  string currentLocal = node.LocalName;
                  int position = 1;

                  while (node.MoveToPrevious()) {
                     if (node.NodeType == XPathNodeType.ProcessingInstruction
                        && node.LocalName == currentLocal) {

                        position++;
                     }
                  }

                  reverseBuffer.Add(String.Concat("processing-instruction(", currentLocal, ")[", position.ToStringInvariant(), "]"));
               }
               break;
            
            case XPathNodeType.Root:
               reverseBuffer.Add("/");
               break;

            case XPathNodeType.SignificantWhitespace:
            case XPathNodeType.Text:
            case XPathNodeType.Whitespace: {

                  int position = 1;

                  while (node.MoveToPrevious()) {
                     if (node.NodeType == XPathNodeType.Text) {

                        position++;
                     }
                  }

                  reverseBuffer.Add(String.Concat("text()[", position.ToStringInvariant(), "]"));
               }
               break;
         }

         if (node.MoveToParent())
            path_impl(node, reverseBuffer);
      }

      public string replace(string input, string pattern, string replacement) {
         return replace(input, pattern, replacement, null);
      }

      public string replace(string input, string pattern, string replacement, string flags) {
         return Regex.Replace(input, pattern, replacement, ParseFlags(flags));
      }

      protected string resolve_uri(string relativeUri, string baseUri) {
         return new Uri(new Uri(baseUri, UriKind.Absolute), relativeUri).ToString();
      }

      public XPathNavigator[] reverse(XPathNodeIterator iter) {
         return iter.Cast<XPathNavigator>().Reverse().ToArray();
      }

      public XPathNavigator root(XPathNavigator arg) {

         var nav = arg.Clone();

         nav.MoveToRoot();

         return nav;
      }

      protected double round_half_to_even(double arg) {
         return Math.Round(arg, MidpointRounding.ToEven);
      }

      protected double round_half_to_even(double arg, int precision) {
         return Math.Round(arg, precision, MidpointRounding.ToEven);
      }

      public string serialize(XPathNodeIterator iter) {
         return serialize(iter, null);
      }

      public string serialize(XPathNodeIterator iter, XPathNodeIterator parameters) {

         var itemFactory = new SystemItemFactory();

         XPathSerializationOptions options = null;

         if (parameters != null
            && parameters.Count == 1) {

            options = new XPathSerializationOptions();
            ((IXmlSerializable)options).ReadXml(parameters.Cast<XPathNavigator>().First().ReadSubtree());
         }

         using (var writer = new StringWriter()) {
            
            IEnumerable<XPathItem> items = iter.Cast<XPathItem>();

            if (options == null)
               itemFactory.Serialize(items, writer);
            else
               itemFactory.Serialize(items, writer, options);
            
            return writer.ToString();
         }
      }

      protected string string_join(XPathNodeIterator iter) {
         return string_join(iter, "");
      }

      protected string string_join(XPathNodeIterator iter, string separator) {
         return String.Join(separator, iter.Cast<XPathNavigator>().Select(n => n.Value));
      }

      public XPathNavigator[] subsequence(XPathNodeIterator iter, int startingLoc) {
         return iter.Cast<XPathNavigator>().Skip(startingLoc).ToArray();
      }

      public XPathNavigator[] subsequence(XPathNodeIterator iter, int startingLoc, int length) {
         return iter.Cast<XPathNavigator>().Skip(startingLoc).Take(length).ToArray();
      }

      public XPathNavigator[] tail(XPathNodeIterator iter) {
         return iter.Cast<XPathNavigator>().Skip(1).ToArray();
      }

      public object tokenize(XPathNodeIterator iter, string pattern) {
         return tokenize(iter, pattern, null);
      }

      public object tokenize(XPathNodeIterator iter, string pattern, string flags) {

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         iter.MoveNext();

         string input = iter.Current.Value;

         if (String.IsNullOrEmpty(input))
            return ExtensionObjectConvert.EmptyIterator;

         return ExtensionObjectConvert.ToInput(Regex.Split(input, pattern, ParseFlags(flags)));
      }

      public XPathNodeIterator trace(XPathNodeIterator iter, string label) {
         
         Trace.WriteLine(string_join(iter, " "), label);

         return iter;
      }

      protected string upper_case(string arg) {
         return arg.ToUpper();
      }

      protected XPathNodeIterator zero_or_one(XPathNodeIterator iter) {

         if (iter.Count > 1)
            throw new XsltException("fn:zero-or-one called with a sequence containing more than one item.");

         return iter;
      }

      RegexOptions ParseFlags(string flags) {

         RegexOptions options = RegexOptions.None;

         if (flags == null)
            return options;

         char[] flagChars = flags.ToCharArray();

         for (int i = 0; i < flagChars.Length; i++) {

            switch (flagChars[i]) {
               case 's':
                  options |= RegexOptions.Singleline;
                  break;

               case 'm':
                  options |= RegexOptions.Multiline;
                  break;

               case 'i':
                  options |= RegexOptions.IgnoreCase;
                  break;

               case 'x':
                  options |= RegexOptions.IgnorePatternWhitespace;
                  break;

               default:
                  throw new XsltException("The flags argument can only contain the following characters: 'i', 'm', 's', 'x'.");
            }
         }

         return options;
      }
   }
}
