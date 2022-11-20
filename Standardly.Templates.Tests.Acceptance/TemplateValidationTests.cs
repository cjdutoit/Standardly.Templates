// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Standardly.Core.Brokers.Files;
using Standardly.Core.Brokers.Loggings;
using Standardly.Core.Brokers.RegularExpressions;
using Standardly.Core.Models.Configurations.Retries;
using Standardly.Core.Services.Foundations.Files;
using Standardly.Core.Services.Foundations.Templates;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        private readonly ILoggingBroker loggingBroker;
        private readonly IFileService fileService;
        private readonly ITemplateService templateService;



        public TemplateValidationTests()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            string templateFolderPath = Path.Combine(Path.GetDirectoryName(assembly), @"Templates");
            string templateDefinitionFileName = "Template.json";
            this.loggingBroker = InitialiseLogger();

            this.fileService = new FileService(
                fileBroker: new FileBroker(),
                retryConfig: new RetryConfig(),
                loggingBroker: this.loggingBroker);

            this.templateService = new TemplateService(
                fileBroker: new FileBroker(),
                regularExpressionBroker: new RegularExpressionBroker(),
                loggingBroker: this.loggingBroker);
        }

        private ILoggingBroker InitialiseLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Standardly", LogLevel.Warning);
            });

            ILogger<LoggingBroker> logger = loggerFactory.CreateLogger<LoggingBroker>();
            return new LoggingBroker(logger);
        }
    }
}
