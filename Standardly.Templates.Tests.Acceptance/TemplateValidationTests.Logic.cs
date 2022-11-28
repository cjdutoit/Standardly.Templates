// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Standardly.Core.Models.Clients.Exceptions;
using Standardly.Core.Models.Foundations.Templates;
using Standardly.Core.Models.Foundations.Templates.Exceptions;
using Standardly.Core.Models.Orchestrations;
using Xunit;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        [Fact]
        public void ShouldGenerateAllTemplates()
        {
            // given
            InvalidReplacementTemplateException invalidReplacementTemplateException =
                new InvalidReplacementTemplateException();

            Dictionary<string, string> replacementDictionary = GetReplacementDictionary();
            List<Template> templates = this.standardlyTemplateClient.FindAllTemplates();
            output.WriteLine($"Templates Found: {templates.Count}");
            output.WriteLine(
                $"Files for manual validation: {replacementDictionary["$solutionFolder$"].Replace(@"\\", @"\")}"
                + Environment.NewLine + Environment.NewLine);

            TemplateGenerationInfo templateGenerationInfo = new TemplateGenerationInfo
            {
                Templates = templates,
                ReplacementDictionary = replacementDictionary,
                EntityModelDefinition = new List<Core.Models.Foundations.Templates.EntityModels.EntityModel>(),
            };

            // when
            try
            {
                this.standardlyGenerationClient.GenerateCode(templateGenerationInfo);

                output.WriteLine("The templates rendered successfully. "
                        + $"Please complete some manual validation on the output file located here:  "
                        + $"{replacementDictionary["$solutionFolder$"].Replace(@"\\", @"\")}");
            }
            catch (StandardlyClientValidationException validationException)
            {
                StringBuilder errorMessages = new StringBuilder();
                errorMessages.AppendLine(validationException.InnerException.Message);

                if (validationException.InnerException.Data.Count > 0)
                {
                    errorMessages.AppendLine("Found the following unexpected tags:" + Environment.NewLine);

                    foreach (DictionaryEntry dictionaryEntry in validationException.InnerException.Data)
                    {
                        string errors = ((List<string>)dictionaryEntry.Value)
                               .Select(value => value).Aggregate((t1, t2) => t1 + $"{Environment.NewLine}" + t2);

                        errorMessages.AppendLine($"{dictionaryEntry.Key}");
                    }
                }

                Assert.Fail(errorMessages.ToString());
            }
            catch (StandardlyClientDependencyException dependencyException)
            {
                StringBuilder errorMessages = new StringBuilder();
                errorMessages.AppendLine($"{dependencyException.InnerException.Message}.  "
                    + $"{dependencyException.InnerException?.InnerException?.Message}");

                if (dependencyException.InnerException.Data.Count > 0)
                {
                    errorMessages.AppendLine("Found the following errors:" + Environment.NewLine);

                    foreach (DictionaryEntry dictionaryEntry in dependencyException.InnerException.Data)
                    {
                        string errors = ((List<string>)dictionaryEntry.Value)
                               .Select(value => value).Aggregate((t1, t2) => t1 + $"{Environment.NewLine}" + t2);

                        errorMessages.AppendLine($"{dictionaryEntry.Key}");
                    }
                }

                Assert.Fail(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

