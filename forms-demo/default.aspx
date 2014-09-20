<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="forms_demo._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Panel ID="Panel1" runat="server">
            <asp:Label ID="LocationTag" runat="server" Text="Location: "></asp:Label>
            <asp:TextBox ID="location" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="RoleTag" runat="server" Text="Role: "></asp:Label>
            <asp:TextBox ID="role" runat="server"></asp:TextBox>
        </asp:Panel>
    
        <asp:Button ID="signButton" runat="server" Text="Sign PDF" OnClick="signButton_Click" />
    
    </div>
    </form>
</body>
</html>
