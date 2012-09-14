<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="atom html" 
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:html="http://www.w3.org/1999/xhtml"
   xmlns:atom="http://www.w3.org/2005/Atom">

   <xsl:import href="layout.xslt" />

   <xsl:variable name="title" select="/atom:entry/atom:title" />

   <xsl:template name="content">
      <h1>
         <xsl:value-of select="/atom:entry/atom:title"/>
      </h1>
      <xsl:copy-of select="/atom:entry/atom:content/html:div/node()" />      
   </xsl:template>
   
</xsl:stylesheet>