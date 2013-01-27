<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>

   <xsl:template name="content">
      
      <h1>XslCompiledTransform Extensions</h1>
      <p>The following are extension functions specifically targeted to the XslCompiledTransform processor.</p>
      <ul>
         <li>
            <a href="w3c.xpath.xsl">XPath 2.0 and 3.0 Functions</a>
         </li>
         <li>
            <a href="w3c.xmlschema.xsl">Constructor functions for XML Schema built-in atomic types</a>
         </li>
      </ul>
   </xsl:template>

</xsl:stylesheet>
