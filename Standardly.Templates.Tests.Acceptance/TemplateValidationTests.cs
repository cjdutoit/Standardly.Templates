// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Standardly.Core.Brokers.Files;
using Standardly.Core.Brokers.Loggings;
using Standardly.Core.Brokers.RegularExpressions;
using Standardly.Core.Clients;
using Standardly.Core.Models.Configurations.Retries;
using Standardly.Core.Services.Foundations.Files;
using Standardly.Core.Services.Foundations.Templates;
using Standardly.Core.Services.Processings.Templates;
using Tynamix.ObjectFiller;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        private readonly ILoggingBroker loggingBroker;
        private readonly IFileService fileService;
        private readonly ITemplateProcessingService templateProcessingService;
        private readonly IStandardlyTemplateClient standardlyTemplateClient;

        public TemplateValidationTests()
        {
            string assembly = Assembly.GetExecutingAssembly().Location;
            string templateFolderPath = Path.Combine(Path.GetDirectoryName(assembly), @"Templates");
            string templateDefinitionFileName = "Template.json";
            this.loggingBroker = InitialiseLogger();

            this.standardlyTemplateClient =
                new StandardlyTemplateClient(templateFolderPath, templateDefinitionFileName, loggingBroker);

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

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 5).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static Dictionary<string, string> GetReplacementDictionaryWithRandomValues()
        {
            Dictionary<string, string> replacementsDictionary = new Dictionary<string, string>
            {
                { "$baseBranch$", GetRandomString() },
                { "$$previousBranch$$", GetRandomString() },
                { "$rootnamespace$", GetRandomString() },
                { "$safeitemname$", GetRandomString() },
                { "$safeItemNameSingular$", GetRandomString() },
                { "$safeItemNamePlural$", GetRandomString() },
                { "$parameterSafeItemNameSingular$", GetRandomString() },
                { "$parameterSafeItemNamePlural$", GetRandomString() },
                { "$parameterSafeItemNameSingularLower$", GetRandomString() },
                { "$parameterSafeItemNamePluralLower$", GetRandomString() },
                { "$lowerDescriptionName$", GetRandomString() },
                { "$upperDescriptionName$", GetRandomString() },
                { "$lowerPluralDescriptionName$", GetRandomString() },
                { "$upperPluralDescriptionName$", GetRandomString() },
                { "$solutionFolder$", GetRandomString() },
                { "$templateFolder$", GetRandomString() },
                { "$projectName$", GetRandomString() },
                { "$projectFolder$", GetRandomString() },
                { "$projectFile$", GetRandomString() },
                { "$unitTestProjectName$", GetRandomString() },
                { "$unitTestProjectFolder$", GetRandomString() },
                { "$unitTestProjectFile$", GetRandomString() },
                { "$acceptanceTestProjectName$", GetRandomString() },
                { "$acceptanceTestProjectFolder$", GetRandomString() },
                { "$acceptanceTestProjectFile$", GetRandomString() },
                { "$infrastructureBuildProjectName$", GetRandomString() },
                { "$infrastructureBuildProjectFolder$", GetRandomString() },
                { "$infrastructureBuildProjectFile$", GetRandomString() },
                { "$infrastructureProvisionProjectName$", GetRandomString() },
                { "$infrastructureProvisionProjectFolder$", GetRandomString() },
                { "$infrastructureProvisionProjectFile$", GetRandomString() },
                { "$displayName$", GetRandomString() },
                { "$username$", GetRandomString() },
                { "$brokers$", GetRandomString() },
                { "$models$", GetRandomString() },
                { "$services$", GetRandomString() },
                { "$foundations$", GetRandomString() },
                { "$processings$", GetRandomString() },
                { "$orchestrations$", GetRandomString() },
                { "$coordinations$", GetRandomString() },
                { "$controllers$", GetRandomString() },
                { "$year$", GetRandomString() },
                { "$copyright$", GetRandomString() },
                { "$license$", GetRandomString() },
                { "$draftPullRequest$", GetRandomString() }
            };


            return replacementsDictionary;
        }
    }
}
