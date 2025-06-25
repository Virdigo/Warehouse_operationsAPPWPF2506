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

        public ReceiptExpenseDocumentDetailsWindow(int idDoc)
        {
            InitializeComponent();
            LoadInformationByDocumentId(idDoc);
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
        private void PrintDocument()
        {
            var items = DetailsListView.ItemsSource.Cast<Information_about_documents>().ToList();
            if (items == null || !items.Any())
            {
                MessageBox.Show("Нет данных для печати");
                return;
            }

            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                ColumnWidth = double.PositiveInfinity
            };

            // Шапка
            Paragraph title = new Paragraph(new Run("ТОВАРНАЯ НАКЛАДНАЯ"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(title);

            doc.Blocks.Add(new Paragraph(new Run($"Дата печати: {DateTime.Now:dd.MM.yyyy HH:mm}")));
            doc.Blocks.Add(new Paragraph(new Run($"Количество позиций: {items.Count}")));

            // Таблица
            Table table = new Table();
            doc.Blocks.Add(table);

            int columns = 6;
            for (int i = 0; i < columns; i++)
                table.Columns.Add(new TableColumn());

            table.RowGroups.Add(new TableRowGroup());

            // Заголовок таблицы
            TableRow headerRow = new TableRow();
            table.RowGroups[0].Rows.Add(headerRow);
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("№")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Товар")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Поставщик")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Кол-во")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Цена")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Сумма")))));

            foreach (var cell in headerRow.Cells)
                cell.Padding = new Thickness(5);

            // Данные
            int index = 1;
            foreach (var item in items)
            {
                TableRow row = new TableRow();
                table.RowGroups[0].Rows.Add(row);
                row.Cells.Add(new TableCell(new Paragraph(new Run(index.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.ProductName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.SuppliersName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quanity.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Price.ToString("C2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Cost.ToString("C2")))));

                foreach (var cell in row.Cells)
                    cell.Padding = new Thickness(5);

                index++;
            }

            // Подвал
            doc.Blocks.Add(new Paragraph(new Run("\n\nОтветственное лицо: ________________________\n"))
            {
                Margin = new Thickness(0, 30, 0, 0)
            });

            // Печать
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Печать накладной");
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDocument();

        }
    }
}