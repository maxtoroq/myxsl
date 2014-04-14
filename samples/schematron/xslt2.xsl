<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon" accept-verbs="GET,HEAD,POST" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:svrl="http://purl.oclc.org/dsdl/svrl"
   xmlns:request="http://myxsl.github.io/ns/web/request"
   xmlns:response="http://myxsl.github.io/ns/web/response"
   xmlns:schematron="http://myxsl.github.io/ns/schematron">

   <xsl:import href="~/layout.xslt"/>

   <xsl:output name="report" method="xml" indent="yes"/>

   <xsl:template name="main">

      <xsl:choose>
         <xsl:when test="request:http-method() = 'POST'">

            <xsl:variable name="data">
               <xsl:element name="data" namespace="">
                  <xsl:for-each select="'name', 'email', 'subject', 'comments'">
                     <xsl:element name="{.}" namespace="">
                        <xsl:sequence select="request:form(.)"/>
                     </xsl:element>
                  </xsl:for-each>
               </xsl:element>
            </xsl:variable>

            <xsl:variable name="view-report" select="boolean((request:form('view-report'), '')[1])" as="xs:boolean" />

            <xsl:variable name="schema">
               <!-- Can provide schema as inline document or URI -->
               <schema xmlns="http://purl.oclc.org/dsdl/schematron">

                  <pattern>
                     <title>Grammar check</title>
                     <rule context="/">
                        <assert test="count((*,*/(name|email|comments))) ge 4">
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
                     <rule context="email[string-length() gt 0]">
                        <assert test="matches(string(), '\w+([-+.'']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*')">
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
                        <assert test="$len le $max-chars">
                           Maximum characters allowed: <value-of select="$max-chars" />
                           (current: <value-of select="$len" />).
                        </assert>
                     </rule>
                  </pattern>

               </schema>
            </xsl:variable>

            <xsl:variable name="report" select="schematron:report($schema, $data)"/>

            <xsl:choose>
               <xsl:when test="$view-report">
                  <xsl:sequence select="response:set-content-type('application/xml')"/>
                  <xsl:result-document format="report">
                     <xsl:copy-of select="$report"/>
                  </xsl:result-document>
               </xsl:when>
               <xsl:otherwise>
                  <xsl:call-template name="layout">
                     <xsl:with-param name="instance" select="$data/*" tunnel="yes"/>
                     <xsl:with-param name="report" select="$report/*" tunnel="yes"/>
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
      <xsl:param name="instance" select="()" tunnel="yes"/>
      <xsl:param name="report" select="()" tunnel="yes"/>
      
      <h1>Schematron Validation: XSLT 2.0</h1>
      <p>
         This is an example on how to use a Schematron schema to validate
         an XML document built using values from the form below.
      </p>
      <p>
         The schema can be provided <a href="/redir_src.xqy?{request:file-path()}#L34" target="_blank">inline</a> or as a URI.
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