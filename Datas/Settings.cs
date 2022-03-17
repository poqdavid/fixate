/*
 *      This file is part of Fixate distribution (https://github.com/vortex1409/fixate).
 *      Copyright (c) 2022 contributors
 *
 *      Fixate is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      Fixate is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with Fixate.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace Fixate.Datas;

public struct Settings
{
    private Discord defaultDiscord = new();
    private Voice defaultVoice = new();

    [JsonProperty("discord")]
    public Discord Discord
    {
        get => defaultDiscord;
        set
        {
            defaultDiscord = value;
        }
    }

    [JsonProperty("voice")]
    public Voice Voice
    {
        get => defaultVoice;
        set
        {
            defaultVoice = value;
        }
    }

    public Settings()
    {
        defaultDiscord = new Discord();
        defaultVoice = new Voice();
    }

    /// <summary>
    /// Saves the App settings in selected path.
    /// </summary>
    public static void SaveSetting()
    {
        if (!Directory.Exists(Program.CurrentDir))
        {
            _ = Directory.CreateDirectory(Program.CurrentDir);
        }

        JsonSerializerSettings s = new()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        File.WriteAllText(Program.SettingPath, JsonConvert.SerializeObject(Program.Config, Formatting.Indented, s));
    }

    /// <summary>
    /// Loads the App settings from the selected path.
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            string json_string = File.ReadAllText(Program.SettingPath);
            if (Json.IsValid(json_string))
            {
                JsonSerializerSettings s = new()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                Program.Config = JsonConvert.DeserializeObject<Settings>(json_string, s);
            }
            else
            {
                SaveSetting();
                LoadSettings();
            }
        }
        catch (Exception)
        {
            SaveSetting();
            LoadSettings();
        }
    }
}

public struct Discord
{
    private string defaultToken = "<token>";
    private string defaultCommandPrefix = "!";

    public Discord()
    {
        Token = defaultToken;
        CommandPrefix = defaultCommandPrefix;
    }

    public Discord(string token, string prefix)
    {
        Token = token;
        CommandPrefix = prefix;
    }

    [JsonProperty("token")]
    [DefaultValue("")]
    public string Token
    {
        get => defaultToken;
        set
        {
            defaultToken = value;
        }
    }

    [JsonProperty("prefix")]
    [DefaultValue("")]
    public string CommandPrefix
    {
        get => defaultCommandPrefix;
        set
        {
            defaultCommandPrefix = value;
        }
    }
}

public struct Voice
{
    private string defaultAPIToken = "";
    private string defaultAPIRegion = "";
    private string defaultLanguage = "en-US";
    private string defaultName = "en-US-JennyNeural";
    private string defaultStyle = "chat";

    public Voice()
    {
        APIToken = defaultAPIToken;
        APIRegion = defaultAPIRegion;
        Language = defaultLanguage;
        Name = defaultName;
        Style = defaultStyle;
    }

    public Voice(string apitoken, string apiregion, string language, string name, string style)
    {
        APIToken = apitoken;
        APIRegion = apiregion;
        Language = language;
        Name = name;
        Style = style;
    }

    [JsonProperty("apitoken")]
    [DefaultValue("")]
    public string APIToken
    {
        get => defaultAPIToken;
        set
        {
            defaultAPIToken = value;
        }
    }

    [JsonProperty("apiregion")]
    [DefaultValue("")]
    public string APIRegion
    {
        get => defaultAPIRegion;
        set
        {
            defaultAPIRegion = value;
        }
    }

    [JsonProperty("language")]
    [DefaultValue("en-US")]
    public string Language
    {
        get => defaultLanguage;
        set
        {
            defaultLanguage = value;
        }
    }

    [JsonProperty("name")]
    [DefaultValue("en-US-JennyNeural")]
    public string Name
    {
        get => defaultName;
        set
        {
            defaultName = value;
        }
    }

    [JsonProperty("style")]
    [DefaultValue("chat")]
    public string Style
    {
        get => defaultStyle;
        set
        {
            defaultStyle = value;
        }
    }
}