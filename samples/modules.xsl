<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.1" exclude-result-prefixes="exsl xs fn web request response code doc"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:exsl="http://exslt.org/common"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:web="http://myxsl.github.io/ns/web"
   xmlns:request="http://myxsl.github.io/ns/web/request"
   xmlns:response="http://myxsl.github.io/ns/web/response"
   xmlns:code="http://myxsl.github.io/ns/code"
   xmlns:doc="http://myxsl.net/doc-html"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>
   <xsl:import href="~/App_Code/doc-html.xsl"/>

   <xsl:param name="functionLibrary" as="document(element(library))" web:bind="code:FunctionLibrary.Instance" />
   <xsl:param name="documentation" select="document('~/Bin/myxsl.xml')" as="document(element(doc))"/>

   <xsl:param name="pathInfo" select="request:path-info()"/>
   
   <xsl:variable name="moduleNs" select="concat('http:/', $pathInfo)" as="xs:string"/>
   <xsl:variable name="module" select="$functionLibrary/*/module[@namespace=$moduleNs]" as="element(module)?"/>
   <xsl:variable name="doc:module" select="$module"/>

   <xsl:template match="/" name="main">
      <xsl:choose>
         <xsl:when test="request:query('xml') = '1'">
            <xsl:value-of select="response:set-content-type('application/xml')"/>
            <xsl:copy-of select="$functionLibrary"/>
         </xsl:when>
         <xsl:otherwise>
            <xsl:apply-imports/>
         </xsl:otherwise>
      </xsl:choose>
   </xsl:template>

   <xsl:template name="content">

      <xsl:choose>
         <xsl:when test="$pathInfo">
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

      <xsl:variable name="moduleDoc" select="$documentation/*/members/member[@name=$module/@cref]"/>

      <xsl:apply-templates select="$moduleDoc/summary" mode="doc:html"/>

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
         <xsl:for-each select="fn:tokenize('abcdefghijklmnopqrstuvwxyz', '')">
            <xsl:sort select="."/>
            <xsl:if test="string()">
               <xsl:variable name="s" select="$module/function[starts-with(substring-after(@name, ':'), current())]"/>
               <xsl:if test="$s">
                  <li>
                     <xsl:for-each select="fn:distinct-values($s/@name)">
                        <xsl:sort select="."/>

                        <a href="#{substring-after(., ':')}">
                           <xsl:value-of select="substring-after(., ':')"/>
                        </a>
                        <xsl:text>&#160;&#160;</xsl:text>
                     </xsl:for-each>
                  </li>
               </xsl:if>
            </xsl:if>
         </xsl:for-each>
      </ul>

      <xsl:variable name="functions" select="function"/>

      <xsl:for-each select="fn:distinct-values($functions/@name)">
         <xsl:sort select="."/>
         <xsl:call-template name="function">
            <xsl:with-param name="overloads" select="$functions[@name=current()]"/>
         </xsl:call-template>
      </xsl:for-each>

      <xsl:choose>
         <xsl:when test="starts-with($module/@namespace, 'http://expath.org/')">
            <h2>See also</h2>
            <ul>
               <li>
                  <a href="{fn:replace($module/@namespace, '/ns/', '/spec/')}">EXPath Specification</a>
               </li>
               <xsl:if test="fn:ends-with($module/@namespace, '/http-client')">
                  <li>
                     <a href="/expath/http-client">Live examples</a>
                  </li>
               </xsl:if>
            </ul>
         </xsl:when>
         <xsl:when test="$module/@namespace = 'http://myxsl.github.io/ns/schematron'">
            <h2>See also</h2>
            <ul>
               <li>
                  <a href="/schematron/">Live examples</a>
               </li>
            </ul>
         </xsl:when>
      </xsl:choose>

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

      <xsl:variable name="sortedOverloads" select="exsl:node-set($sortedOverloads-rtf)/*"/>

      <div class="function-doc">

         <h2 id="{substring-after($first/@name, ':')}">
            <xsl:value-of select="$first/@name"/>
            <a href="#" class="top">↑ top</a>
         </h2>

         <xsl:variable name="functionDoc" select="$documentation/*/members/member[@name=$sortedOverloads[last()]/@cref]"/>

         <xsl:if test="$functionDoc/summary">
            <h3>Summary</h3>
            <xsl:apply-templates select="$functionDoc/summary" mode="doc:html"/>
         </xsl:if>

         <xsl:variable name="returnTypes" select="fn:distinct-values($sortedOverloads/@as)"/>

         <h3>
            <xsl:text>Signature</xsl:text>
            <xsl:if test="count($returnTypes) > 1">s</xsl:if>
         </h3>

         <xsl:for-each select="$returnTypes">
            
            <xsl:variable name="returnType" select="string()"/>
            <xsl:variable name="sameReturnType" select="$sortedOverloads[@as=$returnType]"/>

            <pre class="xquery signature">
               <xsl:variable name="paramCount" select="count($sameReturnType[last()]/param)"/>

               <code class="function">
                  <xsl:value-of select="$first/@name"/>
               </code>
               <xsl:text>(</xsl:text>
               <xsl:for-each select="$sameReturnType[last()]/param">
                  <xsl:if test="$paramCount > 1">
                     <xsl:text>&#160;&#xa;   </xsl:text>
                  </xsl:if>
                  <xsl:variable name="pos" select="position()"/>
                  <xsl:variable name="optional" select="$sameReturnType[1][count(param) &lt; $pos]"/>

                  <xsl:if test="$optional">[</xsl:if>
                  <code class="var">
                     <xsl:value-of select="concat('$', @name)"/>
                  </code>
                  <xsl:text>&#160;</xsl:text>
                  <code class="kwrd">as</code>
                  <xsl:value-of select="concat(' ', @as)"/>
                  <xsl:if test="$optional">]</xsl:if>

                  <xsl:if test="position() != last()">, </xsl:if>
               </xsl:for-each>
               <xsl:if test="$paramCount > 1">&#160;&#xa;</xsl:if>
               <xsl:text>) </xsl:text>
               <code class="kwrd">as</code>
               <xsl:value-of select="concat(' ', $returnType)"/>
            </pre>
            
         </xsl:for-each>

         <xsl:if test="$functionDoc/remarks">
            <h3>Remarks</h3>
            <xsl:apply-templates select="$functionDoc/remarks" mode="doc:html"/>
         </xsl:if>
      </div>

   </xsl:template>

</xsl:stylesheet>
