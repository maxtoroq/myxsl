<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="request" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:request="http://myxsl.net/ns/web/request">

   <xsl:param name="name" request:bind="query" />

   <xsl:output omit-xml-declaration="yes"/>

   <xsl:template match="/" name="main">

      <xsl:choose>
         <xsl:when test="$name">
            Hello, <xsl:value-of select="$name"/>
         </xsl:when>
         <xsl:otherwise>
            <form method="get">
               Enter your name: <input name="name" />
            </form>
         </xsl:otherwise>
      </xsl:choose>

   </xsl:template>

</xsl:stylesheet>
