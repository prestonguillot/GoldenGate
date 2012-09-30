using System.Text;
using System.Web.UI.WebControls;

namespace GoldenGate
{
    public class SimplePager : WebControl
    {
        public uint PageSize { get; set; }
        public uint TotalItems { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            var htmlOutPut = new StringBuilder(string.Format(
                @"<div class='galleryPager' data-total-items='{0}' data-page-size='{1}' data-current-page='1'>
                    
                    <ul>
                        <li>Prev</li>
                        <li class='selected'>1</li>", TotalItems, PageSize), 500);
            for (int i = (int)TotalItems - (int)PageSize, page = 2; i > 0; i -= (int)PageSize, page++)
            {
                htmlOutPut.AppendFormat("<li>{0}</li>", page);
            }
            htmlOutPut.Append(
                @"      <li>Next</li>
                    </ul>
                  </div");

            writer.Write(htmlOutPut.ToString());
        }
    }
}