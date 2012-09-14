<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

   // You can delete this file if you are not using ASP.NET MVC
   
   void Application_Start(object sender, EventArgs e) {
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

