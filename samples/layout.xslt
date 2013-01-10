<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="2.1" exclude-result-prefixes="html xs exsl request" 
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:html="http://www.w3.org/1999/xhtml"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:exsl="http://exslt.org/common"
   xmlns:request="http://myxsl.net/ns/web/request">

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
                  <xsl:with-param name="base-title" select="'myxsl.net'"/>
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
            <link rel="stylesheet" href="/Content/screen.css?v2013"/>
            <xsl:call-template name="html-head"/>
         </head>
         <body>
            <xsl:if test="not(request:is-local())">
               <script src="/Content/ga.js"></script>
            </xsl:if>
            <div id="lo-header">
               <span id="lo-logo">
                  <span class="myxsl">myxsl</span>
                  <span class="net">.net</span>
               </span>
            </div>
            <div id="lo-content">
               <xsl:copy-of select="$content"/>
            </div>
            <div id="lo-rightcol">
               <ul>
                  <li>
                     <a href="/">Index</a>
                  </li>
               </ul>
               <h3>About this page</h3>
               <ul>
                  <li>
                     <a href="/redir_src.xqy" target="_blank">Source code</a>
                  </li>
                  <li>
                     <span>Processor: </span>
                     <span class="xsl-vendor">
                        <xsl:value-of select="system-property('xsl:vendor')"/>
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
   
</xsl:stylesheet>

