using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
namespace DeskAnalyzer
{
    class Excel
    {
        string path = "";
        _Application excel = new _Excel.Application();
        Workbook wb;
        Worksheet ws;
        public Excel(String Path, int Sheet) 
        {
            path = Path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }
        public Excel()
        {
          
        }
        public void CreateNewFile()
        {
            this.wb = excel.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            this.ws = wb.Worksheets[1];
        }
        public void CreateNewSheet()
        {
            Worksheet tempSheet = wb.Worksheets.Add(After:ws);
        }
        public string ReadCell(int i, int j)
        {
            i++; j++;
            if (ws.Cells[i, j].Value2 != null)
            {
                return ws.Cells[i, j].Value2;
            }
            else
            {
                return "";
            }
        }
        public void WriteToCell(int i,int j,string s)
        {
            i++; j++;
            ws.Cells[i, j].Value2 = s;
        }
        public void Save()
        {
            wb.Save();
        }
        public void SaveAs(string path)
        {
            wb.SaveAs(path);
        }
        public void SelectWorksheet(int SheetNumber)
        {
            this.ws = wb.Worksheets[SheetNumber];
        }
        public void DeletWorksheet(int SheetNumber)
        {
            wb.Worksheets[SheetNumber].Delet();
        }
        public void ProtectSheet()
        {
            ws.Protect();
        }
        public void ProtectSheet(string Password)
        {
            ws.Protect(Password);
        }
        public void UnprotectSheet()
        {
            ws.Unprotect();
        }
        public void UnprotectSheet(string Password)
        {
            ws.Unprotect(Password);
        }
        public void Close()
        {
            wb.Close();
        }

    }
}
