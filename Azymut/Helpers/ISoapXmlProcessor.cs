using System.Xml.Linq;

namespace Azymut.Helpers
{
    public interface ISoapXmlProcessor
    {
        XDocument GetFullXmlSoapRequest(string method, Dictionary<string, string> parameters);
    }
}