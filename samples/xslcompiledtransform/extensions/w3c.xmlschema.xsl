<?xml version="1.0" encoding="utf-8"?>
<?page processor="system"?>
<?output-cache cache-profile="library" ?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="exsl fn xs math app"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:exsl="http://exslt.org/common"
   xmlns:app="http://myxsl.net/"
   xmlns="http://www.w3.org/1999/xhtml">
   
   <xsl:import href="~/layout.xslt"/>
   <xsl:import href="~/App_Code/xslt_highlighter_api.xsl"/>

   <xsl:variable name="samples-rtf" xmlns="">
      <xs:boolean>
         <xsl:value-of select="xs:boolean('0')"/>
      </xs:boolean>
      <xs:date>
         <xsl:value-of select="xs:date('2002-10-10-05:00')"/>
      </xs:date>
      <xs:dateTime>
         <xsl:value-of select="xs:dateTime('2002-10-10T12:00:00-05:00')"/>
      </xs:dateTime>
      <xs:decimal>
         <xsl:value-of select="xs:decimal('2.10')"/>
      </xs:decimal>
      <xs:double>
         <xsl:value-of select="xs:double('-INF')"/>
      </xs:double>
      <xs:duration>
         <xsl:value-of select="xs:duration('P1347Y')"/>
      </xs:duration>
      <xs:float>
         <xsl:value-of select="xs:float('INF')"/>
      </xs:float>
      <xs:string>
         <xsl:value-of select="xs:string('a')"/>
      </xs:string>
      <xs:time>
         <xsl:value-of select="xs:time('13:20:00-05:00')"/>
      </xs:time>
   </xsl:variable>

   <xsl:variable name="samples" select="exsl:node-set($samples-rtf)"/>

   <xsl:template name="content">
      
      <h1>Constructor functions for XML Schema built-in atomic types for XslCompiledTransform</h1>
      <p>
         myxsl.net implements the following <a href="http://www.w3.org/TR/xpath-functions-30/#constructor-functions">Constructor functions for XML Schema built-in atomic types</a>
         for XslCompiledTransform.
      </p>
      
      <h2>Namespace Bindings</h2>
      <ul>
         <li>
            xs = <strong>http://www.w3.org/2001/XMLSchema</strong>
         </li>
      </ul>

      <h2>Function Index</h2>
      <ul>
         <xsl:for-each select="fn:tokenize('abcdefghijklmnopqrstuvwxyz', '')">
            <xsl:sort select="."/>
            <xsl:if test="string()">
               <xsl:variable name="s" select="$samples/*[starts-with(local-name(), current())]"/>
               <xsl:if test="$s">
                  <li>
                     <xsl:for-each select="$s">
                        <xsl:sort select="local-name()"/>

                        <a href="#{translate(name(), ':', '-')}">
                           <xsl:value-of select="local-name()"/>
                        </a>
                        <xsl:text>&#160;&#160;</xsl:text>
                     </xsl:for-each>
                  </li>
               </xsl:if>
            </xsl:if>
         </xsl:for-each>
      </ul>

      <xsl:for-each select="$samples/*">
         <xsl:call-template name="function">
            <xsl:with-param name="sampleVar" select="'samples-rtf'"/>
         </xsl:call-template>
      </xsl:for-each>

      <h2>See Also</h2>
      <ul>
         <li>
            <a href="w3c.xpath.xsl">XPath 2.0 and 3.0 functions for XslCompiledTransform</a>
         </li>
      </ul>

   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="sampleVar"/>

      <div class="function-doc">
         <h2 id="{translate(name(), ':', '-')}">
            <xsl:value-of select="name()"/>
            <a href="#" class="top">↑ top</a>
         </h2>
         <h3>Examples</h3>
         <div class="sample-code">
            <xsl:variable name="sampleCode" select="document('')/*/xsl:variable[@name=$sampleVar]/*[name()=name(current())]"/>

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
               <a href="http://www.w3.org/TR/xmlschema-2/#{local-name()}">XML Schema Part 2: Datatypes</a>
            </li>
         </ul>
      </div>
   </xsl:template>

</xsl:stylesheet>
