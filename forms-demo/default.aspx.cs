using EContract.Dssp.Client;
using forms_demo.Properties;
using System;

namespace forms_demo
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void signButton_Click(object sender, EventArgs e)
        {
            Session["Location"] = this.location.Text;
            Session["Role"] = this.role.Text;
            Session["Visible"] = this.visible.SelectedValue;
            int value;
            Int32.TryParse(this.PageNr.Text, out value);
            Session["Page"] = value;
            Int32.TryParse(this.X.Text, out value);
            Session["X"] = value;
            Int32.TryParse(this.Y.Text, out value);
            Session["Y"] = value;
            Session["CustomText"] = this.CustomText.Text;
            Response.Redirect("Sign.aspx");
        }

        

            protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.VisiblePanel.Visible = this.visible.SelectedValue != "None";
        }
    }
}