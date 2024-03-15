using System.Xml.Linq;

namespace Azymut.Helpers
{
    public class SoapXmlProcessor : ISoapXmlProcessor
    {
        private XDocument GetEnvelopeAndBody()
        {
            XDocument result = new XDocument(
                new XElement("Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", @"http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement("Body")
                    ) // Envelope
                );
            result.Declaration = new XDeclaration("1.0", "utf-8", "true");

            return result;
        }

        private XElement GetRequestMethodWithParameters(string method, Dictionary<string, string> parameters)
        {
            // Add method
            XElement result = new XElement(method);

            foreach (string param in parameters.Keys)
            {
                result.Add(new XElement(param, parameters[param]));
            }

            return result;
        }

        public XDocument GetFullXmlSoapRequest(string method, Dictionary<string, string> parameters)
        {
            // Generate envelope and body
            XDocument result = GetEnvelopeAndBody();

            // Generate method with parameters (request)
            XElement request = GetRequestMethodWithParameters(method, parameters);

            // Insert request into Body node
            result.Root.Element("Body").Add(request);

            return result;
        }
    }
}
