// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Standardly.Core.Brokers.Files;
using Standardly.Core.Brokers.Loggings;
using Standardly.Core.Brokers.RegularExpressions;
using Standardly.Core.Clients;
using Standardly.Core.Models.Configurations.Retries;
using Standardly.Core.Services.Foundations.Files;
using Standardly.Core.Services.Foundations.Templates;
using Standardly.Core.Services.Processings.Files;
using Standardly.Core.Services.Processings.Templates;
using Tynamix.ObjectFiller;
using Xunit.Abstractions;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        private readonly ILoggingBroker loggingBroker;
        private readonly IFileService fileService;
        private readonly IFileProcessingService fileProcessingService;
        private readonly ITemplateProcessingService templateProcessingService;
        private readonly IStandardlyTemplateClient standardlyTemplateClient;
        private readonly IStandardlyGenerationClient standardlyGenerationClient;
        private readonly ITestOutputHelper output;

        public TemplateValidationTests(ITestOutputHelper output)
        {
            this.output = output;
            string assembly = Assembly.GetExecutingAssembly().Location;
            string templateFolderPath = Path.Combine(Path.GetDirectoryName(assembly), @"Templates");
            string templateDefinitionFileName = "Template.json";
            this.loggingBroker = InitialiseLogger();

            this.standardlyTemplateClient =
                new StandardlyTemplateClient(templateFolderPath, templateDefinitionFileName, loggingBroker);

            this.standardlyGenerationClient = new StandardlyGenerationClient(loggingBroker)
            {
                ScriptExecutionIsEnabled = false
            };

            this.fileService = new FileService(
                fileBroker: new FileBroker(),
                retryConfig: new RetryConfig(),
                loggingBroker: this.loggingBroker);

            this.fileProcessingService =
                new FileProcessingService(this.fileService, this.loggingBroker);

            var templateService = new TemplateService(
                fileBroker: new FileBroker(),
                regularExpressionBroker: new RegularExpressionBroker(),
                loggingBroker: this.loggingBroker);

            this.templateProcessingService = new TemplateProcessingService(templateService, this.loggingBroker);
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

        private void PerformFileCreations(
            List<Core.Models.Foundations.Templates.Tasks.Actions.Files.File> files,
            Dictionary<string, string> replacementDictionary)
        {
            files.ForEach(file =>
            {
                string sourceString = this.fileProcessingService.ReadFromFile(file.Template);

                string transformedSourceString =
                    this.templateProcessingService.TransformString(sourceString, replacementDictionary);

                var fileExists = this.fileProcessingService.CheckIfFileExists(file.Target);
                var isRequired = !fileExists || file.Replace == true;

                if (isRequired)
                {
                    this.fileProcessingService.WriteToFile(file.Target, transformedSourceString);
                }
            });
        }

        private Dictionary<string, string> GetReplacementDictionary()
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            var templateFolder = Path.Combine(Path.GetDirectoryName(assembly), "Templates");
            var solutionFolder = Path.Combine(Path.GetDirectoryName(assembly), "ActualOutput");

            return GetReplacementDictionary(
                    templateFolder: templateFolder,
                    nameSingular: "Student",
                    namePlural: "Students",
                    rootNameSpace: "Test.Api",
                    solutionFolder: solutionFolder,
                    displayName: "Test User",
                    gitHubUsername: "TestUser"
                );
        }

        private Dictionary<string, string> GetReplacementDictionary(
             string templateFolder,
             string nameSingular,
             string namePlural,
             string rootNameSpace,
             string solutionFolder,
             string displayName,
             string gitHubUsername
             )
        {
            return this.GetReplacementDictionary(
                templateFolder,
                nameSingular,
                namePlural,
                gitHubBaseBranchName: "main",
                rootNameSpace: rootNameSpace,
                solutionFolder,
                projectName: rootNameSpace,
                projectFolder: $"{solutionFolder}\\{rootNameSpace}",
                projectFile: $"{rootNameSpace}.csproj",
                unitTestProjectName: $"{rootNameSpace}.Tests.Unit",
                unitTestProjectFolder: $"{solutionFolder}\\{rootNameSpace}.Tests.Unit",
                unitTestProjectFile: $"{rootNameSpace}.Tests.Unit.csproj",
                acceptanceTestProjectName: $"{rootNameSpace}.Tests.Acceptance",
                acceptanceTestProjectFolder: $"{solutionFolder}\\{rootNameSpace}.Tests.Acceptance",
                acceptanceTestProjectFile: $"{rootNameSpace}.Tests.Acceptance.csproj",
                infrastructureBuildProjectName: $"{rootNameSpace}.Infrastructure.Build",
                infrastructureBuildProjectFolder: $"{solutionFolder}\\{rootNameSpace}.Infrastructure.Build",
                infrastructureBuildProjectFile: $"{rootNameSpace}.Infrastructure.Build.csproj",
                infrastructureProvisionProjectName: $"{rootNameSpace}.Infrastructure.Provision",
                infrastructureProvisionProjectFolder: $"{solutionFolder}\\{rootNameSpace}.Infrastructure.Provision",
                infrastructureProvisionProjectFile: $"{rootNameSpace}.Infrastructure.Provision.csproj",
                draftPullRequest: false,
                copyright: "// ---------------------------------------------------------------\r\n"
                    + "// Copyright (c) Christo du Toit. All rights reserved.\r\n"
                    + "// Licensed under the MIT License.\r\n"
                    + "// See License.txt in the project root for license information.\r\n"
                    + "// ---------------------------------------------------------------",
                license: "Licensed under the MIT License",
                displayName,
                gitHubUsername
                );
        }

        private Dictionary<string, string> GetReplacementDictionary(
            string templateFolder,
            string nameSingular,
            string namePlural,
            string gitHubBaseBranchName,
            string rootNameSpace,
            string solutionFolder,
            string projectName,
            string projectFolder,
            string projectFile,
            string unitTestProjectName,
            string unitTestProjectFolder,
            string unitTestProjectFile,
            string acceptanceTestProjectName,
            string acceptanceTestProjectFolder,
            string acceptanceTestProjectFile,
            string infrastructureBuildProjectName,
            string infrastructureBuildProjectFolder,
            string infrastructureBuildProjectFile,
            string infrastructureProvisionProjectName,
            string infrastructureProvisionProjectFolder,
            string infrastructureProvisionProjectFile,
            bool draftPullRequest,
            string copyright,
            string license,
            string displayName,
            string gitHubUsername
            )
        {
            Dictionary<string, string> replacementsDictionary = new Dictionary<string, string>();

            var parameterSafeItemNameSingular = PrivatePropertyName(nameSingular);
            var parameterSafeItemNamePlural = PrivatePropertyName(namePlural);
            var parameterSafeItemNameSingularLower = parameterSafeItemNameSingular.ToLower();
            var parameterSafeItemNamePluralLower = parameterSafeItemNamePlural.ToLower();
            var lowerDescriptionName = DescriptionPropertyName(nameSingular);
            var upperDescriptionName = UpperDescriptionPropertyName(nameSingular);
            var lowerPluralDescriptionName = DescriptionPropertyName(namePlural);
            var upperPluralDescriptionName = UpperDescriptionPropertyName(namePlural);
            replacementsDictionary.Add("$basebranch$", gitHubBaseBranchName);
            replacementsDictionary.Add("$previousBranch$", gitHubBaseBranchName);
            replacementsDictionary.Add("$currentBranch$", gitHubBaseBranchName);
            replacementsDictionary.Add("$rootnamespace$", rootNameSpace);
            replacementsDictionary.Add("$safeitemname$", nameSingular);
            replacementsDictionary.Add("$safeItemNameSingular$", nameSingular);
            replacementsDictionary.Add("$safeItemNamePlural$", namePlural);
            replacementsDictionary.Add("$parameterSafeItemNameSingular$", parameterSafeItemNameSingular);
            replacementsDictionary.Add("$parameterSafeItemNamePlural$", parameterSafeItemNamePlural);
            replacementsDictionary.Add("$parameterSafeItemNameSingularLower$", parameterSafeItemNameSingularLower);
            replacementsDictionary.Add("$parameterSafeItemNamePluralLower$", parameterSafeItemNamePluralLower);
            replacementsDictionary.Add("$lowerDescriptionName$", lowerDescriptionName);
            replacementsDictionary.Add("$upperDescriptionName$", upperDescriptionName);
            replacementsDictionary.Add("$lowerPluralDescriptionName$", lowerPluralDescriptionName);
            replacementsDictionary.Add("$upperPluralDescriptionName$", upperPluralDescriptionName);
            replacementsDictionary.Add("$solutionFolder$", solutionFolder.Replace("\\", "\\\\"));
            replacementsDictionary.Add("$templateFolder$", templateFolder.Replace("\\", "\\\\"));
            replacementsDictionary.Add("$projectName$", projectName);
            replacementsDictionary.Add("$projectFolder$", projectFolder.Replace("\\", "\\\\"));
            replacementsDictionary.Add("$projectFile$", projectFile);
            replacementsDictionary.Add("$unitTestProjectName$", unitTestProjectName);

            replacementsDictionary.Add(
                "$unitTestProjectFolder$",
                unitTestProjectFolder.Replace("\\", "\\\\"));

            replacementsDictionary.Add("$unitTestProjectFile$", unitTestProjectFile);

            replacementsDictionary.Add(
                "$acceptanceTestProjectName$",
                acceptanceTestProjectName);

            replacementsDictionary.Add(
                "$acceptanceTestProjectFolder$",
                acceptanceTestProjectFolder.Replace("\\", "\\\\"));

            replacementsDictionary.Add(
                "$acceptanceTestProjectFile$",
                acceptanceTestProjectFile);

            replacementsDictionary.Add(
                "$infrastructureBuildProjectName$",
                infrastructureBuildProjectName);

            replacementsDictionary.Add(
                "$infrastructureBuildProjectFolder$",
                infrastructureBuildProjectFolder.Replace("\\", "\\\\"));

            replacementsDictionary.Add(
                "$infrastructureBuildProjectFile$",
                infrastructureBuildProjectFile);

            replacementsDictionary.Add(
                "$infrastructureProvisionProjectName$",
                infrastructureProvisionProjectName);

            replacementsDictionary.Add(
                "$infrastructureProvisionProjectFolder$",
                infrastructureProvisionProjectFolder.Replace("\\", "\\\\"));

            replacementsDictionary.Add(
                "$infrastructureProvisionProjectFile$",
                infrastructureProvisionProjectFile);

            replacementsDictionary.Add("$draftPullRequest$",
                draftPullRequest == true ? "-d " : "");

            replacementsDictionary.Add("$displayName$", displayName);
            replacementsDictionary.Add("$username$", gitHubUsername);
            replacementsDictionary.Add("$brokers$", "Brokers");
            replacementsDictionary.Add("$models$", "Models");
            replacementsDictionary.Add("$services$", "Services");
            replacementsDictionary.Add("$foundations$", "Foundations");
            replacementsDictionary.Add("$processings$", "Processings");
            replacementsDictionary.Add("$orchestrations$", "Orchestrations");
            replacementsDictionary.Add("$coordinations$", "Coordinations");
            replacementsDictionary.Add("$controllers$", "Controllers");
            replacementsDictionary.Add("$year$", DateTime.Now.Year.ToString());
            replacementsDictionary.Add("$copyright$", copyright);
            replacementsDictionary.Add("$license$", license);

            return replacementsDictionary;
        }

        private static string PrivatePropertyName(string input)
        {
            var words = SplitCamelCase(input);
            string result = string.Join("", words);

            if (string.IsNullOrEmpty(result) || char.IsLower(result[0]))
                return result;

            return char.ToLower(result[0]) + result.Substring(1);
        }

        private static string DescriptionPropertyName(string input)
        {
            var words = SplitCamelCase(input);
            string result = string.Join(" ", words);

            if (string.IsNullOrEmpty(result))
                return result;

            return result.ToLower();
        }

        private static string UpperDescriptionPropertyName(string input)
        {
            var words = SplitCamelCase(input);
            string result = string.Join(" ", words);

            if (string.IsNullOrEmpty(result))
                return result;

            return char.ToUpper(result[0]) + result.Substring(1).ToLower();
        }

        private static IEnumerable<string> SplitCamelCase(string input)
        {
            string[] words = Regex.Matches(input, "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)")
                                    .OfType<Match>()
                                    .Select(m => m.Value)
                                    .ToArray();

            return words;
        }
    }
}
