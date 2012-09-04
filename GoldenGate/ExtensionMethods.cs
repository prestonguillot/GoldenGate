using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace GoldenGate
{
    public static class ExtensionMethods
    {
        public static bool ListExists(this SPWeb curWeb, string listName)
        {
            listName = (listName ?? String.Empty).Trim().ToLower();
            return curWeb.Lists.Cast<SPList>().Any(list => list.Title.ToLower().Equals(listName));
        }

        public static void EmitErrorMessage(this WebPart curPart, string errorMessage)
        {
            curPart.Controls.Add(new LiteralControl(String.Format(@"<div class='error'>{0}</div>", errorMessage)));
        }
    }
}