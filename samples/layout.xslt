<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="2.1" exclude-result-prefixes="fn xs exsl html request" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:exsl="http://exslt.org/common"
   xmlns:html="http://www.w3.org/1999/xhtml"
   xmlns:request="http://myxsl.github.io/ns/web/request"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:variable name="title" as="xs:string?"/>
   <xsl:variable name="title-mode" select="'prepend'"/>
   <xsl:variable name="title-separator" select="' - '" as="xs:string"/>

   <xsl:output method="xhtml" omit-xml-declaration="yes" indent="no"
     doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
     doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

   <xsl:template match="/" name="main">
      <xsl:call-template name="layout"/>
   </xsl:template>
   
   <xsl:template name="layout">
      <xsl:param name="content">
         <xsl:call-template name="content"/>
      </xsl:param>
      
      <html xmlns="http://www.w3.org/1999/xhtml">
         <head>
            <title>
               <xsl:call-template name="title">
                  <xsl:with-param name="base-title" select="'myxsl'"/>
                  <xsl:with-param name="title">
                     <xsl:variable name="h1" select="(exsl:node-set($content)//html:h1)[1]"/>
                     <xsl:choose>
                        <xsl:when test="$h1">
                           <xsl:value-of select="$h1"/>
                        </xsl:when>
                        <xsl:otherwise>
                           <xsl:value-of select="$title"/>
                        </xsl:otherwise>
                     </xsl:choose>
                  </xsl:with-param>
               </xsl:call-template>
            </title>
            <link rel="stylesheet" href="/Content/screen.css?v20130124"/>
            <link rel="stylesheet" href="/Content/vs-light.css"/>
            <xsl:call-template name="html-head"/>
         </head>
         <body>
            <div id="lo-header">
               <span id="lo-logo">
                  <span class="myxsl">myxsl</span>
               </span>
            </div>
            <div id="lo-content">
               <xsl:call-template name="breadcrumbs"/>
               <xsl:copy-of select="$content"/>
            </div>
            <div id="lo-rightcol">
               <h3>About this page</h3>
               <ul>
                  <li>
                     <a href="/redir_src.xqy" target="_blank">Source code</a>
                  </li>
                  <li>
                     <span>Processor: </span>
                     <span class="xsl-vendor">
                        <xsl:choose>
                           <xsl:when test="system-property('xsl:product-name')">
                              <xsl:value-of select="concat(system-property('xsl:product-name'), ' ', system-property('xsl:product-version'))"/>
                           </xsl:when>
                           <xsl:otherwise>
                              <xsl:value-of select="system-property('xsl:vendor')"/>
                           </xsl:otherwise>
                        </xsl:choose>
                     </span>
                     <br/>
                     <span>XSLT Version: </span>
                     <span>
                        <xsl:value-of select="system-property('xsl:version')"/>
                     </span>
                  </li>
               </ul>
            </div>
            <div id="lo-footer">
               <xsl:call-template name="footer"/>
            </div>
         </body>
      </html>
   </xsl:template>

   <xsl:template name="title">
      <xsl:param name="base-title"/>
      <xsl:param name="title"/>
      
      <xsl:choose>
         <xsl:when test="$title">
            <xsl:choose>
               <xsl:when test="$title-mode = 'append'">
                  <xsl:value-of select="concat($base-title, $title-separator, $title)"/>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:value-of select="concat($title, $title-separator, $base-title)"/>
               </xsl:otherwise>
            </xsl:choose>
         </xsl:when>
         <xsl:otherwise>
            <xsl:value-of select="$base-title"/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>

   <xsl:template name="html-head"/>
   <xsl:template name="footer"/>

   <xsl:template name="breadcrumbs">

      <xsl:variable name="pathInfo" select="request:path-info()"/>

      <xsl:variable name="path" as="xs:string">
         <xsl:choose>
            <xsl:when test="$pathInfo">
               <xsl:value-of select="request:file-path()"/>
            </xsl:when>
            <xsl:otherwise>
               <xsl:value-of select="request:path()"/>
            </xsl:otherwise>
         </xsl:choose>
      </xsl:variable>

      <xsl:variable name="parts" select="fn:tokenize($path, '/')[string()]"/>
      
      <xsl:if test="count($parts)">
         <div id="lo-breadcrumbs">
            <a href="/">home</a>
            <xsl:for-each select="$parts">
               <xsl:text> › </xsl:text>
               <xsl:variable name="pos" select="position()"/>
               <xsl:variable name="isFile" select="fn:matches(., '\.(xsl|xqy|atom|xhtml)$', 'i')"/>
               
               <a>
                  <xsl:attribute name="href">
                     <xsl:text>/</xsl:text>
                     <xsl:value-of select="fn:string-join($parts[position() &lt;= $pos], '/')"/>
                     <xsl:if test="not($isFile)">/</xsl:if>
                     <xsl:value-of select="request:url('Query,KeepDelimiter')"/>
                  </xsl:attribute>
                  <xsl:choose>
                     <xsl:when test="$isFile">
                        <xsl:value-of select="fn:string-join(fn:tokenize(., '\.')[position() &lt; last()], '.')"/>
                     </xsl:when>
                     <xsl:otherwise>
                        <xsl:value-of select="."/>
                     </xsl:otherwise>
                  </xsl:choose>
               </a>
            </xsl:for-each>
            <xsl:if test="$pathInfo">
               <xsl:text> › </xsl:text>
               <a href="{request:path()}">
                  <xsl:value-of select="substring($pathInfo, 2)"/>
               </a>
            </xsl:if>
         </div>
      </xsl:if>

   </xsl:template>
   
</xsl:stylesheet>

