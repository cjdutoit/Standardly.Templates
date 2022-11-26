// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
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
                Assert.Fail(ex.Message);
            }
        }
    }
}

