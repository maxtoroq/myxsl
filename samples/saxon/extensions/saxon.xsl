<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon"?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>

   <xsl:variable name="samples-saxon" xmlns="">
      <saxon:serialize>
         <xsl:variable name="a" as="element()">
            <foo xmlns="">Hello there!</foo>
         </xsl:variable>
         <xsl:value-of select="saxon:serialize($a, 'xml')"/>
      </saxon:serialize>
   </xsl:variable>

   <xsl:template name="content">
      
      <h1>Saxon Extensions for HE</h1>
      <p>
         <a href="http://www.saxonica.com/documentation/extensions/functions.xml">Saxon extension functions</a>
         are natively available in Saxon-PE and Saxon-EE. myxsl.net provides its own implementations for Saxon-HE.
      </p>
      <h2>Namespace Bindings</h2>
      <ul>
         <li>saxon = <strong>http://saxon.sf.net/</strong></li>
      </ul>

      <h2>Function Index</h2>
      <ul>
         <xsl:for-each select="$samples-saxon/*">
            <li>
               <a href="#{replace(name(), ':', '-')}">
                  <xsl:value-of select="name()"/>
               </a>
            </li>
         </xsl:for-each>
      </ul>

      <xsl:for-each select="$samples-saxon/*">
         <xsl:call-template name="function">
            <xsl:with-param name="sampleVar" select="'samples-saxon'"/>
         </xsl:call-template>
      </xsl:for-each>

   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="sampleVar" as="xs:string"/>

      <h2 id="{replace(name(), ':', '-')}">
         <a href="http://www.saxonica.com/documentation/extensions/functions/{local-name()}.xml">
            <xsl:value-of select="name()"/>
         </a>
      </h2>
      <div>
         <xsl:variable name="sampleCode" select="document('')/*/xsl:variable[@name=$sampleVar]/*[local-name()=local-name(current())]" as="element()"/>

         <code>
            <xsl:value-of select="$sampleCode/*/@select"/>
         </code>
         
         <xsl:value-of select="concat(' returns ', string())"/>
      </div>
   </xsl:template>

</xsl:stylesheet>
