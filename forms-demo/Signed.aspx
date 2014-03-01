<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Signed.aspx.cs" Inherits="forms_demo.Signed" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>
            <asp:Label ID="msg" runat="server" Text=""></asp:Label><br />
            <asp:BulletedList ID="signatures" runat="server"></asp:BulletedList>
        </p>
        
        <asp:Button ID="home" runat="server" Text="Home" OnClick="home_Click" />
    </div>
    </form>
</body>
</html>
