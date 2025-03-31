using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDevopsPool;

class Config
{
    public string Url { get; set; }
    public string Token { get; set; }

    public Config(string url, string token)
    {
        this.Url = url;
        this.Token = token;
    }
}

class AliasConfig
{
    public string DefaultAlias { get; set; } = string.Empty;
    public Dictionary<string, Config> Aliases { get; set; } = new();
}
