using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzDevopsPool;

class AzureDevopsConnection
{
    public Uri AzureDevopsUri;
    public string AzureDevopsPATToken;

    public AzureDevopsConnection(string url, string token)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentException("Url cannot be empty");
        if (string.IsNullOrEmpty(token)) throw new ArgumentException("Token cannot be empty");

        this.AzureDevopsUri = new Uri(url);
        this.AzureDevopsPATToken = token;
    }
}
