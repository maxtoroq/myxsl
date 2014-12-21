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
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using Saxon.Api;

namespace myxsl.saxon {

   sealed class XdmNodeNavigator : XPathNavigator {

      XdmNode currentNode;
      IEnumerator _currentSequence;

      XmlNameTable _NameTable;

      public override bool CanEdit {
         get { return false; }
      }

      public override string BaseURI {
         get { return currentNode.BaseUri.AbsoluteUri; }
      }

      public override string Name {
         get {
            string prefix = Prefix;
            string local = LocalName;

            if (!String.IsNullOrEmpty(prefix)) {
               return String.Concat(prefix, ":", local);
            }

            return local; 
         }
      }

      public override string NamespaceURI {
         get {
            if (currentNode.NodeName == null) {
               return "";
            }

            return currentNode.NodeName.Uri 
               ?? "";
         }
      }

      public override string Prefix {
         get {
            switch (NodeType) {
               case XPathNodeType.Attribute:
               case XPathNodeType.Element:
                  return currentNode.NodeName.Prefix ?? ""; 
               
               default:
                  return "";
            }
         }
      }

      public override string Value {
         get { return currentNode.StringValue; }
      }

      public override XPathNodeType NodeType {
         get {
            switch (currentNode.NodeKind) {
               case XmlNodeType.Attribute:
                  return XPathNodeType.Attribute;

               // XdmNode.NodeKind returns None for namespace nodes
               case XmlNodeType.None:
                  return XPathNodeType.Namespace;

               case XmlNodeType.Text:
               case XmlNodeType.CDATA:
                  return XPathNodeType.Text;
               
               case XmlNodeType.Comment:
                  return XPathNodeType.Comment;
               
               case XmlNodeType.Document:
               case XmlNodeType.DocumentFragment:
                  return XPathNodeType.Root;

               case XmlNodeType.Element:
               case XmlNodeType.EndElement:
                  return XPathNodeType.Element;

               case XmlNodeType.ProcessingInstruction:
                  return XPathNodeType.ProcessingInstruction;

               case XmlNodeType.SignificantWhitespace:
                  return XPathNodeType.SignificantWhitespace;

               case XmlNodeType.Whitespace:
                  return XPathNodeType.Whitespace;

               default:
                  return XPathNodeType.All;
            }
         }
      }

      public override string LocalName {
         get {
            if (currentNode.NodeName == null) {
               return "";
            }

            return currentNode.NodeName.LocalName 
               ?? ""; 
         }
      }

      public override string OuterXml {
         get { return currentNode.OuterXml; }
         set {
            if (!this.CanEdit) {
               throw new InvalidOperationException("Cannot set OuterXml because this navigator does not support editing.");
            }
            
            throw new NotImplementedException("set_OuterXml not implemented.");
         }
      }

      public override object UnderlyingObject {
         get { return currentNode; }
      }

      public override XmlNameTable NameTable {
         get {
            return _NameTable
               ?? (_NameTable = new NameTable());
         }
      }

      public override bool IsEmptyElement {
         get {
            if (NodeType != XPathNodeType.Element) {
               return false;
            }

            return !currentNode.EnumerateAxis(XdmAxis.Child).MoveNext();
         }
      }

      private IEnumerator currentSequence {
         get {
            if (_currentSequence == null) {

               switch (NodeType) {
                  case XPathNodeType.Attribute:

                     IEnumerator attrIter = currentNode.Parent.EnumerateAxis(XdmAxis.Attribute);
                     
                     while (attrIter.MoveNext() && !attrIter.Current.Equals(currentNode)) ;

                     _currentSequence = attrIter;

                     break;

                  case XPathNodeType.Namespace:

                     IEnumerator nsIter = currentNode.Parent.EnumerateAxis(XdmAxis.Namespace);

                     while (nsIter.MoveNext() && !nsIter.Current.Equals(currentNode)) ;

                     _currentSequence = nsIter;

                     break;
                  
                  default:
                     _currentSequence = currentNode.EnumerateAxis(XdmAxis.FollowingSibling);
                     break;
               }
            }
            return _currentSequence;
         }
         set {
            _currentSequence = value;
         }
      }

      public XdmNodeNavigator(XdmNode node) {
         
         if (node == null) throw new ArgumentNullException("node");

         this.currentNode = node;
      }

      bool MoveTo(XdmAxis axis) {

         IEnumerator en = this.currentNode.EnumerateAxis(axis);

         if (en.MoveNext()) {
            this.currentNode = (XdmNode)en.Current;

            switch (axis) {
               case XdmAxis.Attribute:
               case XdmAxis.Child:
               case XdmAxis.Namespace:
                  this.currentSequence = en;
                  break;

               default:
                  this.currentSequence = null;
                  break;
            }

            return true;
         }

         return false;
      }

      bool MoveToNextInCurrentSequence() {

         if (this.currentSequence.MoveNext()) {
            this.currentNode = (XdmNode)this.currentSequence.Current;
            return true;
         }

         return false;
      }

      public override XPathNavigator Clone() {
         return new XdmNodeNavigator(this.currentNode);
      }

      public override string GetAttribute(string localName, string namespaceURI) {
         return this.currentNode.GetAttributeValue(new QName(namespaceURI, localName)) ?? "";
      }

      public override bool MoveToFirstChild() {

         switch (this.NodeType) {
            case XPathNodeType.Root:
            case XPathNodeType.Element:
               return MoveTo(XdmAxis.Child);
         }

         return false;
      }

      public override bool MoveToNext() {

         XPathNodeType nodeType = this.NodeType;

         if (nodeType == XPathNodeType.Attribute
            || nodeType == XPathNodeType.Namespace) {
            return false;
         }

         return MoveToNextInCurrentSequence();
      }

      public override bool MoveToPrevious() {

         XPathNodeType nodeType = this.NodeType;

         if (nodeType == XPathNodeType.Attribute
            || nodeType == XPathNodeType.Namespace) {
            return false;
         }

         return MoveTo(XdmAxis.PrecedingSibling);
      }

      public override bool MoveToParent() {
         return MoveTo(XdmAxis.Parent);
      }

      public override bool MoveToFirstAttribute() {

         if (this.NodeType != XPathNodeType.Element) {
            return false;
         }

         return MoveTo(XdmAxis.Attribute);
      }

      public override bool MoveToNextAttribute() {

         if (this.NodeType != XPathNodeType.Attribute) {
            return false;
         }

         return MoveToNextInCurrentSequence();
      }

      public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) {

         if (this.NodeType != XPathNodeType.Element) {
            return false;
         }

         IEnumerator en = this.currentNode.EnumerateAxis(XdmAxis.Namespace);

         while (en.MoveNext()) {

            XdmNode node = (XdmNode)en.Current;

            if (!NamespaceScopeOk(node, namespaceScope)) {
               continue;
            }

            this.currentNode = node;
            this.currentSequence = en;
            return true;
         }

         return false;
      }

      public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) {

         if (this.NodeType != XPathNodeType.Namespace) {
            return false;
         }

         while (this.currentSequence.MoveNext()) {

            XdmNode node = (XdmNode)this.currentSequence.Current;

            if (!NamespaceScopeOk(node, namespaceScope)) {
               continue;
            }

            this.currentNode = node;
            return true;
         }

         return false;
      }

      static bool NamespaceScopeOk(XdmNode nsNode, XPathNamespaceScope namespaceScope) {
         
         switch (namespaceScope) {
            case XPathNamespaceScope.All:
               return true;

            case XPathNamespaceScope.ExcludeXml:
               
               return nsNode.NodeName == null
                  || nsNode.NodeName.LocalName != "xml";

            case XPathNamespaceScope.Local:
               
               if (nsNode.NodeName != null
                  && nsNode.NodeName.LocalName == "xml") {
                  
                  return false;
               }
                  
               XdmNode parentNode = nsNode.Parent.Parent;

               if (parentNode != null) {
                  IEnumerator parentScope = parentNode.EnumerateAxis(XdmAxis.Namespace);

                  while (parentScope.MoveNext()) {
                     XdmNode pNsNode = (XdmNode)parentScope.Current;

                     if (nsNode.NodeName == pNsNode.NodeName
                        && nsNode.StringValue == pNsNode.StringValue) {
                        
                        return false;
                     }
                  }
               }

               return true;

            default:
               throw new ArgumentOutOfRangeException("namespaceScope");
         }
      }

      public override bool MoveTo(XPathNavigator other) {

         XdmNodeNavigator navigator = other as XdmNodeNavigator;

         if ((navigator != null) && navigator.currentNode.Root.Equals(this.currentNode.Root)) {
            this.currentNode = navigator.currentNode;
            this.currentSequence = null;
            return true;
         }
         return false;
      }

      public override bool MoveToId(string id) {
         throw new NotImplementedException("MoveToId not implemented.");
      }

      public override bool IsSamePosition(XPathNavigator other) {

         XdmNodeNavigator navigator = other as XdmNodeNavigator;

         if (navigator == null) {
            return false;
         }

         return navigator.currentNode.Equals(this.currentNode);
      }

      //public override void WriteSubtree(XmlWriter writer) {
      //   this.currentNode.WriteTo(writer);
      //}
   }
}
