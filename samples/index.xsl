<?xml version="1.0" encoding="utf-8" ?>
<?output-cache duration="60" vary-by-param="none" ?>

<xsl:stylesheet version="2.1" exclude-result-prefixes="fn web code"
   xmlns="http://www.w3.org/1999/xhtml"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:web="http://maxtoroq.github.io/myxsl/web"
   xmlns:code="http://maxtoroq.github.io/myxsl/code">

   <xsl:import href="layout.xslt"/>

   <xsl:param name="function-library" as="document-node(element(library))" web:bind="code:FunctionLibrary.Instance" />

   <xsl:variable name="title" select="'Build dynamic websites with XSLT'"/>
   <xsl:variable name="title-mode" select="'append'"/>
   
   <xsl:template name="content">
      
      <h1>Build dynamic websites with XSLT</h1>
      <p>
         <strong>myxsl</strong> is an open-source extension to the
         <a href="http://www.asp.net/">ASP.NET</a> framework that allows programmers to
         build dynamic websites using <a href="http://www.w3.org/Style/XSL/">XSLT</a> and
         other XML standards, such as <a href="http://www.w3.org/XML/Query/">XQuery</a>
         and <a href="http://www.schematron.com/">Schematron</a>.
      </p>
      <p>
         <strong>myxsl</strong> provides dynamic compilation using
         <a href="http://msdn.microsoft.com/en-us/library/system.web.compilation.buildprovider.aspx">build providers</a>
         that generate optimized code for your web pages.
      </p>
      <h2>
         Supported Processors
      </h2>
      <ul id="xslt-proc">
         <li>
            <a href="http://msdn.microsoft.com/en-us/library/system.xml.xsl.xslcompiledtransform.aspx">.NET Framework</a>
         </li>
         <li>
            <a href="http://saxon.sourceforge.net/">Saxon</a>
         </li>
      </ul>
      <div style="clear: both;"></div>
      <p>
         A provider design enables for other processors to plug-in.
      </p>

      <h2>Features</h2>
      <p>
         This project is still on alpha stage. Some features are more stable than others.
         So far, the only released feature is the <a href="/mvc">view engines</a> for ASP.NET MVC.
      </p>
      <ul>
         <li>
            <h3>ASP.NET integration</h3>
            <ul>
               <li>
                  <a href="/compilation.xsl">Compilation / Static errors detection</a>
               </li>
               <li>
                  <a href="/params.xsl">Parameter binding (XSLT only)</a>
               </li>
               <li>
                  <a href="/outputcache.xsl">Output caching (XSLT only)</a>
               </li>
               <li>
                  <a href="/trace.xsl">Tracing</a>
               </li>
               <li>
                  <a href="/mvc">MVC</a>
               </li>
               <li>
                  <a href="/simplified.xhtml">Simplified stylesheet</a>
               </li>
            </ul>
         </li>
         <li>
            <h3>Other XML standards</h3>
            <ul>
               <li>
                  <a href="/schematron/">Schematron</a>
               </li>
               <li>
                  <a href="/xml-stylesheet.atom">
                     <code>xml-stylesheet</code>
                  </a>
               </li>
            </ul>
         </li>
         <li>
            <h3>Functions</h3>
            <ul>
               <li>
                  <a href="/xslcompiledtransform/extensions/">XslCompiledTransform Extensions</a>
               </li>
               <li>
                  <a href="/saxon/extensions/">Saxon Extensions</a>
               </li>
            </ul>
            <br/>
            <ul>
               <xsl:for-each select="$function-library/*/module">
                  <xsl:sort select="@namespace"/>

                  <li>
                     <a href="{concat('/modules.xsl/', substring-after(@namespace, 'http://'))}">
                        <xsl:value-of select="@namespace"/>
                     </a>
                  </li>
               </xsl:for-each>
            </ul>
         </li>
      </ul>

      <h2 id="source">Source code</h2>
      <p>
         Visit the <a href="https://github.com/myxsl">project page</a>.
      </p>

      <h2 id="feedback">Feedback</h2>
      <p>
         Visit the <a href="https://github.com/maxtoroq/myxsl/issues">project forums</a> for feedback, support and discussion.
      </p>

      <h2>Resources</h2>
      <ul>
         <li>
            <a href="http://maxtoroq.github.io/2013/07/razor-vs-xslt.html">Razor vs. XSLT</a>
         </li>
         <li>
            <a href="http://www.onenaught.com/posts/8/xslt-in-server-side-web-frameworks">Why Use XSLT in Server Side Web Frameworks For Output Generation?</a>
         </li>
         <li>
            <a href="http://www.w3.org/2007/11/schema-for-xslt20.xsd">Schema for XSLT 2.0</a>:
            Get XSLT 2.0 intellisense on Visual Studio (be sure to remove the <code>@schemaLocation</code> attribute on
            <code>&lt;xsd:import></code> declarations).
         </li>
      </ul>
   
   </xsl:template>

</xsl:stylesheet>
