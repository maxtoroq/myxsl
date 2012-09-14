<%@ Page Language="C#" %>
<%@ Import Namespace="System.Net" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
   <script runat="server">
      
      protected int statusCode;
      protected string title;
      protected string message;

      protected void Page_Load(object sender, EventArgs e) {
         try {
            HttpException httpEx = (HttpException)Server.GetLastError();
            statusCode = httpEx.GetHttpCode();
            title = ((HttpStatusCode)statusCode).ToString();
            message = httpEx.Message;

            this.Response.StatusCode = statusCode;
         } catch { 
            // Error page cannot fail
         }
      }
      
   </script>
   <title><%= statusCode %> - <%= title %></title>
</head>
<body>
   <div>
      <h1><%= statusCode %> - <%= title %></h1>
      <p><%= message %></p>
   </div>

   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
   <!-- Prevent friendly errors -->
</body>
</html>

