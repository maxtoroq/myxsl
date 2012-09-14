<?xml version="1.0" encoding="utf-8"?>
<?page processor="system"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="exsl fn xs math"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:exsl="http://exslt.org/common"
   xmlns="http://www.w3.org/1999/xhtml">
   
   <xsl:import href="layout.xslt"/>

   <xsl:variable name="samples-xpath2-rtf" xmlns="">
      <abs>
         <xsl:value-of select="fn:abs(-5)" />
      </abs>
      <avg>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:avg(exsl:node-set($numbers-rtf)/*)" />
      </avg>
      <base-uri>
         <xsl:value-of select="fn:base-uri(document(''))"/>
      </base-uri>
      <compare>
         <xsl:value-of select="fn:compare('foo', 'bar')"/>
      </compare>
      <current-date>
         <xsl:value-of select="fn:current-date()"/>
      </current-date>
      <current-dateTime>
         <xsl:value-of select="fn:current-dateTime()"/>
      </current-dateTime>
      <current-time>
         <xsl:value-of select="fn:current-time()"/>
      </current-time>
      <distinct-values>
         <xsl:variable name="numbers-rtf">
            <num>2</num>
            <num>2</num>
            <num>5</num>
         </xsl:variable>
         <xsl:value-of select="fn:string-join(fn:distinct-values(exsl:node-set($numbers-rtf)/*), ', ')" />
      </distinct-values>
      <empty>
         <xsl:value-of select="fn:empty(document('')/foo)"/>
      </empty>
      <encode-for-uri>
         <xsl:value-of select="fn:encode-for-uri('100% organic')"/>
      </encode-for-uri>
      <ends-with>
         <xsl:value-of select="fn:ends-with('hello', 'lo')"/>
      </ends-with>
      <error/>
      <exactly-one>
         <xsl:value-of select="fn:exactly-one(document('')/*/@version)"/>
      </exactly-one>
      <exists>
         <xsl:value-of select="fn:exists(document('')/foo)"/>
      </exists>
      <in-scope-prefixes>
         <xsl:value-of select="fn:string-join(fn:in-scope-prefixes(document('')/*), ', ')" />
      </in-scope-prefixes>
      <lower-case>
         <xsl:value-of select="fn:lower-case('ABc!D')"/>
      </lower-case>
      <matches>
         <xsl:value-of select="fn:matches('abracadabra', 'BRA', 'i')"/>
      </matches>
      <max>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:max(exsl:node-set($numbers-rtf)/*)" />
      </max>
      <min>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:min(exsl:node-set($numbers-rtf)/*)" />
      </min>
      <namespace-uri-for-prefix>
         <xsl:value-of select="fn:namespace-uri-for-prefix('xsl', document('')/*)" />
      </namespace-uri-for-prefix>
      <one-or-more>
         <xsl:value-of select="fn:one-or-more(document('')/*/@*)"/>
      </one-or-more>
      <replace>
         <xsl:value-of select="fn:replace('abracadabra', 'bra', '*')"/>
      </replace>
      <resolve-uri>
         <xsl:value-of select="fn:resolve-uri('/foo', 'http://example.com/bar')"/>
      </resolve-uri>
      <reverse>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:reverse(exsl:node-set($numbers-rtf)/*)" />
      </reverse>
      <root>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:root(exsl:node-set($numbers-rtf)/*[1])"/>
      </root>
      <round-half-to-even>
         <xsl:value-of select="fn:round-half-to-even(-2.5)"/>
      </round-half-to-even>
      <string-join>
         <xsl:value-of select="fn:string-join(document('')/*/@*, ', ')"/>
      </string-join>
      <subsequence>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:subsequence(exsl:node-set($numbers-rtf)/*, 2, 1)"/>
      </subsequence>
      <tokenize>
         <xsl:value-of select="fn:string-join(fn:tokenize('2006-12-25T12:15:00', '[\-T:]'), ', ')"/>
      </tokenize>
      <trace>
         <xsl:value-of select="fn:trace('test', '')"/>
      </trace>
      <upper-case>
         <xsl:value-of select="fn:upper-case('abcd')"/>
      </upper-case>
      <zero-or-one>
         <xsl:value-of select="fn:zero-or-one(document('')/*/@version)"/>
      </zero-or-one>
      <xs:boolean>
         <xsl:value-of select="xs:boolean('0')"/>
      </xs:boolean>
      <xs:date>
         <xsl:value-of select="xs:date('2002-10-10-05:00')"/>
      </xs:date>
      <xs:dateTime>
         <xsl:value-of select="xs:dateTime('2002-10-10T12:00:00-05:00')"/>
      </xs:dateTime>
      <xs:decimal>
         <xsl:value-of select="xs:decimal('2.10')"/>
      </xs:decimal>
      <xs:double>
         <xsl:value-of select="xs:double('-INF')"/>
      </xs:double>
      <xs:duration>
         <xsl:value-of select="xs:duration('P1347Y')"/>
      </xs:duration>
      <xs:float>
         <xsl:value-of select="xs:float('INF')"/>
      </xs:float>
      <xs:string>
         <xsl:value-of select="xs:string('a')"/>
      </xs:string>
      <xs:time>
         <xsl:value-of select="xs:time('13:20:00-05:00')"/>
      </xs:time>
   </xsl:variable>

   <xsl:variable name="samples-xpath3-rtf" xmlns="">
      <has-children>
         <xsl:value-of select="fn:has-children(document(''))"/>
      </has-children>
      <head>
         <xsl:value-of select="fn:head(document('')/*/@*)"/>
      </head>
      <tail>
         <xsl:value-of select="fn:tail(document('')/*/@*)"/>
      </tail>
      <math:acos>
         <xsl:value-of select="math:acos(.5)"/>
      </math:acos>
      <math:asin>
         <xsl:value-of select="math:asin(.5)"/>
      </math:asin>
      <math:atan>
         <xsl:value-of select="math:atan(.5)"/>
      </math:atan>
      <math:cos>
         <xsl:value-of select="math:cos(.5)"/>
      </math:cos>
      <math:exp>
         <xsl:value-of select="math:exp(2)"/>
      </math:exp>
      <math:exp10>
         <xsl:value-of select="math:exp10(.5)"/>
      </math:exp10>
      <math:log>
         <xsl:value-of select="math:log(2)"/>
      </math:log>
      <math:log10>
         <xsl:value-of select="math:log10(2)"/>
      </math:log10>
      <math:pi>
         <xsl:value-of select="math:pi()"/>
      </math:pi>
      <math:pow>
         <xsl:value-of select="math:pow(2, 3)"/>
      </math:pow>
      <math:sin>
         <xsl:value-of select="math:sin(.5)"/>
      </math:sin>
      <math:sqrt>
         <xsl:value-of select="math:sqrt(9)"/>
      </math:sqrt>
      <math:tan>
         <xsl:value-of select="math:tan(.5)"/>
      </math:tan>
   </xsl:variable>
   
   <xsl:template name="html-head">
      <style>
         #content li { padding-bottom:1em; }
      </style>
   </xsl:template>
      
   <xsl:template name="content">
      
      <h1>XPath 2.0 functions for XslCompiledTransform</h1>
      <p>
         myxsl.net implements the following <a href="http://www.w3.org/TR/xpath-functions/">XPath 2.0 functions</a>
         for XslCompiledTransform (which currently supports XPath 1.0 only):
      </p>
      <ul>
         <xsl:for-each select="exsl:node-set($samples-xpath2-rtf)/*">
            <li>
               <xsl:choose>
                  <xsl:when test="namespace-uri() = 'http://www.w3.org/2001/XMLSchema'">
                     <a href="http://www.w3.org/TR/xmlschema-2/#{local-name()}">
                        <xsl:value-of select="name()"/>
                     </a>
                  </xsl:when>
                  <xsl:otherwise>
                     <a href="http://www.w3.org/TR/xpath-functions/#func-{local-name()}">
                        <xsl:value-of select="concat('fn:', name())"/>
                     </a>
                  </xsl:otherwise>
               </xsl:choose>
               <xsl:if test="count(node())">
                  <br/>
                  <xsl:text>e.g. </xsl:text>
                  <code>
                     <xsl:value-of select="document('')/*/xsl:variable[@name='samples-xpath2-rtf']/*[name()=name(current())]/*/@select"/>
                  </code>
                  <xsl:value-of select="concat(' returns ', string())"/>
               </xsl:if>
            </li>
         </xsl:for-each>
      </ul>
      
      <p>Also the following <a href="http://www.w3.org/TR/xpath-functions-30/">XPath 3.0 functions</a>:</p>
      <ul>
         <xsl:for-each select="exsl:node-set($samples-xpath3-rtf)/*">
            <li>
               <a href="http://www.w3.org/TR/xpath-functions-30/#func-{translate(name(), ':', '-')}">
                  <xsl:if test="not(contains(name(), ':'))">fn:</xsl:if>
                  <xsl:value-of select="name()"/>
               </a>
               <xsl:if test="count(node())">
                  <br/>
                  <xsl:text>e.g. </xsl:text>
                  <code>
                     <xsl:value-of select="document('')/*/xsl:variable[@name='samples-xpath3-rtf']/*[local-name()=local-name(current())]/*/@select"/>
                  </code>
                  <xsl:value-of select="' returns '"/>
                  <xsl:choose>
                     <xsl:when test="*">
                        <pre style="overflow:scroll">
                           <xsl:apply-templates select="*" mode="escape"/>
                        </pre>
                     </xsl:when>
                     <xsl:otherwise>
                        <xsl:value-of select="string()"/>
                     </xsl:otherwise>
                  </xsl:choose>
               </xsl:if>
            </li>
         </xsl:for-each>
      </ul>
   </xsl:template>

</xsl:stylesheet>
