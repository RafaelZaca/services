// WCS_PG.Services/Services/ClpCommunicationService.cs (versão completa)

using S7.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;

namespace WCS_PG.Services.Services
{
    public class ClpCommunicationService
    {
        private readonly ILogger<ClpCommunicationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private Plc _plc;
        private bool _connected = false;
        private readonly object _lockObject = new object();
        private readonly string _dbName;
        private readonly int _maxPositions;

        public ClpCommunicationService(
            ILogger<ClpCommunicationService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            // Obter configurações adicionais do appsettings.json
            _dbName = _configuration["PlcConfig:DbName"] ?? "DB70";
            _maxPositions = int.TryParse(_configuration["PlcConfig:MaxPositions"], out int maxPos) ? maxPos : 1000;

            InitializePlc();
        }

        private void InitializePlc()
        {
            try
            {
                // Obter configurações do appsettings.json
                var cpuTypeStr = _configuration["PlcConfig:CpuType"] ?? "S71500";
                var cpuType = (CpuType)Enum.Parse(typeof(CpuType), cpuTypeStr);
                var ip = _configuration["PlcConfig:IpAddress"];
                var rack = Convert.ToInt16(_configuration["PlcConfig:Rack"] ?? "0");
                var slot = Convert.ToInt16(_configuration["PlcConfig:Slot"] ?? "1");

                _plc = new Plc(cpuType, ip, rack, slot);
                _logger.LogInformation($"PLC inicializado com IP: {ip}, Rack: {rack}, Slot: {slot}, CPU Type: {cpuType}, DB: {_dbName}, MaxPositions: {_maxPositions}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar PLC");
                throw;
            }
        }

        public bool Connect()
        {
            lock (_lockObject)
            {
                if (_connected) return true;

                try
                {
                    _plc.Open();
                    _connected = _plc.IsConnected;

                    if (_connected)
                    {
                        _logger.LogInformation("Conexão com PLC estabelecida com sucesso");
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao estabelecer conexão com PLC");
                    }

                    return _connected;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao conectar ao PLC");
                    _connected = false;
                    return false;
                }
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                if (!_connected) return;

                try
                {
                    _plc.Close();
                    _connected = false;
                    _logger.LogInformation("Desconectado do PLC");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao desconectar do PLC");
                }
            }
        }

        public bool EnsureConnection()
        {
            if (!_connected)
            {
                return Connect();
            }
            return true;
        }

        // Método para escrever uma posição do Banco_Sorter_DT
        public bool WriteItemToSorterBank(int position, string sku, int quantity, int rampDest)
        {
            if (position < 0 || position >= _maxPositions)
            {
                _logger.LogError($"Posição inválida: {position}. Deve estar entre 0 e {_maxPositions - 1}.");
                return false;
            }

            if (!EnsureConnection())
            {
                _logger.LogError("Não foi possível estabelecer conexão com o PLC");
                return false;
            }

            try
            {
                // Calcular o endereço base para a posição
                int baseAddress = position * 60; // Cada item ocupa 60 bytes na UDT

                // Escrever SKU (String[30]) na posição 2.0 + baseAddress
                byte[] skuBytes = new byte[30];
                Encoding.ASCII.GetBytes(sku.PadRight(30, '\0').Substring(0, 30)).CopyTo(skuBytes, 0);
                _plc.Write($"{_dbName}.DBB{baseAddress + 2}", skuBytes);

                // Escrever Quantidade (DInt) na posição 34.0 + baseAddress
                _plc.Write($"{_dbName}.DBD{baseAddress + 34}", quantity);

                // Escrever RampDest (DInt) na posição 42.0 + baseAddress
                _plc.Write($"{_dbName}.DBD{baseAddress + 42}", rampDest);

                _logger.LogInformation($"Escrito na posição {position}: SKU={sku}, Quantidade={quantity}, RampDest={rampDest}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao escrever no PLC na posição {position}");
                return false;
            }
        }

        // Método para definir o pedido como finalizado
        public bool SetOrderCompleted(bool completed)
        {
            if (!EnsureConnection())
            {
                _logger.LogError("Não foi possível estabelecer conexão com o PLC");
                return false;
            }

            try
            {
                // Escrever PedidoFinalizado (Bool) na posição 0.0
                _plc.Write($"{_dbName}.DBX0.0", completed);
                _logger.LogInformation($"PedidoFinalizado definido como {completed}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir PedidoFinalizado no PLC");
                return false;
            }
        }

        // Método para limpar todas as posições do banco
        public bool ClearSorterBank()
        {
            if (!EnsureConnection())
            {
                _logger.LogError("Não foi possível estabelecer conexão com o PLC");
                return false;
            }

            try
            {
                // Limpar todas as entradas (60 bytes * maxPositions)
                byte[] zeroBytes = new byte[60 * _maxPositions];
                _plc.Write($"{_dbName}.DBB0", zeroBytes);

                _logger.LogInformation("Banco_Sorter_DT limpo com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar Banco_Sorter_DT no PLC");
                return false;
            }
        }

        public bool SyncWaveWithPlc(List<PickRequestItem> items, Dictionary<string, int> rampMappings)
        {
            if (!EnsureConnection())
            {
                _logger.LogError("Não foi possível estabelecer conexão com o PLC");
                return false;
            }

            try
            {
                // Primeiro, limpar o banco
                ClearSorterBank();

                // Depois, escrever cada item em uma posição
                int position = 0;
                int itemsEscritos = 0;

                foreach (var item in items)
                {
                    if (position >= _maxPositions)
                    {
                        _logger.LogWarning($"Limite de {_maxPositions} posições atingido. Alguns itens não foram escritos no PLC.");
                        break;
                    }

                    // Usar o rampMapping fornecido
                    if (rampMappings.TryGetValue(item.PickRequestId, out int rampId))
                    {
                        WriteItemToSorterBank(position, item.Sku, item.ExpectedQuantity ?? 0, rampId);
                        position++;
                        itemsEscritos++;
                    }
                }

                // Definir pedido como finalizado
                SetOrderCompleted(true);

                _logger.LogInformation($"Wave sincronizada com sucesso. {itemsEscritos} itens escritos no PLC.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar wave com o PLC");
                return false;
            }
        }

    }
}