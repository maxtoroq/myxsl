<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon"?>
<?output-cache cache-profile="library" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:exsl="http://exslt.org/common"
   xmlns:app="http://myxsl.net/"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>
   <xsl:import href="~/App_Code/xslt_highlighter_api.xsl"/>

   <xsl:variable name="samples" xmlns="">
      <exsl:node-set>
         <xsl:variable name="a" as="element()">
            <foo xmlns="">Hello there!</foo>
         </xsl:variable>
         <xsl:value-of select="exsl:node-set($a)/name()"/>
      </exsl:node-set>
   </xsl:variable>

   <xsl:template name="content">
      
      <h1>EXSLT Extensions</h1>
      <p>
         <a href="http://www.saxonica.com/documentation/extensions/exslt.xml">EXSLT extension functions</a>
         are natively available in Saxon-PE and Saxon-EE. myxsl.net provides its own implementations for Saxon-HE.
      </p>
      <h2>Namespace Bindings</h2>
      <ul>
         <li>exsl = <strong>http://exslt.org/common</strong></li>
      </ul>

      <h2>Function Index</h2>
      <ul>
         <xsl:for-each select="$samples/*">
            <li>
               <a href="#{replace(name(), ':', '-')}">
                  <xsl:value-of select="name()"/>
               </a>
            </li>
         </xsl:for-each>
      </ul>

      <xsl:for-each select="$samples/*">
         <xsl:call-template name="function">
            <xsl:with-param name="sampleVar" select="'samples'"/>
         </xsl:call-template>
      </xsl:for-each>

   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="sampleVar" as="xs:string"/>

      <div class="function-doc">
         <h2 id="{replace(name(), ':', '-')}">
            <a href="http://www.exslt.org/exsl/functions/{local-name()}/index.html">
               <xsl:value-of select="name()"/>
            </a>
         </h2>
         <div class="sample-code">
            <xsl:variable name="sampleCode" select="document('')/*/xsl:variable[@name=$sampleVar]/*[local-name()=local-name(current())]" as="element()"/>

            <xsl:call-template name="app:highlight-xslt">
               <xsl:with-param name="items" select="$sampleCode/*"/>
            </xsl:call-template>

            <xsl:if test="string()">
               <xsl:value-of select="concat(' returns ', string())"/>
            </xsl:if>
         </div>
      </div>
   </xsl:template>

</xsl:stylesheet>
