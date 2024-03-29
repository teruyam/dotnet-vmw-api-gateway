﻿using dotnet_vmw_api_gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace dotnet_vmw_api_gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggregationStatusController
    {
        private static readonly HttpClient _client;

        static AggregationStatusController()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(2);
        }

        private readonly ILogger<AggregationStatusController> _logger;
        private readonly IConfiguration _configuration;

        public AggregationStatusController(ILogger<AggregationStatusController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<AggregationStatus> Get()
        {
            var aggregationStatus = new AggregationStatus()
            {
                UserEntries = await this.getUserEntries(),
                LicenseStatus = await this.getLicenseStatus(),
            };
            return aggregationStatus;
        }

        private async Task<UserEntry[]> getUserEntries()
        {
            string rankingApiUrl = _configuration.GetValue<string>("API_GATEWAY_RANKING_API_URL");
            if (string.IsNullOrEmpty(rankingApiUrl))
            {
                return new UserEntry[] { };
            }
            try
            {
                string json = await _client.GetStringAsync(rankingApiUrl);
                if (string.IsNullOrEmpty(json))
                {
                    return new UserEntry[] { };
                }
                return JsonSerializer.Deserialize<UserEntry[]>(json);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Fetching UserEntries timed out.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch UserEntries.", ex);
            }
            return new UserEntry[] { };
        }

        private async Task<LicenseStatus> getLicenseStatus()
        {
            string licensingStatusUrl = _configuration.GetValue<string>("API_GATEWAY_LICENSING_STATUS_URL");
            if (string.IsNullOrEmpty(licensingStatusUrl))
            {
                return new LicenseStatus() { Enabled = false };
            }
            try
            {
                string json = await _client.GetStringAsync(licensingStatusUrl);
                if (string.IsNullOrEmpty(json))
                {
                    return new LicenseStatus() { Enabled = false };
                }
                return JsonSerializer.Deserialize<LicenseStatus>(json);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Fetching LicenseStatus timed out.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch LicenseStatus.", ex);
            }
            return new LicenseStatus() { Enabled = false };
        }

    }
}
