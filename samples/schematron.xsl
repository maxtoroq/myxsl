<?xml version="1.0" encoding="utf-8"?>
<?page processor="saxon" accept-verbs="GET,HEAD,POST" request:bind-initial-template="http-method" ?>

<xsl:stylesheet version="2.0" exclude-result-prefixes="#all"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:svrl="http://purl.oclc.org/dsdl/svrl"
   xmlns:request="http://myxsl.net/ns/web/request"
   xmlns:response="http://myxsl.net/ns/web/response"
   xmlns:validation="http://myxsl.net/ns/validation">

   <xsl:import href="layout.xslt"/>

   <xsl:output name="report" method="xml" indent="yes"/>

   <xsl:template name="HEAD">
      <xsl:call-template name="GET"/>
   </xsl:template>

   <xsl:template name="GET">
      <xsl:param name="contact-xml" as="document-node()?"/>
      <xsl:param name="contact-svrl" as="document-node()?"/>

      <xsl:call-template name="layout">
         <xsl:with-param name="instance" select="$contact-xml/*" tunnel="yes"/>
         <xsl:with-param name="report" select="$contact-svrl/*" tunnel="yes"/>
      </xsl:call-template>
   </xsl:template>

   <xsl:template name="POST">
      <xsl:param name="data">
         <xsl:element name="data" namespace="">
            <xsl:for-each select="'name', 'email', 'subject', 'comments'">
               <xsl:element name="{.}" namespace="">
                  <xsl:sequence select="request:form(.)"/>
               </xsl:element>
            </xsl:for-each>
         </xsl:element>
      </xsl:param>
      <xsl:param name="view-report" select="boolean((request:form('view-report'), '')[1])" as="xs:boolean" />

      <xsl:variable name="report">
         <xsl:sequence select="validation:validate-with-schematron('clitype:AppRules.contact', $data)"/>
      </xsl:variable>

      <xsl:choose>
         <xsl:when test="$view-report">
            <xsl:sequence select="response:set-content-type('application/xml')"/>
            <xsl:result-document format="report">
               <xsl:copy-of select="$report"/>
            </xsl:result-document>
         </xsl:when>
         <xsl:otherwise>
            <xsl:call-template name="GET">
               <xsl:with-param name="contact-xml" select="$data"/>
               <xsl:with-param name="contact-svrl" select="$report"/>
            </xsl:call-template>
         </xsl:otherwise>
      </xsl:choose>

   </xsl:template>

   <xsl:template name="content">
      <xsl:param name="instance" tunnel="yes"/>
      <xsl:param name="report" tunnel="yes"/>
      
      <h1>Schematron Validation</h1>
      <p>
         This is an example on how to use a Schematron schema to validate
         an XML document built using values from the form below.
      </p>
      <p>
         <a href="/redir_src.xqy?/App_Code/rules/contact.sch" target="_blank">Click here</a> to see the schema source.<br/>
      </p>
      <p>
         Using Rick Jelliffe's implementation of <a href="http://www.schematron.com">ISO Schematron</a>, the
         above schema is coverted to an XSLT validator, which is executed by the same processor used to render
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