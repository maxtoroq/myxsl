<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.1" exclude-result-prefixes="exsl xs fn request response code"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:exsl="http://exslt.org/common"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:request="http://myxsl.net/ns/web/request"
   xmlns:response="http://myxsl.net/ns/web/response"
   xmlns:code="http://myxsl.net/ns/code"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="layout.xslt"/>

   <xsl:param name="functionLibrary" as="document(element(library))" code:bind="FunctionLibrary.Instance" />

   <xsl:template name="html-head">
      <style>
         pre.xquery code.function { font-weight: bold; }
         pre.xquery code.kwrd { color: blue; }
         pre.xquery code.var { color: #07A; }
      </style>
   </xsl:template>

   <xsl:template name="content">

      <xsl:variable name="pathInfo" select="request:path-info()"/>

      <xsl:choose>
         <xsl:when test="$pathInfo">
            <xsl:variable name="moduleNs" select="concat('http:/', $pathInfo)" as="xs:string"/>
            <xsl:variable name="module" select="$functionLibrary/*/module[@namespace=$moduleNs]" as="element(module)?"/>

            <xsl:choose>
               <xsl:when test="$module">
                  <xsl:apply-templates select="$module"/>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:value-of select="response:set-status(404)"/>
                  <h1>
                     Module <xsl:value-of select="$moduleNs"/> not found.
                  </h1>
               </xsl:otherwise>
            </xsl:choose>

         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-templates select="$functionLibrary/*"/>
         </xsl:otherwise>
      </xsl:choose>

   </xsl:template>

   <xsl:template match="library">

      <h1>Function Library</h1>

      <ul>
         <xsl:for-each select="module">
            <xsl:sort select="@namespace"/>
            <li>
               <a href="{concat('/modules.xsl/', substring-after(@namespace, 'http://'))}">
                  <xsl:value-of select="@namespace"/>
               </a>
            </li>
         </xsl:for-each>
      </ul>

   </xsl:template>

   <xsl:template match="module">
      <xsl:variable name="module" select="."/>

      <h1>
         <xsl:value-of select="@namespace"/>
      </h1>

      <h2>Namespace Bindings</h2>
      <ul>
         <xsl:for-each select="fn:in-scope-prefixes(.)">
            <xsl:if test=". != 'xml'">
               <li>
                  <xsl:value-of select="."/>
                  <xsl:text> = </xsl:text>
                  <strong>
                     <xsl:value-of select="fn:namespace-uri-for-prefix(., $module)"/>
                  </strong>
                  <xsl:if test="(
                     (. = $module/@predeclaredPrefix)
                     or (. = 'xml' and fn:namespace-uri-for-prefix(., $module) = 'http://www.w3.org/XML/1998/namespace')
                     or (. = 'xs' and fn:namespace-uri-for-prefix(., $module) = 'http://www.w3.org/2001/XMLSchema')
                     or (. = 'xsi' and fn:namespace-uri-for-prefix(., $module) = 'http://www.w3.org/2001/XMLSchema-instance')
                     or (. = 'fn' and fn:namespace-uri-for-prefix(., $module) = 'http://www.w3.org/2005/xpath-functions')
                     or (. = 'local' and fn:namespace-uri-for-prefix(., $module) = 'http://www.w3.org/2005/xquery-local-functions')
                  )"> (predeclared in XQuery)</xsl:if>
               </li>
            </xsl:if>
         </xsl:for-each>
      </ul>

      <h2>Function Index</h2>

      <ul>
         <xsl:for-each select="fn:distinct-values(function/@name)">
            <xsl:sort select="."/>
            <li>
               <a href="#{substring-after(., ':')}">
                  <xsl:value-of select="."/>
               </a>
            </li>
         </xsl:for-each>
      </ul>

      <xsl:variable name="functions" select="function"/>

      <xsl:for-each select="fn:distinct-values(function/@name)">
         <xsl:sort select="."/>
         <xsl:call-template name="function">
            <xsl:with-param name="overloads" select="$functions[@name=current()]"/>
         </xsl:call-template>
      </xsl:for-each>

   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="overloads" as="element(function)+"/>

      <xsl:variable name="first" select="$overloads[1]"/>
      
      <xsl:variable name="sortedOverloads-rtf">
         <xsl:for-each select="$overloads">
            <xsl:sort select="count(param)"/>
            <xsl:copy-of select="."/>
         </xsl:for-each>
      </xsl:variable>

      <h2 id="{substring-after($first/@name, ':')}">
         <xsl:value-of select="$first/@name"/>
      </h2>

      <!--<h3>Signature</h3>-->
      <xsl:for-each select="$overloads">
         <xsl:sort select="count(param)"/>
         <pre class="xquery">
            
            <xsl:variable name="paramCount" select="count(param)"/>

            <!--<xsl:if test="position() > 1">
               <xsl:text>&#160;&#xa;&#160;&#xa;</xsl:text>
            </xsl:if>-->
            <code class="function">
               <xsl:value-of select="$first/@name"/>
            </code>
            <xsl:text>(</xsl:text>
            <xsl:for-each select="param">
               <xsl:if test="$paramCount > 1">
                  <xsl:text>&#160;&#xa;   </xsl:text>
               </xsl:if>
               <code class="var">
                  <xsl:value-of select="concat('$', @name)"/>
               </code>
               <xsl:text>&#160;</xsl:text>
               <code class="kwrd">as</code>
               <xsl:value-of select="concat(' ', @as)"/>
               <xsl:if test="position() != last()">, </xsl:if>
            </xsl:for-each>
            <xsl:if test="$paramCount > 1">&#160;&#xa;</xsl:if>
            <xsl:text>) </xsl:text>
            <code class="kwrd">as</code>
            <xsl:value-of select="concat(' ', @as)"/>
         </pre>
         <xsl:if test="@description">
            <p>
               <xsl:value-of select="@description"/>
            </p>
         </xsl:if>
      </xsl:for-each>

      <xsl:choose>
         <xsl:when test="$first/../@namespace = 'http://expath.org/ns/http-client'">
            <a href="/expath/http-client">See examples</a>
         </xsl:when>
         <xsl:when test="
            $first/../@namespace = 'http://myxsl.net/ns/validation'
            and substring-after($first/@name, ':') = 'validate-with-schematron'">
            <a href="/schematron.xsl">See examples</a>
         </xsl:when>
      </xsl:choose>

   </xsl:template>

</xsl:stylesheet>
