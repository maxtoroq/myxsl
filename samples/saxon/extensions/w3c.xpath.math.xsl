<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon"?>
<?output-cache cache-profile="library" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:app="http://maxtoroq.github.io/myxsl/"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>
   <xsl:import href="~/App_Code/xslt_highlighter_api.xsl"/>

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
      
      <h1>XPath 3.0 Math</h1>
      <p>
         <a href="http://www.w3.org/TR/xpath-functions-30/">XPath 3.0 Math functions</a>
         are natively available in Saxon-PE and Saxon-EE. <strong>myxsl</strong> provides its own implementations for Saxon-HE.
      </p>
      <h2>Namespace Bindings</h2>
      <ul>
         <li>math = <strong>http://www.w3.org/2005/xpath-functions/math</strong></li>
      </ul>

      <h2>Function Index</h2>
      <ul>
         <xsl:for-each-group select="$samples-xpath3/*" group-by="substring(local-name(), 1, 1)">
            <xsl:sort select="current-grouping-key()"/>

            <li>
               <xsl:for-each select="current-group()">
                  <xsl:sort select="local-name()"/>
                  
                  <a href="#{replace(name(), ':', '-')}">
                     <xsl:value-of select="local-name()"/>
                  </a>
                  <xsl:text>&#160;&#160;</xsl:text>
               </xsl:for-each>
            </li>
         </xsl:for-each-group>
      </ul>

      <xsl:for-each select="$samples-xpath3/*">
         <xsl:call-template name="function">
            <xsl:with-param name="sampleVar" select="'samples-xpath3'"/>
         </xsl:call-template>
      </xsl:for-each>
      
   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="sampleVar" as="xs:string"/>

      <div class="function-doc">
         <h2 id="{replace(name(), ':', '-')}">
            <xsl:value-of select="name()"/>
            <a href="#" class="top">↑ top</a>
         </h2>
         <h3>Examples</h3>
         <div class="sample-code">
            <xsl:variable name="sampleCode" select="document('')/*/xsl:variable[@name=$sampleVar]/*[local-name()=local-name(current())]" as="element()"/>

            <xsl:call-template name="app:highlight-xslt">
               <xsl:with-param name="items" select="$sampleCode/*"/>
            </xsl:call-template>

            <xsl:if test="string()">
               <xsl:text> returns </xsl:text>
               <code>
                  <xsl:value-of select="string()"/>
               </code>
            </xsl:if>
         </div>
         <h3>See also</h3>
         <ul>
            <li>
               <a href="http://www.w3.org/TR/xpath-functions-30/#func-{replace(name(), ':', '-')}">XPath 3.0 Functions and Operators</a>
            </li>
         </ul>
      </div>
   </xsl:template>

</xsl:stylesheet>
