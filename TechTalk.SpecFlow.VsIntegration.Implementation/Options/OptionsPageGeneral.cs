﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using TechTalk.SpecFlow.IdeIntegration.Options;
using TechTalk.SpecFlow.VsIntegration.Implementation.SingleFileGenerator;

namespace TechTalk.SpecFlow.VsIntegration.Implementation.Options
{
    /// <summary>
    // Extends a standard dialog functionality for implementing ToolsOptions pages, 
    // with support for the Visual Studio automation model, Windows Forms, and state 
    // persistence through the Visual Studio settings mechanism.
    /// </summary>
    [Guid("D41B81C9-8501-4124-B75F-0F194E37178C")]
    [ComVisible(true)]
    public class OptionsPageGeneral : DialogPage
    {
        [Category("Analysis Settings")]
        [Description("Controls whether SpecFlow should collect binding information and step suggestions from the feature files. (restart required)")]
        [DisplayName(@"Enable project-wide analysis")]
        [DefaultValue(OptionDefaultValues.EnableAnalysisDefaultValue)]
        public bool EnableAnalysis { get; set; }

        [Category("Analysis Settings")]
        [Description("Controls whether SpecFlow Visual Studio integration should offer re-generating the feature files on configuration change.")]
        [DisplayName(@"Disable re-generate feature file popup")]
        [DefaultValue(OptionDefaultValues.DisableRegenerateFeatureFilePopupOnConfigChangeDefaultValue)]
        public bool DisableRegenerateFeatureFilePopupOnConfigChange { get; set; }

        private bool enableSyntaxColoring = true;
        private CustomToolSwitch _customToolSwitch;

        [Category("Editor Settings")]
        [Description("Controls whether the different syntax elements of the feature files should be indicated in the editor.")]
        [DisplayName(@"Enable Syntax Coloring")]
        [DefaultValue(OptionDefaultValues.EnableSyntaxColoringDefaultValue)]
        [RefreshProperties(RefreshProperties.All)]
        public bool EnableSyntaxColoring
        {
            get { return enableSyntaxColoring; }
            set
            {
                enableSyntaxColoring = value;
                if (!value)
                {
                    EnableOutlining = false;
                    EnableIntelliSense = false;
                }
            }
        }

        [Category("Editor Settings")]
        [Description("Controls whether the scenario blocks of the feature files should be outlined in the editor.")]
        [DisplayName(@"Enable Outlining")]
        [DefaultValue(OptionDefaultValues.EnableOutliningDefaultValue)]
        public bool EnableOutlining { get; set; }

        [Category("Editor Settings")]
        [Description("Controls whether the step definition match status should be indicated with a different color in the editor. (beta)")]
        [DisplayName(@"Enable Step Match Coloring")]
        [DefaultValue(OptionDefaultValues.EnableStepMatchColoringDefaultValue)]
        public bool EnableStepMatchColoring { get; set; }

        [Category("Editor Settings")]
        [Description("Controls whether the tables should be formatted automatically when you type \"|\" character.")]
        [DisplayName(@"Enable Table Formatting")]
        [DefaultValue(OptionDefaultValues.EnableTableAutoFormatDefaultValue)]
        public bool EnableTableAutoFormat { get; set; }


        [Category("IntelliSense")]
        [Description("Controls whether completion lists should be displayed for the feature files.")]
        [DisplayName(@"Enable IntelliSense")]
        [DefaultValue(OptionDefaultValues.EnableIntelliSenseDefaultValue)]
        public bool EnableIntelliSense { get; set; }

        private string _maxStepInstancesSuggestions = String.Empty;
        [Category("IntelliSense")]
        [Description("Limit quantity of IntelliSense step instances suggestions for each step template.")]
        [DisplayName(@"Max Step Instances Suggestions")]
        [DefaultValue(OptionDefaultValues.MaxStepInstancesSuggestionsDefaultValue)]
        public string MaxStepInstancesSuggestions
        {
            get { return _maxStepInstancesSuggestions; }
            set
            {
                int parsedValue;
                if (int.TryParse(value, out parsedValue) && parsedValue >= 0)
                {
                    _maxStepInstancesSuggestions = parsedValue.ToString();
                }
                else
                {
                    _maxStepInstancesSuggestions = string.Empty;
                }
            }
        }

        [Category("Tracing")]
        [Description("Controls whether diagnostic trace messages should be emitted to the output window.")]
        [DisplayName(@"Enable Tracing")]
        [DefaultValue(OptionDefaultValues.EnableTracingDefaultValue)]
        public bool EnableTracing { get; set; }

        [Category("Tracing")]
        [Description("Specifies the enabled the tracing categories in a comma-seperated list. Use \"all\" to trace all categories.")]
        [DisplayName(@"Tracing Categories")]
        [DefaultValue(OptionDefaultValues.TracingCategoriesDefaultValue)]
        public string TracingCategories { get; set; }


        [Category("Code Behind File Generation")]
        [Description("Specifies the mode how the code behind file is generated")]
        [DisplayName("Generation Mode")]
        [DefaultValue(OptionDefaultValues.GenerationModeDefaultValue)]
        public GenerationMode GenerationMode { get; set; }

        [Category("Code Behind File Generation")]
        [Description("Specifies the path to TechTalk.SpecFlow.VisualStudio.CodeBehindGenerator.exe")]
        [DisplayName("Path to cmd tool")]
        [DefaultValue(OptionDefaultValues.CodeBehindFileGeneratorPath)]
        public string PathToCodeBehindGeneratorExe { get; set; }

        [Category("Code Behind File Generation")]
        [Description("Specifies the path where to save data exchange files (Default: %TEMP%)")]
        [DisplayName("Data Exchange Path")]
        [DefaultValue(OptionDefaultValues.CodeBehindFileGeneratorPath)]
        public string CodeBehindFileGeneratorExchangePath { get; set; }


        [Category("Legacy")]
        [Description("Enables code-behind file generation via CustomTool. Turn off when you use the MSBuild integration.")]
        [DisplayName("Enable SpecFlowSingleFileGenerator CustomTool")]
        [DefaultValue(false)]
        public bool LegacyEnableSpecFlowSingleFileGeneratorCustomTool { get; set; }
        public const string UsageStatisticsCategory = "Usage statistics";

        [Category(UsageStatisticsCategory)]
        [Description("Disables sending error reports and other statistics transmissions.")]
        [DisplayName("Opt-Out of Data Collection")]
        [DefaultValue(OptionDefaultValues.DefaultOptOutDataCollection)]
        public bool OptOutDataCollection { get; set; }

        private const string FormattingOptions = "Formatting Options";

        [Category(FormattingOptions)]
        [Description("Enable removing excessive line breaks during formatting")]
        [DisplayName("Normalize line breaks")]
        [DefaultValue(OptionDefaultValues.NormalizeLineBreaksDefaultValue)]
        public bool NormalizeLineBreaks { get; set; }

        [Category(FormattingOptions)]
        [Description("Set the amount of line breaks before each scenario during formatting")]
        [DisplayName("Line breaks before scenario")]
        [DefaultValue(OptionDefaultValues.DefaultLineBreaksBeforeScenario)]
        public int LineBreaksBeforeScenario { get; set; }

        [Category(FormattingOptions)]
        [Description("Set the amount of line breaks before each examples block during formatting")]
        [DisplayName("Line breaks before examples")]
        [DefaultValue(OptionDefaultValues.DefaultLineBreaksBeforeExamples)]
        public int LineBreaksBeforeExamples { get; set; }

        [Category(FormattingOptions)]
        [Description("Use tabs instead of spaces for indents during formatting")]
        [DisplayName("Use tabs for indent")]
        [DefaultValue(OptionDefaultValues.UseTabsForIndentDefaultValue)]
        public bool UseTabsForIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before feature keyword")]
        [DisplayName("Indent size for feature")]
        [DefaultValue(OptionDefaultValues.DefaultFeatureIndent)]
        public int FeatureIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before scenario keyword")]
        [DisplayName("Indent size for scenario")]
        [DefaultValue(OptionDefaultValues.DefaultScenarioIndent)]
        public int ScenarioIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before each step")]
        [DisplayName("Indent size for step")]
        [DefaultValue(OptionDefaultValues.DefaultStepIndent)]
        public int StepIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before each table line")]
        [DisplayName("Indent size for table")]
        [DefaultValue(OptionDefaultValues.DefaultTableIndent)]
        public int TableIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before each line of multi-line string argument")]
        [DisplayName("Indent size for multi-line string argument")]
        [DefaultValue(OptionDefaultValues.DefaultMultilineIndent)]
        public int MultilineIndent { get; set; }

        [Category(FormattingOptions)]
        [Description("Set indent size before example keyword")]
        [DisplayName("Indent size for example")]
        [DefaultValue(OptionDefaultValues.DefaultExampleIndent)]
        public int ExampleIndent { get; set; }

        public OptionsPageGeneral()
        {
            _customToolSwitch = new CustomToolSwitch(Dte);

            EnableAnalysis = OptionDefaultValues.EnableAnalysisDefaultValue;
            EnableSyntaxColoring = OptionDefaultValues.EnableSyntaxColoringDefaultValue;
            EnableOutlining = OptionDefaultValues.EnableOutliningDefaultValue;
            EnableIntelliSense = OptionDefaultValues.EnableIntelliSenseDefaultValue;
            MaxStepInstancesSuggestions = OptionDefaultValues.MaxStepInstancesSuggestionsDefaultValue;
            EnableTableAutoFormat = OptionDefaultValues.EnableTableAutoFormatDefaultValue;
            EnableStepMatchColoring = OptionDefaultValues.EnableStepMatchColoringDefaultValue;
            EnableTracing = OptionDefaultValues.EnableTracingDefaultValue;
            TracingCategories = OptionDefaultValues.TracingCategoriesDefaultValue;
            DisableRegenerateFeatureFilePopupOnConfigChange = OptionDefaultValues.DisableRegenerateFeatureFilePopupOnConfigChangeDefaultValue;
            GenerationMode = OptionDefaultValues.GenerationModeDefaultValue;
            PathToCodeBehindGeneratorExe = OptionDefaultValues.CodeBehindFileGeneratorPath;
            CodeBehindFileGeneratorExchangePath = OptionDefaultValues.CodeBehindFileGeneratorExchangePath;
            OptOutDataCollection = OptionDefaultValues.DefaultOptOutDataCollection;
            LegacyEnableSpecFlowSingleFileGeneratorCustomTool = _customToolSwitch.IsEnabled();
            NormalizeLineBreaks = OptionDefaultValues.NormalizeLineBreaksDefaultValue;
            LineBreaksBeforeScenario = OptionDefaultValues.DefaultLineBreaksBeforeScenario;
            LineBreaksBeforeExamples = OptionDefaultValues.DefaultLineBreaksBeforeExamples;
            UseTabsForIndent = OptionDefaultValues.UseTabsForIndentDefaultValue;
            FeatureIndent = OptionDefaultValues.DefaultFeatureIndent;
            ScenarioIndent = OptionDefaultValues.DefaultScenarioIndent;
            StepIndent = OptionDefaultValues.DefaultStepIndent;
            TableIndent = OptionDefaultValues.DefaultTableIndent;
            MultilineIndent = OptionDefaultValues.DefaultMultilineIndent;
            ExampleIndent = OptionDefaultValues.DefaultExampleIndent;
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();
            LegacyEnableSpecFlowSingleFileGeneratorCustomTool = _customToolSwitch.IsEnabled();
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            if (LegacyEnableSpecFlowSingleFileGeneratorCustomTool)
            {
                _customToolSwitch.Enable();
            }
            else
            {
                _customToolSwitch.Disable();
            }
        }

        private DTE Dte
        {
            get { return Package.GetGlobalService(typeof(DTE)) as DTE; }
        }
    }
}
