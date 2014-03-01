<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Sign.aspx.cs" Inherits="forms_demo.Sign" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form name="PendingRequestForm" method="post" action="https://www.e-contract.be/dss-ws/start">
        <input type="hidden" id="PendingRequest" RunAt="Server" />
    </form>
    <script type="text/javascript">
        PendingRequestForm.submit();
    </script>
</body>
</html>
