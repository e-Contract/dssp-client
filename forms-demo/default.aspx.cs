using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            Response.Redirect("Sign.aspx");
        }
    }
}