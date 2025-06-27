    using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Warehouse_operationsAPPWPF.Models;
using Warehouse_operationsAPPWPF.Services;

namespace Warehouse_operationsAPPWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReceiptExpenseDocumentDetailsWindow.xaml
    /// </summary>
    public partial class ReceiptExpenseDocumentDetailsWindow : Window
    {
        private readonly ApiServiceInformation _infoService = new ApiServiceInformation();
        private readonly int _documentId;

        public ReceiptExpenseDocumentDetailsWindow(int idDoc)
        {
            InitializeComponent();
            _documentId = idDoc;
            LoadInformationByDocumentId(idDoc);
        }
        private async Task<Receipt_and_expense_documents> GetReceiptAndExpenseDocumentById()
        {
            var apiService = new ApiServiceReceiptAndExpenseDocuments();
            var all = await apiService.GetReceiptAndExpenseDocumentsList();
            return all.FirstOrDefault(d => d.id_doc == _documentId);
        }
        private async Task<List<Receipt_and_expense_documents>> GetReceiptAndExpenseDocuments()
        {
            var apiService = new ApiServiceReceiptAndExpenseDocuments();
            return await apiService.GetReceiptAndExpenseDocumentsList();
        }
        private async void LoadInformationByDocumentId(int idDoc)
        {
            try
            {
                var filtered = await _infoService.GetInformationByDocumentIdAsync(idDoc);
                DetailsListView.ItemsSource = filtered;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки информации: {ex.Message}");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private async Task PrintDocument()
        {
            var items = DetailsListView.ItemsSource.Cast<Information_about_documents>().ToList();
            if (items == null || !items.Any())
            {
                MessageBox.Show("Нет данных для печати");
                return;
            }

            var documentInfo = await GetReceiptAndExpenseDocumentById();
            if (documentInfo == null)
            {
                MessageBox.Show("Документ не найден.");
                return;
            }

            // Создание документа
            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(30),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                ColumnWidth = double.PositiveInfinity
            };

            // Заголовок
            doc.Blocks.Add(new Paragraph(new Run("ТОВАРНАЯ НАКЛАДНАЯ"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Документ № и дата
            Table headerTable = new Table();
            headerTable.Columns.Add(new TableColumn());
            headerTable.Columns.Add(new TableColumn());
            headerTable.RowGroups.Add(new TableRowGroup());
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run($"Номер документа: {_documentId}"))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run($"Дата составления: {documentInfo.date:dd.MM.yyyy}"))));
            headerTable.RowGroups[0].Rows.Add(headerRow);
            doc.Blocks.Add(headerTable);

            // Пользователь и тип
            doc.Blocks.Add(new Paragraph(new Run($"Тип документа: {(documentInfo.ReceiptAndexpense_documents ? "Приход" : "Расход")}")));
            doc.Blocks.Add(new Paragraph(new Run($"Пользователь: {documentInfo.UsersName}")));
            doc.Blocks.Add(new Paragraph(new Run($"Дата печати: {DateTime.Now:dd.MM.yyyy HH:mm}")));

            doc.Blocks.Add(new Paragraph(new Run("\nПеречень товаров:"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Margin = new Thickness(0, 20, 0, 10)
            });

            // Таблица
            Table table = new Table();
            int columns = 9;
            for (int i = 0; i < columns; i++)
                table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());

            // Заголовок таблицы
            TableRow head = new TableRow();
            string[] headers = new[]
            {
    "№", "Наименование товара", "Ед. изм.", "Кол-во", "Цена, руб.",
    "Сумма без НДС", "НДС %", "Сумма НДС", "Сумма с НДС"
};
            foreach (var h in headers)
            {
                var cell = new TableCell(new Paragraph(new Bold(new Run(h))))
                {
                    Padding = new Thickness(4),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5)
                };
                head.Cells.Add(cell);
            }
            table.RowGroups[0].Rows.Add(head);

            // Данные
            int idx = 1;
            decimal totalWithoutNds = 0, totalNds = 0, totalWithNds = 0;
            foreach (var item in items)
            {
                decimal price = item.Price;
                int qty = item.Quanity;
                decimal total = item.Cost;
                decimal withoutNds = Math.Round(total / 1.18m, 2);
                decimal nds = Math.Round(total - withoutNds, 2);
                decimal withNds = withoutNds + nds;

                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(idx.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.ProductName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run("шт"))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(qty.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(price.ToString("F2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(withoutNds.ToString("F2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run("18%"))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(nds.ToString("F2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(withNds.ToString("F2")))));

                foreach (var cell in row.Cells)
                {
                    cell.Padding = new Thickness(3);
                    cell.BorderBrush = Brushes.Black;
                    cell.BorderThickness = new Thickness(0.5);
                }

                table.RowGroups[0].Rows.Add(row);

                totalWithoutNds += withoutNds;
                totalNds += nds;
                totalWithNds += withNds;
                idx++;
            }

            // Итого строка
            TableRow totalRow = new TableRow();
            totalRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("ИТОГО"))) { TextAlignment = TextAlignment.Center })
            {
                ColumnSpan = 5,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(3)
            });
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(totalWithoutNds.ToString("F2")))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run("18%"))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(totalNds.ToString("F2")))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(totalWithNds.ToString("F2")))));

            foreach (var cell in totalRow.Cells)
            {
                cell.Padding = new Thickness(3);
                cell.BorderBrush = Brushes.Black;
                cell.BorderThickness = new Thickness(0.5);
            }

            table.RowGroups[0].Rows.Add(totalRow);
            doc.Blocks.Add(table);

            // Итоги текстом
            doc.Blocks.Add(new Paragraph(new Run($"\nВсего отпущено на сумму: {totalWithNds:C2}"))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 0)
            });

            // Подписи
            doc.Blocks.Add(new Paragraph(new Run("Ответственное лицо: ________________________")));
            doc.Blocks.Add(new Paragraph(new Run("Подпись: ________________________")));

            // Печать
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                dlg.PrintDocument(idpSource.DocumentPaginator, "Печать товарной накладной");
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDocument();

        }
    }
}