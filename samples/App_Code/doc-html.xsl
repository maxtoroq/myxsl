<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" exclude-result-prefixes="xs local"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:local="http://maxtoroq.github.io/myxsl/doc-html"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:variable name="local:module" as="element(module)?"/>

   <xsl:template match="@*|node()" mode="local:html">
      <xsl:copy>
         <xsl:apply-templates select="@*|node()" mode="local:html"/>
      </xsl:copy>
   </xsl:template>

   <xsl:template match="code|strong" mode="local:html">
      <xsl:element name="{local-name()}">
         <xsl:apply-templates select="@*|node()" mode="local:html"/>
      </xsl:element>
   </xsl:template>

   <xsl:template match="summary" mode="local:html">
      <div class="summary">
         <xsl:apply-templates mode="local:html"/>
      </div>
   </xsl:template>

   <xsl:template match="remarks" mode="local:html">
      <div class="remarks">
         <xsl:apply-templates mode="local:html"/>
      </div>
   </xsl:template>

   <xsl:template match="paramref" mode="local:html">
      <code>
         <xsl:value-of select="concat('$', @name)"/>
      </code>
   </xsl:template>

   <xsl:template match="para" mode="local:html">
      <p>
         <xsl:apply-templates mode="local:html"/>
      </p>
   </xsl:template>

   <xsl:template match="see" mode="local:html">

      <xsl:variable name="otherFunction" select="$module/function[@cref=current()/@cref]"/>
      <xsl:variable name="localName" select="substring-after($otherFunction/@name, ':')"/>

      <a href="#{$localName}">
         <xsl:value-of select="$localName"/>
      </a>
   </xsl:template>

</xsl:stylesheet>
