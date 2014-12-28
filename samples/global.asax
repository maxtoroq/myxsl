<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

   void Application_Start(object sender, EventArgs e) {

      // when a reference exists to both myxsl.xml.xsl and myxsl.saxon
      // saxon is used as default
      myxsl.common.Processors.Xslt.Default = "system";
      
      RegisterRoutes(RouteTable.Routes);
   }
   
   void RegisterRoutes(RouteCollection routes) {

      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(null, "{controller}/{action}/{id}",
         new { action = "index", id = UrlParameter.Optional },
         new { controller = @"^(mvc)$" }
      );
   }
       
</script>

