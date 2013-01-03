<?xml version="1.0" encoding="utf-8"?>
<?page processor="system"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="exsl fn xs math"
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:fn="http://www.w3.org/2005/xpath-functions"
   xmlns:math="http://www.w3.org/2005/xpath-functions/math"
   xmlns:exsl="http://exslt.org/common"
   xmlns="http://www.w3.org/1999/xhtml">
   
   <xsl:import href="~/layout.xslt"/>

   <xsl:variable name="samples-xpath-rtf" xmlns="">
      <fn:abs>
         <xsl:value-of select="fn:abs(-5)" />
      </fn:abs>
      <fn:avg>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:avg(exsl:node-set($numbers-rtf)/*)" />
      </fn:avg>
      <fn:base-uri>
         <xsl:value-of select="fn:base-uri(document(''))"/>
      </fn:base-uri>
      <fn:compare>
         <xsl:value-of select="fn:compare('foo', 'bar')"/>
      </fn:compare>
      <fn:current-date>
         <xsl:value-of select="fn:current-date()"/>
      </fn:current-date>
      <fn:current-dateTime>
         <xsl:value-of select="fn:current-dateTime()"/>
      </fn:current-dateTime>
      <fn:current-time>
         <xsl:value-of select="fn:current-time()"/>
      </fn:current-time>
      <fn:distinct-values>
         <xsl:variable name="numbers-rtf">
            <num>2</num>
            <num>2</num>
            <num>5</num>
         </xsl:variable>
         <xsl:value-of select="fn:string-join(fn:distinct-values(exsl:node-set($numbers-rtf)/*), ', ')" />
      </fn:distinct-values>
      <fn:empty>
         <xsl:value-of select="fn:empty(document('')/foo)"/>
      </fn:empty>
      <fn:encode-for-uri>
         <xsl:value-of select="fn:encode-for-uri('100% organic')"/>
      </fn:encode-for-uri>
      <fn:ends-with>
         <xsl:value-of select="fn:ends-with('hello', 'lo')"/>
      </fn:ends-with>
      <fn:error/>
      <fn:exactly-one>
         <xsl:value-of select="fn:exactly-one(document('')/*/@version)"/>
      </fn:exactly-one>
      <fn:exists>
         <xsl:value-of select="fn:exists(document('')/foo)"/>
      </fn:exists>
      <fn:has-children>
         <xsl:value-of select="fn:has-children(document(''))"/>
      </fn:has-children>
      <fn:head>
         <xsl:value-of select="fn:head(document('')/*/@*)"/>
      </fn:head>
      <fn:in-scope-prefixes>
         <xsl:value-of select="fn:string-join(fn:in-scope-prefixes(document('')/*), ', ')" />
      </fn:in-scope-prefixes>
      <fn:lower-case>
         <xsl:value-of select="fn:lower-case('ABc!D')"/>
      </fn:lower-case>
      <fn:matches>
         <xsl:value-of select="fn:matches('abracadabra', 'BRA', 'i')"/>
      </fn:matches>
      <fn:max>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:max(exsl:node-set($numbers-rtf)/*)" />
      </fn:max>
      <fn:min>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:min(exsl:node-set($numbers-rtf)/*)" />
      </fn:min>
      <fn:namespace-uri-for-prefix>
         <xsl:value-of select="fn:namespace-uri-for-prefix('xsl', document('')/*)" />
      </fn:namespace-uri-for-prefix>
      <fn:one-or-more>
         <xsl:value-of select="fn:one-or-more(document('')/*/@*)"/>
      </fn:one-or-more>
      <fn:replace>
         <xsl:value-of select="fn:replace('abracadabra', 'bra', '*')"/>
      </fn:replace>
      <fn:resolve-uri>
         <xsl:value-of select="fn:resolve-uri('/foo', 'http://example.com/bar')"/>
      </fn:resolve-uri>
      <fn:reverse>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:reverse(exsl:node-set($numbers-rtf)/*)" />
      </fn:reverse>
      <fn:root>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:root(exsl:node-set($numbers-rtf)/*[1])"/>
      </fn:root>
      <fn:round-half-to-even>
         <xsl:value-of select="fn:round-half-to-even(-2.5)"/>
      </fn:round-half-to-even>
      <fn:serialize>
         <xsl:variable name="items-rtf">
            <a>1</a>
            <xsl:text>text</xsl:text>
            <b>2</b>
         </xsl:variable>
         <xsl:variable name="serialization-parameters-rtf">
            <output:serialization-parameters xmlns:output="http://www.w3.org/2010/xslt-xquery-serialization">
               <output:method value="text" />
            </output:serialization-parameters>
         </xsl:variable>
         <xsl:value-of select="fn:serialize(exsl:node-set($items-rtf)/node(), exsl:node-set($serialization-parameters-rtf))"/>
      </fn:serialize>
      <fn:string-join>
         <xsl:value-of select="fn:string-join(document('')/*/@*, ', ')"/>
      </fn:string-join>
      <fn:subsequence>
         <xsl:variable name="numbers-rtf">
            <num>1</num>
            <num>15</num>
            <num>48</num>
         </xsl:variable>
         <xsl:value-of select="fn:subsequence(exsl:node-set($numbers-rtf)/*, 2, 1)"/>
      </fn:subsequence>
      <fn:tail>
         <xsl:value-of select="fn:tail(document('')/*/@*)"/>
      </fn:tail>
      <fn:tokenize>
         <xsl:value-of select="fn:string-join(fn:tokenize('2006-12-25T12:15:00', '[\-T:]'), ', ')"/>
      </fn:tokenize>
      <fn:trace>
         <xsl:value-of select="fn:trace('test', '')"/>
      </fn:trace>
      <fn:upper-case>
         <xsl:value-of select="fn:upper-case('abcd')"/>
      </fn:upper-case>
      <fn:zero-or-one>
         <xsl:value-of select="fn:zero-or-one(document('')/*/@version)"/>
      </fn:zero-or-one>
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

   <xsl:variable name="samples-xpath" select="exsl:node-set($samples-xpath-rtf)"/>

   <xsl:template name="content">
      
      <h1>XPath 2.0 and 3.0 functions for XslCompiledTransform</h1>
      <p>
         myxsl.net implements the following <a href="http://www.w3.org/TR/xpath-functions-30/">XPath 2.0 and 3.0 functions</a>
         for XslCompiledTransform (which currently supports XPath 1.0 only):
      </p>
      
      <h2>Namespace Bindings</h2>
      <ul>
         <li>
            fn = <strong>http://www.w3.org/2005/xpath-functions</strong>
         </li>
         <li>
            math = <strong>http://www.w3.org/2005/xpath-functions/math</strong>
         </li>
         <li>
            xs = <strong>http://www.w3.org/2001/XMLSchema</strong>
         </li>
      </ul>

      <h2>Function Index</h2>
      <ul>
         <xsl:for-each select="$samples-xpath/*">
            <li>
               <a href="#{translate(name(), ':', '-')}">
                  <xsl:value-of select="name()"/>
               </a>
            </li>
         </xsl:for-each>
      </ul>

      <xsl:for-each select="$samples-xpath/*">
         <xsl:call-template name="function">
            <xsl:with-param name="sampleVar" select="'samples-xpath-rtf'"/>
         </xsl:call-template>
      </xsl:for-each>

   </xsl:template>

   <xsl:template name="function">
      <xsl:param name="sampleVar"/>

      <h2 id="{translate(name(), ':', '-')}">
         <a href="http://www.w3.org/TR/xpath-functions-30/#func-{translate(name(), ':', '-')}">
            <xsl:attribute name="href">
               <xsl:choose>
                  <xsl:when test="namespace-uri() = 'http://www.w3.org/2001/XMLSchema'">
                     <xsl:value-of select="concat('http://www.w3.org/TR/xmlschema-2/#', local-name())"/>
                  </xsl:when>
                  <xsl:otherwise>
                     <xsl:text>http://www.w3.org/TR/xpath-functions-30/#func-</xsl:text>
                     <xsl:if test="namespace-uri() = 'http://www.w3.org/2005/xpath-functions/math'">
                        <xsl:text>math-</xsl:text>
                     </xsl:if>
                     <xsl:value-of select="local-name()"/>
                  </xsl:otherwise>
               </xsl:choose>
            </xsl:attribute>
            <xsl:value-of select="name()"/>
         </a>
      </h2>
      <div>
         <xsl:variable name="sampleCode" select="document('')/*/xsl:variable[@name=$sampleVar]/*[name()=name(current())]"/>

         <code>
            <xsl:value-of select="$sampleCode/*/@select"/>
         </code>

         <xsl:value-of select="concat(' returns ', string())"/>
      </div>
   </xsl:template>

</xsl:stylesheet>
