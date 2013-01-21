<?xml version="1.0" encoding="utf-8"?>
<?page processor="system" accept-verbs="GET,HEAD,POST" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:exsl="http://exslt.org/common"
   xmlns:svrl="http://purl.oclc.org/dsdl/svrl"
   xmlns:request="http://myxsl.net/ns/web/request"
   xmlns:response="http://myxsl.net/ns/web/response"
   xmlns:validation="http://myxsl.net/ns/validation"
   xmlns="http://www.w3.org/1999/xhtml">

   <xsl:import href="~/layout.xslt"/>

   <xsl:template match="/" name="main">
      
      <xsl:choose>
         <xsl:when test="request:http-method() = 'POST'">

            <xsl:variable name="data-rtf">
               <data xmlns="">
                  <name>
                     <xsl:value-of select="request:form('name')"/>
                  </name>
                  <email>
                     <xsl:value-of select="request:form('email')"/>
                  </email>
                  <subject>
                     <xsl:value-of select="request:form('subject')"/>
                  </subject>
                  <comments>
                     <xsl:value-of select="request:form('comments')"/>
                  </comments>
               </data>
            </xsl:variable>

            <xsl:variable name="data" select="exsl:node-set($data-rtf)"/>
            <xsl:variable name="view-report" select="boolean(request:form('view-report'))" as="xs:boolean" />

            <xsl:variable name="schema-rtf">
               <schema xmlns="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt1">
                  <ns prefix="fn" uri="http://www.w3.org/2005/xpath-functions"/>
                  <pattern>
                     <title>Grammar check</title>
                     <rule context="/">
                        <assert test="count(*/*[fn:matches(name(), '^(name|email|comments)$')]) >= 3">
                           Missing content.
                        </assert>
                     </rule>
                  </pattern>

                  <pattern id="name-length" is-a="string-length">
                     <param name="el" value="name"/>
                     <param name="max-chars" value="20"/>
                     <param name="required" value="true()"/>
                  </pattern>

                  <pattern id="email-length" is-a="string-length">
                     <param name="el" value="email"/>
                     <param name="max-chars" value="50"/>
                     <param name="required" value="true()"/>
                  </pattern>

                  <pattern id="email-syntax">
                     <rule context="email[string-length() > 0]">
                        <assert test='fn:matches(string(), "\w+([-+.&apos;&apos;]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")'>
                           The e-mail is invalid.
                        </assert>
                     </rule>
                  </pattern>

                  <pattern id="subject-length" is-a="string-length">
                     <param name="el" value="subject"/>
                     <param name="max-chars" value="20"/>
                  </pattern>

                  <pattern id="comments-length" is-a="string-length">
                     <param name="el" value="comments"/>
                     <param name="max-chars" value="1000"/>
                     <param name="required" value="true()"/>
                  </pattern>

                  <pattern id="string-length" abstract="true">
                     <title>Validates input length.</title>

                     <rule context="$el">
                        <let name="len" value="string-length()" />
                        <let name="required" value="false()"/>

                        <assert test="not($required) or $len > 0">
                           You must enter your <name/>.
                        </assert>
                        <assert test="$len &lt;= $max-chars">
                           Maximum characters allowed: <value-of select="$max-chars" />
                           (current: <value-of select="$len" />).
                        </assert>
                     </rule>
                  </pattern>
               </schema>
            </xsl:variable>

            <xsl:variable name="schema" select="exsl:node-set($schema-rtf)"/>
            <xsl:variable name="report" select="exsl:node-set(validation:schematron-report($data, $schema))"/>

            <xsl:choose>
               <xsl:when test="$view-report">
                  <xsl:value-of select="response:set-content-type('application/xml')"/>
                  <xsl:copy-of select="$report"/>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:call-template name="layout">
                     <xsl:with-param name="content">
                        <xsl:call-template name="content">
                           <xsl:with-param name="instance" select="$data/*"/>
                           <xsl:with-param name="report" select="$report/*"/>
                        </xsl:call-template>
                     </xsl:with-param>
                  </xsl:call-template>
               </xsl:otherwise>
            </xsl:choose>

         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="layout"/>
         </xsl:otherwise>
      </xsl:choose>

   </xsl:template>

   <xsl:template name="content">
      <xsl:param name="instance" select="/.."/>
      <xsl:param name="report" select="/.."/>
      
      <h1>Schematron Validation: XSLT 1.0</h1>
      <p>
         This is an example on how to use a Schematron schema to validate
         an XML document built using values from the form below.
      </p>
      <p>
         The schema can be provided <a href="/redir_src.xqy?{request:file-path()}#L42" target="_blank">inline</a> or as a URI.
      </p>
      <p>
         Using Rick Jelliffe's implementation of <a href="http://www.schematron.com">ISO Schematron</a>, the
         schema is coverted to an XSLT validator, which is executed by the same processor used to render
         this page.
      </p>
      <form id="form1" method="post">
         <xsl:if test="$report/svrl:failed-assert">
            <ul id="error-list" class="error">
               <xsl:for-each select="$report/svrl:failed-assert">
                  <li>
                     <xsl:value-of select="."/>
                  </li>
               </xsl:for-each>
            </ul>
         </xsl:if>
         <table>
            <tr>
               <td>Name:</td>
               <td>
                  <input type="text" name="name" value="{$instance/name}"/>
               </td>
            </tr>
            <tr>
               <td>E-mail:</td>
               <td>
                  <input type="text" name="email" value="{$instance/email}"/>
               </td>
            </tr>
            <tr>
               <td>Subject:</td>
               <td>
                  <input type="text" name="subject" value="{$instance/subject}"/>
               </td>
            </tr>
            <tr>
               <td>Comments:</td>
               <td>
                  <textarea name="comments" rows="3" cols="30">
                     <xsl:value-of select="$instance/comments"/>
                  </textarea>
               </td>
            </tr>
            <tr>
               <td></td>
               <td>
                  <input id="viewReport" type="checkbox" name="view-report" value="true"/>
                  <label for="viewReport">View Report</label>
               </td>
            </tr>
            <tr>
               <td></td>
               <td>
                  <input type="submit" value="Send"/>
               </td>
            </tr>
         </table>
      </form>
      
   </xsl:template>

</xsl:stylesheet>