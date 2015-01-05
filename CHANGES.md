Changes
=======

saxon.v0.4.0
------------
- Changed root namespace and assembly name
- Removed AllowPartiallyTrustedCallers attribute
- Depending on Saxon-HE 9.5.0.2
- XdmWriter, available via XPathItemFactory.BuildNode().AppendChild()

xml.xsl.v0.4.0
--------------
- New assembly
- Changed root namespace

web.mvc.v0.4.0
--------------
- Changed root namespace and assembly name
- Removed AllowPartiallyTrustedCallers attribute

web.v0.4.0
----------
- New assembly
- Changed root namespace
- Changed expression builder to use single attribute namespace and prefix in content
- Full VirtualPathProvider support

web.schematron.v0.4.0
---------------------
- New assembly
- Changed root namespace

v0.4.0
------
- Changed root namespace and assembly name
- Moved *web*, *xml.xsl* and *schematron* out
- XPath modules namespace change
- Removed AllowPartiallyTrustedCallers attribute
- Removed *configuration*, using static members
- XPathItemFactory.BuildNode
- Changed XPathFunctionAttribute signature
- Moved XPathNavigatorEqualityComparer to *common*

v0.3.1
------
- Fixed [#4](https://github.com/maxtoroq/myxsl/issues/4): Incorrect node conversion of IXmlSerializable types
- XPathItemFactory.Serialize(IXmlSerializable, XmlWriter)
- XmlRootPrefixed attribute

saxon.v0.3.0
------------
Preview

web.mvc.v0.3.0
--------------
Preview

v0.3.0
------
Preview
