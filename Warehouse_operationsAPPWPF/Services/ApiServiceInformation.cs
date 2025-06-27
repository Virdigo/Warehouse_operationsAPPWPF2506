using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Warehouse_operationsAPPWPF.Models;

namespace Warehouse_operationsAPPWPF.Services
{
    internal class ApiServiceInformation
    {
        private readonly HttpClient _httpClient;

        public ApiServiceInformation()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7034/api/Information_about_documents/") };
        }

        internal async Task<List<Information_about_documents>> GetInformationAsync()
        {
            var response = await _httpClient.GetAsync("");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Information_about_documents>>();
        }

        internal async Task<List<Product>> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5093/api/Product/"); // Изменить на правильный URL API
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Product>>();
        }

        internal async Task<List<Receipt_and_expense_documents>> GetReceiptAndExpenseDocumentsList()
        {
            var response = await _httpClient.GetAsync("https://localhost:7034/api/Receipt_and_expense_documents/"); // Изменить на правильный URL API
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Receipt_and_expense_documents>>();
        }

        internal async Task<List<Suppliers>> GetSuppliersAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:7034/api/Suppliers/"); // Изменить на правильный URL API
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Suppliers>>();
        }

        internal async Task AddInformationAsync(Information_about_documents information)
        {
            var response = await _httpClient.PostAsJsonAsync("POST", information);
            response.EnsureSuccessStatusCode();
        }

        internal async Task UpdateInformationAsync(Information_about_documents information)
        {
            var response = await _httpClient.PutAsJsonAsync($"PUT/{information.id_inf_doc}", information);
            response.EnsureSuccessStatusCode();
        }

        internal async Task DeleteInformationAsync(int id_inf_doc)
        {
            var response = await _httpClient.DeleteAsync($"DELETE/{id_inf_doc}");
            response.EnsureSuccessStatusCode();
        }

        internal async Task<List<Information_about_documents>> GetInformationByDocumentIdAsync(int documentId)
        {
            var response = await _httpClient.GetAsync($"byDocument/{documentId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Information_about_documents>>();
        }
    }
}