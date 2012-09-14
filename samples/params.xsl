<?xml version="1.0" encoding="utf-8"?>
<?page language="C#" processor="saxon" accept-verbs="GET,HEAD,POST" request:bind-initial-template="http-method" validate-request="true" enable-session-state="true" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:code="http://myxsl.net/ns/code"
   xmlns:request="http://myxsl.net/ns/web/request"
   xmlns:response="http://myxsl.net/ns/web/response"
   xmlns:session="http://myxsl.net/ns/web/session">

   <xsl:import href="layout.xslt"/>

   <xsl:param name="tags" as="xs:string*" code:bind="Request.PathInfo.Split('/').Skip(1)" />
   <xsl:param name="number" as="xs:integer?" request:bind="query"/>
   <xsl:param name="session-val" session:bind="?remove=true"/>
   <xsl:param name="user-agent" as="xs:string?" request:bind="header"/>

   <xsl:variable name="examples" as="xs:string*">
      <xsl:sequence select="'/tags/xslt/xquery'"/>
      <xsl:sequence select="'?number=5'"/>
      <!--<xsl:sequence select="'?number=5&amp;number=6'"/>
      <xsl:sequence select="'?number=foo'"/>-->
   </xsl:variable>

   <xsl:template name="HEAD">
      <xsl:call-template name="GET"/>
   </xsl:template>

   <xsl:template name="GET">
      <xsl:call-template name="layout"/>
   </xsl:template>

   <xsl:template name="POST">
      <xsl:param name="session-val-input" select="request:form('session-val-input')" as="xs:string?"/>

      <xsl:sequence select="
         session:set('session-val', $session-val-input),
         response:redirect(request:url('PathAndQuery'))
      "/>
   </xsl:template>

   <xsl:template name="html-head">
      <style>
         #examples-results { font-weight: bold; }
      </style>
   </xsl:template>
      
   <xsl:template name="content">

      <h1>Parameter binding</h1>
      <p>
         With parameter binding you can declaratively bind global parameters to values of
         several sources, such as querystring, form, session state, cookies,
         etc. Although you can pull these values using extension functions, it's
         best to keep your stylesheet portable and testable in different
         environments.
      </p>
      <p>
         You also get type and length validation. Type validation is done using the
         declared sequence type (if the <code>as</code> attribute is used). Length validation depends on the sequence
         type occurrence indicator («», «?», «+», «*»), and/or by using the <code>required</code> attribute.
         These validations are done before invoking the stylesheet, and any failure
         results in the end of the HTTP response with a 400 (Bad Request) status.
      </p>

      <p>Examples:</p>

      <div id="examples-results">
         <div id="tags">
            <xsl:for-each select="$tags[position() gt 1]">
               <p>
                  tag: <span class="tag-name">
                     <xsl:value-of select="."/>
                  </span><br/>
                  length: <span class="tag-length">
                     <xsl:value-of select="string-length()"/>
                  </span>
               </p>
            </xsl:for-each>
         </div>

         <xsl:if test="$number">
            <p id="number">
               number: <span class="value">
                  <xsl:value-of select="$number"/>
               </span>
            </p>
         </xsl:if>

         <xsl:if test="$session-val">
            <p id="session-val">
               The session value is: <span class="value">
                  <xsl:value-of select="$session-val"/>
               </span>
            </p>
         </xsl:if>
         
      </div>

      <form method="post">
         <ul id="examples">
            <xsl:for-each select="for $e in $examples return concat(request:file-path(), $e)">
               <li>
                  <a href="{.}">
                     <xsl:value-of select="."/>
                  </a>
               </li>
            </xsl:for-each>
            
            <li>
               Set a session value: <input name="session-val-input" maxlength="10"/>
            </li>
         </ul>
      </form>

      <p id="user-agent">
         Your User-Agent is: <span class="value">
            <xsl:value-of select="$user-agent"/>
         </span>
      </p>

   </xsl:template>
   
</xsl:stylesheet>
