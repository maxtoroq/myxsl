<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" exclude-result-prefixes="fn xs exsl saxon xslt app"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:exsl="http://exslt.org/common"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:xslt="http://maxtoroq.github.io/myxsl/xslt"
   xmlns:app="http://myxsl.net/">

   <xsl:variable name="xslt-highlighter" select="xslt:compile('~/App_Code/xslt_highlighter.xsl', 'saxon')"/>
   
   <xsl:template name="app:highlight-xslt">
      <xsl:param name="items"/>

      <xsl:variable name="items2">
         <xsl:apply-templates select="$items" mode="normalize-sample-code"/>
      </xsl:variable>

      <xsl:variable name="items3" select="exsl:node-set($items2)/*"/>

      <xsl:variable name="exprOnly" select="count($items3) = 1 
         and $items3/self::xsl:value-of/@select
         and count($items3/@*) = 1"/>

      <xsl:variable name="params" xmlns="">
         <source>
            <xsl:choose>
               <xsl:when test="$exprOnly">
                  
                  <xsl:value-of select="$items3/@select"/>
               </xsl:when>
               <xsl:otherwise>

                  <xsl:variable name="str">
                     <xsl:choose>
                        <xsl:when test="function-available('fn:serialize')" use-when="function-available('fn:serialize')">
                           <xsl:variable name="serialization-params">
                              <output:serialization-parameters xmlns:output="http://www.w3.org/2010/xslt-xquery-serialization">
                                 <output:indent value="yes"/>
                              </output:serialization-parameters>
                           </xsl:variable>
                           <xsl:value-of select="fn:serialize($items3, exsl:node-set($serialization-params)/*)"/>
                        </xsl:when>
                        <xsl:when test="function-available('saxon:serialize')">
                           
                           <xsl:variable name="doc">
                              <xsl:sequence select="$items3"/>
                           </xsl:variable>

                           <xsl:variable name="output" as="element()">
                              <xsl:element name="xsl:output">
                                 <xsl:attribute name="indent" select="'yes'"/>
                                 <xsl:attribute name="omit-xml-declaration" select="'yes'"/>
                                 <xsl:attribute name="saxon:indent-spaces" namespace="http://saxon.sf.net/" select="2"/>
                              </xsl:element>
                           </xsl:variable>

                           <xsl:value-of select="saxon:serialize($doc, $output)"/>
                        </xsl:when>
                        <xsl:otherwise>
                           <xsl:message terminate="yes">Cannot serialize.</xsl:message>
                        </xsl:otherwise>
                     </xsl:choose>
                  </xsl:variable>

                  <xsl:value-of select="fn:replace($str, ' xmlns:xsl=&quot;http://www.w3.org/1999/XSL/Transform&quot;', '')"/>
               </xsl:otherwise>
            </xsl:choose>
         </source>
      </xsl:variable>

      <xsl:variable name="highlighted" select="xslt:transform-starting-at($xslt-highlighter, 'main', /.., exsl:node-set($params)/*)/*"/>

      <xsl:choose xmlns="http://www.w3.org/1999/xhtml">
         <xsl:when test="$exprOnly">
            <code>
               <xsl:copy-of select="$highlighted"/>
            </code>
         </xsl:when>
         <xsl:when test="fn:exists($highlighted)">
            <pre class="xslt">
               <xsl:copy-of select="$highlighted"/>
            </pre>
         </xsl:when>
      </xsl:choose>

   </xsl:template>

   <xsl:template match="/|@*|node()" mode="normalize-sample-code" priority="0">
      <xsl:copy>
         <xsl:copy-of select="@*"/>
         <xsl:apply-templates select="node()" mode="normalize-sample-code"/>
      </xsl:copy>
   </xsl:template>

   <!-- Remove unused namespaces -->
   <xsl:template match="*" mode="normalize-sample-code" priority="0.1">
      <xsl:element name="{name()}" namespace="{namespace-uri()}">
         <xsl:copy-of select="@*"/>
         <xsl:apply-templates select="node()" mode="normalize-sample-code"/>
      </xsl:element>
   </xsl:template>

   <!-- Remove insignificant whitespace -->
   <xsl:template match="*[* and not(text()[normalize-space()])]/text()" mode="normalize-sample-code" priority="0.1"/>

</xsl:stylesheet>
