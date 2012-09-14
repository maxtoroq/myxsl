<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon"?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="math"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="layout.xslt"/>

   <xsl:template name="html-head">
      <style>
         #content li { padding-bottom:1em; }
      </style>
   </xsl:template>

   <xsl:variable name="samples-xpath3" xmlns="">
      <math:acos>
         <xsl:value-of select="math:acos(.5)"/>
      </math:acos>
      <math:asin>
         <xsl:value-of select="math:asin(.5)"/>
      </math:asin>
      <math:atan>
         <xsl:value-of select="math:atan(.5)"/>
      </math:atan>
      <math:cos>
         <xsl:value-of select="math:cos(.5)"/>
      </math:cos>
      <math:exp>
         <xsl:value-of select="math:exp(2)"/>
      </math:exp>
      <math:exp10>
         <xsl:value-of select="math:exp10(.5)"/>
      </math:exp10>
      <math:log>
         <xsl:value-of select="math:log(2)"/>
      </math:log>
      <math:log10>
         <xsl:value-of select="math:log10(2)"/>
      </math:log10>
      <math:pi>
         <xsl:value-of select="math:pi()"/>
      </math:pi>
      <math:pow>
         <xsl:value-of select="math:pow(2, 3)"/>
      </math:pow>
      <math:sin>
         <xsl:value-of select="math:sin(.5)"/>
      </math:sin>
      <math:sqrt>
         <xsl:value-of select="math:sqrt(9)"/>
      </math:sqrt>
      <math:tan>
         <xsl:value-of select="math:tan(.5)"/>
      </math:tan>
   </xsl:variable>

   <xsl:template name="content">
      
      <h1>XPath 3.0 functions for Saxon-HE</h1>
      <p>
         myxsl.net implements the following <a href="http://www.w3.org/TR/xpath-functions-30/">XPath 3.0 functions</a>
         for Saxon-HE (which currently supports XPath 2.0 only):
      </p>
      <ul>
         <xsl:for-each select="$samples-xpath3/*">
            <li>
               <a href="http://www.w3.org/TR/xpath-functions-30/#func-{translate(name(), ':', '-')}">
                  <xsl:value-of select="name()"/>
               </a>
               <xsl:if test="count(node())">
                  <br/>
                  <xsl:text>e.g. </xsl:text>
                  <code>
                     <xsl:value-of select="document('')/*/xsl:variable[@name='samples-xpath3']/*[local-name()=local-name(current())]/*/@select"/>
                  </code>
                  <xsl:value-of select="concat(' returns ', string())"/>
               </xsl:if>
            </li>
         </xsl:for-each>
      </ul>
   </xsl:template>

</xsl:stylesheet>
