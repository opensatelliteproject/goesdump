using System;
using System.Configuration;

namespace OpenSatelliteProject {
    sealed class ProgConfig: ApplicationSettingsBase {
        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("true")]
        public bool GenerateFDFalseColor {
            get { return (bool)this["GenerateFDFalseColor"]; }
            set { this["GenerateFDFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("true")]
        public bool GenerateXXFalseColor {
            get { return (bool)this["GenerateXXFalseColor"]; }
            set { this["GenerateXXFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("true")]
        public bool GenerateNHFalseColor {
            get { return (bool)this["GenerateNHFalseColor"]; }
            set { this["GenerateNHFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("true")]
        public bool GenerateSHFalseColor {
            get { return (bool)this["GenerateSHFalseColor"]; }
            set { this["GenerateSHFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("true")]
        public bool GenerateUSFalseColor {
            get { return (bool)this["GenerateUSFalseColor"]; }
            set { this["GenerateUSFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("false")]
        public bool EraseFilesAfterGeneratingFalseColor {
            get { return (bool)this["EraseFilesAfterGeneratingFalseColor"]; }
            set { this["EraseFilesAfterGeneratingFalseColor"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("false")]
        public bool EnableDCS {
            get { return (bool)this["EnableDCS"]; }
            set { this["EnableDCS"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("false")]
        public bool EnableEMWIN {
            get { return (bool)this["EnableEMWIN"]; }
            set { this["EnableEMWIN"] = value; }
        }
    }
}

