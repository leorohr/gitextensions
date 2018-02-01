using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using ResourceManager;

namespace VstsAndTfsIntegration.Settings
{
    [Export(typeof(IBuildServerSettingsUserControl))]
    [BuildServerSettingsUserControlMetadata(VstsAndTfsAdapter.PluginName)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class VstsAndTfsSettingsUserControl : GitExtensionsControl, IBuildServerSettingsUserControl
    {
        private const string DefaultCollection = "DefaultCollection";
        private static readonly Regex VstsHttpOrSshUrlRegex = new Regex(@"^.+://([^.@]+)(@[^.]*)?\.visualstudio\.com(:\d*)?(/[^/]*)?/_(git|ssh)/(.+)$");

        private string _defaultProjectName;
        private string _tokenManagementUrl;

        public VstsAndTfsSettingsUserControl()
        {
            InitializeComponent();
            Translate();

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            EnableRestApiLink();
        }

        public void Initialize(string defaultProjectName)
        {
            _defaultProjectName = defaultProjectName;
        }

        public void LoadSettings(ISettingsSource buildServerConfig)
        {
            if (buildServerConfig != null)
            {
                TfsTeamCollectionName.Text = buildServerConfig.GetString(VstsAndTfsAdapter.VstsTfsCollectionNameSettingKey, DefaultCollection);
                TfsProjectName.Text = buildServerConfig.GetString(VstsAndTfsAdapter.VstsTfsProjectNameSettingKey, _defaultProjectName);
                TfsBuildDefinitionNameFilter.Text = buildServerConfig.GetString(VstsAndTfsAdapter.VstsTfsBuildDefinitionNameFilterSettingKey, string.Empty);
                TfsServer.Text = buildServerConfig.GetString(VstsAndTfsAdapter.VstsTfsServerUrlSettingKey, string.Empty);
                RestApiToken.Text = buildServerConfig.GetString(VstsAndTfsAdapter.VstsTfsRestApiTokenSettingKey, string.Empty);
            }
        }

        public void SaveSettings(ISettingsSource buildServerConfig)
        {
            if (BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text))
            {
                buildServerConfig.SetString(VstsAndTfsAdapter.VstsTfsServerUrlSettingKey, string.IsNullOrWhiteSpace(TfsServer.Text) ? string.Empty : TfsServer.Text);
                buildServerConfig.SetString(VstsAndTfsAdapter.VstsTfsCollectionNameSettingKey, TfsTeamCollectionName.Text);
                buildServerConfig.SetString(VstsAndTfsAdapter.VstsTfsProjectNameSettingKey, TfsProjectName.Text);
                buildServerConfig.SetString(VstsAndTfsAdapter.VstsTfsBuildDefinitionNameFilterSettingKey, TfsBuildDefinitionNameFilter.Text);
                buildServerConfig.SetString(VstsAndTfsAdapter.VstsTfsRestApiTokenSettingKey, RestApiToken.Text);
            }
        }

        private void TfsBuildDefinitionNameFilter_TextChanged(object sender, EventArgs e)
        {
            labelRegexError.Visible = !BuildServerSettingsHelper.IsRegexValid(TfsBuildDefinitionNameFilter.Text);
        }

        private void TfsServer_TextChanged(object sender, EventArgs e)
        {
            EnableRestApiLink();
            if (TfsServer.Text.Contains("visualstudio.com"))
            {
                TfsTeamCollectionName.Text = DefaultCollection;
                TfsTeamCollectionName.Enabled = false;
            }
        }

        private void EnableRestApiLink()
        {
            _tokenManagementUrl = $"{TfsServer.Text}_details/security/tokens";
            RestApiTokenLink.Enabled = BuildServerSettingsHelper.IsUrlValid(_tokenManagementUrl);
        }

        private void RestApiTokenLink_Click(object sender, EventArgs e)
        {
            Process.Start(_tokenManagementUrl);
        }
    }
}
