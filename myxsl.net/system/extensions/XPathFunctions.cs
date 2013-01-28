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
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using myxsl.net.common;

namespace myxsl.net.system.extensions {

   public class XPathFunctions {

      internal const string Namespace = "http://www.w3.org/2005/xpath-functions";

      public object abs(object arg) {
         // fn:abs($arg as numeric?) as numeric?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null)
            return ExtensionObjectConvert.EmptyIterator;

         return Math.Abs(value.Value);
      }

      public object avg(object args) {
         // fn:avg($arg as xs:anyAtomicType*) as xs:anyAtomicType?

         XPathNodeIterator iter = args as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item, return unchanged
            return args;
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         return iter.Cast<XPathNavigator>().Select(n => n.ValueAsDouble).Average();
      }

      protected object base_uri(XPathNodeIterator arg) {
         // fn:base-uri($arg as node()?) as xs:anyURI?

         XPathNavigator node = ExtensionObjectConvert.ToXPathNavigator(arg);

         if (node == null)
            return ExtensionObjectConvert.EmptyIterator;

         return node.BaseURI;
      }

      public object compare(object comparand1, object comparand2) {
         // fn:compare($comparand1 as xs:string?, $comparand2 as xs:string?) as xs:integer?

         string first = ExtensionObjectConvert.ToString(comparand1);
         string second = ExtensionObjectConvert.ToString(comparand2);

         if (first == null
            || second == null) {
            
            return ExtensionObjectConvert.EmptyIterator;
         }

         return String.Compare(first, second);
      }

      protected string current_date() {
         // fn:current-date() as xs:date
         return XmlConvert.ToString(DateTime.Today, XmlSchemaConstructorFunctions.DateFormat);
      }

      protected string current_dateTime() {
         // fn:current-dateTime() as xs:dateTimeStamp
         return XmlConvert.ToString(DateTime.Now, XmlSchemaConstructorFunctions.DateTimeFormat);
      }

      protected string current_time() {
         // fn:current-time() as xs:time
         return XmlConvert.ToString(DateTime.Now, XmlSchemaConstructorFunctions.TimeFormat);
      }

      protected object document_uri(XPathNodeIterator iter) {
         // fn:document-uri($arg as node()?) as xs:anyURI?

         XPathNavigator node;

         if (ExtensionObjectConvert.IsEmpty(iter)
            || !iter.MoveNext()
            || (node = iter.Current).NodeType != XPathNodeType.Root) {
            return ExtensionObjectConvert.EmptyIterator;
         }

         return node.BaseURI;
      }

      protected bool deep_equal(object arg1, object arg2) {

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
         // fn:distinct-values($arg as xs:anyAtomicType*) as xs:anyAtomicType*

         XPathNavigator[] nodes = iter.Cast<XPathNavigator>().ToArray();
         var distinct = new Dictionary<object, XPathNavigator>();

         foreach (object item in nodes.Select(n => n.TypedValue).Distinct()) {
            if (!distinct.ContainsKey(item))
               distinct.Add(item, nodes.First(n => n.TypedValue.Equals(item)));
         }

         return distinct.Values.ToArray();
      }

      public bool empty(object arg) {
         // fn:empty($arg as item()*) as xs:boolean
         return ExtensionObjectConvert.IsEmpty(arg);
      }

      protected string encode_for_uri(object arg) {
         // fn:encode-for-uri($uri-part as xs:string?) as xs:string

         string value = ExtensionObjectConvert.ToString(arg);

         if (value == null)
            return "";

         return Uri.EscapeDataString(value);
      }

      protected bool ends_with(object arg1, object arg2) {
         // fn:ends-with($arg1 as xs:string?, $arg2 as xs:string?) as xs:boolean

         string first = ExtensionObjectConvert.ToString(arg1) ?? "";
         string second = ExtensionObjectConvert.ToString(arg2) ?? "";

         return first.EndsWith(second);
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

      protected object exactly_one(object arg) {
         // fn:exactly-one($arg as item()*) as item()

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item, return unchanged
            return arg;
         }

         if (iter.Count == 0 || iter.Count > 1)
            throw new XsltException("fn:exactly-one called with a sequence containing zero or more than one item.");

         return iter;
      }

      public bool exists(object arg) {
         // fn:exists($arg as item()*) as xs:boolean

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item
            return true;
         }

         return iter.Count > 0;
      }

      protected bool has_children(XPathNodeIterator arg) {
         //fn:has-children($node as node()?) as xs:boolean

         XPathNavigator node = ExtensionObjectConvert.ToXPathNavigator(arg);

         if (node == null)
            return false;

         return node.HasChildren;
      }

      public object head(object arg) {
         // fn:head($arg as item()*) as item()?

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item
            return arg;
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         return ExtensionObjectConvert.ToXPathNavigator(iter);
      }

      protected object in_scope_prefixes(XPathNavigator element) {
         // fn:in-scope-prefixes($element as element()) as xs:string*
         return ExtensionObjectConvert.ToInputOrEmpty(element.GetNamespacesInScope(XmlNamespaceScope.All).Keys);
      }

      protected string lower_case(object arg) {
         // fn:lower-case($arg as xs:string?) as xs:string

         string str = ExtensionObjectConvert.ToString(arg);

         if (str == null)
            return "";

         return str.ToLower();
      }

      public bool matches(object input, string pattern) {
         return matches(input, pattern, "");
      }

      public bool matches(object input, string pattern, string flags) {
         // fn:matches($input	as xs:string?, $pattern as xs:string, $flags as xs:string) as xs:boolean

         string inputStr = ExtensionObjectConvert.ToString(input) ?? "";

         return Regex.IsMatch(inputStr, pattern, ParseFlags(flags));
      }

      public object max(object arg) {
         // fn:max($arg as xs:anyAtomicType*) as xs:anyAtomicType?

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item, return unchanged
            return arg;
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         return iter.Cast<XPathNavigator>().Max(n => n.TypedValue);
      }

      public object min(object arg) {
         // fn:min($arg as xs:anyAtomicType*) as xs:anyAtomicType?

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item, return unchanged
            return arg;
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         return iter.Cast<XPathNavigator>().Min(n => n.TypedValue);
      }

      public object nilled(XPathNodeIterator iter) {
         // fn:nilled($arg as node()?) as xs:boolean?

         XPathNavigator node = ExtensionObjectConvert.ToXPathNavigator(iter);

         if (node == null
            || node.NodeType != XPathNodeType.Element) {

            return ExtensionObjectConvert.EmptyIterator;
         }

         IXmlSchemaInfo schemaInfo = node.SchemaInfo;

         return schemaInfo != null
            && schemaInfo.IsNil;
      }

      protected object namespace_uri_for_prefix(object prefix, XPathNavigator element) {
         // fn:namespace-uri-for-prefix($prefix as xs:string?, $element as element()) as xs:anyURI?

         IDictionary<string, string> namespaces = element.GetNamespacesInScope(XmlNamespaceScope.All);

         string p = ExtensionObjectConvert.ToString(prefix) ?? "";

         if (!namespaces.ContainsKey(p))
            return ExtensionObjectConvert.EmptyIterator;

         return namespaces[p];
      }

      protected object one_or_more(object arg) {
         // fn:one-or-more($arg as item()*) as item()+

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item
            return arg;
         }

         if (iter.Count == 0)
            throw new XsltException("fn:one-or-more called with a sequence containing no items.");

         return iter;
      }

      protected object parse_xml(object arg) {
         // fn:parse-xml($arg as xs:string?) as document-node(element(*))?
         return parse_xml_impl(arg, fragment: false);
      }

      protected object parse_xml_fragment(object arg) {
         // fn:parse-xml-fragment($arg as xs:string?) as document-node()?
         return parse_xml_impl(arg, fragment: true);
      }

      static object parse_xml_impl(object arg, bool fragment) {

         string str = ExtensionObjectConvert.ToString(arg);

         if (str == null)
            return ExtensionObjectConvert.EmptyIterator;

         var parseOptions = new XmlParsingOptions { 
            ConformanceLevel = (fragment) ? 
               ConformanceLevel.Fragment
               : ConformanceLevel.Document
         };

         var itemFactory = new SystemItemFactory();

         using (var reader = new StringReader(str)) 
            return ExtensionObjectConvert.ToInput(itemFactory.CreateNodeReadOnly(reader, parseOptions));
      }

      public object path(XPathNodeIterator arg) {
         // fn:path($arg as node()?) as xs:string?

         XPathNavigator node = ExtensionObjectConvert.ToXPathNavigator(arg);

         if (node == null)
            return ExtensionObjectConvert.EmptyIterator;

         var reverseBuffer = new List<string>();

         XPathNavigator clone = node.Clone();

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
         return replace(input, pattern, replacement, "");
      }

      public string replace(object input, string pattern, string replacement, string flags) {
         // fn:replace($input as xs:string?, $pattern as xs:string, $replacement as xs:string, $flags as xs:string) as xs:string

         string inputStr = ExtensionObjectConvert.ToString(input) ?? "";

         return Regex.Replace(inputStr, pattern, replacement, ParseFlags(flags));
      }

      protected object resolve_uri(object relativeUri, string baseUri) {
         // fn:resolve-uri($relative as xs:string?, $base as xs:string) as xs:anyURI?

         string relativeUriStr = ExtensionObjectConvert.ToString(relativeUri);

         if (relativeUriStr == null)
            return ExtensionObjectConvert.EmptyIterator;

         return new Uri(new Uri(baseUri, UriKind.Absolute), relativeUriStr).ToString();
      }

      private object reverse(object arg) {
         // fn:reverse($arg as item()*) as item()*

         // TODO: XslCompiledTransform seems to reorder node-sets in document order

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item
            return arg;
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return ExtensionObjectConvert.EmptyIterator;

         return iter.Cast<XPathNavigator>().Reverse().ToArray();
      }

      public object root(XPathNodeIterator arg) {
         // fn:root($arg as node()?) as node()?

         XPathNavigator node = ExtensionObjectConvert.ToXPathNavigator(arg);

         if (node == null)
            return ExtensionObjectConvert.EmptyIterator;

         XPathNavigator clone = node.Clone();

         clone.MoveToRoot();

         return clone;
      }

      protected object round_half_to_even(object arg) {
         // fn:round-half-to-even($arg as numeric?) as numeric?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null)
            return ExtensionObjectConvert.EmptyIterator;

         return Math.Round(value.Value, MidpointRounding.ToEven);
      }

      protected object round_half_to_even(object arg, int precision) {
         // fn:round-half-to-even($arg as numeric?, $precision as xs:integer) as numeric?

         double? value = ExtensionObjectConvert.ToNullableDouble(arg);

         if (value == null)
            return ExtensionObjectConvert.EmptyIterator;

         return Math.Round(value.Value, precision, MidpointRounding.ToEven);
      }

      public string serialize(XPathNodeIterator arg) {
         return serialize(arg, null);
      }

      public string serialize(XPathNodeIterator arg, XPathNodeIterator parameters) {
         // fn:serialize($arg	as item()*, $params as element(output:serialization-parameters)?) as xs:string

         var itemFactory = new SystemItemFactory();

         XPathSerializationOptions options = null;

         if (parameters != null
            && parameters.Count == 1) {

            options = new XPathSerializationOptions();
            ((IXmlSerializable)options).ReadXml(parameters.Cast<XPathNavigator>().First().ReadSubtree());
         }

         using (var writer = new StringWriter()) {
            
            IEnumerable<XPathItem> items = arg.Cast<XPathItem>();

            if (options == null)
               itemFactory.Serialize(items, writer);
            else
               itemFactory.Serialize(items, writer, options);
            
            return writer.ToString();
         }
      }

      protected string string_join(object arg) {
         return string_join(arg, "");
      }

      protected string string_join(object arg, string separator) {
         // fn:string-join($arg1 as xs:string*, $arg2 as xs:string) as xs:string

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item
            return arg.ToString();
         }

         if (ExtensionObjectConvert.IsEmpty(iter))
            return "";

         return String.Join(separator, iter.Cast<XPathNavigator>().Select(n => n.Value));
      }

      public XPathNavigator[] subsequence(XPathNodeIterator sourceSeq, int startingLoc) {
         // fn:subsequence($sourceSeq as item()*, $startingLoc as xs:double) as item()*

         return sourceSeq.Cast<XPathNavigator>().Skip(startingLoc).ToArray();
      }

      public XPathNavigator[] subsequence(XPathNodeIterator sourceSeq, int startingLoc, int length) {
         // fn:subsequence($sourceSeq as item()*, $startingLoc as xs:double, $length as xs:double) as item()*

         return sourceSeq.Cast<XPathNavigator>().Skip(startingLoc).Take(length).ToArray();
      }

      public XPathNavigator[] tail(XPathNodeIterator iter) {
         // fn:tail($arg as item()*) as item()*

         return iter.Cast<XPathNavigator>().Skip(1).ToArray();
      }

      public object tokenize(object input, string pattern) {
         return tokenize(input, pattern, "");
      }

      public object tokenize(object input, string pattern, string flags) {
         // fn:tokenize($input as xs:string?, $pattern as xs:string, $flags as xs:string) as xs:string*

         string inputStr = ExtensionObjectConvert.ToString(input);

         if (!inputStr.HasValue())
            return ExtensionObjectConvert.EmptyIterator;

         return ExtensionObjectConvert.ToInput(Regex.Split(inputStr, pattern, ParseFlags(flags)));
      }

      public object trace(object value, string label) {
         // fn:trace($value as item()*, $label as xs:string) as item()*

         Trace.WriteLine(string_join(value, " "), label);

         return value;
      }

      protected string upper_case(object arg) {
         // fn:upper-case($arg as xs:string?) as xs:string

         string str = ExtensionObjectConvert.ToString(arg);

         if (str == null)
            return "";

         return str.ToUpper();
      }

      protected object zero_or_one(object arg) {
         // fn:zero-or-one($arg as item()*) as item()?

         XPathNodeIterator iter = arg as XPathNodeIterator;

         if (iter == null) {
            // if not iterator then it's a single item

         } else if (iter.Count > 1) {
            throw new XsltException("fn:zero-or-one called with a sequence containing more than one item.");
         }

         return arg;
      }

      RegexOptions ParseFlags(string flags) {

         RegexOptions options = RegexOptions.None;

         if (!flags.HasValue())
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
