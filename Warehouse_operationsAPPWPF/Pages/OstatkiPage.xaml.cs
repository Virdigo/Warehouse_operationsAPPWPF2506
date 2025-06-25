using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;

namespace Warehouse_operationsAPPWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для OstatkiPage.xaml
    /// </summary>
    public partial class OstatkiPage : Page
    {
        private readonly ApiServiceOstatki _apiServiceOstatki;
        private List<Ostatki> _allOstatki;
        public OstatkiPage()
        {
            InitializeComponent();
            _apiServiceOstatki = new ApiServiceOstatki();
            LoadOstatki();
        }
        private async void LoadOstatki_Click(object sender, RoutedEventArgs e)
        {
            await LoadOstatki();
        }

        private async Task LoadOstatki()
        {
            try
            {
                _allOstatki = await _apiServiceOstatki.GetOstatkisAsync();
                OstatkiListView.ItemsSource = _allOstatki;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }
        private void FilterTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только ввод цифр и знак минус
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var filteredOstatki = _allOstatki.AsEnumerable();

          

            if (int.TryParse(MinQuantityFilterTextBox.Text, out var minOstatki))
            {
                filteredOstatki = filteredOstatki.Where(p => p.Quantity_Ostatki >= minOstatki);
            }

            if (int.TryParse(MaxQuantityFilterTextBox.Text, out var maxOstatki))
            {
                filteredOstatki = filteredOstatki.Where(p => p.Quantity_Ostatki <= maxOstatki);
            }

            OstatkiListView.ItemsSource = filteredOstatki.ToList();
        }

        private void AddOstatkiButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddOstatkiPage());
        }

        private void EditOstatkiButton_Click(object sender, RoutedEventArgs e)
        {
            if (OstatkiListView.SelectedItem is Ostatki selectedOstatki)
            {
                NavigationService.Navigate(new AddOstatkiPage(selectedOstatki));
            }
            else
            {
                MessageBox.Show("Выберите продукт для редактирования.");
            }
        }

        private async void DeleteOstatkiButton_Click(object sender, RoutedEventArgs e)
        {
            if (OstatkiListView.SelectedItem is Ostatki selectedOstatki)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить остатки {selectedOstatki.id_warehouses}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _apiServiceOstatki.DeleteOstatkiAsync(selectedOstatki.id_Ostatki);
                        MessageBox.Show("Продукт успешно удален.");
                        await LoadOstatki();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении продукта: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продукт для удаления.");
            }
        }

        private async void PDF_Click(object sender, RoutedEventArgs e)
        {
            var OstatkiInPDF = await _apiServiceOstatki.GetOstatkisAsync();

            // Создание PDF-документа
            var document = new PdfDocument();
            document.Info.Title = "Остатки на складах";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 8);
            var boldFont = new XFont("Verdana", 10);

            double y = 30;

            // Заголовок
            gfx.DrawString("Остатки на складах", new XFont("Verdana", 12), XBrushes.Black, new XPoint(20, y));
            y += 25;

            // Заголовки таблицы
            gfx.DrawString("ID", boldFont, XBrushes.Black, new XPoint(10, y));
            gfx.DrawString("Склад", boldFont, XBrushes.Black, new XPoint(50, y));
            gfx.DrawString("Товар", boldFont, XBrushes.Black, new XPoint(200, y));
            gfx.DrawString("Кол-во", boldFont, XBrushes.Black, new XPoint(400, y));

            y += 20;

            // Заполнение таблицы
            foreach (var item in OstatkiInPDF)
            {
                gfx.DrawString(item.id_Ostatki.ToString(), font, XBrushes.Black, new XPoint(10, y));
                gfx.DrawString(item.WarehousesName ?? "", font, XBrushes.Black, new XPoint(50, y));
                gfx.DrawString(item.ProductName ?? "", font, XBrushes.Black, new XPoint(200, y));
                gfx.DrawString(item.Quantity_Ostatki.ToString(), font, XBrushes.Black, new XPoint(400, y));
                y += 15;

                // Перенос на новую страницу при переполнении
                if (y > page.Height - 30)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 30;
                }
            }

            // Сохранение и открытие PDF
            string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OstatkiExport.pdf");
            document.Save(filename);
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }

        private void excel_Click(object sender, RoutedEventArgs e)
        {
            var ExcelApp = new Excel.Application();

            Excel.Workbook wb = ExcelApp.Workbooks.Add();

            Excel.Worksheet worksheet = ExcelApp.Worksheets.Item[1];

            int indexRows = 1;
            worksheet.Cells[1][indexRows] = "Код остатков";
            worksheet.Cells[2][indexRows] = "Код склада";
            worksheet.Cells[3][indexRows] = "Код товара";
            worksheet.Cells[4][indexRows] = "Количество остатков";

            var printItems = OstatkiListView.Items;

            foreach (Ostatki item in printItems)
            {
                worksheet.Cells[1][indexRows + 1] = indexRows;
                worksheet.Cells[2][indexRows + 1] = item.WarehousesName;
                worksheet.Cells[3][indexRows + 1] = item.ProductName;
                worksheet.Cells[4][indexRows + 1] = item.Quantity_Ostatki;

                indexRows++;
            }
            Excel.Range range = worksheet.Range[worksheet.Cells[2][indexRows + 1],
                    worksheet.Cells[7][indexRows + 1]];

            range.ColumnWidth = 20;

            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

            ExcelApp.Visible = true;
        }

        private void BtnArrowLeft_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }

        private void BtnArrowRight_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new WarehousePage());
        }
    }
}
    

