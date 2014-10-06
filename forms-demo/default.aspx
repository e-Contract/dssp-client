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
            <br />
            <asp:Label ID="VisibleTag" runat="server" Text="Visible: "></asp:Label>
            <asp:DropDownList ID="visible" runat="server" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" AutoPostBack="true">
                <asp:ListItem>None</asp:ListItem>
                <asp:ListItem>Photo</asp:ListItem>
            </asp:DropDownList>
        </asp:Panel>
        <asp:Panel ID="VisiblePanel" runat="server" Visible="False">
            <asp:Label ID="PageNrLabel" runat="server" Text="Page: "></asp:Label>
            <asp:TextBox ID="PageNr" runat="server" ControlToValidate="PageNr"></asp:TextBox><asp:RangeValidator ID="RangeValidator1" runat="server" ErrorMessage="Must be a number &gt; 1" MaximumValue="999999" ControlToValidate="PageNr" MinimumValue="1"></asp:RangeValidator>
            <br />
            <asp:Label ID="XLabel" runat="server" Text="X: "></asp:Label>
            <asp:TextBox ID="X" runat="server"></asp:TextBox>
            <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="X" ErrorMessage="Must be a positive number" MaximumValue="999999" MinimumValue="0"></asp:RangeValidator>
            <br />
            <asp:Label ID="YLabel" runat="server" Text="Y: "></asp:Label>
            <asp:TextBox ID="Y" runat="server"></asp:TextBox>
            <asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="Y" ErrorMessage="Must be a positive number" MaximumValue="999999" MinimumValue="0"></asp:RangeValidator>
            <br />
        </asp:Panel>
    
        <asp:Button ID="signButton" runat="server" Text="Sign PDF" OnClick="signButton_Click" />
    
    </div>
    </form>
</body>
</html>
