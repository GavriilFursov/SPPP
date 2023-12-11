using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace SPPP
{
    internal class WordHelper
    {
        public bool WordTemplate(string date, string model, string time, string accuransy, string location)
        {
            Word.Document doc = null;
            string patch = Application.StartupPath.ToString() + "\\Шаблон.docx";
            try
            {
                Word._Application app = new Word.Application();
                doc = app.Documents.Open(patch);

                doc.Activate();

                doc.Bookmarks["dateTime"].Range.Text = date;
                doc.Bookmarks["accurancy"].Range.Text = accuransy;
                doc.Bookmarks["caliTime"].Range.Text = time;
                doc.Bookmarks["model"].Range.Text = model;

                doc.SaveAs(location);
                doc.Close();
                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
