using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace eShopLegacyWebForms
{
    public partial class SiteMaster : MasterPage
    {
        /// <summary>
        /// Example legacy usage of a session variable embedded in a MasterPage
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            var session = HttpContext.Current?.Session;
            if (session != null)
            {
                SessionInfoLabel.Text = $"{session["MachineName"]}, {session["SessionStartTime"]}";
            }
        }
    }
}