<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="web request" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:web="http://maxtoroq.github.io/myxsl/web"
   xmlns:request="http://maxtoroq.github.io/myxsl/web/request">

   <xsl:param name="name" web:bind="request:query" />

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
