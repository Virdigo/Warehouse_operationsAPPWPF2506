﻿using Newtonsoft.Json;
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
    internal class ApiServiceWarehouses
    {
        private readonly HttpClient _httpClient;

        public ApiServiceWarehouses()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7034/api/warehouses/") };
        }

        public async Task<List<Warehouses>> GetWarehousesAsync()
        {
            var response = await _httpClient.GetAsync("");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Warehouses>>();
        }

        internal async Task AddWarehousesAsync(Warehouses warehouse)
        {
            var response = await _httpClient.PostAsJsonAsync("POST", warehouse);
            response.EnsureSuccessStatusCode();
        }

        internal async Task UpdateWarehousesAsync(Warehouses warehouse)
        {
            var response = await _httpClient.PutAsJsonAsync($"PUT/{warehouse.id_warehouses}?id_users={warehouse.id_users}", warehouse);
            response.EnsureSuccessStatusCode();
        }

        internal async Task DeleteWarehousesAsync(int warehouseId)
        {
            var response = await _httpClient.DeleteAsync($"DELETE/{warehouseId}");
            response.EnsureSuccessStatusCode();
        }
    }

}


