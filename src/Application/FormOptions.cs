using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using SR = XmlNotepad.StringResources;

namespace XmlNotepad
{
    /// <summary>
    /// Summary description for FormOptions.
    /// </summary>
    public partial class FormOptions : System.Windows.Forms.Form
    {
        private Settings _settings;
        private UserSettings _userSettings;

        public FormOptions()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        public Font SelectedFont { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HelpProvider hp = this.Site.GetService(typeof(HelpProvider)) as HelpProvider;
            HelpService hs = this.Site.GetService(typeof(HelpService)) as HelpService;
            if (hp != null && hs.DynamicHelpEnabled)
            {
                hp.HelpNamespace = hs.OptionsHelp;
            }

            // now let the user resize it.
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            HelpProvider hp = this.Site.GetService(typeof(HelpProvider)) as HelpProvider;
            HelpService hs = this.Site.GetService(typeof(HelpService)) as HelpService;
            if (hp != null && hs.DynamicHelpEnabled)
            {
                hp.HelpNamespace = hs.DefaultHelp;
            }


            SaveSettings();
            base.OnClosing(e);
        }

        private void SaveSettings()
        {
            Point location = this.Location;
            Control ctrl = this.Site as Control;
            if (ctrl != null)
            {
                // Make saved location relative to Main window.
                Rectangle ownerBounds = ctrl.TopLevelControl.Bounds;
                location.X -= ownerBounds.Left;
                location.Y -= ownerBounds.Top;
                this._settings["OptionsWindowLocation"] = location;
            }
            this._settings["OptionsWindowSize"] = this.ClientSize;
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            this.AutoSize = false;
            base.OnResizeBegin(e);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            else
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (value != null)
                {
                    this._settings = value.GetService(typeof(Settings)) as Settings;
                    this._userSettings = new UserSettings(this._settings) { Font = this.SelectedFont };

                    List<string> hiddenProperties = new List<string>();
                    if (this._settings.GetString("AnalyticsClientId") == "disabled")
                    {
                        hiddenProperties.Add("AllowAnalytics");
                    }
                    if (this._settings.GetBoolean("DisableUpdateUI"))
                    {
                        hiddenProperties.Add("UpdateLocation");
                        hiddenProperties.Add("EnableUpdate");
                        hiddenProperties.Add("UpdateFrequency");
                    }

                    MemberFilter filter = new MemberFilter(this._userSettings, hiddenProperties.ToArray());
                    this.propertyGrid1.SelectedObject = filter;

                    Size s = this.ClientSize;
                    object size = this._settings["OptionsWindowSize"];
                    if (size != null && (Size)size != Size.Empty)
                    {
                        Size cs = (Size)size;
                        // but not smaller than the computed size.
                        s = new Size(Math.Max(s.Width, cs.Width), Math.Max(s.Height, cs.Height));
                        this.AutoSize = false;
                        this.ClientSize = s;
                    }
                    object location = this._settings["OptionsWindowLocation"];
                    if (location != null && (Point)location != Point.Empty)
                    {
                        Control ctrl = this.Site as Control;
                        if (ctrl != null)
                        {
                            Rectangle ownerBounds = ctrl.TopLevelControl.Bounds;
                            Point relativePos = (Point)location;
                            Point newPos = ownerBounds.Location;
                            newPos.Offset(relativePos.X, relativePos.Y);
                            if (this.IsOnScreen(newPos))
                            {
                                this.Location = newPos;
                                this.StartPosition = FormStartPosition.Manual;
                            }
                        }
                    }
                }
            }
        }

        private void OnButtonCancelClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnButtonOKClick(object sender, System.EventArgs e)
        {
            this._userSettings.Apply();
            this.SelectedFont = this._userSettings.Font;
            this.Close();
        }

        private void OnButtonResetClick(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show(this, SR.ResetOptionsPrompt, SR.ResetOptionsPromptCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation))
            {
                this._userSettings.Reset();
                this.propertyGrid1.SelectedObject = null;
                this.propertyGrid1.SelectedObject = _userSettings;
            }
        }

        public enum WebBrowserVersion
        {
            WinformsWebBrowser,
            WebView2,
        }

        public enum SettingsLocationUX
        {
            Portable = SettingsLocation.Portable,
            Local = SettingsLocation.Local,
            Roaming = SettingsLocation.Roaming
        }

        // This class keeps s a local snapshot of the settings until the user clicks the Ok button,
        // then the Apply() method is called to propagate the new settings to the underlying Settings object.
        // It also provides localizable strings for the property grid.
        public class UserSettings
        {
            private readonly Settings _settings;
            private Font _font;
            private int _treeIndent;
            private ColorTheme _theme;
            private ThemeColors _lightColors;
            private ThemeColors _darkColors;
            private Color _elementColor;
            private Color _schemaAwareTextColor;
            private Color _commentColor;
            private Color _attributeColor;
            private Color _piColor;
            private Color _textColor;
            private Color _cdataColor;
            private Color _backgroundColor;
            private Color _containerBackgroundColor;
            private Color _editorBackgroundColor;
            private string _updateLocation;
            private SettingsLocation _settingsLocation;
            private bool _enableUpdate;
            private TimeSpan _updateFrequency;
            private bool _noByteOrderMark;
            private bool _disableDefaultXslt;
            private bool _autoFormatOnSave;
            private int _indentLevel;
            private IndentChar _indentChar;
            private string _newLineChars;
            private bool _preserveWhitespace;
            private string _language;
            private int _maximumLineLength;
            private int _maximumValueLength;
            private long _maximumLineIndex;
            private bool _autoFormatLongLines;
            private bool _ignoreDTD;
            private bool _enableXsltScripts;
            private WebBrowserVersion _webBrowser;
            private bool _xmlDiffIgnoreChildOrder;
            private bool _xmlDiffIgnoreComments;
            private bool _xmlDiffIgnorePI;
            private bool _xmlDiffIgnoreWhitespace;
            private bool _xmlDiffIgnoreNamespaces;
            private bool _xmlDiffIgnorePrefixes;
            private bool _xmlDiffIgnoreXmlDecl;
            private bool _xmlDiffIgnoreDtd;
            private bool _xmlDiffHideIdentical;
            private bool _allowAnalytics;
            private string _textEditor;
            private bool _schemaAwareText;
            private string _schemaAwareNames;
            private bool _promptOnReload;
            private bool _attributesOnNewLine;

            public UserSettings(Settings s)
            {
                this._settings = s;

                this._theme = (ColorTheme)this._settings["Theme"];
                _lightColors = (ThemeColors)this._settings["LightColors"];
                _darkColors = (ThemeColors)this._settings["DarkColors"];
                LoadColors();
                _updateLocation = this._settings.GetString("UpdateLocation");
                _enableUpdate = this._settings.GetBoolean("UpdateEnabled");
                _updateFrequency = this._settings.GetTimeSpan("UpdateFrequency", TimeSpan.FromDays(1));
                _disableDefaultXslt = this._settings.GetBoolean("DisableDefaultXslt");
                _autoFormatOnSave = this._settings.GetBoolean("AutoFormatOnSave");
                _treeIndent = this._settings.GetInteger("TreeIndent");
                _noByteOrderMark = this._settings.GetBoolean("NoByteOrderMark");
                _indentLevel = this._settings.GetInteger("IndentLevel");
                _indentChar = (IndentChar)this._settings["IndentChar"];
                _attributesOnNewLine = this._settings.GetBoolean("AttributesOnNewLine");
                _newLineChars = this._settings.GetString("NewLineChars");
                _preserveWhitespace = this._settings.GetBoolean("PreserveWhitespace");
                _language = this._settings.GetString("Language");
                _settingsLocation = this._settings.GetLocation();
                _schemaAwareText = this._settings.GetBoolean("SchemaAwareText");
                _schemaAwareNames = this._settings.GetString("SchemaAwareNames");
                _maximumLineLength = this._settings.GetInteger("MaximumLineLength");
                _autoFormatLongLines = this._settings.GetBoolean("AutoFormatLongLines");
                _maximumLineIndex = this._settings.GetLong("MaximumLineIndex");
                _ignoreDTD = this._settings.GetBoolean("IgnoreDTD");
                _enableXsltScripts = this._settings.GetBoolean("EnableXsltScripts");
                _webBrowser = (this._settings.GetString("BrowserVersion") == "WebBrowser") ? WebBrowserVersion.WinformsWebBrowser : WebBrowserVersion.WebView2;

                this._font = this._settings.GetFont();

                this._xmlDiffIgnoreChildOrder = this._settings.GetBoolean("XmlDiffIgnoreChildOrder");
                this._xmlDiffIgnoreComments = this._settings.GetBoolean("XmlDiffIgnoreComments");
                this._xmlDiffIgnorePI = this._settings.GetBoolean("XmlDiffIgnorePI");
                this._xmlDiffIgnoreWhitespace = this._settings.GetBoolean("XmlDiffIgnoreWhitespace");
                this._xmlDiffIgnoreNamespaces = this._settings.GetBoolean("XmlDiffIgnoreNamespaces");
                this._xmlDiffIgnorePrefixes = this._settings.GetBoolean("XmlDiffIgnorePrefixes");
                this._xmlDiffIgnoreXmlDecl = this._settings.GetBoolean("XmlDiffIgnoreXmlDecl");
                this._xmlDiffIgnoreDtd = this._settings.GetBoolean("XmlDiffIgnoreDtd");
                this._allowAnalytics = this._settings.GetBoolean("AllowAnalytics");
                this._textEditor = this._settings.GetString("TextEditor");
                this._promptOnReload = this._settings.GetBoolean("PromptOnReload");
            }

            private void LoadColors()
            {
                ThemeColors colors = this._theme == ColorTheme.Light ? _lightColors : _darkColors;
                _elementColor = colors.Element;
                _schemaAwareTextColor = colors.SchemaAwareTextColor;
                _commentColor = colors.Comment;
                _attributeColor = colors.Attribute;
                _piColor = colors.PI;
                _textColor = colors.Text;
                _cdataColor = colors.CDATA;
                _backgroundColor = colors.Background;
                _containerBackgroundColor = colors.ContainerBackground;
                _editorBackgroundColor = colors.EditorBackground;
            }

            internal void SaveColors()
            {
                ThemeColors colors = this._theme == ColorTheme.Light ? this._lightColors : this._darkColors;
                colors.Element = this._elementColor;
                colors.SchemaAwareTextColor = this._schemaAwareTextColor;
                colors.Comment = this._commentColor;
                colors.CDATA = this._cdataColor;
                colors.Attribute = this._attributeColor;
                colors.PI = this._piColor;
                colors.Text = this._textColor;
                colors.Background = this._backgroundColor;
                colors.ContainerBackground = this._containerBackgroundColor;
                colors.EditorBackground = this._editorBackgroundColor;
            }

            public void Apply()
            {        
                // and copy to cross-platform settings.
                this._settings["FontFamily"] = this._font.FontFamily.Name;
                this._settings["FontSize"] = (double)this._font.SizeInPoints;
                switch (this._font.Style)
                {
                    case FontStyle.Regular:
                        this._settings["FontStyle"] = "Normal";
                        this._settings["FontWeight"] = "Normal";
                        break;
                    case FontStyle.Bold:
                        this._settings["FontStyle"] = "Normal";
                        this._settings["FontWeight"] = "Bold";
                        break;
                    case FontStyle.Italic:
                        this._settings["FontStyle"] = "Italic";
                        this._settings["FontWeight"] = "Normal";
                        break;
                    case FontStyle.Underline:
                        break;
                    case FontStyle.Strikeout:
                        break;
                    default:
                        break;
                }

                this._settings["Theme"] = this._theme;

                SaveColors();
                this._settings["LightColors"] = this._lightColors;
                this._settings["DarkColors"] = this._darkColors;

                this._settings["UpdateEnabled"] = this._enableUpdate;
                this._settings["UpdateLocation"] = this._updateLocation;
                this._settings["UpdateFrequency"] = this._updateFrequency;

                this._settings["DisableDefaultXslt"] = _disableDefaultXslt;
                this._settings["AutoFormatOnSave"] = _autoFormatOnSave;
                this._settings["TreeIndent"] = this._treeIndent;
                this._settings["IndentLevel"] = _indentLevel;
                this._settings["IndentChar"] = _indentChar;
                this._settings["NewLineChars"] = _newLineChars;
                this._settings["PreserveWhitespace"] = _preserveWhitespace;
                this._settings["NoByteOrderMark"] = _noByteOrderMark;
                this._settings["AttributesOnNewLine"] = _attributesOnNewLine;
                this._settings.SetLocation(_settingsLocation);

                this._settings["Language"] = ("" + this._language).Trim();
                this._settings["SchemaAwareText"] = this._schemaAwareText;
                this._settings["SchemaAwareNames"] = this._schemaAwareNames.Trim();
                this._settings["MaximumLineLength"] = this._maximumLineLength;
                this._settings["MaximumValueLength"] = this._maximumValueLength;
                this._settings["AutoFormatLongLines"] = this._autoFormatLongLines;
                this._settings["MaximumLineIndex"]  = this._maximumLineIndex;
                this._settings["IgnoreDTD"] = this._ignoreDTD;

                this._settings["EnableXsltScripts"] = this._enableXsltScripts;
                this._settings["BrowserVersion"] = (this._webBrowser == WebBrowserVersion.WinformsWebBrowser) ? "WebBrowser" : "WebView2";

                this._settings["XmlDiffIgnoreChildOrder"] = this._xmlDiffIgnoreChildOrder;
                this._settings["XmlDiffIgnoreComments"] = this._xmlDiffIgnoreComments;
                this._settings["XmlDiffIgnorePI"] = this._xmlDiffIgnorePI;
                this._settings["XmlDiffIgnoreWhitespace"] = this._xmlDiffIgnoreWhitespace;
                this._settings["XmlDiffIgnoreNamespaces"] = this._xmlDiffIgnoreNamespaces;
                this._settings["XmlDiffIgnorePrefixes"] = this._xmlDiffIgnorePrefixes;
                this._settings["XmlDiffIgnoreXmlDecl"] = this._xmlDiffIgnoreXmlDecl;
                this._settings["XmlDiffIgnoreDtd"] = this._xmlDiffIgnoreDtd;
                this._settings["XmlDiffHideIdentical"] = this._xmlDiffHideIdentical;

                this._settings["AllowAnalytics"] = this._allowAnalytics;
                this._settings["TextEditor"] = this._textEditor;
                this._settings["PromptOnReload"] = this._promptOnReload;

                this._settings.OnChanged("Colors");

            }

            public void Reset()
            {
                this._font = new Font("Courier New", 10, FontStyle.Regular);

                this._theme = ColorTheme.Light;
                this._lightColors = ThemeColors.GetDefaultColors(ColorTheme.Light);
                this._darkColors = ThemeColors.GetDefaultColors(ColorTheme.Dark);
                this.LoadColors();
                _updateLocation = Settings.DefaultUpdateLocation;
                _enableUpdate = true;
                _updateFrequency = TimeSpan.FromDays(1);
                _autoFormatOnSave = true;
                _disableDefaultXslt = false;
                _noByteOrderMark = false;
                _indentLevel = 2;
                _attributesOnNewLine = false;
                _indentChar = IndentChar.Space;
                _newLineChars = Settings.EscapeNewLines("\r\n");
                _language = "";
                this._maximumLineLength = 10000;
                this._maximumValueLength = short.MaxValue;
                this._maximumLineIndex = 1000000;
                _ignoreDTD = false;
                this._allowAnalytics = false;
                this._promptOnReload = true;
            }

            [SRCategory("ThemeCategory")]
            [LocDisplayName("Theme")]
            [SRDescription("ThemeDescription")]
            public ColorTheme Theme
            {
                get
                {
                    return this._theme;
                }
                set
                {
                    if (this._theme != value)
                    {
                        SaveColors();
                        this._theme = value;
                        LoadColors();
                    }
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("ElementColor")]
            [SRDescription("ElementColorDescription")]
            public Color ElementColor
            {
                get
                {
                    return this._elementColor;
                }
                set
                {
                    this._elementColor = value;
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("SchemaAwareTextColor")]
            [SRDescription("SchemaAwareTextColorDescription")]
            public Color SchemaAwareTextColor
            {
                get
                {
                    return this._schemaAwareTextColor;
                }
                set
                {
                    this._schemaAwareTextColor = value;
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("AttributeColor")]
            [SRDescription("AttributeColorDescription")]
            public Color AttributeColor
            {
                get
                {
                    return this._attributeColor;
                }
                set
                {
                    this._attributeColor = value;
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("CommentColor")]
            [SRDescription("CommentColorDescription")]
            public Color CommentColor
            {
                get
                {
                    return this._commentColor;
                }
                set
                {
                    this._commentColor = value;
                }
            }
            [SRCategory("ColorCategory")]
            [LocDisplayName("PiColor")]
            [SRDescription("PiColorDescription")]
            public Color PiColor
            {
                get
                {
                    return this._piColor;
                }
                set
                {
                    this._piColor = value;
                }
            }
            [SRCategory("ColorCategory")]
            [LocDisplayName("TextColor")]
            [SRDescription("TextColorDescription")]
            public Color TextColor
            {
                get
                {
                    return this._textColor;
                }
                set
                {
                    this._textColor = value;
                }
            }
            [SRCategory("ColorCategory")]
            [LocDisplayName("CDataColor")]
            [SRDescription("CDataColorDescription")]
            public Color CDataColor
            {
                get
                {
                    return this._cdataColor;
                }
                set
                {
                    this._cdataColor = value;
                }
            }
            [SRCategory("ColorCategory")]
            [LocDisplayName("BackgroundColor")]
            [SRDescription("BackgroundColorDescription")]
            public Color BackgroundColor
            {
                get
                {
                    return this._backgroundColor;
                }
                set
                {
                    this._backgroundColor = value;
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("ContainerBackgroundColor")]
            [SRDescription("ContainerBackgroundColorDescription")]
            public Color ContainerBackgroundColor
            {
                get
                {
                    return this._containerBackgroundColor;
                }
                set
                {
                    this._containerBackgroundColor = value;
                }
            }

            [SRCategory("ColorCategory")]
            [LocDisplayName("EditorBackgroundColor")]
            [SRDescription("EditorBackgroundColorDescription")]
            public Color EditorBackgroundColor
            {
                get
                {
                    return this._editorBackgroundColor;
                }
                set
                {
                    this._editorBackgroundColor = value;
                }
            }


            [SRCategory("FontCategory")]
            [LocDisplayName("FontPropertyName")]
            [SRDescription("FontDescription")]
            public Font Font
            {
                get
                {
                    return this._font;
                }
                set
                {
                    this._font = value;
                }
            }

            [SRCategory("LanguageCategory")]
            [LocDisplayName("LanguagePropertyName")]
            [SRDescription("LanguageDescription")]
            public string Language
            {
                get
                {
                    return this._language;
                }
                set
                {
                    this._language = value;
                }
            }

            [SRCategory("SchemaCategory")]
            [LocDisplayName("SchemaAwareTextName")]
            [SRDescription("SchemaAwareTextDescription")]
            public bool SchemaAwareText
            {
                get
                {
                    return this._schemaAwareText;
                }
                set
                {
                    this._schemaAwareText = value;
                }
            }

            [SRCategory("SchemaCategory")]
            [LocDisplayName("SchemaAwareNamesName")]
            [SRDescription("SchemaAwareNamesDescription")]
            public string SchemaAwareNames
            {
                get
                {
                    return this._schemaAwareNames;
                }
                set
                {
                    this._schemaAwareNames = value;
                }
            }

            [SRCategory("AnalyticsCategory")]
            [LocDisplayName("AllowAnalytics")]
            [SRDescription("AllowAnalyticsDescription")]
            public bool AllowAnalytics
            {
                get
                {
                    return this._allowAnalytics;
                }
                set
                {
                    this._allowAnalytics = value;
                }
            }

            [SRCategory("UpdateCategory")]
            [LocDisplayName("EnableUpdate")]
            [SRDescription("EnableUpdateDescription")]
            public bool EnableUpdate
            {
                get
                {
                    return this._enableUpdate;
                }
                set
                {
                    this._enableUpdate = value;
                }
            }

            [SRCategory("UpdateCategory")]
            [LocDisplayName("UpdateFrequency")]
            [SRDescription("UpdateFrequencyDescription")]
            public string UpdateFrequency
            {
                get
                {
                    return this._updateFrequency.ToString();
                }
                set
                {
                    try
                    {
                        var newFrequency = TimeSpan.Parse(value);
                        if (newFrequency < TimeSpan.FromMinutes(30))
                        {
                            throw new Exception(StringResources.UpdateFrequencyMinError);
                        }
                        else
                        {
                            this._updateFrequency = newFrequency;
                        }
                    } 
                    catch (FormatException)
                    {
                        throw new Exception(StringResources.UpdateFrequencyFormatError);
                    }
                }
            }

            [SRCategory("UpdateCategory")]
            [LocDisplayName("UpdateLocation")]
            [SRDescription("UpdateLocationDescription")]
            public string UpdateLocation
            {
                get
                {
                    return this._updateLocation;
                }
                set
                {
                    this._updateLocation = value;
                }
            }

            [SRCategory("SettingsCategory")]
            [LocDisplayName("SettingsLocation")]
            [SRDescription("SettingsLocationDescription")]
            public SettingsLocationUX SettingsLocation
            {
                get
                {
                    return (SettingsLocationUX)this._settingsLocation;
                }
                set
                {
                    this._settingsLocation = (SettingsLocation)value;
                }
            }


            [SRCategory("FormatCategory")]
            [LocDisplayName("AutoFormatOnSave")]
            [SRDescription("AutoFormatOnSaveDescription")]
            public bool AutoFormatOnSave
            {
                get
                {
                    return this._autoFormatOnSave;
                }
                set
                {
                    this._autoFormatOnSave = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("TreeIndent")]
            [SRDescription("TreeIndentDescription")]
            public int TreeIndent
            {
                get
                {
                    return this._treeIndent;
                }
                set
                {
                    this._treeIndent = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("IndentLevel")]
            [SRDescription("IndentLevelDescription")]
            public int IndentLevel
            {
                get
                {
                    return this._indentLevel;
                }
                set
                {
                    this._indentLevel = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("IndentChar")]
            [SRDescription("IndentCharDescription")]
            public IndentChar IndentChar
            {
                get
                {
                    return this._indentChar;
                }
                set
                {
                    this._indentChar = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("NewLineChars")]
            [SRDescription("NewLineCharsDescription")]
            public string NewLineChars
            {
                get
                {
                    return this._newLineChars;
                }
                set
                {
                    this._newLineChars = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("PreserveWhitespace")]
            [SRDescription("PreserveWhitespaceDescription")]
            public bool PreserveWhitespace
            {
                get
                {
                    return this._preserveWhitespace;
                }
                set
                {
                    this._preserveWhitespace = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("NoByteOrderMark")]
            [SRDescription("NoByteOrderMarkDescription")]
            public bool NoByteOrderMark
            {
                get
                {
                    return this._noByteOrderMark;
                }
                set
                {
                    this._noByteOrderMark = value;
                }
            }

            [SRCategory("FormatCategory")]
            [LocDisplayName("AttributesOnNewLineProperty")]
            [SRDescription("AttributesOnNewLineDescription")]
            public bool AttributesOnNewLine
            {
                get
                {
                    return this._attributesOnNewLine;
                }
                set
                {
                    this._attributesOnNewLine = value;
                }
            }

            [SRCategory("LongLineCategory")]
            [LocDisplayName("MaximumLineLengthProperty")]
            [SRDescription("MaximumLineLengthDescription")]
            public int MaximumLineLength
            {
                get
                {
                    return this._maximumLineLength;
                }
                set
                {
                    this._maximumLineLength = value;
                }
            }

            [SRCategory("LongLineCategory")]
            [LocDisplayName("MaximumValueLengthProperty")]
            [SRDescription("MaximumValueLengthDescription")]
            public int MaximumValueLength
            {
                get
                {
                    return this._maximumValueLength;
                }
                set
                {
                    this._maximumValueLength = value;
                }
            }

            [SRCategory("LongLineCategory")]
            [LocDisplayName("AutoFormatLongLinesProperty")]
            [SRDescription("AutoFormatLongLinesDescription")]
            public bool AutoFormatLongLines
            {
                get
                {
                    return this._autoFormatLongLines;
                }
                set
                {
                    this._autoFormatLongLines = value;
                }
            }

            [SRCategory("Validation")]
            [LocDisplayName("IgnoreDTDProperty")]
            [SRDescription("IgnoreDTDDescription")]
            public bool IgnoreDTD
            {
                get
                {
                    return this._ignoreDTD;
                }
                set
                {
                    this._ignoreDTD = value;
                }
            }

            [SRCategory("Validation")]
            [LocDisplayName("MaximumLineIndexProperty")]
            [SRDescription("MaximumLineIndexDescription")]
            public long MaximumLineIndex
            {
                get
                {
                    return this._maximumLineIndex;
                }
                set
                {
                    this._maximumLineIndex = value;
                }
            }

            [SRCategory("XsltCategory")]
            [LocDisplayName("EnableXsltScriptsPropertyName")]
            [SRDescription("EnableXsltScriptsDescription")]
            public bool EnableXsltScripts
            {
                get
                {
                    return this._enableXsltScripts;
                }
                set
                {
                    this._enableXsltScripts = value;
                }
            }

            [SRCategory("XsltCategory")]
            [LocDisplayName("DisableDefaultXslt")]
            [SRDescription("DisableDefaultXsltDescription")]
            public bool DisableDefaultXslt
            {
                get
                {
                    return this._disableDefaultXslt;
                }
                set
                {
                    this._disableDefaultXslt = value;
                }
            }

            [SRCategory("XsltCategory")]
            [LocDisplayName("WebBrowserPropertyName")]
            [SRDescription("WebBrowserDescription")]
            public WebBrowserVersion WebBrowserVersion
            {
                get
                {
                    return this._webBrowser;
                }
                set
                {
                    this._webBrowser = value;
                }
            }

            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreChildOrderProperty")]
            [SRDescription("XmlDiffIgnoreChildOrderDescription")]
            public bool XmlDiffIgnoreChildOrder
            {
                get
                {
                    return this._xmlDiffIgnoreChildOrder;
                }
                set
                {
                    this._xmlDiffIgnoreChildOrder = value;
                }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreCommentsProperty")]
            [SRDescription("XmlDiffIgnoreCommentsDescription")]
            public bool XmlDiffIgnoreComments
            {
                get { return this._xmlDiffIgnoreComments; }
                set { this._xmlDiffIgnoreComments = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnorePIProperty")]
            [SRDescription("XmlDiffIgnorePIDescription")]
            public bool XmlDiffIgnorePI
            {
                get { return this._xmlDiffIgnorePI; }
                set { this._xmlDiffIgnorePI = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreWhitespaceProperty")]
            [SRDescription("XmlDiffIgnoreWhitespaceDescription")]
            public bool XmlDiffIgnoreWhitespace
            {
                get { return this._xmlDiffIgnoreWhitespace; }
                set { this._xmlDiffIgnoreWhitespace = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreNamespacesProperty")]
            [SRDescription("XmlDiffIgnoreNamespacesDescription")]
            public bool XmlDiffIgnoreNamespaces
            {
                get { return this._xmlDiffIgnoreNamespaces; }
                set { this._xmlDiffIgnoreNamespaces = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnorePrefixesProperty")]
            [SRDescription("XmlDiffIgnorePrefixesDescription")]
            public bool XmlDiffIgnorePrefixes
            {
                get { return this._xmlDiffIgnorePrefixes; }
                set { this._xmlDiffIgnorePrefixes = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreXmlDeclProperty")]
            [SRDescription("XmlDiffIgnoreXmlDeclDescription")]
            public bool XmlDiffIgnoreXmlDecl
            {
                get { return this._xmlDiffIgnoreXmlDecl; }
                set { this._xmlDiffIgnoreXmlDecl = value; }
            }
            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffIgnoreDtdProperty")]
            [SRDescription("XmlDiffIgnoreDtdDescription")]
            public bool XmlDiffIgnoreDtd
            {
                get { return this._xmlDiffIgnoreDtd; }
                set { this._xmlDiffIgnoreDtd = value; }
            }

            [SRCategory("XmlDiff")]
            [LocDisplayName("XmlDiffHideIdenticalProperty")]
            [SRDescription("XmlDiffHideIdenticalDescription")]
            public bool XmlDiffHideIdentical
            {
                get { return this._xmlDiffHideIdentical; }
                set { this._xmlDiffHideIdentical = value; }
            }
            


            [SRCategory("EditingCategory")]
            [LocDisplayName("TextEditorProperty")]
            [SRDescription("TextEditorDescription")]
            public string Editor
            {
                get
                {
                    return this._textEditor;
                }
                set
                {
                    if (this._textEditor != value)
                    {
                        this._textEditor = value;
                    }
                }
            }


            [SRCategory("EditingCategory")]
            [LocDisplayName("PromptOnReload")]
            [SRDescription("PromptOnReloadDescription")]
            public bool PromptOnReload
            {
                get
                {
                    return this._promptOnReload;
                }
                set
                {
                    this._promptOnReload = value;
                }
            }


        }

        public sealed class MemberFilter : ICustomTypeDescriptor
        {
            private readonly HashSet<string> hidden = new HashSet<string>();
            private readonly object component;

            public MemberFilter(object component, params string[] memberNamesToHide)
            {
                this.component = component;
                hidden = new HashSet<string>(memberNamesToHide);
            }

            AttributeCollection ICustomTypeDescriptor.GetAttributes()
            {
                return TypeDescriptor.GetAttributes(component);
            }

            string ICustomTypeDescriptor.GetClassName()
            {
                return TypeDescriptor.GetClassName(component);
            }

            string ICustomTypeDescriptor.GetComponentName()
            {
                return TypeDescriptor.GetComponentName(component);
            }

            TypeConverter ICustomTypeDescriptor.GetConverter()
            {
                return TypeDescriptor.GetConverter(component);
            }

            EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            {
                EventDescriptor result = TypeDescriptor.GetDefaultEvent(component);
                return (result == null || hidden.Contains(result.Name)) ? null : result;

            }

            PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
            {
                PropertyDescriptor result = TypeDescriptor.GetDefaultProperty(component);
                return (result == null || hidden.Contains(result.Name)) ? null : result;
            }

            object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
            {
                return TypeDescriptor.GetEditor(component, editorBaseType);
            }

            EventDescriptorCollection
            ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            {
                return GetEvents(); // don't filter on attribute; we're calling the shots...
            }
            EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            {
                return GetEvents();
            }
            private EventDescriptorCollection GetEvents()
            {
                EventDescriptorCollection master = TypeDescriptor.GetEvents(component);
                var list = new List<EventDescriptor>(master.Count);
                foreach (EventDescriptor evt in master)
                {
                    if (!hidden.Contains(evt.Name))
                        list.Add(evt);
                }
                return new EventDescriptorCollection(list.ToArray());
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
            {
                return GetProperties(); // don't filter on attribute; we're calling the shots...
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            {
                return GetProperties();
            }
            private PropertyDescriptorCollection GetProperties()
            {
                PropertyDescriptorCollection master = TypeDescriptor.GetProperties(component);
                var list = new List<PropertyDescriptor>(master.Count);
                foreach (PropertyDescriptor prop in master)
                {
                    if (!hidden.Contains(prop.Name))
                        list.Add(prop);
                }
                return new PropertyDescriptorCollection(list.ToArray());
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return component;
            }
        }
    }

}
