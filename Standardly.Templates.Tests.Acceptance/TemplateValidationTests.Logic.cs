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
using Standardly.Core.Models.Foundations.Templates;
using Standardly.Core.Models.Foundations.Templates.Exceptions;
using Standardly.Core.Models.Orchestrations;
using Xunit;

namespace Standardly.Templates.Tests.Acceptance
{
    public partial class TemplateValidationTests
    {
        [Fact]
        public void ShouldVerifyThatAllReplacementVariableHasBeenReplacedInTemplateFiles()
        {
            // given
            InvalidReplacementTemplateException invalidReplacementTemplateException =
                new InvalidReplacementTemplateException();

            Dictionary<string, string> replacementDictionary = GetReplacementDictionary();
            List<Template> templates = this.standardlyTemplateClient.FindAllTemplates();

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
            }
            catch (Exception ex)
            {
                throw;
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

