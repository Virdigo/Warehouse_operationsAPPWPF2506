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

namespace Warehouse_operationsAPPWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddInformationPage.xaml
    /// </summary>
    public partial class AddInformationPage : Page
    {
        private readonly ApiServiceInformation _apiService;
        private readonly Information_about_documents _informationAboutDocuments;

        internal AddInformationPage(Information_about_documents information_about_documents = null)
        {
            InitializeComponent();
            _apiService = new ApiServiceInformation();
            _informationAboutDocuments = information_about_documents ?? new Information_about_documents();

            // Заполнение полей формы
            LoadProductsForComboBox();
            LoadDocumentsForComboBox();
            LoadSuppliersForComboBox();

            if (_informationAboutDocuments.id_inf_doc != 0)  // Если редактирование
            {
                ProductComboBox.SelectedValue = _informationAboutDocuments.id_Product;
                DocumentComboBox.SelectedValue = _informationAboutDocuments.id_doc;
                SupplierComboBox.SelectedValue = _informationAboutDocuments.id_suppliers;
                QuantityTextBox.Text = _informationAboutDocuments.Quanity.ToString();
                PriceTextBox.Text = _informationAboutDocuments.Price.ToString();
            }
        }

        private async void LoadProductsForComboBox()
        {
            try
            {
                var products = await _apiService.GetProductsAsync();  // Получаем продукты через API
                ProductComboBox.ItemsSource = products;
                ProductComboBox.DisplayMemberPath = "Name";
                ProductComboBox.SelectedValuePath = "id_Product";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }

        private async void LoadDocumentsForComboBox()
        {
            try
            {
                var documents = await _apiService.GetReceiptAndExpenseDocumentsList();  // Получаем документы через API
                DocumentComboBox.ItemsSource = documents;
                DocumentComboBox.DisplayMemberPath = "id_doc";
                DocumentComboBox.SelectedValuePath = "id_doc";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}");
            }
        }

        private async void LoadSuppliersForComboBox()
        {
            try
            {
                var suppliers = await _apiService.GetSuppliersAsync();  // Получаем поставщиков через API
                SupplierComboBox.ItemsSource = suppliers;
                SupplierComboBox.DisplayMemberPath = "Name";
                SupplierComboBox.SelectedValuePath = "id_suppliers";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}");
            }
        }

        private void FilterTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только ввод цифр и знак минус
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохраняем ID продукта
                var selectedProduct = (Product)ProductComboBox.SelectedItem;
                _informationAboutDocuments.id_Product = selectedProduct?.id_Product ?? 0;

                // Сохраняем количество
                _informationAboutDocuments.Quanity = int.Parse(QuantityTextBox.Text);

                // Сохраняем ID документа
                var selectedDocument = (Receipt_and_expense_documents)DocumentComboBox.SelectedItem;
                _informationAboutDocuments.id_doc = selectedDocument?.id_doc ?? 0;

                // Сохраняем ID поставщика
                var selectedSupplier = (Suppliers)SupplierComboBox.SelectedItem;
                _informationAboutDocuments.id_suppliers = selectedSupplier?.id_suppliers ?? 0;

                // Сохраняем цену и стоимость
                _informationAboutDocuments.Price = int.Parse(PriceTextBox.Text);
  


                if (_informationAboutDocuments.id_inf_doc == 0)
                {
                    await _apiService.AddInformationAsync(_informationAboutDocuments);
                    MessageBox.Show("Информация успешно добавлена");
                }
                else
                {
                    await _apiService.UpdateInformationAsync(_informationAboutDocuments);
                    MessageBox.Show("Информация успешно обновлена");
                }

                // Возвращаемся на предыдущую страницу
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении информации: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}