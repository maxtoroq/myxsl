<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="internal"
   xmlns:loc="com.qutoric.sketchpath.functions">

   <xsl:import href="xmlspectrum/app/xsl/xmlspectrum.xsl"/>

   <xsl:param name="source"/>
   <xsl:param name="xslt-prefix" select="'xsl'"/>

   <xsl:template name="main">
      <xsl:if test="$source">
         <xsl:variable name="spans" as="item()*">
            <xsl:call-template name="get-result-spans">
               <xsl:with-param name="file-content" select="$source"/>
               <xsl:with-param name="is-xml" select="starts-with(normalize-space($source), '&lt;')"/>
               <xsl:with-param name="is-xsl" select="true()"/>
               <xsl:with-param name="root-prefix" select="$xslt-prefix"/>
            </xsl:call-template>
         </xsl:variable>

         <xsl:apply-templates mode="html2xhtml" select="$spans"/>
      </xsl:if>
   </xsl:template>

   <xsl:template name="get-result-spans">
      <xsl:param name="is-xml" as="xs:boolean"/>
      <xsl:param name="is-xsl" as="xs:boolean"/>
      <xsl:param name="indent-size" select="-1" as="xs:integer"/>
      <xsl:param name="root-prefix"/>
      <xsl:param name="file-content" as="xs:string"/>
      <xsl:param name="do-trim" select="false()"/>

      <xsl:choose>
         <xsl:when test="$is-xml and $indent-size lt 0 and not($do-trim)">
            <!-- for case where XPath is embedded in XML text -->
            <xsl:sequence select="f:render($file-content, $is-xsl, $root-prefix)"/>
         </xsl:when>
         <xsl:when test="$is-xml">
            <!-- for case where XPath is embedded in XML text and indentation required -->
            <xsl:variable name="spans" select="f:render($file-content, $is-xsl, $root-prefix)"/>
            <xsl:variable name="real-indent" select="if ($indent-size lt 0) then 0 else $indent-size" as="xs:integer"/>
            <xsl:sequence select="f:indent($spans, $real-indent, $do-trim)"/>
         </xsl:when>
         <xsl:otherwise>
            <!-- for case where XPath is standalone -->
            <xsl:sequence select="loc:showXPath($file-content)"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>

   <xsl:template match="/|node()|@*" mode="html2xhtml" priority="0">
      <xsl:copy>
         <xsl:apply-templates mode="#current"/>
      </xsl:copy>
   </xsl:template>

   <xsl:template match="element()" mode="html2xhtml" priority="0.1">
      <xsl:element name="{local-name()}" namespace="http://www.w3.org/1999/xhtml">
         <xsl:apply-templates select="@*,node()" mode="#current"/>
      </xsl:element>
   </xsl:template>

</xsl:stylesheet>
