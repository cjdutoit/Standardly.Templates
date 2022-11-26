// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Standardly.Core.Models.Foundations.Templates;
using Standardly.Core.Models.Foundations.Templates.Exceptions;
using Standardly.Core.Models.Processings.Templates.Exceptions;
using Xunit;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        [Fact]
        public void ShouldVerifyThatAllTemplateDeserialise()
        {
            // given
            string searchPattern = "Template.json";
            StringBuilder errorList = new StringBuilder();

            var assembly = Assembly.GetExecutingAssembly().Location;
            var templateFolder = Path.Combine(Path.GetDirectoryName(assembly), "Templates");

            var fileList = this.fileService
                .RetrieveListOfFiles(templateFolder, searchPattern);

            for (int fileCounter = 0; fileCounter <= fileList.Count - 1; fileCounter++)
            {
                try
                {
                    string rawTemplate = this.fileService.ReadFromFile(fileList[fileCounter]);
                    Template template = this.templateProcessingService.ConvertStringToTemplate(rawTemplate);
                }
                catch (TemplateValidationException ex)
                {
                    foreach (DictionaryEntry dictionaryEntry in ex.InnerException.Data)
                    {
                        foreach (string value in dictionaryEntry.Value as List<string>)
                        {
                            errorList
                                .AppendLine($"Template[{fileCounter}].{dictionaryEntry.Key} -> {value}"
                                + Environment.NewLine +
                                $"Path: {fileList[fileCounter].Substring(Path.GetDirectoryName(assembly).Length)}"
                                + Environment.NewLine);
                        }
                    }
                }
            }

            Assert.False(errorList.Length > 0, errorList.ToString());
        }

        [Fact]
        public void ShouldVerifyThatAllVariableHasBeenReplacedAndTemplatesSavedInActualOutputFolder()
        {
            // given
            InvalidReplacementTemplateException invalidReplacementTemplateException =
                new InvalidReplacementTemplateException();

            Dictionary<string, string> replacementDictionary = GetReplacementDictionary();
            List<Template> templates = this.standardlyTemplateClient.FindAllTemplates();

            // when
            for (int templateCounter = 0; templateCounter <= templates.Count - 1; templateCounter++)
            {
                Template template = templates[templateCounter];
                try
                {
                    for (int taskIndex = 0; taskIndex <= template.Tasks.Count - 1; taskIndex++)
                    {
                        var transformedTemplate =
                            this.templateProcessingService
                                .TransformTemplate(template, replacementDictionary);

                        transformedTemplate.Tasks[taskIndex].Actions.ForEach(action =>
                        {
                            this.PerformFileCreations(action.Files, replacementDictionary);
                            this.PerformAppendOpperations(action.Appends, replacementDictionary);
                        });
                    }
                }
                catch (TemplateProcessingDependencyValidationException templateProcessingDependencyValidationException)
                {
                    var templateName = template.Name ?? templateCounter.ToString();

                    foreach (DictionaryEntry dictionaryEntry in
                        templateProcessingDependencyValidationException.InnerException.Data)
                    {
                        invalidReplacementTemplateException.Data
                            .Add($"Template[{templateName}].{dictionaryEntry.Key}", dictionaryEntry.Value);
                    }
                }
            }

            // then
            if (invalidReplacementTemplateException.Data.Count > 0)
            {
                StringBuilder errorMessages = new StringBuilder();
                errorMessages.AppendLine("Found the following unexpected tags:" + Environment.NewLine);


                foreach (DictionaryEntry dictionaryEntry in invalidReplacementTemplateException.Data)
                {
                    string errors = ((List<string>)dictionaryEntry.Value)
                           .Select(value => value).Aggregate((t1, t2) => t1 + $"{Environment.NewLine}" + t2);

                    errorMessages.AppendLine($"{dictionaryEntry.Key}");
                }

                Assert.Fail(errorMessages.ToString());
            }
        }
    }
}

