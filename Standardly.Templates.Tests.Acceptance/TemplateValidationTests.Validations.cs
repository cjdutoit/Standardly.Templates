// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Standardly.Core.Models.Foundations.Templates;
using Standardly.Core.Models.Foundations.Templates.Exceptions;
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
                    Template template = this.templateService.ConvertStringToTemplate(rawTemplate);
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
    }
}

