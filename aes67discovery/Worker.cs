using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SAPLib;
using SAPLib.Events;

namespace aes67discovery
{
    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private SAPLib.SAPService _sap;
        private readonly SAPSettings _settings;

        public Worker(ILogger<Worker> logger, IOptions<SAPSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"AES67Discovery Server started at: { DateTime.Now }");
            _logger.LogInformation($"SapSettings: { JsonSerializer.Serialize(this._settings) }");

            _sap = new SAPLib.SAPService()
            {
                Interval = TimeSpan.FromMilliseconds(_settings.Interval)
            };
            _sap.OnMessageSend += delegate (Object o, MessageSendEventArgs args)
            {
                _logger.LogInformation("Send SAP/SDP packet (0x{0:X02})",
                    args.Packet.MessageIdentifierHash);
            };

            if (!String.IsNullOrEmpty(_settings.NetInterface))
            {
                _logger.LogInformation($"Setting multicast send interface to: { _settings.NetInterface }");
                _sap.SetSendInterface(IPAddress.Parse(_settings.NetInterface));
            }


            foreach (string item in this._settings.SDPFiles)
            {
                try
                {
                    SapPacket program = new SapPacket();
                    program.OriginatingSource = String.IsNullOrEmpty(_settings.NetInterface) ? SAPService.GetLocalIPAddress() : IPAddress.Parse(_settings.NetInterface); ;
                    program.AttachSDP(item);

                    _sap.Program.Add(item, program);
                } 
                catch (FileNotFoundException e)
                {
                    _logger.LogError(e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            _sap.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AES67Discovery Server is stopping.");
            _sap?.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _sap?.Dispose();
        }
    }
}
