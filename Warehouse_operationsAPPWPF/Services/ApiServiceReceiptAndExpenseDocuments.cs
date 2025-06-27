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
    public class ApiServiceReceiptAndExpenseDocuments
    {
        private readonly HttpClient _httpClient;

        // Конструктор с базовым адресом
        public ApiServiceReceiptAndExpenseDocuments()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7034/api/Receipt_and_expense_documents/") };
        }

        // Получить список документов
        internal async Task<List<Receipt_and_expense_documents>> GetReceiptAndExpenseDocumentsList()
        {
            var response = await _httpClient.GetAsync(""); // Получаем все документы
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Receipt_and_expense_documents>>();
        }

        // Добавить новый документ
        internal async Task AddReceipt_and_expense_documentsAsync(Receipt_and_expense_documents receipt_and_expense_documents)
        {
            var response = await _httpClient.PostAsJsonAsync("POST", receipt_and_expense_documents); // Без параметров в URL, данные в теле запроса
            response.EnsureSuccessStatusCode();
        }

        // Обновить документ
        internal async Task UpdateReceipt_and_expense_documentsAsync(Receipt_and_expense_documents receipt_and_expense_documents)
        {
            var response = await _httpClient.PutAsJsonAsync($"PUT/{receipt_and_expense_documents.id_doc}", receipt_and_expense_documents); // Параметры в URL
            response.EnsureSuccessStatusCode();
        }

        // Удалить документ
        internal async Task DeleteReceipt_and_expense_documentsAsync(int docId)
        {
            var response = await _httpClient.DeleteAsync($"DELETE/{docId}"); // Удаляем по id
            response.EnsureSuccessStatusCode();
        }

        // Получить список пользователей
        internal async Task<List<Users>> GetUsersList()
        {
            var response = await _httpClient.GetAsync("https://localhost:7034/api/Users/"); // URL для пользователей
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Users>>();
        }
    }
}
