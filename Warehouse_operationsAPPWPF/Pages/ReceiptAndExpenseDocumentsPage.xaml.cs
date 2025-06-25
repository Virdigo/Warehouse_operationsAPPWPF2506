using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Warehouse_operationsAPPWPF.Models;
using Warehouse_operationsAPPWPF.Services;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Windows.Input;

namespace Warehouse_operationsAPPWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReceiptAndExpenseDocumentsPage.xaml
    /// </summary>
    public partial class ReceiptAndExpenseDocumentsPage : Page
    {
        private readonly ApiServiceReceiptAndExpenseDocuments _apiService;
        private List<Receipt_and_expense_documents> _allReceiptAndExpenseDocuments;
        public ReceiptAndExpenseDocumentsPage()
        {
            InitializeComponent();
            _apiService = new ApiServiceReceiptAndExpenseDocuments();
            LoadReceiptAndExpenseDocuments();
        }
        private async void LoadDocuments_Click(object sender, RoutedEventArgs e)
        {
            await LoadReceiptAndExpenseDocuments();
        }

        private async Task LoadReceiptAndExpenseDocuments()
        {
            try
            {
                _allReceiptAndExpenseDocuments = await _apiService.GetReceiptAndExpenseDocumentsList();
                DocumentsListView.ItemsSource = _allReceiptAndExpenseDocuments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}");
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var filteredProducts = _allReceiptAndExpenseDocuments.AsEnumerable();



            // Фильтр по дате
            if (StartDatePicker.SelectedDate.HasValue)
            {
                var startDate = StartDatePicker.SelectedDate.Value;
                filteredProducts = filteredProducts.Where(p => p.date >= startDate);
            }

            if (EndDatePicker.SelectedDate.HasValue)
            {
                var endDate = EndDatePicker.SelectedDate.Value;
                filteredProducts = filteredProducts.Where(p => p.date <= endDate);
            }

            // Обновление ListView
            DocumentsListView.ItemsSource = filteredProducts.ToList();
        }

        private void AddDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddReceiptAndExpenseDocumentsPage());
        }

        private void EditDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsListView.SelectedItem is Receipt_and_expense_documents selectedReceiptAndExpenseDocuments)
            {
                NavigationService.Navigate(new AddReceiptAndExpenseDocumentsPage(selectedReceiptAndExpenseDocuments));
            }
            else
            {
                MessageBox.Show("Выберите документ для редактирования.");
            }
        }

        private async void DeleteDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsListView.SelectedItem is Receipt_and_expense_documents selectedReceiptAndExpenseDocuments)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить документ {selectedReceiptAndExpenseDocuments.id_doc}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _apiService.DeleteReceipt_and_expense_documentsAsync(selectedReceiptAndExpenseDocuments.id_doc);
                        MessageBox.Show("Документ успешно удален.");
                        await LoadReceiptAndExpenseDocuments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении документа: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите документ для удаления.");
            }
        }

        private void BtnArrowLeft_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SuppliersPage());
        }

        private void BtnArrowRight_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new InformationPage());
        }

        private void excel_Click(object sender, RoutedEventArgs e)
        {
            var ExcelApp = new Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Add();

            Excel.Worksheet worksheet = ExcelApp.Worksheets.Item[1];

            int indexRows = 1;
            worksheet.Cells[1][indexRows] = "Код документа";
            worksheet.Cells[2][indexRows] = "Дата";
            worksheet.Cells[3][indexRows] = "Приходно или расходный документ";
            worksheet.Cells[4][indexRows] = "Пользователь";

            var printItems = DocumentsListView.Items;

            foreach (Receipt_and_expense_documents item in printItems)
            {
                worksheet.Cells[1][indexRows + 1] = indexRows;
                worksheet.Cells[2][indexRows + 1] = item.date;
                worksheet.Cells[3][indexRows + 1] = item.ReceiptAndexpense_documents;
                worksheet.Cells[4][indexRows + 1] = item.UsersName;

                indexRows++;
            }
            Excel.Range range = worksheet.Range[worksheet.Cells[2][indexRows + 1],
                    worksheet.Cells[7][indexRows + 1]];

            range.ColumnWidth = 20;

            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            ExcelApp.Visible = true;
        }

        private async void PDF_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные
            var docs = await _apiService.GetReceiptAndExpenseDocumentsList();

            // Создание PDF-документа
            var document = new PdfDocument();
            document.Info.Title = "Документы прихода/расхода";

            // Настройки шрифтов
            var headerFont = new XFont("Verdana", 12, XFontStyle.Bold);
            var boldFont = new XFont("Verdana", 9, XFontStyle.Bold);
            var normalFont = new XFont("Verdana", 8, XFontStyle.Regular);

            // Начальная страница
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double marginLeft = 20;
            double y = 30;

            // Заголовок
            gfx.DrawString("Документы прихода/расхода", headerFont, XBrushes.Black,
                           new XPoint(marginLeft, y));
            y += 30;

            // Шапка таблицы
            gfx.DrawString("Код документа", boldFont, XBrushes.Black, new XPoint(marginLeft + 0, y));
            gfx.DrawString("Дата", boldFont, XBrushes.Black, new XPoint(marginLeft + 100, y));
            gfx.DrawString("Приход/расход", boldFont, XBrushes.Black, new XPoint(marginLeft + 180, y));
            gfx.DrawString("Код пользователя", boldFont, XBrushes.Black, new XPoint(marginLeft + 300, y));
            y += 20;

            // Линия под шапкой (необязательно)
            gfx.DrawLine(XPens.Black, marginLeft, y, page.Width - marginLeft, y);
            y += 10;

            // Заполнение строк таблицы
            foreach (var item in docs)
            {
                // Если не помещается на странице — открываем новую
                if (y > page.Height - 40)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }

                gfx.DrawString(item.id_doc.ToString(), normalFont, XBrushes.Black, new XPoint(marginLeft + 0, y));
                gfx.DrawString(item.date.ToString("yyyy-MM-dd"), normalFont, XBrushes.Black, new XPoint(marginLeft + 100, y));
                gfx.DrawString(item.ReceiptAndexpense_documents ? "Приход" : "Расход", normalFont, XBrushes.Black, new XPoint(marginLeft + 180, y));
                gfx.DrawString(item.UsersName.ToString(), normalFont, XBrushes.Black, new XPoint(marginLeft + 300, y));
                y += 18;
            }

            // Сохраняем PDF во временную папку и открываем
            string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ReceiptAndExpenseDocuments.pdf");
            document.Save(filename);

            Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = true
            });
        }
        private void DocumentsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DocumentsListView.SelectedItem is Receipt_and_expense_documents selectedDoc)
            {
                var window = new ReceiptExpenseDocumentDetailsWindow(selectedDoc.id_doc);
                window.ShowDialog();
            }
        }
    }
}